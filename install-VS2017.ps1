#Requires -Modules VSSetup

if ([System.IO.Directory]::Exists("source\Cosmos.Build.Builder\bin\Debug"))
{
    [System.IO.Directory]::Delete("source\Cosmos.Build.Builder\bin\Debug", $true)
}

<#if (!(Get-Module VSSetup))
{
    Install-Module -Name VSSetup -Scope CurrentUser -ErrorAction Stop
}#>

Write-Host "Visual Studio 2017 instances:"
Get-VSSetupInstance -All -OutVariable Instances

if ($Instances.Count -eq 0)
{
    Write-Error "No Visual Studio 2017 instances found!" -ErrorAction Stop
}
elseif ($Instances.Count -eq 1)
{
    $CurrentInstance = $Instances[0]
}
else
{
    Write-Host "You have more than one instance of Visual Studio 2017."
    Write-Host "Instance ID of the instance to use:"
    
    $InstanceId = Read-Host

    foreach ($Instance in $Instances)
    {
        if ($Instance.InstanceId -eq $InstanceId)
        {
            $CurrentInstance = $Instance
            break;
        }
    }

    if ($CurrentInstance -eq $null)
    {
        Write-Error "There is no Visual Studio 2017 instance for the given Instance ID!" -ErrorAction Stop
    }
}

Start-Process "Build\Tools\nuget.exe" "restore", "Builder.sln" -Wait
Start-Process "dotnet" "restore", "Cosmos.sln" -Wait

$MSBuildPath = [System.IO.Path]::Combine($CurrentInstance.InstallationPath, "MSBuild", "15.0", "Bin", "MSBuild.exe")
Start-Process "$MSBuildPath" "Builder.sln", "/nologo", "/maxcpucount", '/p:Configuration="Debug"', '/p:Platform="Any CPU"' -Wait 

$BuilderArgs = [string[]]::new(3 + $args.Length)

$BuilderArgs[0] = "-bat"
$BuilderArgs[1] = "-VS2017"
$BuilderArgs[2] = "-VSINSTANCE=" + $CurrentInstance.InstanceId

for ($i = 0; $i -lt $args.Length; $i++)
{
    $BuilderArgs[$i + 3] = $args[$i]
}

Start-Process "source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe" $BuilderArgs
