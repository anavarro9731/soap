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
###Install DotNet 3.1
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

make sure you are on the right subscription before running the command if not, change it first!
az account list
az account set -s "SubscriptionNameOrIdHere"
az account show

# then you will need to identify or create the resource-group 
# finally you can execute
az ad sp create-for-rbac --name SoapServicePrincipal{or ProjectSpecificNameIfYouWantToSegregateSecurity} --role Contributor --scopes /subscriptions/{SubID}/resourceGroups/{ResourceGroup1}  

 
to verify 
az ad sp list --display-name SoapServicePrincipal
or to see all existing you could try
az ad sp list --all --query "[?contains(displayName,'Soap')||contains(displayName,'Principal')].{Name: displayName, Id: objectId}" --output table

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
"appId": "00000000-0000-0000-0000-000000000000",
"displayName": "SoapServicePrincipal",
"name": "http://SoapServicePrincipal",
"password": "00000000-0000-0000-0000-000000000000",
"tenant": "00000000-0000-0000-0000-000000000000"
}
```
#### Devops key

You will need to create a Personal Access Token(s) with proper permissions to satisfy to variables
For the ado-pat variable you need a PAT which must be able to read from the source repo for the ```.version``` 
file and the config repo for the ```Config.cs``` file.
For the nuget-key variable the PAT must be able to read/write from the package feed.
Both variables can use the same PAT.

### Create Source Code and Infrastructure

Open powershell.

run `az login`  to make sure the CLI is authenticated.
If you have multiple organisations/accounts be careful that your are logged into the right one.
 az account list
 az account set -s "SubscriptionNameOrIdHere"
 az devops configure --defaults organization=https://dev.azure.com/YOURORGANIZATION/

then run 

`echo YOURADMINPAT | az devops login --org https://dev.azure.com/YOURORGANISATION/ --verbose
az devops configure --defaults organization=https://YOURORGANIZATION/`
`

test with `az devops project list --detect false` the detect may be needed whenever the current folder is an 
azure devops git repo as those credentials and org will always trump what you enter on CLI without it 

cls
~~~~
Now goto the directory where the Soap repo was created
be aware that in powershell 

Running the ```.\create-new-service.psm1``` script will create a Devops project and pipeline.  

Now wait for the resource group you defined when you ran ```.\create-new-service.psm1``` to be populated
with the required services which will occur after the script finishes and the azure devops build runs this can take 
15-20 mins.


###Running Locally

1. Open Rider and Install the following Rider plugins from the File -> Settings -> Plugins window:
- https://plugins.jetbrains.com/plugin/9525--env-files-support
- https://plugins.jetbrains.com/plugin/11220-azure-toolkit-for-rider (This will install Azurite support)
- https://plugins.jetbrains.com/plugin/10249-powershell

Then install Azure Functions Core Tools. Since there is an Azure Functions project you should see a notification
with a link to install it in the event log which is located in the lower right hand corner next to a green bubble.
It's important to make sure however that you install the version of the runtime that works with the version of 
.NET that you are running. You can see the match [here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-versions?tabs=in-process%2Cv4&pivots=programming-language-csharp)
and find the correct link to install [here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v3%2Cwindows%2Ccsharp%2Cportal%2Cbash#v2)

Rider comes default with Resharper features builtin so you may want to check that the keyboard map is to your liking.
You can check this from File -> Settings -> Keymap

2. Open the new solution in Rider and add the Soap Nuget feed
https://pkgs.dev.azure.com/anavarro9731/soap-feed/_packaging/soap-pkgs/nuget/v3/index.json
   
3. Set Powershell x86 as the Rider Shell by editing the path located at: File > Settings > Tools > Terminal
   and pointing it at `C:\Program Files (x86)\PowerShell\7\pwsh.exe`

4. The following are the cloud services that need to be considered in regards to **local development** when the function app is running in the cloud none of the following apply.

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

5. Last you need to build and run the client app.
Before you can do that you need to import the files into the solution, 
you can do that by using the "attach folder" feature.

Press {Alt+1} to make sure the Solution Explorer is in view.
Then right-click on the solution root node and select Add->Attach Existing Folder
then choose the "app" folder in the root of the new repo.
This will add it as a node in the solution file/folder treeview.

Next, open a terminal {Ctrl+Alt+1} goto the root/src/components/modules folder of the repo
Run `yarn install`
Next, goto the root/src/app folder of the repo
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
  
- Sometimes you may want to test by connecting your local dev environment to production data. The right way to do this
is to edit the DEV/Config.cs file and for the db connection set the Container and DB property values manually. The container
  should be set to "REL" and the db to "cdb-{yourappname}-rel". Ideally everything else can stay the same. The one thing 
  you must check first, is that the version of the code running locally will not corrupt the data in the production database
  by using a newer schema (including via a new Db upgrade task)

# Common Errors 
- If you are getting errors in frontend, building or running that suddenly appear, check that
    - make sure you don't accidentally have another package.json higher up in the folder structure where your app is located
    - you have used fixed versions of all packages in your package.json. i.e. "x.x.x" rather than "^x.x.x"
- If your having trouble with build server suddenly failing to build, even without a code change, check the image is not set to "win-latest" for example in you azure-pipelines.yml file. if so, fix it to the last version e.g. "win-2019" 
- Pkgs take 15 mins to be available to nuget clients on azure devops feed even after being visible in AzureDevops. This means when you release a new version of the Soap packages, create-new-service won't use them for 15 mins.
- Azurite has been known to generate errors that are a different problem underlying. CORS is set for Azurite dev instance server-side on startup see BlobStorage.DevStorageSetup IF NOT ALREADY SET
- Errors about CORS on Azurite seem to occur if you upgrade past 12.3.0 of     "@azure/storage-blob": "12.3.0", dependency, this may be fixed in future but as of 12.9.0 is was not
- When using Jetbrains Rider [@2020.3] after upgrading a nuget package which is both directly and implicitly installed in projects. (e.g. Datastore) You need to invalidate caches/restart for it to properly display the implicit imports
- Changing the .env variables requires restarting parcel and clearing the cache, so just run srv.ps1 and it will do it for you rather than yarn serve
- When publishing the soap project itself, make sure that pwsh-bootstrap is loaded for the soap project and not another project which could happen if for example you were running test.ps1 recently. Second make sure the client app is running
  against a packaged version of soap. e.g. srv.ps1 -ToRemoteSoap. Both these problems should alert you through issues
  when publishing but its good to note them.
- Had some very wierd situation on a new machine where I was getting an error about now being able to find the local.settings.json file when starting the function app locally. Closing ALL instances of Rider and reloading seemed to fix it.
- Be careful about accidentally upgrade the Azure Functions Runtime to a version that doesn't support your current configuration. It's easy todo because upgrading the Core Tools plugin in Rider will upgrade the runtime too.
- if you get node-gyp errors while running yarn install check this page https://github.com/nodejs/node-gyp#on-windows you may need to install python and vs build tools for mspackr-extract to work
- 401 and 404 mean the same thing, bad PAT when you get an error saying it can't read the config from the service. check the local.settings.json file
- If you get an error when accessing the API about not being able to compile the config due to the wrong
version of System.Runtime dll, make sure you are not running the wrong version of Azure functions core tools.
  Reread the instructions about that above and you can check in Rider -> Settings -> Azure -> Functions