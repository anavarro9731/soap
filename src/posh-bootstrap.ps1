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
		[string] $forceVersion,
		[string] $azureFunctionProject,
		[string] $azureAppName,
		[string] $azureResourceGroup,
		[string] $azureDevopsOrganisation,
		[string] $azureWebJobsServiceBus,
		[string] $azLoginCmd
	)

	if ($PrepareNewVersion) {
		Prepare-NewVersion -projects @(
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
		) `
        -azureDevopsProject "soap" `
        -azureDevopsOrganisation "anavarro9731" `
        -azureDevopsPat  "7ii2qmaehovdujwjgblveash2zc5lc2sqnirjc5f62hkdrdqhwzq"`
        -repository "soap" `
        -forceVersion $forceVersion
	}

	if ($BuildAndTest) {
		Build-And-Test -testProjects @(
		"Soap.UnitTests"
		)
	}

	if ($PackAndPublish) {
		
		if ([string]::IsNullOrWhiteSpace($azureFunctionProject) -eq $false) {
		$f = Create-FunctionProjectParams `
		 $azureFunctionProject `
		 $azureAppName `
		 $azureResourceGroup `
		 $azureDevopsOrganisation `
		 $azureDevopsPat `
		 $azureWebJobsServiceBus `
		 $azLoginCmd
		}

		Pack-And-Publish -allProjects @(
		"Soap.PfBase.Api",
		"Soap.PfBase.Logic",
		"Soap.PfBase.Tests",
		"Soap.PfBase.Models",
		"Soap.PfBase.Messages",
		"Soap.Config",
		"Soap.Auth0",
		"Soap.Bus",
		"Soap.Context",
		"Soap.Interfaces",
		"Soap.Interfaces.Messages",
		"Soap.MessagePipeline",
		"Soap.NotificationServer",
		"Soap.Utility"
		) -unlistedProjects @(
		) `
		-azureFunctionProject $f `
        -nugetApiKey $nugetApiKey `
		-nugetFeedUri "https://pkgs.dev.azure.com/anavarro9731/soap/_packaging/soap/nuget/v3/index.json"
	}
}
