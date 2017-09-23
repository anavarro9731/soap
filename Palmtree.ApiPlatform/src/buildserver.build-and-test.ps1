<#

restore depdendent nuget packages
builds solution
runs domain tests against all test projects 

#>

Param(
    $testPackages = @(
        "Palmtree.ApiPlatform.Tests",
        "Palmtree.Sso.Api.Domain.Tests"         	         	    
    )
)

function RestoreNugetPackages {

    Log-Step "Restoring Nuget Packages..."
    
    dotnet restore
}

function BuildAllProjects {

    Log-Step "Building All Projects..." 
    #if you do not clear the duild directory each time, it's important to build before packing, otherwise you might package an older version of a dependent assembly before the dependent assembly has been rebuilt.

    dotnet build

}

function RunDomainTests {

    Param([array]$testProjects)
   
    Log-Step "Running Tests..."

    foreach($project in $testProjects) {

        WriteHost "Running Tests for $project"

        cd $project

        dotnet test --no-build #already built

        $result = $?

        if ($result -ne $true) { 
        
            throw "Test run for $project failed"             
        }

        Set-WorkingDirectory $PSScriptRoot

    }
}

#entry method
function Main {
  
    Set-WorkingDirectory $PSScriptRoot

    RestoreNugetPackages

    BuildAllProjects

    RunDomainTests $testPackages
        
    Log "Done."        
}

#go
Main
