using namespace System.Management.Automation.Host

function Throw-IfPreviousCommandFailed {

    Param([Parameter(Mandatory = $true)]$message)

    $result = $?

    if ($result -ne $true) {

        Set-WorkingDirectoryToProjectRoot
        throw $message
    }
}

function Log-Ok {

    Write-Host "Ok." -foregroundcolor green
}

function Log {

    Param ([string] $msg)

    Write-Host $msg -foregroundcolor green
}

function Log-Step {

    Param ([string] $msg)

    Write-Host $msg -foregroundcolor cyan -backgroundcolor black
}

function Ask-YesNo {

    Param ($question)

    $confirmation = $(Write-Host "$question [y/n]?"  -foregroundcolor yellow -backgroundcolor black -NoNewLine; Read-Host; )

    while ($confirmation -ne "y" -and $confirmation -ne "n") {
    $confirmation = $(Write-Host "$question [y/n]?"  -foregroundcolor yellow -backgroundcolor black -NoNewLine; Read-Host; )
    }

    if ($confirmation -eq "y") {
    return $true
    }
    else {
    return $false
    }
}

function Set-WorkingDirectory {

    Param([Parameter(Mandatory = $true)]$folder)

    Push-Location $folder
    [Environment]::CurrentDirectory = $folder #not set by push-location
}

function Set-WorkingDirectoryToProjectRoot {

    $folder = Get-ProjectRoot
    
    Set-WorkingDirectory $folder
}

function Get-ProjectRoot {
    return $PSScriptRoot
}

function IsEmpty([string] $s)  {
    return [String]::IsNullOrWhiteSpace($s)
}

function EmptyConcat([string] $s, [string] $prompt) {
    $result = (IsEmpty $s) ? $(Read-Host -Prompt "$prompt") : $s
    return $result
}

function Get-MessageName([string] $s = $null) {
    $messageName = EmptyConcat $s 'Enter The new message name'
    if (IsEmpty $messageName) {
        Write-Host 'Message Name cannot be blank'
        Exit -1
    }
    $messageName = $messageName -replace '[^A-Za-z0-9]', ''
    return $messageName
}

function Get-QueryDataName([string] $s = $null) {
    $name = EmptyConcat $s 'Enter the name of the data that this query is retrieving'
    if (IsEmpty $name) {
        Write-Host 'Data Name cannot be blank'
        Exit -1
    }
    $name = $name -replace '[^A-Za-z0-9]', ''
    return $name
}

function Create-Menu {
    [CmdletBinding()]

    $Title = "Menu"
    $Question = "Choose Item to Create"
    $command = [ChoiceDescription]::new('&Command', 'Create Command Handler')
    $event = [ChoiceDescription]::new('&Event', 'Create Event Handler')
    $form = [ChoiceDescription]::new('&Form', 'Create Form Handlers')
    $query = [ChoiceDescription]::new('&Query', 'Create Query Handlers')

    $options = [ChoiceDescription[]]($command, $event, $form, $query)

    $result = $host.ui.PromptForChoice($Title, $Question, $options, 0)

    switch ($result) {
        0 { Create-Command }
        1 { Create-Event }
        2 { Create-Form }
        3 { Create-Query }
    }
}
function Create-Command {

    Param(
    [string] $commandName,
    [string] $eventName,
    [string] $formCommandName
    )
    
    Set-WorkingDirectoryToProjectRoot
    
    # create command object
    Log-Step "Creating Command"
    cd $messagesFolder
    cd Commands
    $nextCommandNumber =  $(ls | Select-Object -ExpandProperty "Name" | ForEach-Object { $_.Substring(1,3) } | measure -Maximum | Select-Object -ExpandProperty Maximum) + 1
    $commandName1 = Get-MessageName $commandName
    $commandName2 = 'C' + $nextCommandNumber + "v1_" + $commandName1
    $commandNamespace = $(ls | Select-Object -First 1 | Get-Content | Select-Object -First 1)
    $commandContent="
    __commandnamespace__
    {
        using Soap.PfBase.Messages;
        using Soap.Interfaces.Messages;

        public class __commandname__ : ApiCommand
        {
            public override void Validate()
            {
            }
        }
    }"
    $commandContent = $commandContent.Replace("__commandnamespace__", $commandNamespace)
    $commandContent = $commandContent.Replace("__commandname__", $commandName2)
    $commandName3 = $commandName2 + ".cs"
    Set-Content -Path $commandName3 -Value $commandContent

    #create process
    Log-Step "Creating Process"
    Set-WorkingDirectoryToProjectRoot
    cd $logicFolder
    cd Processes
    $nextProcessNumber =  $(ls | Select-Object -ExpandProperty "Name" | ForEach-Object { $_.Substring(1,3) } | measure -Maximum | Select-Object -ExpandProperty Maximum) + 1
    $processName1 = "P" + $nextProcessNumber + "_C" + $nextCommandNumber + "__Handle" + $commandName1
    
    $eventContent="
                    {
                        var queryData = await GetData();
                        
                        await PublishResponse(queryData);   
                    }
                    
                    async Task<object> GetData()
                    {
                        //* todo
                        return Task.FromResult(new object());                   
                    }
                    
                    async Task PublishResponse(object queryData)
                    {            
                        var response = new $eventName 
                        {
                            //* todo
                        };
                    
                        await Bus.Publish(
                            response,
                            new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.ReplyToWebSocketSender));                
                    }
    "
    $formDataEventContent="
                    {
                        var formData = await GetFormData();
                        
                        await PublishFormDataEvent(new $eventName(), formData);
                    }

                    async Task<$formCommandName> GetFormData() {

                        var formData = new $formCommandName
                        {
                            //* todo
                        };
                        return formData;
                    }
    "
    $standardContent="
                    {
                        await Handle__commandnumber__();    
                    }
                    
                    async Task Handle__commandnumber__()
                    {
                        //* todo
                    }
    "
            
    $processContent="
    __processnamespace__
    {
        using System;
        using System.Threading.Tasks;
        __commandnamespace__;
        __eventnamespace__
        using Soap.Interfaces;
        using Soap.PfBase.Logic.ProcessesAndOperations;
    
        public class __processname1__ : Process, IBeginProcess<__commandname2__>
        {
            public Func<__commandname2__, Task> BeginProcess =>
                async message =>
                    {                    
                        __content__       
                    };
        }
    }
    "
    $processNamespace = $(ls | Select-Object -First 1 | Get-Content | Select-Object -First 1)
    if ([string]::IsNullOrEmpty($eventName)) {
        $processContent = $processContent.Replace("__content__", $standardContent);
        $processContent = $processContent.Replace("__eventnamespace__", "");
    } else {
        if ($eventName.Contains("FormData")) {
            $processContent = $processContent.Replace("__content__", $formDataEventContent);
            
        } else {
            $processContent = $processContent.Replace("__content__", $eventContent);
        }
        $processContent = $processContent.Replace("__eventnamespace__", $commandNamespace.Replace("namespace", "using").Replace("Commands","Events") + ";")
    }
    $processContent = $processContent.Replace("__processnamespace__", $processNamespace)
    $processContent = $processContent.Replace("__commandnamespace__", $commandNamespace.Replace("namespace", "using"))
    $processContent = $processContent.Replace("__processname1__", $processName1)
    $processContent = $processContent.Replace("__commandname1__", $commandName1)
    $processContent = $processContent.Replace("__commandname2__", $commandName2)
    $processContent = $processContent.Replace("__processnumber__", $nextProcessNumber)
    $processContent = $processContent.Replace("__commandnumber__", $nextCommandNumber)
    $processName2 = $processName1 + ".cs"
    Set-Content -Path $processName2 -Value $processContent
    
    #create message functions
    Log-Step "Creating Message Functions"
    Set-WorkingDirectoryToProjectRoot
    cd $logicFolder
    cd MessageFunctions
    $functionsContent="
    __functionsnamespace__
    {
        using System.Threading.Tasks;
        __processnamespace__;
        __commandnamespace__;
        using Soap.Interfaces;
        using Soap.Interfaces.Messages;
    
        public class C__commandnumber__v1Functions : IMessageFunctionsClientSide<__commandname__>
        {
            public IContinueProcess<__commandname__>[] HandleWithTheseStatefulProcesses { get; }
    
            public Task Handle(__commandname__ msg) => this.Get<__process__>().Call(x => x.BeginProcess)(msg);
    
            public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
                this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);
    
            public void Validate(__commandname__ msg)
            {
            }
        }
    }"
    $functionsNamespace = $(ls | Select-Object -First 1 | Get-Content | Select-Object -First 1)
    $functionsContent = $functionsContent.Replace("__functionsnamespace__", $functionsNamespace)
    $functionsContent = $functionsContent.Replace("__commandnamespace__", $commandNamespace.Replace("namespace", "using"))
    $functionsContent = $functionsContent.Replace("__processnamespace__", $processNamespace.Replace("namespace", "using"))
    $functionsContent = $functionsContent.Replace("__commandname__", $commandName2)
    $functionsContent = $functionsContent.Replace("__process__", $processName1)
    $functionsContent = $functionsContent.Replace("__commandnumber__", $nextCommandNumber)
    $functionsName = 'C' + $nextCommandNumber + "v1Functions.cs"
    Set-Content -Path $functionsName -Value $functionsContent
    
    #register command functions
    Log-Step "Registering Functions"
    Set-WorkingDirectoryToProjectRoot
    cd $logicFolder
    Set-Content -Path "MessageFunctionRegistration.cs" -Value $(Get-Content -Path "MessageFunctionRegistration.cs").Replace("/* ##NEXT## */", "Register(new C" + $nextCommandNumber + "v1Functions());`r`n            /* ##NEXT## */")
    
    #print permissions to give
    Log "New Permission"
    Write-Host "nameof($commandName2),"
    
    Set-WorkingDirectoryToProjectRoot
    return $commandName2
}
function Create-Event {
    
    Param(
        [string] $eventName,
        [ValidateSet($null, $true, $false)]
        [object] $createHandlers,
        [string] $formCommandName
    )
    
    Set-WorkingDirectoryToProjectRoot

    if ($null -eq $createHandlers) {
        $createHandlers = Ask-YesNo("Create Handlers?")
    }
    
    # create event object
    Log-Step "Creating Event"
    cd $messagesFolder
    cd Events
    $nextEventNumber =  $(ls | Select-Object -ExpandProperty "Name" | ForEach-Object { $_.Substring(1,3) } | measure -Maximum | Select-Object -ExpandProperty Maximum) + 1
    $eventName1 = Get-MessageName $eventName
    $eventName2 = 'E' + $nextEventNumber + "v1_" + $eventName1
    $eventNamespace = $(ls | Select-Object -First 1 | Get-Content | Select-Object -First 1)
    $eventBaseClass = if ($eventName1.Contains("FormData")) { "UIFormDataEvent<$formCommandName>"  } else { "ApiEvent" } 
    $eventContent="
    __eventnamespace__
    {
        __commandnamespace__
        using Soap.PfBase.Messages;
        using Soap.Interfaces.Messages;

        public class __eventname__ : $eventBaseClass
        {
            public override void Validate()
            {
            }
        }
    }"
    $eventContent = $eventContent.Replace("__eventnamespace__", $eventNamespace)
    $eventContent = $eventContent.Replace("__eventname__", $eventName2)
    $eventContent = $eventContent.Replace("__commandnamespace__", $eventNamespace.Replace("namespace", "using").Replace("Events","Commands") + ";")
    $eventName3 = $eventName2 + ".cs"
    Set-Content -Path $eventName3 -Value $eventContent

    if ($createHandlers -eq $true)
    {
        #create process
        Log-Step "Creating Process"
        Set-WorkingDirectoryToProjectRoot
        cd $logicFolder
        cd Processes
        $nextProcessNumber = $( ls | Select-Object -ExpandProperty "Name" | ForEach-Object { $_.Substring(1, 3) } | measure -Maximum | Select-Object -ExpandProperty Maximum ) + 1
        $processName1 = "P" + $nextProcessNumber + "_E" + $nextEventNumber + "__Handle" + $eventName1

        $processContent = "
        __processnamespace__
        {
            using System;
            using System.Threading.Tasks;
            __eventnamespace__;
            using Soap.Interfaces;
            using Soap.PfBase.Logic.ProcessesAndOperations;
        
            public class __processname1__ : Process, IBeginProcess<__eventname2__>
            {
                public Func<__eventname2__, Task> BeginProcess =>
                    async message =>
                        {
                        await Handle__eventnumber__();
        
                        async Task Handle__eventnumber__()
                        {
                            //* todo
                        }
                        };
            }
        }
        "
        $processNamespace = $( ls | Select-Object -First 1 | Get-Content | Select-Object -First 1 )
        $processContent = $processContent.Replace("__processnamespace__", $processNamespace)
        $processContent = $processContent.Replace("__eventnamespace__",$eventNamespace.Replace("namespace", "using"))
        $processContent = $processContent.Replace("__processname1__", $processName1)
        $processContent = $processContent.Replace("__eventname1__", $eventName1)
        $processContent = $processContent.Replace("__eventname2__", $eventName2)
        $processContent = $processContent.Replace("__processnumber__", $nextProcessNumber)
        $processContent = $processContent.Replace("__eventnumber__", $nextEventNumber)
        $processName2 = $processName1 + ".cs"
        Set-Content -Path $processName2 -Value $processContent

        #create message functions
        Log-Step "Creating Message Functions"
        Set-WorkingDirectoryToProjectRoot
        cd $logicFolder
        cd MessageFunctions
        $functionsContent = "
        __functionsnamespace__
        {
            using System.Threading.Tasks;
            __processnamespace__;
            __eventnamespace__;
            using Soap.Interfaces;
            using Soap.Interfaces.Messages;
        
            public class E__eventnumber__v1Functions : IMessageFunctionsClientSide<__eventname__>
            {
                public IContinueProcess<__eventname__>[] HandleWithTheseStatefulProcesses { get; }
        
                public Task Handle(__eventname__ msg) => this.Get<__process__>().Call(x => x.BeginProcess)(msg);
        
                public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
                    this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);
        
                public void Validate(__eventname__ msg)
                {
                }
            }
        }"
        $functionsNamespace = $( ls | Select-Object -First 1 | Get-Content | Select-Object -First 1 )
        $functionsContent = $functionsContent.Replace("__functionsnamespace__", $functionsNamespace)
        $functionsContent = $functionsContent.Replace("__eventnamespace__",$eventNamespace.Replace("namespace", "using"))
        $functionsContent = $functionsContent.Replace("__processnamespace__",$processNamespace.Replace("namespace", "using"))
        $functionsContent = $functionsContent.Replace("__eventname__", $eventName2)
        $functionsContent = $functionsContent.Replace("__process__", $processName1)
        $functionsContent = $functionsContent.Replace("__eventnumber__", $nextEventNumber)
        $functionsName = 'E' + $nextEventNumber + "v1Functions.cs"
        Set-Content -Path $functionsName -Value $functionsContent

        #register event functions
        Log-Step "Registering Functions"
        Set-WorkingDirectoryToProjectRoot
        cd $logicFolder
        Set-Content -Path "MessageFunctionRegistration.cs" -Value $( Get-Content -Path "MessageFunctionRegistration.cs" ).Replace("/* ##NEXT## */", "Register(new E" + $nextEventNumber + "v1Functions());`r`n            /* ##NEXT## */")

        #print permissions to give
        Log "New Permission"
        Write-Host "nameof($eventName2),"
    }
    Set-WorkingDirectoryToProjectRoot
    return $eventName2
}
function Create-Query {
    
    Log "Creating Query"
    
    $dataName = Get-QueryDataName
    
    $eventName = Create-Event "Got$dataName" $false
    
    $ignore = Create-Command "Get$dataName" $eventName
    
    Log-Ok
}
function Create-Form {

    Log "Creating Auto Form Messages"
    
    $finalCommandName = Get-MessageName

    $finalCommandNameComplete = Create-Command $finalCommandName #* final command
    
    #* query for form data

    $dataName = "$($finalCommandNameComplete.Substring(0,4))FormData"
    
    $eventName = Create-Event "Got$dataName" $false $finalCommandNameComplete

    $ignore = Create-Command "Get$dataName" $eventName $finalCommandNameComplete
    
    Log-Ok
}

Log-Step "Initialising Environment"
# Set Project Root Folder

$logicFolder = $(ls -Path .\* -Include *.Logic -Directory -Recurse -Depth 2 -Exclude  Soap.PfBase.*)
$functionAppFolder = $(ls -Path .\* -Include *.Afs -Directory -Recurse -Depth 2 -Exclude  Soap.PfBase.*)
$messagesFolder = $(ls -Path .\* -Include *.Messages -Directory -Recurse -Depth 2 -Exclude Soap.Interfaces.*, Soap.PfBase.*)

Log "Logic: $($logicFolder | Select-Object -ExpandProperty Name)"
Log "Function App: $($functionAppFolder | Select-Object -ExpandProperty Name)"
Log "Messages:  $($messagesFolder | Select-Object -ExpandProperty Name)"

Create-Menu

