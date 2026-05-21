output "resource_group_name" {
	value = data.azurerm_resource_group.lab.name
}

output "resource_group_location" {
	value = data.azurerm_resource_group.lab.location
}

output "cosmos_account_name" {
	value = data.azurerm_cosmosdb_account.lab.name
}

output "cosmos_account_endpoint" {
	value = data.azurerm_cosmosdb_account.lab.endpoint
}

output "cosmos_account_capabilities" {
	value = data.azurerm_cosmosdb_account.lab.capabilities
}
