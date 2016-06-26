@REM This file is used to build the Cosmos sources.

cls
del source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.*

"Build\Tools\nuget.exe" restore "source\Cosmos.sln"

"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe" "source\Builder.sln" /nologo /maxcpucount /p:Configuration="Debug" /p:Platform="Any CPU"

start source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe -bat -VS2015 %1 %2 %3 %4 %5 %6 %7 %8 %9
