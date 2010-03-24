@echo off

REM Necessary to set dir when running as admin
cd %~dp0

echo Compiling cosmos

%windir%\Microsoft.NET\Framework\V3.5\msbuild ..\..\source\Cosmos.sln /verbosity:quiet /nologo

echo Copying files

xcopy /Y ..\..\Source\libraries\MDbg\raw.* .
xcopy /Y ..\..\Source\libraries\MDbg\corapi.* .
xcopy /Y ..\..\source2\Build\Cosmos.Build.MSBuild\Cosmos.targets .
xcopy /Y ..\..\source2\Build\Cosmos.Build.Common\bin\Debug\Cosmos.Build.Common.* .
xcopy /Y ..\..\source2\Build\Cosmos.Build.MSBuild\bin\Debug\Cosmos.Build.MSBuild.* .
xcopy /Y ..\..\source\IL2CPU\Cosmos.IL2CPU.Debug\bin\Debug\Cosmos.Compiler.Debug.* .
xcopy /Y ..\..\source2\debug\Cosmos.Debug.Common\bin\debug\Cosmos.Debug.Common.* .
xcopy /Y ..\..\source2\debug\Cosmos.Debug.HostProcess\bin\debug\Cosmos.Debug.HostProcess.* .
xcopy /Y ..\..\source2\debug\Cosmos.Debug.VSDebugEngine\bin\debug\Cosmos.Debug.VSDebugEngine.* .
xcopy /Y ..\..\source\Cosmos\Cosmos.Hardware\bin\debug\Cosmos.Hardware.* .
xcopy /Y ..\..\source\Cosmos.Hardware.Plugs\bin\x86\debug\Cosmos.Hardware.Plugs.* .
xcopy /Y ..\..\source2\IL2PCU\Cosmos.IL2CPU\bin\debug\Cosmos.IL2CPU.* .
xcopy /Y ..\..\source2\IL2PCU\Cosmos.IL2CPU.x86\bin\debug\Cosmos.IL2CPU.X86.* .
xcopy /Y ..\..\source\Cosmos\Cosmos.Kernel\bin\debug\Cosmos.Kernel.* .
xcopy /Y ..\..\source\Cosmos\Cosmos.Kernel.Plugs\bin\x86\debug\Cosmos.Kernel.Plugs.* .
xcopy /Y ..\..\source\Cosmos\Cosmos.System\bin\debug\Cosmos.Sys.* .
xcopy /Y ..\..\source\Cosmos\Cosmos.Sys.Plugs\bin\x86\debug\Cosmos.Sys.Plugs.* .
xcopy /Y ..\..\source\Cosmos.Kernel.FileSystems\bin\debug\Cosmos.Sys.FileSystem.* .
xcopy /Y ..\..\source2\VSIP\Cosmos.VS.Package\bin\debug\Cosmos.VS.Package.* .
xcopy /Y ..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosProject.zip .





rem echo Create msbuild file
rem md "%ProgramFiles%\MSBuild\Cosmos\"
rem xcopy /Y .\cosmos.targets "%ProgramFiles%\MSBuild\Cosmos\"
pause