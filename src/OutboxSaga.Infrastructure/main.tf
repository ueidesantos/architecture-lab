terraform {
  required_version = ">= 1.7.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  features {}
}

data "azurerm_resource_group" "lab" {
  name = var.resource_group_name
}

data "azurerm_cosmosdb_account" "lab" {
  name                = var.cosmos_account_name
  resource_group_name = data.azurerm_resource_group.lab.name
}