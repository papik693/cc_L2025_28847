output "function_app_name" {
  value = azurerm_linux_function_app.func.name
}

output "function_app_default_hostname" {
  value = azurerm_linux_function_app.func.default_hostname
}

output "database_connection_string" {
  value     = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${azurerm_mssql_server.sql.administrator_login};Password=${random_password.password.result};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}

output "instrumentation_key" {
  value     = var.enable_application_insights ? azurerm_application_insights.appinsights[0].instrumentation_key : null
} 