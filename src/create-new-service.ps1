$NewName = Read-Host -Prompt 'Enter The New Service Name (Allowed Characters A-Z,a-z and ".")'
$DiskLocation = Read-Host -Prompt 'Enter The Target Directory (e.g. c:\code)'

if (-Not ($NewName -match '^[A-Za-z\.]+$')) {
	Write-Host "$NewName does not match regex"
	Return
}

if (-Not ($DiskLocation -match '^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$')) {
	Write-Host "$DiskLocation is not a valid directory path format"
	Return
}

if (-Not (Test-Path -PathType Container $DiskLocation)) {
	#create it
	New-Item -ItemType Directory -Force -Path $DiskLocation
}
	
Write-Host "Please wait..."

$newLocation = "$DiskLocation".trim('\') + "\$NewName"

cp ./Soap.Api.Sample $newLocation -Recurse -Force 

cd $newLocation

Get-ChildItem -Filter "*Soap.Api.Sample*" -Recurse | Where {$_.FullName -notlike "*\obj\*"} | Where {$_.FullName -notlike "*\bin\*"} |  Rename-Item -NewName {$_.name -replace "Soap.Api.Sample","$NewName" }  

Get-ChildItem -Recurse -File -Include *.cs,*.csproj | ForEach-Object {
	(Get-Content $_).replace('Soap.Api.Sample',"$NewName") | Set-Content $_
}

dotnet new sln

Get-ChildItem -Recurse -File -Filter “*.csproj” | ForEach-Object { dotnet sln add $_.FullName }
