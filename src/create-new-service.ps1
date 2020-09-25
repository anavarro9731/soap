Function Test-IsGitInstalled
{
	return -Not ((Get-Command git 2>$null) -eq $null)
}

Function Replace-ConfigLine([string] $old, [string] $new) 
{
	(Get-Content .\pwsh-bootstrap.ps1).replace($old, $new) | Set-Content .\pwsh-bootstrap.ps1
}

if (-Not (Test-IsGitInstalled)) {
	Write-Host "Git is not installed"
	return
}

#* VARIABLES
$AzureDevopsOrganisationName = Read-Host -Prompt 'Enter The Azure Devops Organisation Name'
$AzureDevopsOrganisationUrl = "https://dev.azure.com/$AzureDevopsOrganizationName"
$ServiceName = Read-Host -Prompt 'Enter The New Service Name (Allowed Characters A-Z,a-z and ".")'
	if (-Not ($ServiceName -match '^[A-Za-z\.]+$'))
	{
		Write-Host "$ServiceName does not match regex"
		Return
	}
$AzureName = $ServiceName.Replace(".", "-")
$AzureDevopsPersonalAccessToken = Read-Host -Prompt 'Enter An Azure Devops Personal Access Token with at least Permissions to read from the new source repo'
$env:AZURE_DEVOPS_EXT_PAT = $AzureDevopsPersonalAccessToken
$AzureResourceGroup = Read-Host -Prompt 'Enter The Azure Resource Group the new resources should be created under'
$AzureLocation = Read-Host -Prompt 'Enter The Azure Location (e.g. uksouth) where the new resources should be created'
$DiskLocation = Read-Host -Prompt 'Enter The Target Directory (e.g. c:\code)'
	if (-Not ($DiskLocation -match '^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$')) {
		Write-Host "$DiskLocation is not a valid directory path format"
		Return
	}
	if (-Not (Test-Path -PathType Container $DiskLocation)) {
		#create it
		New-Item -ItemType Directory -Force -Path $DiskLocation
	}
	$DiskLocation = $DiskLocation.trim('\')

Write-Host "Please wait..."

# Create and copy files
$newLocation = "$DiskLocation".trim('\') + "\$ServiceName\src"
if (Test-Path $newLocation) {
	Remove-Item -Recurse -Force $newLocation
}
mkdir $newLocation
cp .\Soap.Api.Sample\**.* $newLocation -Recurse -Force
cd $newLocation

Get-ChildItem -Filter "*Soap.Api.Sample*" -Recurse | Where {$_.FullName -notlike "*\obj\*"} | Where {$_.FullName -notlike "*\bin\*"} |  Rename-Item -NewName {$_.name -replace "Soap.Api.Sample","$ServiceName" }
Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.ps1 | ForEach-Object {
	(Get-Content $_).replace('Soap.Api.Sample',"$ServiceName") | Set-Content $_
}

$removals = ls -r . -filter *.cs | select-string "##REMOVE-IN-COPY##" | select path
$removals | % { Remove-Item $_.Path }

# Create new solution
dotnet new sln -n $ServiceName
Get-ChildItem -Recurse -File -Filter "*.csproj" | ForEach-Object { dotnet sln add $_.FullName }
cd ..

# Set variables in pwsh-bootstrap
Replace-ConfigLine '"Soap.Auth0,"' ""
Replace-ConfigLine '"Soap.Bus,"' ""
Replace-ConfigLine '"Soap.Config,"' ""
Replace-ConfigLine '"Soap.Context,"' ""
Replace-ConfigLine '"Soap.Interfaces,"' ""
Replace-ConfigLine '"Soap.Interfaces.Messages,"' ""
Replace-ConfigLine '"Soap.MessagePipeline,"' ""
Replace-ConfigLine '"Soap.NotificationServer,"' ""
Replace-ConfigLine '"Soap.PfBase.Api,"' ""
Replace-ConfigLine '"Soap.PfBase.Logic,"' ""
Replace-ConfigLine '"Soap.PfBase.Tests,"' ""
Replace-ConfigLine '"Soap.PfBase.Models,"' ""
Replace-ConfigLine '"Soap.PfBase.Messages,"' ""
Replace-ConfigLine '"Soap.Utility,"' ""
Replace-ConfigLine '"Soap.UnitTests"' "$ServiceName.Tests"
Replace-ConfigLine '-azureDevopsOrganisation "anavarro9731" ``' "-azureDevopsOrganisation `"$AzureDevopsOrganisationName`" ``"
Replace-ConfigLine '-azureDevopsProject `"soap`" ``' "-azureDevopsProject `"$AzureName`" ``"
Replace-ConfigLine '-azureDevopsPat  "j35ssqoabmwviu7du4yin6lmw3l2nc4okz37tcdmpirl3ftgyiia" ``' "-azureDevopsPat `"$AzureDevopsPersonalAccessToken`" ``"
Replace-ConfigLine '-repository "soap" ``' "-repository `"$AzureName`" ``"
Replace-ConfigLine '-azureAppName "soap-api-sample" ``' "-azureAppName `"$AzureName`" ``"
Replace-ConfigLine '-azureResourceGroup "rg-soap" ``' "-azureResourceGroup `"$AzureResourceGroup`" ``"
Replace-ConfigLine '-azureLocation "uksouth" ``' "-azureLocation `"$AzureLocation`" ``"
Replace-ConfigLine '-nugetFeedUri "https://pkgs.dev.azure.com/anavarro9731/soap/_packaging/soap/nuget/v3/index.json" ``' "https://pkgs.dev.azure.com/$AzureDevopsOrganisationName/$AzureDevopsProjectName/_packaging/$AzureName/nuget/v3/index.json`" ``"

#Create config repo
az repos create  --organization AzureDevopsOrganisationUrl --project $AzureName --name "$AzureName.config"
$configRepoRoot = "$DiskLocation\$ServiceName.config"
cd $newLocation
dotnet new classlib -f netcoreapp3.1 -n Config
cd Config
del Class1.cs
mkdir DEV
cd DEV
cp "$DiskLocation\Soap\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" -Recurse -Force
cd $configRepoRoot
git init
git add -A
git commit -m "initial"
git remote add origin "https://dev.azure.com/$AzureDevopsOrganisationName/$AzureName/_git/$AzureName.config"
git push -u origin --all

# Create azure project, pipeline and repo
az devops project create  --organization AzureDevopsOrganisationUrl  --name $AzureName
$mainRepoRoot = "$DiskLocation\$ServiceName"
cd $mainRepoRoot
git init
git add -A
git commit -m "initial"
git remote add origin "https://dev.azure.com/$AzureDevopsOrganisationName/$AzureName/_git/$AzureName"
git push -u origin --all
az pipelines create --name '$AzureName' --description 'Pipeline for $AzureName' --yaml-path "./azure-pipelines.yml"
./pwsh-bootstrap.ps1
Run -PrepareNewVersion -forceVersion 0.1.0

