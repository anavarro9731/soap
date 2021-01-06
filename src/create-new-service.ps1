Param(
	[string] $Arg_AzureDevopsOrganisationName,
	[string] $Arg_ServiceName,
	[string] $Arg_AzPersonalAccessToken,
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
Function Replace-ConfigLine([string] $old, [string] $new)
{
	(Get-Content .\pwsh-bootstrap.ps1) | % { $_.replace($old, $new) } | Set-Content .\pwsh-bootstrap.ps1
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
Function Get-AzureDevopsOrganisationName([string] $s = $null) {
	$OrgName = $s ?? (Read-Host -Prompt 'Enter The Azure Devops Organisation Name')
	if ([String]::IsNullOrWhiteSpace($OrgName)) {
		Write-Host 'Azure Devops Organisation Name cannot be blank'
		Exit -1
	}
	Return $OrgName
}
Function Get-ServiceName([string] $s = $null)  {
	$ServiceName = $s ?? (Read-Host -Prompt 'Enter The New Service Name (Allowed Characters A-Z,a-z and ".")')
	if (-Not ($ServiceName -match '^[A-Za-z\.]+$'))
	{
		Write-Host "Service Name `"$ServiceName`" does not match regex"
		Exit -1
	}
	Return $ServiceName
}
Function Get-AzPersonalAccessToken([string] $s = $null)  {
	$PAT = $s ?? (Read-Host -Prompt 'Enter An Azure Devops Personal Access Token with Admin permissions')
	if ([String]::IsNullOrWhiteSpace($PAT)) {
		Write-Host 'Personal Access Token cannot be blank'
		Exit -1
	}
	Return $PAT
}
Function Get-AzResourceGroup([string] $s = $null)  {
	$ResourceGroup = $s ?? (Read-Host -Prompt 'Enter The Azure Resource Group the new resources should be created under')
	if ([String]::IsNullOrWhiteSpace($ResourceGroup)) {
		Write-Host 'Resource Group cannot be blank'
		Exit -1
	}
	Return $ResourceGroup
}
Function Get-AzLocation([string] $s = $null)  {
	$Location = $s ?? (Read-Host -Prompt 'Enter The Azure Location (e.g. uksouth) where the new resources should be created')
	if ([String]::IsNullOrWhiteSpace($Location)) {
		Write-Host 'Azure Location cannot be blank'
		Exit -1
	}
	Return $Location
}
Function Get-TenantId([string] $s = $null)  {
	$TenantId = $s ?? (Read-Host -Prompt 'Enter The TenantId of the ServicePrincipal needed to create the infratructure')
	if ([String]::IsNullOrWhiteSpace($TenantId)) {
		Write-Host 'ServicePrincipal TenantId cannot be blank'
		Exit -1
	}
	Return $TenantId
}
Function Get-ClientId([string] $s = $null)  {
	$ClientId = $s ?? (Read-Host -Prompt 'Enter The Azure ClientId of the ServicePrincipal needed to create the infratructure')
	if ([String]::IsNullOrWhiteSpace($ClientId)) {
		Write-Host 'ServicePrincipal ClientId cannot be blank'
		Exit -1
	}
	Return $ClientId
}
Function Get-ClientSecret([string] $s = $null)  {
	$ClientSecret = $s ?? (Read-Host -Prompt 'Enter The Azure ClientSecret of the ServicePrincipal needed to create the infratructure')
	if ([String]::IsNullOrWhiteSpace($ClientSecret)) {
		Write-Host 'ServicePrincipal ClientSecret cannot be blank'
		Exit -1
	}
	Return $ClientSecret
}
Function Get-PathOnDisk([string] $s = $null)  {
	$DiskLocation = $s ?? (Read-Host -Prompt 'Enter The Target Directory (e.g. c:\code)')
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

Function Get-HealthCheckUrl([string] $AzureName) {
	
	return "https://$AzureName-##ENVSUFFIX##.azurewebsites.net/api/CheckHealth"
}

Test-PreReqs

Log-Step "Acquiring Variables..."

$AzureDevopsOrganisationName = Get-AzureDevopsOrganisationName $Arg_AzureDevopsOrganisationName
$AzureDevopsOrganisationUrl = "https://dev.azure.com/$AzureDevopsOrganisationName/"
$ServiceName = Get-ServiceName $Arg_ServiceName
$AzureName = $ServiceName.Replace(".", "-")
$AzPersonalAccessToken = Get-AzPersonalAccessToken $Arg_AzPersonalAccessToken
$env:AZURE_DEVOPS_EXT_PAT = $AzPersonalAccessToken
$AzResourceGroup = Get-AzResourceGroup $Arg_AzResourceGroup
$AzLocation = Get-AzLocation $Arg_AzLocation
$PathOnDisk = Get-PathOnDisk $Arg_PathOnDisk
$ServiceRoot = Get-ServiceRoot $PathOnDisk $ServiceName
$ConfigRepoRoot = "$PathOnDisk\$ServiceName.config"
$SoapFeedUri = "https://pkgs.dev.azure.com/anavarro9731/soap-feed/_packaging/soap-pkgs/nuget/v3/index.json"
$TenantId = Get-TenantId $Arg_TenantId
$ClientId = Get-ClientId $Arg_ClientId
$ClientSecret = Get-ClientSecret $Arg_ClientSecret
$HealthCheckUrl = Get-HealthCheckUrl $AzureName

$vars = "AzureDevopsOrganisationName:$AzureDevopsOrganisationName`r`n"+
"AzureDevopsOrganisationUrl:$AzureDevopsOrganisationUrl`r`n"+ 
"ServiceName:$ServiceName`r`n"+  
"AzureName:$AzureName`r`n"+ 
"AzPersonalAccessToken:$AzPersonalAccessToken`r`n"+ 
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
Copy-Item ".\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" "$ConfigRepoRoot\DEV_Config.cs" -Recurse -Force
Copy-Item ".\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" "$ConfigRepoRoot\VNEXT_Config.cs" -Recurse -Force
Copy-Item ".\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" "$ConfigRepoRoot\REL_Config.cs" -Recurse -Force
Copy-Item ".\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" "$ConfigRepoRoot\LIVE_Config.cs" -Recurse -Force
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
az devops project create --organization $AzureDevopsOrganisationUrl --name $AzureName
az repos create  --organization $AzureDevopsOrganisationUrl --project $AzureName --name "$AzureName.config"

Log "Uploading Config Repo"

git add -A
git commit -m "initial"
git remote add origin "https://dev.azure.com/$AzureDevopsOrganisationName/$AzureName/_git/$AzureName.config"
git push -u origin --all

Log-Step "Creating Service Repo, Please wait..."

Set-Location $PSScriptRoot
CreateOrClean-Directory $ServiceRoot

Log "Creating Client App"

Set-Location $PSScriptRoot
$Excluded = @('.cache','dist','node_modules','srv.ps1','story.md','yarn**.*')
$sourceAppDir = "$PSScriptRoot\soapjs\app"
$folders = Get-ChildItem -Path $sourceAppDir | Where {($_.PSIsContainer) -and ($Excluded -notcontains $_.Name)}
foreach ($f in $folders){
	Copy-Item -Path $sourceAppDir\$($f.Name) -Destination $ServiceRoot\app\$($f.Name) -Recurse -Force
}
Copy-Item "$sourceAppDir\*.*" "$ServiceRoot\app\" -Exclude $Excluded
Set-Content -Path "$ServiceRoot\app\.env" -Value "##RUN configure-local-environment SCRIPT TO POPULATE##"

Log "Creating Server App"

Set-Location $PSScriptRoot
$Excluded = @('bin', 'obj', 'published')
Copy-Item .\Soap.Api.Sample\**.* "$ServiceRoot" -Recurse -Force
Copy-Item .\pwsh-bootstrap.ps1 "$ServiceRoot" -Force
Copy-Item .\configure-local-environment.ps1 "$ServiceRoot" -Force
Copy-Item ..\azure-pipelines.yml "$ServiceRoot" -Force
#* cull src directory from paths
(Get-Content $ServiceRoot\azure-pipelines.yml) | % { $_.replace('src\', '') } | Set-Content $ServiceRoot\azure-pipelines.yml
(Get-Content $ServiceRoot\azure-pipelines.yml) | % { $_.replace('src/', '') } | Set-Content $ServiceRoot\azure-pipelines.yml
Set-Location $ServiceRoot
Replace-ConfigLine '"Soap.Api.Sample\Soap.Api.Sample.Afs"' "`"$ServiceName.Afs`""
Get-ChildItem -Filter "*Soap.Api.Sample*" -Recurse | Where {$_.FullName -notlike "*\obj\*"} | Where {$_.FullName -notlike "*\bin\*"} |  Rename-Item -NewName {$_.name -replace "Soap.Api.Sample","$ServiceName" }
Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.ps1 | ForEach-Object {
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
Remove-ConfigLine '"Soap.Auth0"' ""
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
Replace-ConfigLine '"Soap.UnitTests"' "`"$ServiceName.Tests`""
#* populate correct build script vars
Replace-ConfigLine "-azureDevopsOrganisation `"anavarro9731`" ``" "-azureDevopsOrganisation `"$AzureDevopsOrganisationName`" ``"
Replace-ConfigLine "-azureDevopsProject `"soap`" ``" "-azureDevopsProject `"$AzureName`" ``"
Replace-ConfigLine "-azureDevopsPat  `"j35ssqoabmwviu7du4yin6lmw3l2nc4okz37tcdmpirl3ftgyiia`" ``" "-azureDevopsPat `"$AzPersonalAccessToken`" ``"
Replace-ConfigLine "-repository `"soap`" ``" "-repository `"$AzureName`" ``"
Replace-ConfigLine "-azureAppName `"soap-api-sample`" ``" "-azureAppName `"$AzureName`" ``"
Replace-ConfigLine "-azureResourceGroup `"rg-soap`" ``" "-azureResourceGroup `"$AzResourceGroup`" ``"
Replace-ConfigLine "-azureLocation `"uksouth`" ``" "-azureLocation `"$AzLocation`" ``"
#* remove nuget props used only for libraries (and which require a nuget feed which the az cli can't create)
Remove-ConfigLine "-nugetFeedUri `"https://pkgs.dev.azure.com/anavarro9731/soap-feed/_packaging/soap-pkgs/nuget/v3/index.json`" ``"
Remove-ConfigLine '-nugetApiKey $nugetApiKey'
#* run it and load the modules
./pwsh-bootstrap.ps1 

Log "Uploading Repo"

git add -A
git commit -m "initial"
git remote add origin "$AzureDevopsOrganisationUrl$AzureName/_git/$AzureName"
Run -PrepareNewVersion -forceVersion 0.1.0-alpha -Push SILENT

Log-Step "Creating Pipeline"

az pipelines create --name "$AzureName" --description "Pipeline for $AzureName" --yaml-path "./azure-pipelines.yml" --skip-run #* must come after files committed to repo, variables need to be added for this to work
az pipelines variable create --pipeline-name "$AzureName" --project "$AzureName" --org "$AzureDevopsOrganisationUrl" --name "ado-pat" --value "$AzPersonalAccessToken"
az pipelines variable create --pipeline-name "$AzureName" --project "$AzureName" --org "$AzureDevopsOrganisationUrl" --name "az-tenantid" --value "$TenantId"
az pipelines variable create --pipeline-name "$AzureName" --project "$AzureName" --org "$AzureDevopsOrganisationUrl" --name "az-clientid" --value "$ClientId"
az pipelines variable create --pipeline-name "$AzureName" --project "$AzureName" --org "$AzureDevopsOrganisationUrl" --name "az-clientsecret" --value "$ClientSecret"
az pipelines variable create --pipeline-name "$AzureName" --project "$AzureName" --org "$AzureDevopsOrganisationUrl" --name "healthcheck-url" --value "$HealthCheckUrl"

Log-Step "Triggering Infrastructure Creation"
 
Run -PrepareNewVersion -forceVersion 0.2.0-alpha -Push SILENT

