$NewName = Read-Host -Prompt 'Enter The New Service Name (Allowed Characters A-Z,a-z and ".")'
$DiskLocation = Read-Host -Prompt 'Enter The Target Directory (e.g. c:\code)'
$PackageFeedUrl = Read-Host -Prompt 'Enter The Nuget Package Feed Url (e.g. https://f.feedz.io/companyA/feed1/nuget)'
$SymbolsFeedUrl = Read-Host -Prompt 'Enter The Symbols Feed Url (e.g. https://f.feedz.io/companyA/feed1/symbols)'

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

$newLocation = "$DiskLocation".trim('\') + "\$NewName\src"

mkdir $newLocation

cp .\Soap.Api.Sample\**.* $newLocation -Recurse -Force 

cd $newLocation

Get-ChildItem -Filter "*Soap.Api.Sample*" -Recurse | Where {$_.FullName -notlike "*\obj\*"} | Where {$_.FullName -notlike "*\bin\*"} |  Rename-Item -NewName {$_.name -replace "Soap.Api.Sample","$NewName" }  

Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.ps1 | ForEach-Object {
	(Get-Content $_).replace('Soap.Api.Sample',"$NewName") | Set-Content $_
}

(Get-Content .\posh-bootstrap.ps1).replace('##packagefeedurl##', $PackageFeedUrl) | Set-Content .\posh-bootstrap.ps1
(Get-Content .\posh-bootstrap.ps1).replace('##symbolsfeedurl##', $SymbolsFeedUrl) | Set-Content .\posh-bootstrap.ps1

dotnet new sln -n $NewName

Get-ChildItem -Recurse -File -Filter “*.csproj” | ForEach-Object { dotnet sln add $_.FullName }
