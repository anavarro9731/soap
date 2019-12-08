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
        	[Alias('k')]
        	[string] $nugetApiKey
	)
	
	if ($PrepareNewVersion) {
		Prepare-NewVersion -projects @(
			"Soap.Api.Sample.Domain.Messages",
			"Soap.Api.Sample.Domain.Constants",
			"Soap.Api.Sample.Domain.Logic",
			"Soap.Api.Sample.Domain.Models",
			"Soap.Api.Sample.Domain.Tests",
            		"Soap.Api.Sample.Endpoint.Http",
			"Soap.Api.Sample.Endpoint.Msmq",
			"Soap.Api.Sample.Endpoint.Tests"
			)
	}

	if ($BuildAndTest) {
		Build-And-Test -testProjects @(
			"Soap.Api.Sample.Domain.Tests"
			)
	}

    if ($PackAndPublish) {
        Pack-And-Publish -standardProjects @(
			"Soap.Api.Sample.Domain.Messages",
			"Soap.Api.Sample.Domain.Constants",
			"Soap.Api.Sample.Endpoint.Tests"
			) `
        -unlistedProjects @(
			"Soap.Api.Sample.Domain.Logic",
			"Soap.Api.Sample.Domain.Models",
			""
			) `
        -octopusProjects @(
            		"Soap.Api.Sample.Endpoint.Http",
			"Soap.Api.Sample.Endpoint.Msmq") `
        -nugetFeedUri "##packagefeedurl##" `
        -nugetSymbolFeedUri "##symbolsfeedurl##" `
        -nugetApiKey $nugetApiKey
    }
}

