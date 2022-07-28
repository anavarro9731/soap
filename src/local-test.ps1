    [cmdletbinding()]
    param([switch]$PackAndPublish, [switch]$ConfigureLocalEnvironment, [switch] $CreateNewService, [switch]$ReleaseMode)

    if ($PackAndPublish) {
        
        
        .\pwsh-bootstrap.ps1
        git checkout master
        git push
        [System.Console]::ResetColor()
        git add -A
        git commit -a -m "build test"
        Run -PrepareNewVersion -push "SKIP" 
        if ($ReleaseMode) {
            Run -CreateRelease -push "SKIP"
        }
        Run -PackAndPublish `
       -nugetApiKey $( [System.Environment]::GetEnvironmentVariable('nuget-api-key')) `
       -azureDevopsPat $( [System.Environment]::GetEnvironmentVariable('ado-pat')) `
       -azClientId $( [System.Environment]::GetEnvironmentVariable('az-clientid')) `
       -azClientSecret $( [System.Environment]::GetEnvironmentVariable('az-clientsecret')) `
       -azTenantId $( [System.Environment]::GetEnvironmentVariable('az-tenantid'))

    } elseif ($ConfigureLocalEnvironment) {
        
        .\configure-local-environment.ps1 `
        -Arg_TenantId $( [System.Environment]::GetEnvironmentVariable('az-tenantid')) `
        -Arg_ClientId $( [System.Environment]::GetEnvironmentVariable('az-clientid')) `
        -Arg_ClientSecret $( [System.Environment]::GetEnvironmentVariable('az-clientsecret')) `
        -Arg_EnvironmentPartitionKey aaronn `
        -Arg_ResourceGroup rg-soap `
        -Arg_VNextFunctionAppName soapapisample-vnext
        
    } elseif ($CreateNewService) {
        
        $RandomSuffix = -join ((65..90) + (97..122) | Get-Random -Count 5 | % {[char]$_})
        .\create-new-service.ps1 `
	    -Arg_AzureDevopsOrganisationName "anavarro9731" `
        -Arg_ServiceName "TestProject.$RandomSuffix" `
        -Arg_RepoAndPackagingAzPersonalAccessToken $( [System.Environment]::GetEnvironmentVariable('ado-pat')) `
        -Arg_AdminAzPersonalAccessToken $( [System.Environment]::GetEnvironmentVariable('ado-pat-admin')) `
        -Arg_AzResourceGroup "rg-testproject-$RandomSuffix" `
        -Arg_AzLocation "uksouth" `
        -Arg_TenantId $( [System.Environment]::GetEnvironmentVariable('az-tenantid')) `
        -Arg_ClientId $( [System.Environment]::GetEnvironmentVariable('az-clientid')) `
        -Arg_ClientSecret $( [System.Environment]::GetEnvironmentVariable('az-clientsecret')) `
        -Arg_PathOnDisk "c:\source"
        
    }

    #[System.Environment]::SetEnvironmentVariable('nuget-api-key', "X")
    #[System.Environment]::SetEnvironmentVariable('ado-pat', "X")
    #[System.Environment]::SetEnvironmentVariable('az-clientid', "X")
    #[System.Environment]::SetEnvironmentVariable('az-clientsecret', "X")
    #[System.Environment]::SetEnvironmentVariable('az-tenantid', "X")
   