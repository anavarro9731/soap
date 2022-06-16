param(
    [switch]$UpgradeSoap,
    [switch]$UseSoapPackage,
    [switch]$UseSoapSource
)


Remove-Item -Recurse -Force .\.parcel-cache\
Remove-Item -Recurse -Force .\dist\    


if ($UpgradeSoap) {
    
    del package.json #otherwise parcel reads this one when compiling the components and it really screws up the import of @soap/vars and the .env stuff
    
    cd components
    Copy-Item -Path .\package.json.bak -Destination .\package.json
    yarn install #node_modules could be missing depending on scenario
    $v = npm view @soap/modules version
    Write-Host "Current Version: $v"
    npm version $v --allow-same-version #sometimes you have published versions that don't get checked in, this ensures you always bump from the latest remote version
    $v = npm version minor
    Write-Host "New Version: $v"
    npm publish
    cd ..

    Copy-Item -Path .\package.json.soappackage -Destination .\package.json
    yarn install #update outdated lock file
    iex "yarn upgrade @soap/modules@$v" #get the new version of soap
    
} elseif ($UseSoapPackage) {

    del package.json #otherwise parcel reads this one when compiling the components and it really screws up the import of @soap/vars and the .env stuff
    
    cd components
    Copy-Item -Path .\package.json.bak -Destination .\package.json
    yarn install #could be missing depending on scenario
    $v = npm view @soap/modules version
    Write-Host "Current Version: $v"
    npm version $v #set to current version since package.json was just overwritten
    $v = npm version minor
    Write-Host "New Version: $v"
    npm publish
    #cleanup 
    Remove-Item .\dist\ -Recurse
    Remove-Item .\node_modules\ -Recurse
    Remove-Item .\package.json
    Remove-Item .\yarn.lock
    cd ..
    
    Copy-Item -Path .\package.json.soappackage -Destination .\package.json
    yarn install
    iex "yarn upgrade @soap/modules@$v"
}
elseif ($UseSoapSource) 
{
    Remove-Item .\node_modules\@soap -Recurse
    Remove-Item .\yarn.lock
    
    cd components
    #cleanup crap from when it wasn't being used as source, package.json and node_modules will screw things up, the others are just messy
    Remove-Item .\dist\ -Recurse
    Remove-Item .\node_modules\ -Recurse
    Remove-Item .\package.json
    Remove-Item .\yarn.lock
    cd..
    
    Copy-Item -Path .\package.json.soapsource -Destination .\package.json
    yarn install
}

yarn serve
