# developers must maintaint the run function as projects are added to the solution

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
		[string] $nugetApiKey,
		[string] $azureDevopsPat,
		[string] $azClientId,
		[string] $azClientSecret,
		[string] $azTenantId,
		[string] $forceVersion
	)


	
	# an array of the relative paths on disk without the trailing slash to this file of all class library projects which will be published as nuget packages / can be an empty array
	# Note .csproj and foldername must match for the folder containing the project
	$libraryProjects = @(
	"Soap.Auth0",
	"Soap.Bus",
	"Soap.Config",
	"Soap.Context",
	"Soap.Interfaces",
	"Soap.Interfaces.Messages",
	"Soap.MessagePipeline",
	"Soap.NotificationServer",
	"Soap.PfBase.Api",
	"Soap.PfBase.Logic",
	"Soap.PfBase.Tests",
	"Soap.PfBase.Models",
	"Soap.PfBase.Messages",
	"Soap.Utility"
	)

	# the relative path on disk without the trailing slash to this file of all xUnit test projects / can be an empty array
	# Note .csproj and foldername must match for the folder containing the project
	$testProjects = @(
	"Soap.UnitTests" `
	)
	
	# the relative path on disk without the trailing slash to ths file of the single azure function app project to be published / optional can be $null or empty string
	# Note .csproj and foldername must match for the folder containing the project
	$azureFunctionProject = "Soap.Api.Sample\Soap.Api.Sample.Afs"
	
	

	# options
	# -allProjects (provided by $libraryProjects and $azureFunctionProject, do not modify, can be empty)
	# -azureDevopsOrganisation this is the name of the Organisation in azure devops which contains the repo for this solution / optional depending if using azure devops 
	# -azureDevopsProject this is the name of the Project in azure devops which contains the repo for this solution / optional depending if using azure devops
	# -azureDevopsPat could be provided the paramater but usually hardcoded here since this is run locally 
	# -repository this is the name of the repository in either Github or AzureDevops
	# -gitHubUserName if using a GitHub repo this is the name of the user who owns the repo / optional depending if using github, only public Github projects supported
	#- forceVersion the package version number to force (e.g. 1.0.0, 1.1.4 or 1.12.0-alpha)
	
	if ($PrepareNewVersion) {
		
		Prepare-NewVersion -projects $($libraryProjects + $azureFunctionProject)`
        -azureDevopsOrganisation "anavarro9731" `
        -azureDevopsProject "soap" `
        -azureDevopsPat  "7ii2qmaehovdujwjgblveash2zc5lc2sqnirjc5f62hkdrdqhwzq"`
        -repository "soap" `
        -forceVersion $forceVersion
	}

	# options
	# -testProjects these are the projects with xUnit tests to be executed / can be an empty array
	
	if ($BuildAndTest) {
		Build-And-Test -testProjects $testProjects
	}
	
	# options
	# -allProjects (provided by $libraryProjects, do not modify, can be empty)
	# -unlistedProjects an array of project names whose nuget package should be unlisted / optional applicable only when the feed is hosted on nuget.org
	# -azureFunctionProject (provided by $f, do not modify, can be null)
	# -nugetFeedUri this is the URI of the nuget feed / optional applicable only when deploying class libraries
	# -nugetApiKey (provided by $nugetApiKey, do not modify, optional applicable only when deploying class libraries
	
	if ($PackAndPublish) {
		
		if ([string]::IsNullOrWhiteSpace($azureFunctionProject) -eq $false) {
			
			# options (all mandatory if deploying functionapp)
			# -project (provided by $azureFunctionProject variable, do not modify)
			# -azureAppName functionapp project name in azure
			# -azureResourceGroup resource group containing function project
			# -azureDevopsOrganisation azure devops organisation used to set functionapp env vars 
			# -azureDevopsPat (provided by $azureDevopsPat, do not modify) azure devops SCM PAT, passed to functionapp env vars
			# -azClientId (provided by $azClientId, do not modify)
			# -azClientSecret (provided by $azClientSecret, do not modify)
			# -azTenantId (provided by $azTenantId, do not modify)
			# -soapApplicationKey an arbitrary string which becomes part of the ConfigId used to identify the right app config to use
			$f = New-FunctionProjectParams -project  $azureFunctionProject `
			-azureAppName "soap-api-sample" `
			-azureResourceGroup "rg-soap" `
			-azureDevopsOrganisation "anavarro9731" `
		 	-azureDevopsPat $azureDevopsPat `
		 	-soapApplicationKey "SAP"
		}

		Pack-And-Publish -allProjects $libraryProjects `
		-unlistedProjects @( `
		) `
		-azureFunctionProject $f `
		-azClientId $azClientId `
		-azClientSecret $azClientSecret `
		-azTenantId $azTenantId `
		-nugetFeedUri "https://pkgs.dev.azure.com/anavarro9731/soap/_packaging/soap/nuget/v3/index.json" `
        -nugetApiKey $nugetApiKey
	}
}
