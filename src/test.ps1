    [cmdletbinding()]
    param([switch]$PackAndPublish, [switch]$ConfigureLocalEnvironment, [switch] $CreateNewService, [switch]$ReleaseMode)

    if ($PackAndPublish) {
        
        
        .\pwsh-bootstrap.ps1
        git push
        [System.Console]::ResetColor()
        git add -A
        git commit -a -m "build test"
        Run -PrepareNewVersion -push "SKIP" # need to update nugetApitKey in pwsh-bootstrap too because of this call
        if ($ReleaseMode) {
            Run -CreateRelease -push "SKIP"
        }
        Run -PackAndPublish `
       -nugetApiKey "u6hiiuutqr4ztdzxiyqyrhsu5nkqswl5lh44gxu4zukuiqqtz5fq" `
       -azureDevopsPat "u6hiiuutqr4ztdzxiyqyrhsu5nkqswl5lh44gxu4zukuiqqtz5fq" `
       -azClientId "fbff5a19-e7a7-49a2-829c-ad415b507577" `
       -azClientSecret "T4~7Q~ZTK2so.EgoyBjvs7oLKCUdgwz4m6GaV" `
       -azTenantId "f8d686ac-a87f-4d1c-bbcf-d08873871dcd"

    } elseif ($ConfigureLocalEnvironment) {
        
        .\configure-local-environment.ps1 `
        -Arg_ClientId "fbff5a19-e7a7-49a2-829c-ad415b507577" `
        -Arg_ClientSecret "T4~7Q~ZTK2so.EgoyBjvs7oLKCUdgwz4m6GaV" `
        -Arg_TenantId "f8d686ac-a87f-4d1c-bbcf-d08873871dcd" `
        -Arg_EnvironmentPartitionKey aaronn `
        -Arg_ResourceGroup rg-soap `
        -Arg_VNextFunctionAppName soapapisample-vnext
        
    } elseif ($CreateNewService) {
        
        $RandomSuffix = -join ((65..90) + (97..122) | Get-Random -Count 5 | % {[char]$_})
        .\create-new-service.ps1 `
	    -Arg_AzureDevopsOrganisationName "anavarro9731" `
        -Arg_ServiceName "TestProject.$RandomSuffix" `
        -Arg_AzPersonalAccessToken "u6hiiuutqr4ztdzxiyqyrhsu5nkqswl5lh44gxu4zukuiqqtz5fq"`
        -Arg_AzResourceGroup "rg-testproject-$RandomSuffix" `
        -Arg_AzLocation "uksouth" `
        -Arg_TenantId "f8d686ac-a87f-4d1c-bbcf-d08873871dcd" `
        -Arg_ClientId "fbff5a19-e7a7-49a2-829c-ad415b507577" `
        -Arg_ClientSecret "T4~7Q~ZTK2so.EgoyBjvs7oLKCUdgwz4m6GaV" `
        -Arg_PathOnDisk "c:\source"
        
    }
