    [cmdletbinding()]
    param([string]$PackAndPublish, [switch]$ConfigureLocalEnvironment, [switch] $CreateNewService)

    if ($PackAndPublish) {
        
        .\pwsh-bootstrap.ps1
        git push
        [System.Console]::ResetColor()
        git add -A
        git commit -a -m "build test"
        Run -PrepareNewVersion
        Run -PackAndPublish `
       -nugetApiKey "j35ssqoabmwviu7du4yin6lmw3l2nc4okz37tcdmpirl3ftgyiia" `
       -azureDevopsPat "j35ssqoabmwviu7du4yin6lmw3l2nc4okz37tcdmpirl3ftgyiia" `
       -azClientId "fbff5a19-e7a7-49a2-829c-ad415b507577" `
       -azClientSecret "34ME15tmte0Hm8x6oXuV_deo~bgzJqK~H-" `
       -azTenantId "f8d686ac-a87f-4d1c-bbcf-d08873871dcd"

    } elseif ($ConfigureLocalEnvironment) {
        
        .\configure-local-environment.ps1 -AzClientId "fbff5a19-e7a7-49a2-829c-ad415b507577" `
        -AzClientSecret "34ME15tmte0Hm8x6oXuV_deo~bgzJqK~H-" `
        -AzTenantId "f8d686ac-a87f-4d1c-bbcf-d08873871dcd" `
        -EnvironmentPartitionKey aaronn `
        -ResourceGroup rg-soap `
        -FunctionAppName soapapisample-vnext
        
    } elseif ($CreateNewService) {
        
        $RandomSuffix = -join ((65..90) + (97..122) | Get-Random -Count 5 | % {[char]$_})
        .\create-new-service.ps1 `
	    -Arg_AzureDevopsOrganisationName "anavarro9731" `
        -Arg_ServiceName "TestProject.$RandomSuffix" `
        -Arg_AzPersonalAccessToken "j35ssqoabmwviu7du4yin6lmw3l2nc4okz37tcdmpirl3ftgyiia"`
        -Arg_AzResourceGroup "rg-testproject-$RandomSuffix" `
        -Arg_AzLocation "uksouth" `
        -Arg_TenantId "f8d686ac-a87f-4d1c-bbcf-d08873871dcd" `
        -Arg_ClientId "fbff5a19-e7a7-49a2-829c-ad415b507577" `
        -Arg_ClientSecret "34ME15tmte0Hm8x6oXuV_deo~bgzJqK~H-" `
        -Arg_PathOnDisk "c:\source"
        
    }
