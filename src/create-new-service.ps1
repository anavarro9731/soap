Param(
	[string] $Arg_AzureDevopsOrganisationName,
	[string] $Arg_ServiceName,
	[string] $Arg_AdminAzPersonalAccessToken,
	[string] $Arg_RepoAndPackagingAzPersonalAccessToken,
	[string] $Arg_AzResourceGroup,
	[string] $Arg_AzLocation,
	[string] $Arg_TenantId,
	[string] $Arg_PathOnDisk,
	[string] $Arg_ClientId,
	[string] $Arg_ClientSecret
)


Function Log {
	Param ([string] $Msg)
	Write-Host $Msg -foregroundcolor green
}
Function Log-Step {
	Param ([string] $Msg)
	Write-Host $Msg -foregroundcolor cyan -backgroundcolor black
}
Function Test-IsGitInstalled
{
	return -Not ((Get-Command git 2>$null) -eq $null)
}
Function Replace-TextWithinALineOfPwshBootstrap([string] $old, [string] $new)
{
	(Get-Content .\pwsh-bootstrap.ps1) | % { $_.replace($old, $new) } | Set-Content .\pwsh-bootstrap.ps1
}
Function Replace-ALineOfPwshBootstrapThatContains([string] $searchString, [string] $replacementLine)
{
(Get-Content .\pwsh-bootstrap.ps1) | % { if ($_.contains($searchString)) { $replacementLine } else { $_ } } | Set-Content .\pwsh-bootstrap.ps1
}

Function Remove-ConfigLine([string] $old)
{
	(Get-Content .\pwsh-bootstrap.ps1) | Where-Object {$_ -notmatch $old } | Set-Content .\pwsh-bootstrap.ps1
}
Function Test-PreReqs {
	if (-Not (Test-IsGitInstalled)) {
		Write-Host "Git is not installed"
		return
	}
}
Function IsEmpty([string] $s)  {
	Return [String]::IsNullOrWhiteSpace($s)
}
Function EmptyConcat([string] $s, [string] $prompt) { #if nothing is passed in then prompt for it, this is for when this script is called externally by test.ps1
	$result = (IsEmpty $s) ? $(Read-Host -Prompt "$prompt") : $s
	return $result
}
Function Get-AzureDevopsOrganisationName([string] $s = $null) {
	$OrgName = EmptyConcat $s 'Enter The Azure Devops Organisation Name'
	if (IsEmpty $OrgName) {
		Write-Host 'Azure Devops Organisation Name cannot be blank'
		Exit -1
	}
	Return $OrgName
}
Function Get-ServiceName([string] $s = $null)  {
	$ServiceName = EmptyConcat $s 'Enter The New Service Name (Allowed Characters A-Z,a-z and ".") 5-18 chars'
	if (-Not ($ServiceName -match '^[A-Za-z\.]{5,18}$'))
	{
		Write-Host "Service Name `"$ServiceName`" does not match regex"
		Exit -1
	}	
	Return $ServiceName
}
Function Get-RepoAndPackagingAzPersonalAccessToken([string] $s = $null)  {
	$PAT = EmptyConcat $s 'Enter An Azure Devops Personal Access Token with permissions to read from the repo to read the config and permission to read and push packages to/from the associated package feed'
	# it may seem like this token should be able to write to the repo too, but that is left up to the git config
	# the current soap feed shouldn't need auth, so technically read permission for packages is not needed but that could change in future
	# we will also have to consider in future how to handle additional feeds that are providing messages packages for other services
	if (IsEmpty $PAT) {
		Write-Host 'Personal Access Token cannot be blank'
		Exit -1
	}
	Return $PAT
}
Function Get-AdminAzPersonalAccessToken([string] $s = $null)  {
	$PAT = EmptyConcat $s 'Enter An Azure Devops Personal Access Token with Admin permissions required to create the repo. This will only be used by the script and will NOT be stored.'
	if (IsEmpty $PAT) {
		Write-Host 'Personal Access Token cannot be blank'
		Exit -1
	}
	Return $PAT
}
Function Get-AzResourceGroup([string] $s = $null)  {
	$ResourceGroup = EmptyConcat $s 'Enter The Azure Resource Group the new resources should be created under'
	if (IsEmpty $ResourceGroup) {
		Write-Host 'Resource Group cannot be blank'
		Exit -1
	}
	Return $ResourceGroup
}
Function Get-AzLocation([string] $s = $null)  {
	$Location = EmptyConcat $s 'Enter The Azure Location (e.g. uksouth) where the new resources should be created'
	if (IsEmpty $Location) {
		Write-Host 'Azure Location cannot be blank'
		Exit -1
	}
	Return $Location
}
Function Get-TenantId([string] $s = $null)  {
	$TenantId = EmptyConcat $s 'Enter The TenantId of the ServicePrincipal needed to create the infrastructure'
	if (IsEmpty $TenantId) {
		Write-Host 'ServicePrincipal TenantId cannot be blank'
		Exit -1
	}
	Return $TenantId
}
Function Get-ClientId([string] $s = $null)  {
	$ClientId = EmptyConcat $s 'Enter The Azure ClientId of the ServicePrincipal needed to create the infrastructure'
	if (IsEmpty $ClientId) {
		Write-Host 'ServicePrincipal ClientId cannot be blank'
		Exit -1
	}
	Return $ClientId
}
Function Get-ClientSecret([string] $s = $null)  {
	$ClientSecret = EmptyConcat $s 'Enter The Azure ClientSecret of the ServicePrincipal needed to create the infrastructure'
	if (IsEmpty $ClientSecret) {
		Write-Host 'ServicePrincipal ClientSecret cannot be blank'
		Exit -1
	}
	Return $ClientSecret
}
Function Get-PathOnDisk([string] $s = $null)  {
	$DiskLocation = EmptyConcat $s 'Enter The Target Directory (e.g. c:\code)'
	if (-Not ($DiskLocation -match '^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$')) {
		Write-Host "$DiskLocation is not a valid directory path format"
		Exit -1
	}
	if (-Not (Test-Path -PathType Container $DiskLocation)) {
		New-Item -ItemType Directory -Force -Path $DiskLocation
	}
	$DiskLocation = $DiskLocation.trim('\')
	Return $DiskLocation
}

Function Get-ServiceRoot ([string] $DiskLocation, [string] $ServiceName) {
	Return "$DiskLocation\$ServiceName"
}

Function CreateOrClean-Directory ([string] $Directory) {
	if (Test-Path $Directory) {
		Remove-Item -Recurse -Force $Directory
	}
	mkdir $Directory
}

Function Get-HealthCheckUrl([string] $AzureDevopsName) {
	$FunctionAppName = $AzureDevopsName.ToLower() -replace '[^a-z0-9]', '' #* FRAGILE must match Pack-And-Publish logic
	return "https://$FunctionAppName##ENVSUFFIX##.azurewebsites.net/api/CheckHealth"
}

Test-PreReqs

Log-Step "Acquiring Variables..."

$AzureDevopsOrganisationName = Get-AzureDevopsOrganisationName $Arg_AzureDevopsOrganisationName
$AzureDevopsOrganisationUrl = "https://dev.azure.com/$AzureDevopsOrganisationName/"
$ServiceName = Get-ServiceName $Arg_ServiceName
$AzureDevopsName = $ServiceName.Replace(".", "-")
$AdminAzPersonalAccessToken = Get-AdminAzPersonalAccessToken $Arg_AdminAzPersonalAccessToken
$RepoAndPackagingAzPersonalAccessToken = Get-RepoAndPackagingAzPersonalAccessToken $Arg_RepoAndPackagingAzPersonalAccessToken
$env:AZURE_DEVOPS_EXT_PAT = $AdminAzPersonalAccessToken #this is how the az devops command authenticates and why admin priveleges are needed
$AzResourceGroup = Get-AzResourceGroup $Arg_AzResourceGroup
$AzLocation = Get-AzLocation $Arg_AzLocation
$PathOnDisk = Get-PathOnDisk $Arg_PathOnDisk
$ServiceRoot = Get-ServiceRoot $PathOnDisk $ServiceName
$ConfigRepoRoot = "$PathOnDisk\$ServiceName.config"
$SoapFeedUri = "https://pkgs.dev.azure.com/anavarro9731/soap-feed/_packaging/soap-pkgs/nuget/v3/index.json"
$TenantId = Get-TenantId $Arg_TenantId
$ClientId = Get-ClientId $Arg_ClientId
$ClientSecret = Get-ClientSecret $Arg_ClientSecret
$HealthCheckUrl = Get-HealthCheckUrl $AzureDevopsName

$vars = "AzureDevopsOrganisationName:$AzureDevopsOrganisationName`r`n"+
"AzureDevopsOrganisationUrl:$AzureDevopsOrganisationUrl`r`n"+ 
"ServiceName:$ServiceName`r`n"+  
"AzureDevopsName:$AzureDevopsName`r`n"+ 
"AzPersonalAccessToken:$RepoAndPackagingAzPersonalAccessToken`r`n"+ 
"env:AZURE_DEVOPS_EXT_PAT:$env:AZURE_DEVOPS_EXT_PAT`r`n"+ 
"AzResourceGroup:$AzResourceGroup`r`n"+ 
"AzLocation:$AzLocation`r`n"+
"PathOnDisk:$PathOnDisk`r`n"+
"ServiceRoot:$ServiceRoot`r`n"+
"ConfigRepoRoot:$ConfigRepoRoot`r`n"+
"SoapFeedUri:$SoapFeedUri`r`n"+ 
"PSScriptRoot:$PSScriptRoot`r`n"+
"TenantId:$TenantId`r`n"+
"ClientId:$ClientId`r`n"+
"ClientSecret:$ClientSecret`r`n"

Log $vars

Log-Step "Creating Config Repo, Please wait..."

CreateOrClean-Directory $ConfigRepoRoot
Copy-Item "..\.gitignore" "$ConfigRepoRoot\.gitignore" -Force
Copy-Item ".\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" "$ConfigRepoRoot\DEV_Config.cs"  -Force
Copy-Item ".\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" "$ConfigRepoRoot\VNEXT_Config.cs"  -Force
Copy-Item ".\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" "$ConfigRepoRoot\REL_Config.cs"  -Force
Copy-Item ".\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" "$ConfigRepoRoot\LIVE_Config.cs"  -Force
Set-Location $ConfigRepoRoot
git init
dotnet new classlib -f "netcoreapp3.1" -n Config
mv DEV_Config.cs Config
mv VNEXT_Config.cs Config
mv REL_Config.cs Config
mv LIVE_Config.cs Config
cd Config
(Get-Content .\DEV_Config.cs) | % { $_.replace("namespace Soap.Api.Sample.Afs", "namespace Config.DEV") } | Set-Content .\DEV_Config.cs
(Get-Content .\VNEXT_Config.cs) | % { $_.replace("namespace Soap.Api.Sample.Afs", "namespace Config.VNEXT") } | Set-Content .\VNEXT_Config.cs
(Get-Content .\REL_Config.cs) | % { $_.replace("namespace Soap.Api.Sample.Afs", "namespace Config.REL") } | Set-Content .\REL_Config.cs
(Get-Content .\LIVE_Config.cs) | % { $_.replace("namespace Soap.Api.Sample.Afs", "namespace Config.LIVE") } | Set-Content .\LIVE_Config.cs
mkdir DEV
mkdir VNEXT
mkdir REL
mkdir LIVE
mv DEV_Config.cs DEV/Config.cs
mv VNEXT_Config.cs VNEXT/Config.cs
mv REL_Config.cs REL/Config.cs
mv LIVE_Config.cs LIVE/Config.cs
del Class1.cs
dotnet nuget add source $SoapFeedUri
dotnet add package Soap.Config -s $SoapFeedUri
cd $ConfigRepoRoot
dotnet new sln -n Config
Get-ChildItem -Recurse -File -Filter "*.csproj" | ForEach-Object { dotnet sln add $_.FullName }

Log "Creating Azure Devops Project"
az devops project create --organization $AzureDevopsOrganisationUrl --name $AzureDevopsName --detect false

Log "Creating Azure Devops Config Repo"
az repos create  --organization $AzureDevopsOrganisationUrl --project $AzureDevopsName --name "$AzureDevopsName.config"  --detect false

Log "Uploading Config Repo"
git add -A
git commit -m "initial"
git remote add origin "https://dev.azure.com/$AzureDevopsOrganisationName/$AzureDevopsName/_git/$AzureDevopsName.config"
$gitPushCmd = "git push https://whatever:$RepoAndPackagingAzPersonalAccessToken@dev.azure.com/$AzureDevopsOrganisationName/$AzureDevopsName/_git/$AzureDevopsName.config master --set-upstream"
Write-Host $gitPushCmd
iex $gitPushCmd

Log-Step "Creating Service Repo, Please wait..."

Set-Location $PSScriptRoot
CreateOrClean-Directory $ServiceRoot

Log "Creating Client App"

Set-Location $PSScriptRoot
$sourceAppDir = "$PSScriptRoot\soapjs\app"
Copy-Item "$sourceAppDir\" "$ServiceRoot\app\"
Set-Content -Path "$ServiceRoot\app\.env" -Value "##RUN configure-local-environment SCRIPT TO POPULATE##" 
Copy-Item "$sourceAppDir\index.service-template.js" "$ServiceRoot\app\index.js"
Copy-Item "$sourceAppDir\.npmrc" "$ServiceRoot\app\"
Copy-Item "$sourceAppDir\package.json" "$ServiceRoot\app\"
Copy-Item "$sourceAppDir\index.html" "$ServiceRoot\app\"
Copy-Item "$sourceAppDir\soap-vars.js" "$ServiceRoot\app\"



Log "Creating Server App"

Set-Location $PSScriptRoot

New-Item -ItemType Directory -Force -Path $ServiceRoot
Copy-Item .\Soap.Api.Sample\* "$ServiceRoot" -Recurse -Force -Exclude $Excluded
@('bin', 'obj', 'published') | foreach{ Get-ChildItem -Path $ServiceRoot -Include $_ -Recurse -Force | Remove-Item -Force -Recurse }

Copy-Item .\pwsh-bootstrap.ps1 "$ServiceRoot" -Force
Copy-Item .\configure-local-environment.ps1 "$ServiceRoot" -Force
Copy-Item .\new-message.ps1 "$ServiceRoot" -Force
Copy-Item ..\azure-pipelines.yml "$ServiceRoot" -Force
#* cull src directory from paths
(Get-Content $ServiceRoot\azure-pipelines.yml) | % { $_.replace('src\', '') } | Set-Content $ServiceRoot\azure-pipelines.yml
(Get-Content $ServiceRoot\azure-pipelines.yml) | % { $_.replace('src/', '') } | Set-Content $ServiceRoot\azure-pipelines.yml
Set-Location $ServiceRoot

Log "Customizing Project Files"

Replace-TextWithinALineOfPwshBootstrap '"Soap.Api.Sample\Soap.Api.Sample.Afs"' "`"$ServiceName.Afs`""
Replace-TextWithinALineOfPwshBootstrap '"Soap.Api.Sample\Soap.Api.Sample.Messages"' "`"$ServiceName.Messages`""

Get-ChildItem -Filter "*Soap.Api.Sample*" -Recurse | Where {$_.FullName -notlike "*\obj\*"} | Where {$_.FullName -notlike "*\bin\*"} |  Rename-Item -NewName {$_.name -replace "Soap.Api.Sample","$ServiceName" }
Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.ps1,*.js,*.jsx | ForEach-Object {
	(Get-Content $_).replace('Soap.Api.Sample',"$ServiceName") | Set-Content $_
}
$Removals = ls -r . -filter *.cs | select-string "##REMOVE-IN-COPY##" | select path
$Removals | % { Remove-Item $_.Path }
Set-Content -Path "$ServiceRoot\$ServiceName.Afs\local.settings.json" -Value "##RUN configure-local-environment SCRIPT TO POPULATE##"
git init
dotnet new sln -n $ServiceName
Get-ChildItem -Recurse -File -Filter "*.csproj" | ForEach-Object { dotnet sln add $_.FullName }
Get-ChildItem -Recurse -File -Filter "*.csproj" | ForEach-Object {
	dotnet remove $_ reference '..\..\Soap.PfBase.Api\Soap.PfBase.Api.csproj'
	dotnet remove $_ reference '..\..\Soap.PfBase.Models\Soap.PfBase.Models.csproj'
	dotnet remove $_ reference '..\..\Soap.PfBase.Logic\Soap.PfBase.Logic.csproj'
	dotnet remove $_ reference '..\..\Soap.PfBase.Messages\Soap.PfBase.Messages.csproj'
	dotnet remove $_ reference '..\..\Soap.PfBase.Tests\Soap.PfBase.Tests.csproj'
	if ($_ -like '*.Models.csproj') {
		dotnet add $_ package Soap.PfBase.Models -s $soapFeedUri
	}
	if ($_ -like '*.Afs.csproj') {
		dotnet add $_ package Soap.PfBase.Api -s $soapFeedUri
	}
	if ($_ -like '*.Logic.csproj') {
		dotnet add $_ package Soap.PfBase.Logic -s $soapFeedUri
	}
	if ($_ -like '*.Tests.csproj') {
		dotnet add $_ package Soap.PfBase.Tests -s $soapFeedUri
	}
	if ($_ -like '*.Messages.csproj') {
		dotnet add $_ package Soap.PfBase.Messages -s $soapFeedUri
	}
}

Log "Configuring Pwsh-Bootstrap Script"


#* remove all library projects (which will now be referenced from the soap feed)
Remove-ConfigLine '"Soap.Idaam"' ""
Remove-ConfigLine '"Soap.Client"' ""
Remove-ConfigLine '"Soap.Bus"' ""
Remove-ConfigLine '"Soap.Config"' ""
Remove-ConfigLine '"Soap.Context"' ""
Remove-ConfigLine '"Soap.Interfaces"' ""
Remove-ConfigLine '"Soap.Interfaces.Messages"' ""
Remove-ConfigLine '"Soap.MessagePipeline"' ""
Remove-ConfigLine '"Soap.NotificationServer"' ""
Remove-ConfigLine '"Soap.PfBase.Api"' ""
Remove-ConfigLine '"Soap.PfBase.Logic"' ""
Remove-ConfigLine '"Soap.PfBase.Tests"' ""
Remove-ConfigLine '"Soap.PfBase.Models"' ""
Remove-ConfigLine '"Soap.PfBase.Messages"' ""
Remove-ConfigLine '"Soap.Utility"' ""
#* give tests project a new name
Replace-TextWithinALineOfPwshBootstrap '"Soap.UnitTests"' "`"$ServiceName.Tests`""
#* populate correct build script vars
Replace-TextWithinALineOfPwshBootstrap "-azureDevopsOrganisation `"anavarro9731`" ``" "-azureDevopsOrganisation `"$AzureDevopsOrganisationName`" ``"
Replace-TextWithinALineOfPwshBootstrap "-azureDevopsProject `"soap`" ``" "-azureDevopsProject `"$AzureDevopsName`" ``"
Replace-ALineOfPwshBootstrapThatContains "-azureDevopsPat  `"" "		-azureDevopsPat `"$RepoAndPackagingAzPersonalAccessToken`" ``"
Replace-TextWithinALineOfPwshBootstrap "-repository `"soap`" ``" "-repository `"$AzureDevopsName`" ``"
Replace-TextWithinALineOfPwshBootstrap "-azureAppName `"soap-api-sample`" ``" "-azureAppName `"$AzureDevopsName`" ``"
Replace-TextWithinALineOfPwshBootstrap "-azureResourceGroup `"rg-soap`" ``" "-azureResourceGroup `"$AzResourceGroup`" ``"
Replace-TextWithinALineOfPwshBootstrap "-azureLocation `"uksouth`" ``" "-azureLocation `"$AzLocation`" ``"
#* run it and load the modules
./pwsh-bootstrap.ps1

Log "Ignoring files"

Copy-Item "$PSScriptRoot\..\.gitignore" "$ServiceRoot\"

Log "Uploading Repo"

Set-Location $ServiceRoot

git add -A
git commit -m "initial"
git remote add origin "$AzureDevopsOrganisationUrl$AzureDevopsName/_git/$AzureDevopsName"

Run -PrepareNewVersion -forceVersion 0.1.0-alpha -Push SILENT

Log-Step "Creating Pipeline"

az pipelines create --name "$AzureDevopsName" --description "Pipeline for $AzureDevopsName" --yaml-path "./azure-pipelines.yml" -p "$AzureDevopsName" -p "$AzureDevopsName" --repository "$AzureDevopsName" --repository-type tfsgit --skip-run #* must come after files committed to repo, variables need to be added for this to work
az pipelines variable create --pipeline-name "$AzureDevopsName" --project "$AzureDevopsName" --org "$AzureDevopsOrganisationUrl" --name "ado-pat" --value "$RepoAndPackagingAzPersonalAccessToken"
az pipelines variable create --pipeline-name "$AzureDevopsName" --project "$AzureDevopsName" --org "$AzureDevopsOrganisationUrl" --name "az-tenantid" --value "$TenantId"
az pipelines variable create --pipeline-name "$AzureDevopsName" --project "$AzureDevopsName" --org "$AzureDevopsOrganisationUrl" --name "az-clientid" --value "$ClientId"
az pipelines variable create --pipeline-name "$AzureDevopsName" --project "$AzureDevopsName" --org "$AzureDevopsOrganisationUrl" --name "az-clientsecret" --value "$ClientSecret"
az pipelines variable create --pipeline-name "$AzureDevopsName" --project "$AzureDevopsName" --org "$AzureDevopsOrganisationUrl" --name "healthcheck-url" --value "$HealthCheckUrl"
az pipelines variable create --pipeline-name "$AzureDevopsName" --project "$AzureDevopsName" --org "$AzureDevopsOrganisationUrl" --name "nuget-key" --value "$RepoAndPackagingAzPersonalAccessToken"

Log-Step "Triggering Infrastructure Creation"

Run -PrepareNewVersion -forceVersion 0.2.0-alpha -Push SILENT

