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
		"Soap.Config",
		"Soap.PfBase.Api",
		"Soap.PfBase.Logic",
		"Soap.PfBase.Tests",
		"Soap.PfBase.Models",
		"Soap.PfBase.Messages"
		) `
        -azureDevopsProject "soap.config" `
        -azureDevopsOrganisation "anavarro9731" `
        -azureDevopsPat  "y6gg7funryd4ffv32s4fugxzqgjpeqz5gl4xi2dftdf7mcb5pkia"`
        -repository "soap.config" `
        -forceVersion $forceVersion
	}

	if ($BuildAndTest) {
		Build-And-Test -testProjects @(
		"Soap.PfBase.Tests"
		)
	}

	if ($PackAndPublish) {
		Pack-And-Publish -allProjects @(
		"Soap.Config"
		) -unlistedProjects @(
		) `
        -nugetApiKey $nugetApiKey
	}
}
