cd "C:\Users\zilon\AppData\Roaming\Cosmos User Kit\Kernel\"
copy "C:\source\Cosmos\source\Kernel\Cosmos.System3_Plugs\bin\Debug\netstandard1.5\Cosmos.System3_Plugs.dll"
copy "C:\source\Cosmos\source\Kernel\00-CPU\Cosmos.CPU_Plugs\bin\Debug\netstandard1.5\Cosmos.CPU_Plugs.dll"

cd C:\source\Cosmos\source\Cosmos.Build.MSBuild
copy cosmos.targets "C:\Users\zilon\AppData\Roaming\Cosmos User Kit\Build\VSIP"
copy cosmos.targets "C:\source\Cosmos\Build\VSIP\"
copy cosmos.targets "C:\source\Cosmos\Build\VSIP\MSBuild\"
copy cosmos.targets "C:\source\Cosmos\source\Cosmos.Build.MSBuild\bin\Debug\net462\win7-x86\"

@timeout 1
