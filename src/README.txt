* DEV ENVIRONMENT

Install Chocolatey

Install Jetbrains Rider 
> choco list rider
> choco install ????

Install Azure CLI and extensions
> choco install azure-cli
> az extension add --name azure-devops
> az extension list (to verify)

* CREATING NEW SERVICE

Use the create-new-service powershell from the soap project
> .\create-new-service.psm1

create a pipeline linked to the azure-pipelines.yml file you just uploaded (will this happen automatically?)

Edit the pipeline variables in portal adding nuget-key and ado-pat (can you specify these in the YAML with *******)

Create a new 
- documentdb instance, 
- azure blob storage, 
- seq key, 
- service bus queue, 
- mailgun apikey, 
- azure functions app (name: org-api-apiname, all defaults or obvious + new storage: stg-orgapiapiname)
    add slots for dev, vnext, release, and preprod
(to script defaults possibly in future)

Edit the MyProject.Api.MyApi.Afs Helpers.cs file and update the ConfigId vars for local execution

Edit the local.settings.json file and update the dev environment service bus connection string


VERBOSE: Authenticating to Azure ...
VERBOSE: Building your Azure drive ...
PS /home/aaron> az ad sp create-for-rbac --name DevopsServicePrincipal
Changing "DevopsServicePrincipal" to a valid URI of "http://DevopsServicePrincipal", which is the required format used for service principal names
Creating a role assignment under the scope of "/subscriptions/56f4beb0-5d15-4f0b-941a-350bd8721d1c"
  Retrying role assignment creation: 1/36
{
  "appId": "fbff5a19-e7a7-49a2-829c-ad415b507577",
  "displayName": "DevopsServicePrincipal",
  "name": "http://DevopsServicePrincipal",
  "password": "34ME15tmte0Hm8x6oXuV_deo~bgzJqK~H-",
  "tenant": "f8d686ac-a87f-4d1c-bbcf-d08873871dcd"
}
PS /home/aaron>

Run -PackAndPublish `
-nugetApiKey "incorrect" `
-azureDevopsPat "7ii2qmaehovdujwjgblveash2zc5lc2sqnirjc5f62hkdrdqhwzq" `
-azureFunctionProject "Soap.Api.Sample\Soap.Api.Sample.Afs" `
-azureAppName "soap-api-sample" `
-azureResourceGroup "rg-soap" `
-azureDevopsOrganisation "anavarro9731" `
-azureWebJobsServiceBus "Endpoint=sb://sb-soap-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=uXpkZEaaCaskdFNvGdrgR38eGgJ1QE5JskVJbSK5Tl0=" `
-azureInfrastructureLogin "anavarro9731@gmail.com" `
-azureInfrastructurePwd "GEL`"06`"vilitas"
