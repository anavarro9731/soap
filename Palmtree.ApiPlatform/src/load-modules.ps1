$vstsCredentials = @("anavarro9731","VST`"04`"supremus")
$moduleRootUri = "https://anavarro9731.visualstudio.com/defaultcollection/powershell/_apis/git/repositories/powershell/items?api-version=1.0&scopepath="
$modules = @(
 "build.psm1"
)

Push-Location $PSScriptRoot

Write-Host "Importing Custom Modules"

# Base64-encodes the Personal Access Token (PAT) appropriately
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $vstsCredentials[0],$vstsCredentials[1])))

foreach ($module in $modules) {
 Invoke-RestMethod -Uri "$moduleRootUri$module" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -ContentType "text/plain; charset=UTF-8" -OutFile $module
 Import-Module ".\$module" -Verbose -Global -Force
}
