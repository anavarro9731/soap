# DEV ENVIRONMENT 
~ setup time 10-20 mins

##Steps
###Install Powershell
Some tools still require the x86 version so while you can install x64 SxS with x86, 
be sure to install and use x86 with the commands in this guide. 

You can get the latest version of powershell [here](https://github.com/PowerShell/PowerShell)

Then run the following commands from a PWSH x86 *Elevated* command prompt
`Set-ExecutionPolicy Unrestricted` to allow all scripts to run.

###Install Chocolatey
```
@"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command " [System.Net.ServicePointManager]::SecurityProtocol = 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" && SET "PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin"
```
###Install Jetbrains Rider 
```
choco install jetbrains-rider
```
###Install Azure CLI and extensions
```
choco install azure-cli
az extension add --name azure-devops
```
###Install GIT
```
choco install git
```
###Install Client-side tools
```
choco install nodejs
npm install -g yarn
```
###Install Local Dev tools
```
lodctr /R (corrupted perf counters will cause azurite emulator errors, run this command twice in succession)
npm install -g azurite
```
### Verify Installations
```
choco list --local-only
node -v
yarn -v
```
### Restart your machine

Cosmos Emulator in particular throws errors about multiple instances without a restart but it's good practice anyway with so many critical installs.

# CREATING A NEW SERVICE AND PIPELINE
~ setup time 15-30 mins

##Steps 

### Accounts
- Make sure you have an Azure account (portal.azure.com)
- make sure you have an Azure devops organisation (dev.azure.com or goto All Resource -> Azure Devops Organisation from the Azure Portal)

### Source code 
Use the create-new-service.ps1 script from the soap project to create a new project
```
.\create-new-service.psm1
```

### Add Authorisation Variables
Running the ```.\create-new-service.psm1``` script will create a Devops project and pipeline.  
Next you must edit the pipeline variables for the new build. 
These are listed in the ```azure-pipelines.yml``` file in the root of the new project.
#### Creating an Azure Service Principal
To obtain  az-clientid, az-tenantid, az-clientsecret values you will need to create a service principal login.
You will need to do this from the Azure CloudShell as global admin for the right permissions.
Once logged into shell.azure.com as global admin run:
```
 az ad sp create-for-rbac --name ServicePrincipalName
```
See [here](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest) for details 
#### Devops key
The ado-pat and nuget-key values you will need to create a Personal Access Token(s) with proper permissions.
For ado-pat the PAT must be able to read from the source repo for the ```.version``` file and the config repo for the ```Config.cs``` file.
For nuget-key the PAT must be able to read/write from the package feed.
Both variables can use the same PAT.  

### Infrastructure
Push the new initial commit which should be waiting locally to build the Azure infrastructure.
From a PWSH prompt in the repo root run:
```
git push
```
Now wait for the resource group you defined in ```.\create-new-service.psm1```

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
Azure ServiceBus|No|**Session-Enabled Queue on VNext instance** (SessionId=EPK)<br />Subscriptions and Queues will be created for each EPK when you run the /CheckHealth function|AzureWebJobsServiceBus
Azure SignalR Service|No|**Group-Enabled Messages on VNext instance** (Group=EPK)<br />SignalR Groups will be created for your EPK and all connections initiated from your machine will be added to that Group, finally any Websocket Broadcasts will be limited to your the Group for your EPK. These will expire when you kill of your connections.|AzureSignalRConnectionString
Azure Storage (Blob)|You will need to start the Azurite instance or you will get an error. In rider this is done from the View>Tool Windows>Services window. Assuming you installed the Rider Azure toolkit plugin as specified above, simply press Play on the instance.|**Azurite Local Instance**<br />Azurite settings are fixed in the local.settings.json file and do not change. When running in Development mode, the FunctionContext will set some additional properties which cannot be set by config such as CORS on the Azurite instance during function startup. On startup the function app will also print an Azurite SAS to the console which you can append to any manual Azurite request for testing HTTP. Finally, there have been noted instanced where Azurite settings do not update as expected, this seems to happen only initially after install. If you get CORS errors, [in Rider] stop the instance from the services tool window, right click on the named instance node and choose "Clean Azurite" then start it again to fix this problem.|AzureWebJobsStorage
Azure CosmosDb|No|**CosmosDb Emulator Local Instance or Cosmos containers for each EPK**<br />The recommended approach is to use a cloud instance with one database, which shares it's resources across containers, having one container per EPK. To use this approach you don't need to do anything more than set the EPK. This is recommended because for unknown reasons the local emulator is much slower than an actual cloud instance. If using local Cosmos Emulator, you can install using `choco install azure-cosmosdb-emulator` and then edit the shortcut and set the following switches on the command: `"C:\Program Files\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe" /PartitionCount=250 /DisableRateLimiting`. Next set the 4 local.settings.json Cosmos values using the data in the connection string which you can get from the emulator homepage (after its installed), by default this is https://localhost:8081/. More Emulator Info [here](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)

Assuming you have followed the steps in order and have already pushed your initial commit as explained in the final step of the
[link](#creating-a-new-service-and-pipeline) section then it should already have created all the VNEXT cloud services.
You know need to edit the local.settings.json file and update the following settings with values from the new cloud resources
which we cannot possible know at the time you run create-new-service.ps1. In future a script that reads these values could
be created to update the local.settings file.

Variable Name|How to get Value
---|---
AzureWebJobsServiceBus|Copy connection String from portal (sb-*yourapiname*-vnext)
AzureSignalRConnectionString|Copy connection String from portal (signalr-*yourapiname*-vnext)
CosmosDbKey| Copy from the portal (cdb-*yourapiname*) 

A note on the emulators: You may want to run `netstat -ao` to check nothing is using port 8081 before starting the cosmos emulator or ports 10000-10003
before starting azurite, otherwise you will need to change the ports they run on and change the connection strings in local.settings.json

Now you are ready for local development and can run the Azure Function Project ```YourProjectName.Afs```
You will also need to start the client-side project from the terminal by running the following command `.\srv.ps1` from the `root\src\js\yourprojectname\` folder

Finally check the service health using `http://localhost:7071/api/CheckHealth` endpoint which will also
- create developer specific database
- create developer specific subscriptions and queues

### NOTES
 
- Pkgs take 15 mins to be available to nuget clients on azure devops feed even after being visible in AzureDevops
- Azure SDK releases found [here](https://azure.github.io/azure-sdk/releases/latest/dotnet.html)
- When using Jetbrains Rider [@2020.2] after upgrading a nuget package which is both directly and implicitly installed in projects. (e.g. Datastore) You need to invalidate caches/restart for it to properly display pickup the implicit imports
- Changing the .env variables requires restarting parcel

### BackLog
MUST
- Update to new [Azure.Cosmos] CosmosDb SDK and new CircuitBoard (currently using the really old (2 versions back) SDK this is a datastore change)
SHOULD
- What to do about the local and cloud fighting each other when debugging and no dev database?
- Hide Datastore extension methods from Soap (https://www.meziantou.net/declaring-internalsvisibleto-in-the-csproj.htm)
COULD
- Adding a Special Flag or Tag to Denote builds that were sent to product (which will need new Run -InstallProd switch which runs az slot swap and tags so when your looking at the release branch you can see which version went to production)
- Fixing DateTime fragility by abstracting all date/time functionality and/or using NodaTime rather than DateTime. (NodaTime will not pass IsSystemType checks may need to adjust those functions (there are private versions))
- Request/Reply Queries for data between services (i.e. sending and waiting in-process as a way to query another service rather than a series of bus messages and a statefulprocess) implement using MessageSessions in Azure ServiceBus
- Client-Side Batching using native Azure ServiceBus feature
- Add DataStore "Document Size Limit" on write error. It will still fail with a less friendly native error from CosmosDB as-is. Also, hitting the 2MB limit should not happen really without bad design.
- Cleanup i18next module in soapjs
- Add AssemblyReferences files for all assemblies
- CheckHealth to Test SignalR
