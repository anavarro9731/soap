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

#set script vars
[string] $nugetLocalDirPath = "C:\Nuget.Local"
[string] $configuration = "Debug"
[string] $major = "keep";
[string] $minor = "keep";

$packages = @(
	[System.Tuple]::Create("Palmtree.ApiPlatform.Endpoint.Clients",$true),
	[System.Tuple]::Create("Palmtree.ApiPlatform.Endpoint.Http.Infrastructure",$true),
	[System.Tuple]::Create("Palmtree.ApiPlatform.Endpoint.Infrastructure",$true),
	[System.Tuple]::Create("Palmtree.ApiPlatform.Endpoint.Msmq.Infrastructure",$true),
	[System.Tuple]::Create("Palmtree.ApiPlatform.ThirdPartyClients.Mailgun",$true),
	[System.Tuple]::Create("Palmtree.ApiPlatform.DomainTests.Infrastructure",$true),
	[System.Tuple]::Create("Palmtree.ApiPlatform.EndpointTests.Infrastructure",$true),
	[System.Tuple]::Create("Palmtree.ApiPlatform.MessagesSharedWithClients", $true)
	[System.Tuple]::Create("Palmtree.ApiPlatform.Infrastructure",$false),
	[System.Tuple]::Create("Palmtree.ApiPlatform.MessagePipeline", $false),
	[System.Tuple]::Create("Palmtree.ApiPlatform.Utility", $false),
	[System.Tuple]::Create("Palmtree.ApiPlatform.Interfaces", $false)
)

#create local nuget directory, if not exist
if (!(Test-Path -Path $nugetLocalDirPath -PathType Container)) {
	New-Item -Path $nugetLocalDirPath -ItemType Directory
}

$bumpversions = {
	$confirmation = Read-Host "Do you want to bump the version numbers? (responding no will clear your nuget cache) [y/n]? [Enter]=yes"
	if (($confirmation -eq 'y') -or ($confirmation -eq "")) {
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

Write-Host "Major Version: $(if ($major) {$major} else {"unchanged"})" -foregroundcolor green -backgroundcolor black
Write-Host "Minor Version: $(if ($minor) {$minor} else {"unchanged"})" -foregroundcolor green -backgroundcolor black

$publishtomyget = {
	$confirmation = Read-Host "Do you want to publish to myget? [y/n] [Enter=yes]"
	if (($confirmation -eq 'y') -or ($confirmation -eq "")) {
		$script:publishyn = $true;
	} elseif ($confirmation -eq 'n') {
		$script:publishyn = $false;
	} else {
		&$publishtomyget	
	}
}

&$publishtomyget


foreach ($package in $packages) {
    
    [string] $packageDirPath = "$(Split-Path $MyInvocation.MyCommand.Path)\$($package.Item1)"
    [string] $packageName = $($package.Item1)
    [bool] $isUnlisted = $(!$package.Item2)

	$buildnumber = Increment-Version "$packageDirPath\$($packageName).csproj" $major $minor
    
    if (Test-Path -Path "$packageDirPath\bin\$configuration\*.nupkg" -PathType Leaf) {
	    # Purge old packages
	    $command = "Remove-Item -Path '$packageDirPath\bin\$configuration\*.nupkg'"
	    Write-Host "`r`nPURGE PREVIOUS $($packageName) NUGET PACKAGES --> $command`r`n"  -foregroundcolor darkgreen -backgroundcolor black
	    iex $command
    }
	
	# Pack project
	$command = "dotnet pack $packageDirPath\$packageName.csproj --configuration $configuration --include-symbols"
	Write-Host "`r`nPACK $packageName VS PROJECT --> $command`r`n"  -foregroundcolor green -backgroundcolor black
	iex $command
	
	# Copy nuget packages to local folder
	$command = "Copy-Item -Path '$packageDirPath\bin\$configuration\*.nupkg' -Destination '$nugetLocalDirPath' -Force -PassThru"
	Write-Host "`r`nCOPY $packageName NUGET PACKAGES TO $nugetLocalDirPath --> $command`r`n"  -foregroundcolor green -backgroundcolor black
	iex $command
    
    #publish to myget
    if ($script:publishyn) {

        $command = "$packageDirPath\..\..\tools\nuget push $nugetLocalDirPath\$packageName.$buildnumber.nupkg 7cde1967-fe13-4672-91ef-f1deb3543e78 -Source https://www.myget.org/F/anavarro9731/api/v2/package"
        iex $command

        if ($isUnlisted) {
            $command = "$packageDirPath\..\..\tools\nuget delete $packageName $buildNumber -Source https://www.myget.org/F/anavarro9731/api/v2/package -ApiKey 7cde1967-fe13-4672-91ef-f1deb3543e78"
            iex $command
        }
        $command = "$packageDirPath\..\..\tools\nuget push $nugetLocalDirPath\$packageName.$buildnumber.symbols.nupkg 7cde1967-fe13-4672-91ef-f1deb3543e78 -Source https://www.myget.org/F/anavarro9731/symbols/api/v2/package"
        iex $command
    }
}

Write-Host "Done."

