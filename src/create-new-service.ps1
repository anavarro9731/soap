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

if (-Not (Test-IsGitInstalled)) {
	Write-Host "Git is not installed"
	return
}

#* VARIABLES
$AzureDevopsOrganisationName = Read-Host -Prompt 'Enter The Azure Devops Organisation Name'
$AzureDevopsOrganisationUrl = "https://dev.azure.com/$AzureDevopsOrganisationName/"
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

#* Create source folders and copy files
$serviceRoot = "$DiskLocation\$ServiceName"
if (Test-Path $serviceRoot) {
	Remove-Item -Recurse -Force $serviceRoot
}
mkdir $serviceRoot
cp .\Soap.Api.Sample\**.* "$serviceRoot" -Recurse -Force
cp .\pwsh-bootstrap.ps1 "$serviceRoot" -Force
cp ..\azure-pipelines.yml "$serviceRoot" -Force
(Get-Content $serviceRoot\azure-pipelines.yml) | % { $_.replace('src\', '') } | Set-Content $serviceRoot\azure-pipelines.yml
(Get-Content $serviceRoot\azure-pipelines.yml) | % { $_.replace('src/', '') } | Set-Content $serviceRoot\azure-pipelines.yml

$configRepoRoot = "$DiskLocation\$ServiceName.config"
if (Test-Path $configRepoRoot) {
	Remove-Item -Recurse -Force $configRepoRoot
}
mkdir $configRepoRoot

cp ".\Soap.Api.Sample\Soap.Api.Sample.Afs\SampleConfig.cs" "$configRepoRoot\DEV_Config.cs" -Recurse -Force

#* Setup config repo

cd $configRepoRoot
git init

dotnet new classlib -f "netcoreapp3.1" -n Config
mv DEV_Config.cs Config
cd Config
del Class1.cs

$soapFeedUri = "https://pkgs.dev.azure.com/anavarro9731/soap-feed/_packaging/soap-pkgs/nuget/v3/index.json"
dotnet nuget add source $soapFeedUri
dotnet add package Soap.Config -s $soapFeedUri 

cd $configRepoRoot
az devops project create --organization $AzureDevopsOrganisationUrl --name $AzureName
az repos create  --organization $AzureDevopsOrganisationUrl --project $AzureName --name "$AzureName.config"
git add -A
git commit -m "initial"
git remote add origin "https://dev.azure.com/$AzureDevopsOrganisationName/$AzureName/_git/$AzureName.config"
git push -u origin --all



#* Setup service repo

cd $serviceRoot

Replace-ConfigLine '"Soap.Api.Sample\Soap.Api.Sample.Afs"' "`"$ServiceName.Afs`""

Get-ChildItem -Filter "*Soap.Api.Sample*" -Recurse | Where {$_.FullName -notlike "*\obj\*"} | Where {$_.FullName -notlike "*\bin\*"} |  Rename-Item -NewName {$_.name -replace "Soap.Api.Sample","$ServiceName" }
Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.ps1 | ForEach-Object {
	(Get-Content $_).replace('Soap.Api.Sample',"$ServiceName") | Set-Content $_
}

$removals = ls -r . -filter *.cs | select-string "##REMOVE-IN-COPY##" | select path
$removals | % { Remove-Item $_.Path }

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

##* Set variables in pwsh-bootstrap

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
Replace-ConfigLine '"Soap.UnitTests"' "`"$ServiceName.Tests`""
Replace-ConfigLine "-azureDevopsOrganisation `"anavarro9731`" ``" "-azureDevopsOrganisation `"$AzureDevopsOrganisationName`" ``"
Replace-ConfigLine "-azureDevopsProject `"soap`" ``" "-azureDevopsProject `"$AzureName`" ``"
Replace-ConfigLine "-azureDevopsPat  `"j35ssqoabmwviu7du4yin6lmw3l2nc4okz37tcdmpirl3ftgyiia`" ``" "-azureDevopsPat `"$AzureDevopsPersonalAccessToken`" ``"
Replace-ConfigLine "-repository `"soap`" ``" "-repository `"$AzureName`" ``"
Replace-ConfigLine "-azureAppName `"soap-api-sample`" ``" "-azureAppName `"$AzureName`" ``"
Replace-ConfigLine "-azureResourceGroup `"rg-soap`" ``" "-azureResourceGroup `"$AzureResourceGroup`" ``"
Replace-ConfigLine "-azureLocation `"uksouth`" ``" "-azureLocation `"$AzureLocation`" ``"
Remove-ConfigLine "-nugetFeedUri `"https://pkgs.dev.azure.com/anavarro9731/soap-feed/_packaging/soap-pkgs/nuget/v3/index.json`" ``"
Remove-ConfigLine '-nugetApiKey $nugetApiKey'
./pwsh-bootstrap.ps1 
git add -A
git commit -m "initial"
git remote add origin "https://dev.azure.com/$AzureDevopsOrganisationName/$AzureName/_git/$AzureName"
git push -u origin --all

az pipelines create --name "$AzureName" --description "Pipeline for $AzureName" --yaml-path "./azure-pipelines.yml" #* must come after files committed to repo

Run -PrepareNewVersion -forceVersion 0.1.0

