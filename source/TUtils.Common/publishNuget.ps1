param
(
	[Parameter(Mandatory = $true,
	           HelpMessage = "project file folder."  )]
	[string] $projectFolderPath,
	[Parameter(Mandatory = $true,
	           HelpMessage = "Specifies the api key of the nuget feed e.g.: 18c9e998-f799-3d23-a197-1909c3fa43ea"  )]
	[string] $nugetApiKey,
	[Parameter(Mandatory = $true,
	           HelpMessage = "Debug, Release"  )]
	[string] $BuildConfiguration = "Debug",
	[Parameter(Mandatory = $false,
	           HelpMessage = "Specifies the url of Nexus3 server. Optional"  )]
	[string] $urlNugetFeed="https://api.nuget.org/v3/index.json"
)


if($projectFolderPath.EndsWith('\')) {
	$projectFolderPath = $projectFolderPath.Substring(0,($projectFolderPath.Length-1))
}


function CheckRequirements {
	if((Get-Host).Version.Major -le 6) {
		Write-Host -ForegroundColor Red "ERROR - install powershell 7 or higher"
		exit 982354
	}
}

#ensure $projectFolderPath\bin\tools and $projectFolderPath\bin\tools\nuget.exe
$sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$targetNugetExe = "$projectFolderPath\bin\tools\nuget.exe"
if(-Not (Test-Path -Path "$projectFolderPath\bin\tools")) {
	New-Item -ItemType directory -Path "$projectFolderPath\bin\tools" | Out-Null
}
if(-Not (Test-Path -Path $targetNugetExe)) {
	Invoke-WebRequest $sourceNugetExe -OutFile "$projectFolderPath\bin\tools\nuget.exe"
}

# set nuget alias
Set-Alias nuget $targetNugetExe -Scope Global -Verbose

# find latest *.nupkg
$outputFolder="$projectFolderPath\bin\nuget"

$nugets = Get-ChildItem "$outputFolder\*.nupkg" | Sort-Object LastAccessTime -Descending | Select-Object -First 1
if($nugets.length -eq 0) {
	Write-Host -ForegroundColor Red "no *.nupkg's in binary folder"
	exit 896825
}

$nuget = $nugets[0]
$nugetFileName = $nuget.Name
$nugetFilePathSource="$outputFolder\$nugetFileName"

Write-Host "nugetFilePathSource=$nugetFilePathSource"

# push to nuget feed
nuget push $nugetFilePathSource -apikey $nugetApiKey -source $urlNugetFeed


