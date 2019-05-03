$NewName = Read-Host -Prompt 'Enter The New Service Name Regex[A-Za-zz\.]'

if ($NewName -match '^[A-Za-z\.]+$') {
	
	Write-Host "Please wait..."

	cp ./Soap.Api.Sample "../../$NewName" -Recurse -Force 

	cd "../../$NewName"

	Get-ChildItem -Filter “*Soap.Api.Sample*” -Recurse | Where {$_.FullName -notlike "*\obj\*"} | Where {$_.FullName -notlike "*\bin\*"} |  Rename-Item -NewName {$_.name -replace ‘Soap.Api.Sample’,’Kurve.Api.First’ }  

	Get-ChildItem -Recurse -File -Include *.cs,*.csproj | ForEach-Object {
		(Get-Content $_).replace('Soap.Api.Sample',"$NewName") | Set-Content $_
	}
}
else {
	Write-Host "$NewName does not match regex"
}