# DEV ENVIRONMENT 
~ setup time 10-20 mins

##Steps 
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
npm install -g npm 
```
### Verify Installations
```
choco list --local-only
node -v
npm -v 
```
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
See here for details (https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest) 
#### Devops key
The ado-pat and nuget-key values you will need to create a Personal Access Token(s) with proper permissions.
For ado-pat the PAT must be able to read from the source repo for the ```.version``` file and the config repo for the ```Config.cs``` file.
For nuget-key the PAT must be able to read/write from the package feed.
Both variables can use the same PAT.  

### Infrastructure
Push the new initial commit which should be waiting locally to build the Azure infrastructure.
From a POSH prompt in the repo root run:
```
git push
```
Now wait for the resource group you defined in ```.\create-new-service.psm1```

###Running Locally
Open the new Solution in Jetbrains Rider. 
Edit the local.settings.json file and update with the settings from the new cloud resources

Now you are ready for local development and can run the Azure Function Project ```YourProject.Afs``` 

### NOTES

- Pkgs take 15 mins to be available to nuget clients on azure devops feed even after being visible in AzureDevops 