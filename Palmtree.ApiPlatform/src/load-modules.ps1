
$vstsCredentials = @("anavarro9731","VST`"04`"supremus")
$moduleRootUri = "https://anavarro9731.visualstudio.com/defaultcollection/powershell/_apis/git/repositories/powershell/items?api-version=1.0&scopepath="
$modules = @(
 "build.psm1",
 "prepare-new-version.psm1",
 "build-and-test.psm1"
)

Push-Location $PSScriptRoot

Write-Host "Importing Custom Modules"

# Base64-encodes the Personal Access Token (PAT) appropriately
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $vstsCredentials[0],$vstsCredentials[1])))

foreach ($module in $modules) {
 Invoke-RestMethod -Uri "$moduleRootUri$module" -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -ContentType "text/plain; charset=UTF-8" -OutFile $module
 Import-Module ".\$module" -Verbose -Global -Force
 #make sure to .gitgnore .psm1 files as we don't remove this because it is needed by 'Using' statements in other scripts which may also commit changes
}

function global:Run {

	Param(
		[switch]$PrepareNewVersion,
		[switch]$BuildAndTest,
		[switch]$PackAndPublish
	)
	
	if ($PrepareNewVersion) {
		Prepare-NewVersion -projects @(
			"Palmtree.ApiPlatform.ThirdPartyClients.Mailgun",
			"Palmtree.ApiPlatform.Interfaces",
			"Palmtree.ApiPlatform.MessagePipeline",
			"Palmtree.ApiPlatform.Utility",
			"Palmtree.ApiPlatform.DomainTests.Infrastructure",
			"Palmtree.ApiPlatform.Endpoint.Clients",
			"Palmtree.ApiPlatform.Endpoint.Http.Infrastructure",
			"Palmtree.ApiPlatform.Endpoint.Infrastructure",
			"Palmtree.ApiPlatform.Endpoint.Msmq.Infrastructure",
			"Palmtree.ApiPlatform.EndpointTests.Infrastructure",
			"Palmtree.ApiPlatform.MessagesSharedWithClients")
	}

	if ($BuildAndTest) {
		Build-And-Test -testPackages @(
			"Palmtree.ApiPlatform.Tests",
			"Palmtree.Sso.Api.Domain.Tests"
		)
	}

    if ($PackAndPublish) {
        Pack-And-Publish -projectsToPublish @(
			"Palmtree.ApiPlatform.ThirdPartyClients.Mailgun",
			"Palmtree.ApiPlatform.Interfaces",
			"Palmtree.ApiPlatform.MessagePipeline",
			"Palmtree.ApiPlatform.Utility",
			"Palmtree.ApiPlatform.DomainTests.Infrastructure",
			"Palmtree.ApiPlatform.Endpoint.Clients",
			"Palmtree.ApiPlatform.Endpoint.Http.Infrastructure",
			"Palmtree.ApiPlatform.Endpoint.Infrastructure",
			"Palmtree.ApiPlatform.Endpoint.Msmq.Infrastructure",
			"Palmtree.ApiPlatform.EndpointTests.Infrastructure",
			"Palmtree.ApiPlatform.MessagesSharedWithClients"	         	    
		) -unlistedProjects @(
			"Palmtree.ApiPlatform.Interfaces",
			"Palmtree.ApiPlatform.MessagePipeline",
			"Palmtree.ApiPlatform.Utility"
		) -mygetFeedUri = "https://www.myget.org/F/anavarro9731/api/v2/package" -mygetSymbolFeedUri = "https://www.myget.org/F/anavarro9731/symbols/api/v2/package" -mygetApiKey = "7cde1967-fe13-4672-91ef-f1deb3543e78"
    }
}