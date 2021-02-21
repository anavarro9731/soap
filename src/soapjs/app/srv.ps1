param(
    [switch]$ToRemoteSoap,
    [switch]$ToLocalSoap,
    [switch]$UpgradeRemoteSoap
)

function RepublishModules {
    cd ..\components\modules
    npm version minor
    npm publish
    cd ..\..\app
}

#kill -name node
if (Test-Path .parcel-cache) {
    Remove-Item -Recurse -Force .parcel-cache #v2    
}
if (Test-Path .cache) {
    Remove-Item -Recurse -Force .cache #v1    
}
if (Test-Path dist) {
    Remove-Item -Recurse -Force dist    
}

if ($UpgradeRemoteSoap) {
    RepublishModules
    yarn upgrade @soap/modules@latest
} elseif ($ToRemoteSoap) { # take from feed requires publishing
    if (Test-Path .\components)
    {
        Remove-Item .\components
    }
    if (Test-Path .\babel.config.json)
    {
        Remove-Item .\babel.config.json
    }
    
    Remove-Item .\node_modules\ -Recurse
    Remove-Item .\yarn.lock
    
    Copy-Item -Path .\package.json.apponly -Destination .\package.json
    
    yarn install
    RepublishModules
    yarn upgrade @soap/modules@latest
}
elseif ($ToLocalSoap) # take from local folder, via symlink
{
    if (-Not (Test-Path .\components))
    {
        Write-Host "creating symlink"
        New-Item -Path .\components -ItemType SymbolicLink -Value ..\components\modules\src
    }
    
    Remove-Item .\node_modules\ -Recurse
    Remove-Item .\yarn.lock
    
    Copy-Item -Path .\package.json.withsoap -Destination .\package.json
    Copy-Item ..\components\modules\babel.config.json
    
    yarn install
}
yarn run serve
