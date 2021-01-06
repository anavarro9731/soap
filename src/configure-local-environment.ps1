Param(
[string] $AzClientId,
[string] $AzClientSecret,
[string] $AzTenantId,
[string] $EnvironmentPartitionKey,
[string] $ResourceGroup,
[string] $FunctionAppName
)

Function Log {
    Param ([string] $Msg)
    Write-Host $Msg -foregroundcolor green
}
Function Log-Step {
    Param ([string] $Msg)
    Write-Host $Msg -foregroundcolor cyan -backgroundcolor black
}
Function IsEmpty([string] $s)  {
    Return [String]::IsNullOrWhiteSpace($s)
}
Function Get-TenantId([string] $s = $null) {
    $TenantId = $s ?? (Read-Host -Prompt 'Enter The TenantId of the ServicePrincipal needed to create the infratructure')
    if ([String]::IsNullOrWhiteSpace($TenantId)) {
        Write-Host 'ServicePrincipal TenantId cannot be blank'
        Exit -1
    }
    Return $TenantId
}
Function Get-ClientId([string] $s = $null) {
    $ClientId = $s ?? (Read-Host -Prompt 'Enter The Azure ClientId of the ServicePrincipal needed to create the infratructure')
    if ([String]::IsNullOrWhiteSpace($ClientId)) {
        Write-Host 'ServicePrincipal ClientId cannot be blank'
        Exit -1
    }
    Return $ClientId
}
Function Get-ClientSecret([string] $s = $null) {
    $ClientSecret = $s ?? (Read-Host -Prompt 'Enter The Azure ClientSecret of the ServicePrincipal needed to create the infratructure')
    if ([String]::IsNullOrWhiteSpace($ClientSecret)) {
        Write-Host 'ServicePrincipal ClientSecret cannot be blank'
        Exit -1
    }
    Return $ClientSecret
}
Function Get-ResourceGroup([string] $s = $null) {
    $ResourceGroup = $s ?? (Read-Host -Prompt 'Enter The Azure Resource Group Which Hosts Your App')
    if ([String]::IsNullOrWhiteSpace($ResourceGroup)) {
        Write-Host 'Azure ResourceGroup cannot be blank'
        Exit -1
    }
    Return $ResourceGroup
}
Function Get-VNextFunctionAppName([string] $s = $null) {
    $VNextFunctionAppName = $s ?? (Read-Host -Prompt 'Enter The Name Of The Function App For The VNEXT environment')
    if (-Not ($VNextFunctionAppName -match '-vnext$'))
    {
        Write-Host "Function App Name `"$VNextFunctionAppName`" must end with `"-vnext`""
        Exit -1
    }
    Return $VNextFunctionAppName
}
Function Get-EnvironmentPartitionKey([string] $s = $null) {
    $EnvPartitionKey = $s ?? (Read-Host -Prompt 'Enter An Environment Partition Key, This should be [a-z], 6-10 characters in length and be unique among all developers')
    if (-Not ($EnvPartitionKey -match '^[a-z]{6,10}$'))
    {
        Write-Host "Environment Partition Key `"$EnvPartitionKey`" does not match regex ^[a-z]{6,10}$"
        Exit -1
    }
    if ($EnvPartitionKey.toLower() -match '\b(?:vnext|dev|rel|live)\b') {
        Write-Host "Environment Partition Key cannot be vnext,dev,rel,live"
        Exit -1
    }
    Return $EnvPartitionKey
}
Function LoginToAzure {
    Param(
        [Parameter(Mandatory = $true)]
        [string] $azClientId,
        [Parameter(Mandatory = $true)]
        [string] $azClientSecret,
        [Parameter(Mandatory = $true)]
        [string] $azTenantId
    )

    Log "Logging into Azure"
    $azLoginCmd = "az login --service-principal -u `"$azClientId`" -p `"$azClientSecret`" --tenant `"$azTenantId`""
    Invoke-Expression $azLoginCmd
}

Log-Step "Authenticate"

$AzSpTenantId = Get-TenantId $AzTenantId
$AzSpClientId = Get-ClientId $AzClientId
$AzSpClientSecret = Get-ClientSecret $AzClientSecret
$EnvironmentPartitionKey = Get-EnvironmentPartitionKey $EnvironmentPartitionKey
$ResourceGroup = Get-ResourceGroup $ResourceGroup
$FunctionAppName = Get-VNextFunctionAppName $FunctionAppName

$vars = "AzSpTenantId:$AzSpTenantId\r\n"+
        "AzSpClientId:$AzSpClientId\r\n"+
        "AzSpClientSecret:$AzSpClientSecret\r\n"+
        "EnvPartitionKey:$EnvironmentPartitionKey\r\n"+
        "ResourceGroup:$ResourceGroup\r\n"+
        "FunctionAppName:$FunctionAppName\r\n"+
        "PSScriptRoot:$PSScriptRoot\r\n"        
Log $vars

LoginToAzure $AzSpClientId $AzSpClientSecret $AzSpTenantId

Log-Step "Downloading Function App Settings"
$cmd = "az functionapp config appsettings list -g $ResourceGroup -n $FunctionAppName | ConvertFrom-Json"
Write-Host $cmd
$VARS = iex $cmd
$var_AppId = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "AppId" | select -ExpandProperty value)
$var_AzureDevopsOrganisation = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "AzureDevopsOrganisation" | select -ExpandProperty value)
$var_AzureDevopsPat = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "AzureDevopsPat" | select -ExpandProperty value)
$var_AzureResourceGroup = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "AzureResourceGroup" | select -ExpandProperty value)
$var_AzureWebJobsServiceBus = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "AzureWebJobsServiceBus" | select -ExpandProperty value)
$var_AzureBusNamespace = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "AzureBusNamespace" | select -ExpandProperty value)
$var_AzureSignalRConnectionString = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "AzureSignalRConnectionString" | select -ExpandProperty value)
$var_CosmosDbEndpointUri = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "CosmosDbEndpointUri" | select -ExpandProperty value)
$var_CosmosDbKey = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "CosmosDbKey" | select -ExpandProperty value)
$var_CosmosDbDatabasename = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "CosmosDbDatabasename" | select -ExpandProperty value)
$var_APPINSIGHTS_INSTRUMENTATIONKEY = (az functionapp config appsettings list -g rg-soap -n soapapisample-vnext | ConvertFrom-Json | where name -eq "APPINSIGHTS_INSTRUMENTATIONKEY" | select -ExpandProperty value)

if ($null -eq $VARS) { throw "Could not retrieve app settings from VNEXT" }
if (IsEmpty($var_AppId)) { throw "Property `"AppId`" could not be found in App Settings" }
if (IsEmpty($var_AzureDevopsOrganisation)) { throw "Property `"AzureDevopsOrganisation`" could not be found in App Settings" }
if (IsEmpty($var_AzureDevopsPat)) { throw "Property `"AzureDevopsPat`" could not be found in App Settings" }
if (IsEmpty($var_AzureResourceGroup)) { throw "Property `"AzureResourceGroup`" could not be found in App Settings" }
if (IsEmpty($var_AzureWebJobsServiceBus)) { throw "Property `"AzureWebJobsServiceBus`" could not be found in App Settings" }
if (IsEmpty($var_AzureBusNamespace)) { throw "Property `"AzureBusNamespace`" could not be found in App Settings" }
if (IsEmpty($var_AzureSignalRConnectionString)) { throw "AzureSignalRConnectionString `"Property`" could not be found in App Settings" }
if (IsEmpty($var_CosmosDbEndpointUri)) { throw "Property `"CosmosDbEndpointUri`" could not be found in App Settings" }
if (IsEmpty($var_CosmosDbKey)) { throw "Property `"CosmosDbKey`" could not be found in App Settings" }
if (IsEmpty($var_CosmosDbDatabasename)) { throw "Property `"CosmosDbDatabasename`" could not be found in App Settings" }
if (IsEmpty($var_APPINSIGHTS_INSTRUMENTATIONKEY)) { throw "Property `"APPINSIGHTS_INSTRUMENTATIONKEY`" could not be found in App Settings" }

Log-Step "Downloading ServiceBus Connection String"
$cmd = "az servicebus namespace authorization-rule keys list --resource-group $ResourceGroup --namespace-name sb-$FunctionAppName --name SenderAccessKey --query primaryConnectionString --output tsv"
Write-Host $cmd
$SenderConnString = iex $cmd
if (IsEmpty($SenderConnString)) { throw "Cannot obtain service bus sender key" }

Log-Step "Set local.settings.json properies"
$LocalSettingsJson = @"
{
    `"IsEncrypted`": false,
    `"Values`": {
        `"FUNCTIONS_WORKER_RUNTIME`": `"dotnet`",             
        `"AppId`": `"$var_AppId`",
        `"SoapEnvironmentKey`": `"DEV`",
        `"EnvironmentPartitionKey`": `"$EnvironmentPartitionKey`",
        `"AzureDevopsOrganisation`": `"$var_AzureDevopsOrganisation`",
        `"AzureDevopsPat`": `"$var_AzureDevopsPat`",       
        `"AzureResourceGroup`": `"$var_AzureResourceGroup`",
        `"AzureWebJobsStorage`": `"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;`",
        `"AzureWebJobsServiceBus`": `"$var_AzureWebJobsServiceBus`",
        `"AzureBusNamespace`": `"$var_AzureBusNamespace`",
        `"AzureSignalRConnectionString`": `"$var_AzureSignalRConnectionString`",
        `"AzureSignalRServiceTransportType`": `"Transient`",
        `"CosmosDbEndpointUri`": `"$var_CosmosDbEndpointUri`",
        `"CosmosDbKey`": `"$var_CosmosDbKey`",
        `"CosmosDbDatabasename`": `"$var_CosmosDbDatabasename`"                             	  
    },
    `"Host`": {
        `"CORS`": `"http://localhost:1234`",
        `"CORSCredentials`": true
    }
}
"@
$LocalSettingsJsonPath = (Get-ChildItem -Path "$PSScriptRoot" -Recurse local.settings.json | Select-Object -ExpandProperty FullName | Select -First 1)
Set-Content -Path "$LocalSettingsJsonPath" -Value $LocalSettingsJson

Log-Step "Set .env properies"
$DotEnv = @"
FUNCTIONAPP_ROOT=`"http://localhost:7071/api`"
SERVICEBUS_CONN=`"$SenderConnString`"
APPINSIGHTS_KEY=`"$var_APPINSIGHTS_INSTRUMENTATIONKEY`"
BLOBSTORAGE_URI=`"http://127.0.0.1:10000/devstoreaccount1`"
ENVIRONMENT_PARTITION_KEY=`"$EnvironmentPartitionKey`"
"@
$DotEnvPath = (Get-ChildItem -Path "$PSScriptRoot" -Recurse .env | Select-Object -ExpandProperty FullName | Select -First 1)
Set-Content -Path "$DotEnvPath" -Value $DotEnv


