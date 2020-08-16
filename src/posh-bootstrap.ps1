Invoke-RestMethod -Uri "https://anavarro9731.visualstudio.com/defaultcollection/powershell/_apis/git/repositories/powershell/items?api-version=1.0&scopepath=load-modules.psm1" -ContentType "text/plain; charset=UTF-8" -OutFile ".\load-modules.psm1"
Import-Module ".\load-modules.psm1" -Verbose -Global -Force
Remove-Item ".\load-modules.psm1" -Verbose -Recurse
Load-Modules

#expose this function like a CmdLet
function global:Run {

	Param(
		[switch]$PrepareNewVersion,
		[switch]$BuildAndTest,
		[switch]$PackAndPublish,
		[Alias('nuget-key')]
		[string] $nugetApiKey,
		[Alias('ado-pat')]
		[string] $azureDevopsPat,
		[string] $forceVersion
	)

	if ($PrepareNewVersion) {
		Prepare-NewVersion -projects @(
		"Soap.Interfaces.Config",
		"Soap.Interfaces.Config",
		"Soap.Interfaces.Config",
		"Soap.Interfaces.Config",
		"Soap.Interfaces.Config",
		"Soap.Interfaces.Config"
		) `
        -azureDevopsProject "soap" `
        -azureDevopsOrganisation "anavarro9731" `
        -azureDevopsPat $azureDevopsPat `
        -repository "soap" `
        -forceVersion $forceVersion
	}

	if ($BuildAndTest) {
		Build-And-Test -testProjects @(
		)
	}

	if ($PackAndPublish) {
		Pack-And-Publish -allProjects @(
		"Soap.Interfaces.Config"
		) -unlistedProjects @(
		) `
        -nugetApiKey $nugetApiKey
	}
}
