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

Adjust variables in the posh-boostrap and azure-pipelines files

create a pipeline linked to the azure-pipelines.yml file you just uploaded (will this happen automatically?)

edit the pipeline variables in portal adding nuget-key and ado-pat

Create a new 
- documentdb instance, 
- azure blob storage, 
- seq key, 
- service bus queue, 
- mailgun apikey, 
- azure functions app (name: org-api-apiname, all defaults or obvious + new storage: stg-orgapiapiname)
    add slots for dev, vnext, release, and preprod
(to script defaults possibly in future)

Edit the MyProject.Api.MyApi.Afs Receive.cs file and update the ConfigId vars for local execution

Edit the local.settings.json file and update the dev environment service bus connection string

az login -u anavarro9731@gmail.com -p GEL"06"vilitas





Make the following changes to default setup:
> build plan
select soap feed
select latest win agent

> release plan
set ConfigId vars plus AzureWebJobsServiceBus in AppSettings
change method to ZipDeploy
change to latest win agent
