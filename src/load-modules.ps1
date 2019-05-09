Param(
    [Parameter(Mandatory=$true)]
    [Alias('u')]
	[string] $adoUser,
    [Parameter(Mandatory=$true)]
    [Alias('p')]
    [string] $adoPassword
)

function Go {
	Load-Modules
}

function Load-Modules {

	$adoCredentials = @($adoUser,$adoPassword)
	$adoRootUri = "https://anavarro9731.visualstudio.com/defaultcollection/powershell/_apis/git/repositories/powershell/items?api-version=1.0&scopepath="
	$modules = @(
		"build",
		"prepare-new-version",
		"build-and-test",
		"pack-and-publish"
	)

	# Set Project Root Folder
	$global:projectRoot = $PSScriptRoot
	Push-Location $PSScriptRoot

	Write-Host "Importing Custom Modules"

	# Base64-encodes the Personal Access Token (PAT) appropriately
	$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $adoCredentials[0],$adoCredentials[1])))

	# Set Modules Path for Session
	$env:PSModulePath = $env:PSModulePath + ";$PSScriptRoot\.psModules\"

	# Download and Import all Modules
	foreach ($module in $modules) {
		New-Item ".\.psModules\$module\" -ItemType Directory -Verbose
		Invoke-RestMethod -Uri "$adoRootUri$module.psm1" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -ContentType "text/plain; charset=UTF-8" -OutFile ".\.psModules\$module\$module.psm1"
		Import-Module $module -Verbose -Global -Force
	}

	#Remove folder once modules are loaded
	Remove-Item ".\.psModules\" -Verbose -Recurse
}

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
			"Soap.Pf.MsmqEndpointBase",
            "Soap.Api.Sso\Soap.Api.Sso.Endpoint.Http",
			"Soap.Api.Sso\Soap.Api.Sso.Endpoint.Msmq"
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

Go