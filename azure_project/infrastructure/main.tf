terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

module "app1" {
  source = "../modules/azure_function_app"

  codename                    = "app1"
  location                    = "North Europe"
  resource_group_name         = "cdv"
  enable_application_insights = true
} 

output "app1" {
  value = module.app1
  sensitive = true
}