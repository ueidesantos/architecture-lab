param(
  [string]$WorkingDir = ".",
  [string]$Target = "azurerm_resource_group.lab"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Push-Location $WorkingDir
try {
  terraform init
  terraform destroy -target=$Target -auto-approve
}
finally {
  Pop-Location
}