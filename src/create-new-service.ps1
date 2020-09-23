Function Test-IsGitInstalled
{
    $32BitPrograms = Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*
    $64BitPrograms = Get-ItemProperty     HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*
    $programsWithGitInName = ($32BitPrograms + $64BitPrograms) | Where-Object { $null -ne $_.DisplayName -and $_.Displayname.Contains('Git') }
    $isGitInstalled = $null -ne $programsWithGitInName
    return $isGitInstalled
}

$NewName = Read-Host -Prompt 'Enter The New Service Name (Allowed Characters A-Z,a-z and ".")'
$DiskLocation = Read-Host -Prompt 'Enter The Target Directory (e.g. c:\code)'
$PackageFeedUrl = Read-Host -Prompt 'Enter The Nuget Package Feed Url (e.g. https://f.feedz.io/companyA/feed1/nuget)'
$SymbolsFeedUrl = Read-Host -Prompt 'Enter The Symbols Feed Url (e.g. https://f.feedz.io/companyA/feed1/symbols)'

if (-Not (Test-IsGitInstalled)) {
	Write-Host "Git is not installed"
}

if (-Not ($NewName -match '^[A-Za-z\.]+$')) {
	Write-Host "$NewName does not match regex"
	Return
}

if (-Not ($DiskLocation -match '^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$')) {
	Write-Host "$DiskLocation is not a valid directory path format"
	Return
}

if (-Not ($PackageFeedUrl -match 'http|https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)')) {
	Write-Host "$PackageFeedUrl  is not a valid url path format"
	Return
}

if (-Not ($SymbolsFeedUrl -match 'http|https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)')) {
	Write-Host "$SymbolsFeedUrl is not a valid url path format"
	Return
}
if (-Not (Test-Path -PathType Container $DiskLocation)) {
	#create it
	New-Item -ItemType Directory -Force -Path $DiskLocation
}
Write-Host "Please wait..."

# Create and copy files
$newLocation = "$DiskLocation".trim('\') + "\$NewName\src"
if (Test-Path $newLocation) {
	Remove-Item -Recurse -Force $newLocation
}
mkdir $newLocation
cp .\Soap.Api.Sample\**.* $newLocation -Recurse -Force
cd $newLocation
Get-ChildItem -Filter "*Soap.Api.Sample*" -Recurse | Where {$_.FullName -notlike "*\obj\*"} | Where {$_.FullName -notlike "*\bin\*"} |  Rename-Item -NewName {$_.name -replace "Soap.Api.Sample","$NewName" }
Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.ps1 | ForEach-Object {
	(Get-Content $_).replace('Soap.Api.Sample',"$NewName") | Set-Content $_
}

$removals = ls -r . -filter *.cs | select-string "##REMOVE-IN-COPY##" | select path
$removals | % { Remove-Item $_.Path }

# Set variables in pwsh-bootstrap
(Get-Content .\posh-bootstrap.ps1).replace('##packagefeedurl##', $PackageFeedUrl) | Set-Content .\posh-bootstrap.ps1
(Get-Content .\posh-bootstrap.ps1).replace('##symbolsfeedurl##', $SymbolsFeedUrl) | Set-Content .\posh-bootstrap.ps1

# Create new solution
dotnet new sln -n $NewName
Get-ChildItem -Recurse -File -Filter "*.csproj" | ForEach-Object { dotnet sln add $_.FullName }
cd ..

# Create azure project
az devops project create --organization https://dev.azure.com/anavarro9731 --name test1

# Create new repo
git init
git add -A
git commit -m "initial"
git remote add origin https://anavarro9731@dev.azure.com/anavarro9731/test1/_git/test1
git push -u origin --all

# Config pipeline
az pipelines create --name 'test1' --description 'Pipeline for test1' --yaml-path "./azure-pipelines.yml"
