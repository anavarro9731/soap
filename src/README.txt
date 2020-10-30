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

Edit the pipeline variables for the new build in the portal listed in azure-pipelines.yml
-To get az-clientid, az-tenantid, az-clientsecret you will need to create a service principal login, az ad sp create-for-rbac --name ServicePrincipalName
you will need to do this from the cloudshell as global admin for the right permissions
see here for details (https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest) 
-To get ado-pat you will need to create one with permissions to read from the source repo for the version file and the config repo for the config
-To get nuget-key you will get from nuget feed provider (not needed when using azure devops, use ado-pat instead) 

Push the new initial commit which will build the infrastructure

Edit the local.settings.json file and update with the settings from the new cloud resources

Now you are ready for local development 

The boundaries of a service will always be its data structures (ie. aggregates) but how many is too many? when is your domain too big?
With the collection per aggregate model azure answers this for us: Mar 7 2020
"Azure Cosmos DB accounts using shared database throughput are now limited to 25 collections per database. 
This will allow for better throughput sharing across collections. 
Create additional databases with shared throughput and add more collections, or add collections to the same database with dedicated throughput."

* NOTES

Pkgs take 15 mins to be available to nuget clients on azure devops feed even after being visible in AzureDevops 