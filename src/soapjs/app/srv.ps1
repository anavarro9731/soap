param(
    [switch]$UpgradeSoap,
    [switch]$UseSoapPackage,
    [switch]$UseSoapSource
)

if (Test-Path .parcel-cache) {
    Remove-Item -Recurse -Force .parcel-cache     
}
if (Test-Path dist) {
    Remove-Item -Recurse -Force dist    
}

if ($UpgradeSoap) {
    
    cd components
    yarn install #could be missing depending on scenario
    $v = npm view @soap/modules version
    Write-Host "Current Version: $v"
    $v = npm version minor
    Write-Host "New Version: $v"
    npm publish
    cd ..
    
    iex "yarn upgrade @soap/modules@$v"
    
} elseif ($UseSoapPackage) { 

    Remove-Item .\node_modules\ -Recurse
    Remove-Item .\yarn.lock

    cd components
    Copy-Item -Path .\package.json.lib -Destination .\package.json
    yarn install #could be missing depending on scenario
    $v = npm view @soap/modules version
    Write-Host "Current Version: $v"
    npm version $v #set to current version since package.json was just overwritten
    $v = npm version minor
    Write-Host "New Version: $v"
    npm publish
    cd ..


    Copy-Item -Path .\package.json.soappackage -Destination .\package.json
    yarn install
    iex "yarn upgrade @soap/modules@$v"
}
elseif ($UseSoapSource) 
{
    Remove-Item .\node_modules\ -Recurse
    Remove-Item .\yarn.lock
    
    cd components
    Remove-Item .\node_modules\ -Recurse
    Remove-Item .\yarn.lock
    Remove-Item .\dist\ -Recurse
    Remove-Item .\package.json
    cd..
    
    Copy-Item -Path .\package.json.soapsource -Destination .\package.json
    yarn install
}

yarn serve
