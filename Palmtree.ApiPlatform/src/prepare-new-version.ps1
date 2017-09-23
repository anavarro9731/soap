<# 

This script is for versioning the package before publishing.
WARNING: It should only ever be run while on a RELEASE, HOTFIX or DEVELOP branch. It should never be run from a FEATURE branch.

It will update the version, commit the change, and push the commit to the origin.
(While no one should be developing on develop, if you don't push right away you may risk concurrency issues with other developers.
If you don't commit right away you might pollute the commit.)

See Readme - Versioning Semantics for more details
#>

Param(
    $projects = @(
        "Palmtree.ApiPlatform.ThirdPartyClients.Mailgun",

        "Palmtree.ApiPlatform.Interfaces",
	    "Palmtree.ApiPlatform.MessagePipeline",
	    "Palmtree.ApiPlatform.Utility",

        "Palmtree.ApiPlatform.DomainTests.Infrastructure",
	    "Palmtree.ApiPlatform.Endpoint.Clients",
	    "Palmtree.ApiPlatform.Endpoint.Http.Infrastructure",
	    "Palmtree.ApiPlatform.Endpoint.Infrastructure",
	    "Palmtree.ApiPlatform.Endpoint.Msmq.Infrastructure",
	    "Palmtree.ApiPlatform.EndpointTests.Infrastructure",
        "Palmtree.ApiPlatform.MessagesSharedWithClients"	         	    
    ),
    
    $vstsCredentials = @("anavarro9731", "VST`"04`"supremus")   
)

function ImportModules {
  
    Write-Host "Importing Custom Modules"

    $modules = @(
        "https://anavarro9731.visualstudio.com/defaultcollection/powershell/_apis/git/repositories/powershell/items?api-version=1.0&scopepath=build.psm1"
    )

    # Base64-encodes the Personal Access Token (PAT) appropriately
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $vstsCredentials[0],$vstsCredentials[1])))
       
    foreach ($module in $modules) {
        $moduleFilename = $module.split("=") | Select-Object -Last 1            
        Invoke-RestMethod -Uri $module -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -ContentType "text/plain; charset=UTF-8" -OutFile $moduleFilename
        Import-Module ".\$moduleFilename" -Verbose -Global -Force
        Remove-Item ".\$moduleFilename" -Verbose -Force
    }       
}

function CalcVersionAction {

	function CalcVersionActionInner {
        if (Ask-YesNo("Is this a hotfix?") -eq $true) {
            return [VersionAction]::ReleaseHotfixCommit
        }

        if (Ask-YesNo("Is this version stable and ready for public release?") -eq $true) {
            return [VersionAction]::ReleaseStableCommit
        }
        return [VersionAction]::ReleaseTestCommit
    }

    $action = CalcVersionActionInner

    return $action
}

function ModifyProjectVersions {

	Param ([array]$projects, [EditableVersion]$newVersion)
        
    foreach ($project in $projects) {

	    $xml=New-Object XML

        $scriptFolder = $PSScriptRoot

        [string] $projectFile = "$scriptFolder\$($project)\$($project).csproj"

	    $xml.Load($projectFile)
  
        $xml.Project.PropertyGroup.Version = "$newVersion"

        $xml.Save($projectFile)

    }    
}

function CalcNewVersion {
    
    Param([EditableVersion] $currentVersion)

    Log-Step "Calculating New Version..."

    $calcVersionAction = CalcVersionAction

    $newVersion = [EditableVersion]::new($currentVersion)

    if ($calcVersionAction -eq [VersionAction]::ReleaseHotfixCommit) {
        if ($currentVersion.Minor -ne 0) { throw "$currentVersion has a minor value but you are trying to release a hotfix, switch branches first" }
        $newVersion.Patch = $newVersion.Patch + 1    
    } elseif ($calcVersionAction -eq [VersionAction]::ReleaseStableCommit) {
        if ($currentVersion.Patch -ne 0) { throw "$currentVersion has a patch value but you are trying to release a stable commit, switch branches first" }
        if ($currentVersion.Minor -eq 999) { throw "$currentVersion is already stabilised but you are trying to release a stable commit, switch branches first" }
        $newVersion.Major = $newVersion.Major + 1
        $newVersion.Minor = 0
    } elseif ($calcVersionAction -eq [VersionAction]::ReleaseTestCommit) {
        if ($currentVersion.Patch -ne 0) { throw "$currentVersion has a patch value but you are trying to release a stable commit, switch branches first" }
        if ($currentVersion.Minor -eq 999) { throw "$currentVersion is already stabilised but you are trying to release a test commit, switch branches first" }
        $newVersion.Minor = $newVersion.Minor + 1
    } else {
        throw "Could not calculate new version. Cannot determine use case."
    }
    
    Log "Version $currentVersion will be changed to $newVersion"

    return $newVersion
}



#START

#entry method
function Main {

    ImportModules

    Set-WorkingDirectory $PsScriptRoot

    Git-VerifyIndexAndWorkingTreeIsClean

    $currentVersion = Verify-ProjectVersionsInSync $projects

    $newVersion = CalcNewVersion $currentVersion

    ModifyProjectVersions $projects $newVersion

    Git-Commit $newVersion

    Git-PushCurrentBranch
      
    Log "Done."
    
}

#go
Main
