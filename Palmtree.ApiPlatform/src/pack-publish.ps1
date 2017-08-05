Function Increment-Version ($projectFile, $major, $minor) {

	$xml=New-Object XML
	$xml.Load($projectFile)

	$currentVersion = [version]$xml.Project.PropertyGroup.Version
	
	$newVersionMajor = if ($major) { $major } else { $currentVersion.Major }
	$newVersionMinor = if ($minor) { $minor } else { $currentVersion.Minor }
	$newVersionBuild = if ($major -Or $minor) { 0 } else { $currentVersion.Build + 1 }
	
    $buildnumber = "$newVersionMajor.$newVersionMinor.$newVersionBuild"
    $xml.Project.PropertyGroup.Version = $buildnumber

	$xml.Save($projectFile)

    return $buildnumber
}

[string] $nugetLocalDirPath = "C:\Nuget.Local"
[string] $configuration = "Debug"
[string] $major = "keep";
[string] $minor = "keep";

if (!(Test-Path -Path $nugetLocalDirPath -PathType Container)) {
	New-Item -Path $nugetLocalDirPath -ItemType Directory
}

$packages = @(
	"Palmtree.ApiPlatform.Endpoint.Clients",
	"Palmtree.ApiPlatform.Endpoint.Http.Infrastructure",
	"Palmtree.ApiPlatform.Endpoint.Infrastructure",
	"Palmtree.ApiPlatform.Endpoint.Msmq.Infrastructure",
	"Palmtree.ApiPlatform.ThirdPartyClients.Mailgun",
	"Palmtree.ApiPlatform.DomainTests.Infrastructure",
	"Palmtree.ApiPlatform.EndpointTests.Infrastructure",
	"Palmtree.ApiPlatform.MessagesSharedWithClients"
)

$bumpversions = {
	$confirmation = Read-Host "Do you want to bump the version numbers? Responding [no] will clear your nuget cache [y/n]"
	if ($confirmation -eq 'y') {
		$script:major = Read-Host "Enter new Major Version or [Enter] to keep the same"
		$script:minor = Read-Host "Enter new Minor Version or [Enter] to keep the same"	
	} elseif ($confirmation -eq 'n') {
		nuget locals all -clear
		$script:major = $null
		$script:minor = $null
	} else {
		&$bumpversions	
	}
}
&$bumpversions

Write-Host "major: $major"
Write-Host "minor: $minor"



$publishtomyget = {
	$confirmation = Read-Host "Do you want to publish to myget? [y/n]"
	if ($confirmation -eq 'y') {
		$script:publishyn = $true;
	} elseif ($confirmation -eq 'n') {
		$script:publishyn = $false;
	} else {
		&$publishtomyget	
	}
}
&$publishtomyget


foreach ($package in $packages) {

    [string] $packageDirPath = "$pwd\$package"

	$buildnumber = Increment-Version "$packageDirPath\$package.csproj" $major $minor

    if (Test-Path -Path "$packageDirPath\bin\$configuration\*.nupkg" -PathType Leaf) {
	    # Purge old packages
	    $command = "Remove-Item -Path '$packageDirPath\bin\$configuration\*.nupkg'"
	    Write-Host "`r`nPURGE PREVIOUS $package NUGET PACKAGES --> $command`r`n"  -foregroundcolor darkgreen -backgroundcolor black
	    iex $command
    }
	
	# Pack project
	$command = "dotnet pack $packageDirPath\$package.csproj --configuration $configuration --include-symbols"
	Write-Host "`r`nPACK $package VS PROJECT --> $command`r`n"  -foregroundcolor darkgreen -backgroundcolor black
	iex $command
	
	# Copy nuget packages to local folder
	$command = "Copy-Item -Path '$packageDirPath\bin\$configuration\*.nupkg' -Destination '$nugetLocalDirPath' -Force -PassThru"
	Write-Host "`r`nCOPY $package NUGET PACKAGES TO $nugetLocalDirPath --> $command`r`n"  -foregroundcolor darkgreen -backgroundcolor black
	iex $command
    
    #publish to myget
    if ($script:publishyn) {

        $command = "nuget push $nugetLocalDirPath\$package.$buildnumber.nupkg b29d9be5-2784-44be-affe-027352486826 -Source https://www.myget.org/F/trace-one/api/v2/package"
        iex $command
        
        $command = "nuget push $nugetLocalDirPath\$package.$buildnumber.symbols.nupkg b29d9be5-2784-44be-affe-027352486826 -Source https://www.myget.org/F/trace-one/symbols/api/v2/package"
        iex $command
    }
}

Write-Host "Done."
