@REM This file is used to build the Cosmos sources.


cls

reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set PRGMDIR=%ProgramFiles%||set PRGMDIR=%ProgramFiles(x86)%

"%PRGMDIR%\MSBuild\14.0\Bin\msbuild.exe" "source\Builder.sln" /nologo /maxcpucount /p:Configuration=Debug /p:Platform=x86

start source\Cosmos.Build.Builder\bin\Debug\Cosmos.Build.Builder.exe -bat -VS2015 %1 %2 %3 %4 %5 %6 %7 %8 %9
