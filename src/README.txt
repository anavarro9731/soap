* DEV ENVIRONMENT (Setup time 30 mins)

Install Chocolatey
@"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command " [System.Net.ServicePointManager]::SecurityProtocol = 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" && SET "PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin"

Install Jetbrains Rider 
> choco list rider
> choco install ????
> choco list --local-only (to verify installation)

Install Azure CLI and extensions
> choco install azure-cli
> az extension add --name azure-devops
> az extension list (to verify installation)

* CREATING NEW SERVICE AND PIPELINE (Setup time 30 mins)

Use the create-new-service powershell from the soap project
> .\create-new-service.psm1

Create infrastructure in cloud (to script default creation in future)
- documentdb instance, 
- azure blob storage, 
- seq key, 
- service bus queue, 
- mailgun apikey, 
- azure functions app (name: org-api-apiname, all defaults or obvious + new storage: stg-orgapiapiname)
    add rel slot
- azure functions app (name: org-api-apiname-vnext, all defaults or obvious + stg-orgapiapiname)

Edit the local.settings.json file and update the properties listed in the EnvVars class to the right settings for the DEV environment

Create a new repo and for the files

Update the pwsh-bootstrap.ps1 file 

Upload the files your their new repo

Create a pipeline linked to the azure-pipelines.yml file you just uploaded (will this happen automatically?)

Edit the pipeline variables in portal listed in azure-pipelines.yml
-To get az login you will need to create a service principal login, az ad sp create-for-rbac --name ServicePrincipalName
you will need to do this from the cloudshell as global admin for the right permissions
see here for details (https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest) 
-To get ado-pat you will need to create one with permissions to read from the source repo for the version file and the config repo for the config
-To get nuget-key you will get from nuget feed provider (not needed when using azure devops, use ado-pat instead) 



