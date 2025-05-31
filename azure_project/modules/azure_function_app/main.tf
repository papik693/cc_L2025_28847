resource "azurerm_resource_group" "rg" {
  count    = var.resource_group_name == null ? 1 : 0
  name     = "rg-${var.codename}"
  location = var.location
}

locals {
  resource_group_name = var.resource_group_name == null ? azurerm_resource_group.rg[0].name : var.resource_group_name
}

resource "azurerm_storage_account" "sa" {
  name                     = "${var.codename}sa${random_string.random.result}"
  resource_group_name      = local.resource_group_name
  location                = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "random_string" "random" {
  length  = 8
  special = false
  upper   = false
}

resource "random_password" "password" {
  length  = 16
  special = true
}

resource "azurerm_mssql_server" "sql" {
  name                         = "${var.codename}-sql"
  resource_group_name          = local.resource_group_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = "sqladmin"
  administrator_login_password = random_password.password.result
  public_network_access_enabled = true  # Enable public network access
}

# Add firewall rule to allow all IPs
resource "azurerm_mssql_firewall_rule" "allow_all" {
  name                = "AllowAll"
  server_id           = azurerm_mssql_server.sql.id
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "255.255.255.255"
}

resource "azurerm_mssql_database" "db" {
  name           = "${var.codename}-db"
  server_id      = azurerm_mssql_server.sql.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  sku_name       = "Basic"
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "law" {
  count               = var.enable_application_insights ? 1 : 0
  name                = "${var.codename}-law"
  location            = var.location
  resource_group_name = local.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = 90
}

resource "azurerm_application_insights" "appinsights" {
  count               = var.enable_application_insights ? 1 : 0
  name                = "${var.codename}-app-insights"
  location            = var.location
  resource_group_name = local.resource_group_name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.law[0].id
}

resource "azurerm_service_plan" "asp" {
  name                = "${var.codename}-asp"
  resource_group_name = local.resource_group_name
  location            = var.location
  os_type            = "Linux"
  sku_name           = "Y1"
}

resource "azurerm_linux_function_app" "func" {
  name                       = "${var.codename}-func"
  resource_group_name        = local.resource_group_name
  location                   = var.location
  service_plan_id            = azurerm_service_plan.asp.id
  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key

  site_config {
    application_stack {
      python_version = "3.10"
    }
    
    cors {
      allowed_origins = [
        "https://portal.azure.com",
        "https://ms.portal.azure.com",
        "https://${var.codename}-func.azurewebsites.net"
      ]
      support_credentials = true
    }

    application_insights_connection_string = var.enable_application_insights ? azurerm_application_insights.appinsights[0].connection_string : null
    application_insights_key = var.enable_application_insights ? azurerm_application_insights.appinsights[0].instrumentation_key : null

    # use_32_bit_worker = false
    # always_on = true
    # Enable system packages for ODBC
    app_scale_limit = 1
    health_check_path = "/api/items"
  }

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"       = "python"
    "DATABASE_CONNECTION_STRING"     = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${azurerm_mssql_server.sql.administrator_login};Password=${random_password.password.result};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    "AzureWebJobsStorage"           = azurerm_storage_account.sa.primary_connection_string
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING" = azurerm_storage_account.sa.primary_connection_string
    "WEBSITE_CONTENTSHARE"          = lower(var.codename)
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = var.enable_application_insights ? azurerm_application_insights.appinsights[0].connection_string : ""
    
    
    # Enable startup script

    # Enable startup script
    "WEBSITE_RUN_FROM_PACKAGE"      = "1"
    "ENABLE_ORYX_BUILD"             = "true"
    "SCM_DO_BUILD_DURING_DEPLOYMENT" = "true"
    "ENABLE_DYNAMIC_INSTALL"        = "true"
  }

  lifecycle {
    ignore_changes = [
      app_settings["APPINSIGHTS_INSTRUMENTATIONKEY"],
      # app_settings["APPLICATIONINSIGHTS_CONNECTION_STRING"],
      # app_settings["AzureWebJobsStorage"],
      site_config[0].application_insights_key,
    ]
  }
}
