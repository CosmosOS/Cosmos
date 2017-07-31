cd "%USERPROFILE%\AppData\Roaming\Cosmos User Kit\Kernel\"
copy "..\source\Kernel-x86\00-CPU\Cosmos.CPU_Plugs\bin\Debug\netstandard1.5\Cosmos.CPU_Plugs.dll"
copy "..\source\Kernel-x86\00-CPU\Cosmos.CPU_Asm\bin\Debug\netstandard1.5\Cosmos.CPU_Asm.dll"
copy "..\source\Kernel-x86\80-System\Cosmos.System_Plugs\bin\Debug\netstandard1.5\Cosmos.System_Plugs.dll"

cd C:\source\Cosmos\source\Cosmos.Build.MSBuild
copy cosmos.targets "%USERPROFILE%\AppData\Roaming\Cosmos User Kit\Build\VSIP"
copy cosmos.targets "..\Build\VSIP\"
copy cosmos.targets "..\Build\VSIP\MSBuild\"
copy cosmos.targets "..\source\Cosmos.Build.MSBuild\bin\Debug\net462\win7-x86\"

@timeout 3
