# DEV ENVIRONMENT SETUP
~ setup time 45 mins

##Prerequisites

###Install Powershell
Some tools still require the x86 version so while you can install x64 SxS with x86, 
be sure to install and use x86 with the commands in this guide. 

You can get the latest version of powershell [here](https://github.com/PowerShell/PowerShell)

Then run the following commands from a PWSH x86 *Elevated* command prompt
`Set-ExecutionPolicy Unrestricted` to allow all scripts to run.

###Install Chocolatey
```
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
```
###Install Jetbrains Rider 
```
choco install jetbrains-rider
```
###Install Azure CLI and extensions
```
choco install azure-cli
```
close and reopen powershell
```
az extension add --name azure-devops
```
###Install GIT
```
choco install git
```
Then set the git config using
```powershell
git config --global user.name "John Doe"
git config --global user.email johndoe@example.com
```
you need to be sure that these props are set globally
because the projects created/managed by script do not
set them
###Install Client-side tools
```
choco install nodejs
```
close and reopen powershell
```
npm install -g yarn
```
###Install Local Dev tools
```
lodctr /R (corrupted perf counters will cause azurite emulator errors, run this command twice in succession)
npm install -g azurite
```
###Install DotNet 5
```
https://dotnet.microsoft.com/download
or via choco install 
```
### Verify Installations
```
choco list --local-only
node -v
yarn -v
```
### Restart your machine

There are several items that don't work right otherwise.

## Setting up a new service and associated pipeline

### Accounts
- Make sure you have an Azure account (portal.azure.com)
- make sure you have an Azure devops organisation (dev.azure.com or goto All Resource -> Azure Devops Organisation from the Azure Portal)

#### Creating an Azure Service Principal
To obtain  az-clientid, az-tenantid, az-clientsecret values you will need to create a service principal login.
You will need to do this from the Azure CloudShell as global admin for the right permissions and
Once logged into shell.azure.com as global admin run:
```
 az ad sp create-for-rbac --name SoapServicePrincipal
 make sure you are on the right subscription before running the command if not, change it first!
 az account list
 az account set -s "SubscriptionNameOrIdHere"
```
See [here](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest) for details 

you will then get a series of values in the json response which you need to save somewhere
temporarily that looks like the following:
You need to obtain three 3 values.
1. appId , retain as, clientId
2. password , retain as, clientSecret
3. tenant , retain as, tenantId
```javascript
{
"appId": "12ac9ab7-931a-4fb9-b04b-9e686d75f33e",
"displayName": "SoapServicePrincipal",
"name": "http://SoapServicePrincipal",
"password": "12ac9ab7-931a-4fb9-b04b-9e686d75f33e",
"tenant": "12ac9ab7-931a-4fb9-b04b-9e686d75f33e"
}
```
#### Devops key

You will need to create a Personal Access Token(s) with proper permissions to satisfy to variables
For the ado-pat variable you need a PAT which must be able to read from the source repo for the ```.version``` file and the config repo for the ```Config.cs``` file.
For the nuget-key variable the PAT must be able to read/write from the package feed.
Both variables can use the same PAT.

### Create Source Code and Infrastructure

Open powershell.

run `az login` followed by `az devops login` to make sure the CLI is authenticated.
If you have multiple organisations/accounts be careful that your are logged into the right one.
 az account list
 az account set -s "SubscriptionNameOrIdHere"

then run 
`az devops configure --defaults organization=https://dev.azure.com/YOURORGANIZATION/`

test with `az devops project list`
cls

Running the ```.\create-new-service.psm1``` script will create a Devops project and pipeline.  
Next you must edit the pipeline variables for the new build.
These are listed in the ```azure-pipelines.yml``` file in the root of the new project.

Now wait for the resource group you defined when you ran ```.\create-new-service.psm1``` to be populated
with the required services which will occur after the script finishes and the azure devops build runs this can take 
15-20 mins.


###Running Locally

1. Install the following Rider plugins:
- https://plugins.jetbrains.com/plugin/9525--env-files-support
- https://plugins.jetbrains.com/plugin/11220-azure-toolkit-for-rider (This will install Azurite support)
- https://plugins.jetbrains.com/plugin/10249-powershell

2. Set Powershell x86 as the Rider Shell by editing the path located at: File > Settings > Tools > Terminal
   and pointing it at `C:\Program Files (x86)\PowerShell\7\pwsh.exe`

3. The following are the cloud services that need to be considered in regards to **local development** when the function app is running in the cloud none of the following apply.

Each azure service has a unique way of creating a developer-specific experience.
Some are based on the EPK [Environment Partition Key] which you entered when creating a new service using the
pwsh script create-new-service.ps1 which is subsequently then stored in the local.settings.json file as an environment variable.
While others run a local emulator which is unique to each developers machine. This is only when it is not
possible to run in the cloud with some sort of partitioning in the VNEXT environment as it requires more setup.

Service|Setup Required|Environment Separation Method|Config Variable
---|---|---|---
Azure ServiceBus|No|**Session-Enabled Queue on VNext instance** (SessionId=EPK)<br />Subscriptions and Queues will be created with the name of each EPK appended when you run the /CheckHealth function|AzureWebJobsServiceBus
Azure SignalR Service|No|**Group-Enabled Messages on VNext instance** (Group=EPK)<br />SignalR Groups will be created for your EPK and all connections initiated from your machine will be added to that Group, finally any Websocket Broadcasts will be limited to your the Group for your EPK. These will expire when you kill of your connections.|AzureSignalRConnectionString
Azure Storage (Blob)|You will need to start the Azurite instance or you will get an error. In rider this is done from the View>Tool Windows>Services window. If attempting to start it pops up a dialog asking you to confirm some settings, you need to choose the system global file path to the azure node module (e.g. `~\AppData\Roaming\npm\node_modules\azurite`). Assuming you installed the Rider Azure toolkit plugin as specified above, simply press Play on the instance. |**Azurite Local Instance**<br />Azurite settings are fixed in the local.settings.json file and do not change. When running in Development mode, the FunctionContext will set some additional properties which cannot be set by config such as CORS on the Azurite instance during function startup. On startup the function app will also print an Azurite SAS to the console which you can append to any manual Azurite request for testing HTTP. Finally, there have been noted instanced where Azurite settings do not update as expected, this seems to happen only initially after install. If you get CORS errors, [in Rider] stop the instance from the services tool window, right click on the named instance node and choose "Clean Azurite" then start it again to fix this problem.|AzureWebJobsStorage
Azure CosmosDb|No|**Cosmos containers for each EPK (or CosmosDb Emulator Local Instance)**<br />The recommended approach is to use a cloud instance with one database, which shares it's resources across containers, having one container per EPK. To use this approach you don't need to do anything more than set the EPK. This is recommended because for unknown reasons the local emulator is much slower than an actual cloud instance. If using local Cosmos Emulator, you can install using `choco install azure-cosmosdb-emulator` and then edit the shortcut and set the following switches on the command: `"C:\Program Files\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe" /PartitionCount=250 /DisableRateLimiting`. Next set the 4 local.settings.json Cosmos values using the data in the connection string which you can get from the emulator homepage (after its installed), by default this is https://localhost:8081/. More Emulator Info [here](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)
Auth0|No|**Permissions prefixed for each EPK** (EPK-PermissionName)|Auth0 variables see Auth section
*A note on the emulators: You may want to run `netstat -ao` to check nothing is using ports 10000-10003
before starting azurite, or port 8081 before starting the cosmos emulator, otherwise you will need to change the ports they run on and change the connection strings in local.settings.json*

Assuming you have followed the steps in order from the [link](#creating-a-new-service-and-pipeline) section then it should already have created all the VNEXT cloud services.
You know need to update the local.settings.json and .env files and with values from the new cloud resources
which we cannot possible know at the time you run create-new-service.ps1, to do this run the configure-local-environment.ps1 script
You will need to enter several variables to give the script access to download the online config.

Next, make sure the Azurite service is running per the above instruction.
Then you need to start the Azure Function Project ```YourProjectName.Afs```
You will get errors in the console output first since the queue does not exit,
Ignore these and navigate in a browser to http://localhost:7071/api/checkhealth
Once this script, which will
- create developer specific database
- create developer specific subscriptions and queues,
completes the errors should stop.

Last you need to build and run the client app.
Before you can do that you need to import the files into the solution, 
you can do that by using the "attach folder" feature.

Press {Alt+1} to make sure the Solution Explorer is in view.
Then right-click on the solution root node and select Add->Attach Existing Folder
then choose the "app" folder in the root of the new repo.
This will add it as a node in the solution file/folder treeview.

Next, open a terminal {Ctrl+Alt+1} goto the root/src/app folder of the repo
Run these two commands in succession
`yarn install`
`yarn run serve`
then navigate to http://localhost:1234/ in your browser to see the app running (use F12 to verify there are no error)

### Environments

There are several:
InMemory - This is the name of the environment we give to running unit tests
Dev - This is the name of the environment when you run in the IDE locally (uses the local.settings.json) file for config
Vnext - This is the test environment and correlates to the code on the master branch
Release - This environment correlates to whatever code was released most recently on a release branch and includes hotfixes. It is a function app slot.
Live - This environment is for production code. It is the production slot of the same function app used for release. There is no pwsh trigger for this environment, instead it receives updates via slot swaps with the Release slot.
TODO config for LIVE not sorted yet, should be created when release is created.

### Pushing a new version to VNEXT environment

Run `pwsh-bootstrap.ps1` from the repo root/src folder to load the modules
Make sure you are on the `master` branch.
Then run the command `Run -PrepareNewVersion` this will increment the version.
Choose the type of change [Breaking|Minor]
Finally, choose whether to push the commit.
If you push the commit and the version has been incremented a new version will be released to the VNEXT environment

### Creating a new Release and sending to the release environment

Run `pwsh-bootstrap.ps1` from the repo root/src folder to load the modules
Make sure you are on the `master` branch.dotnet 
Then run the command `Run -CreateRelease` this will sort out creating the new branch and adjusting the versions for both the new release branch and master.
It will push the release to Azure and create the environment if necessary.
Finally, it will leave you on the master branch afterwards.

# NOTES

- Azure SDK releases found [here](https://azure.github.io/azure-sdk/releases/latest/dotnet.html)
- We are stuck using the -no-source-maps flag in parcel v1 because auth0 library wont work otherwise, parcel team not fixing
  the issue before v2, at present parcel v2 still buggy and doesn't work for us. 1. won't cancel properly, random errors
  
# Common Errors 
- Pkgs take 15 mins to be available to nuget clients on azure devops feed even after being visible in AzureDevops. This means when you release a new version of the Soap packages, create-new-service won't use them for 15 mins.
- Azurite has been known to generate CORS errors for other things, e.g. when the problem is really the SAS token generated server side
- When using Jetbrains Rider [@2020.3] after upgrading a nuget package which is both directly and implicitly installed in projects. (e.g. Datastore) You need to invalidate caches/restart for it to properly display the implicit imports
- Changing the .env variables requires restarting parcel
- When publishing the soap project itself, make sure that pwsh-bootstrap is loaded for the soap project and not another project which could happen if for example you were running test.ps1 recently. Second make sure the client app is running
  against a packaged version of soap. e.g. srv.ps1 -ToRemoteSoap. Both these problems should alert you through issues
  when publishing but its good to note them.
  

