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
			"Soap.Integrations.Mailgun",
			"Soap.If.Interfaces",
			"Soap.If.MessagePipeline",
			"Soap.If.Utility",
			"Soap.Pf.ClientServerMessaging",
			"Soap.Pf.DomainTestsBase",
			"Soap.Pf.EndpointClients",
			"Soap.Pf.EndpointInfrastructure",
			"Soap.Pf.EndpointTestsBase",
			"Soap.Pf.HttpEndpointBase",
			"Soap.Pf.MessageContractsBase",
			"Soap.Pf.DomainLogicBase",
			"Soap.Pf.DomainModelsBase",
			"Soap.Pf.MsmqEndpointBase",
			"Soap.Api.Sso\Soap.Api.Sso.Endpoint.Http",
			"Soap.Api.Sso\Soap.Api.Sso.Endpoint.Msmq"
			)
	}

	if ($BuildAndTest) {
		Build-And-Test -testProjects @(
			"Soap.Tests"
			)
	}

    if ($PackAndPublish) {
        Pack-And-Publish -standardProjects @(
			"Soap.Integrations.Mailgun",
			"Soap.If.Interfaces",
			"Soap.If.MessagePipeline",
			"Soap.If.Utility",
			"Soap.Pf.ClientServerMessaging",
			"Soap.Pf.DomainTestsBase",
			"Soap.Pf.EndpointClients",
			"Soap.Pf.EndpointInfrastructure",
			"Soap.Pf.EndpointTestsBase",
			"Soap.Pf.HttpEndpointBase",
			"Soap.Pf.MessageContractsBase",
			"Soap.Pf.DomainLogicBase",
			"Soap.Pf.DomainModelsBase",
			"Soap.Pf.MsmqEndpointBase"
			) `
        -unlistedProjects @(
            "Soap.If.Interfaces",
			"Soap.If.MessagePipeline",
			"Soap.If.Utility") `
        -octopusProjects @(
            "Soap.Api.Sso\Soap.Api.Sso.Endpoint.Http",
			"Soap.Api.Sso\Soap.Api.Sso.Endpoint.Msmq") `
        -nugetFeedUri "https://www.myget.org/F/anavarro9731/api/v2/package" `
        -nugetSymbolFeedUri "https://www.myget.org/F/anavarro9731/symbols/api/v2/package" `
        -nugetApiKey $nugetApiKey 
    }
}
