@echo off

REM Necessary to set dir when running as admin
cd /D %~dp0

echo Compiling cosmos
set "THE_OUTPUT_PATH=%CD%"
set "ProgFiles=%ProgramFiles%
if not "[%ProgramFiles(x86)%]"=="[]" set "ProgFiles=%ProgramFiles(x86)%

cd "..\..\source"
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild Cosmos.sln /maxcpucount /verbosity:normal /nologo /p:Configuration=Bootstrap /p:Platform=x86 "/p:OutputPath=%THE_OUTPUT_PATH%"
rem /t:Rebuild
cd ..\Build\VSIP\

echo Copying files
REM xcopy /Y ..\..\Source\libraries\MDbg\raw.* .
REM xcopy /Y ..\..\Source\libraries\MDbg\corapi.* .
REM xcopy /Y ..\..\source2\Build\Cosmos.Build.MSBuild\Cosmos.targets .
REM xcopy /Y ..\..\source2\Build\Cosmos.Build.Common\bin\Debug\Cosmos.Build.Common.* .
REM xcopy /Y ..\..\source2\Build\Cosmos.Build.MSBuild\bin\Debug\Cosmos.Build.MSBuild.* .
REM xcopy /Y ..\..\source2\IL2CPU\Cosmos.IL2CPU.Debug\bin\Debug\Cosmos.Compiler.Debug.* .
REM xcopy /Y ..\..\source2\debug\Cosmos.Debug.Common\bin\debug\Cosmos.Debug.Common.* .
REM xcopy /Y ..\..\source2\debug\Cosmos.Debug.HostProcess\bin\debug\Cosmos.Debug.HostProcess.* .
REM xcopy /Y ..\..\source2\debug\Cosmos.Debug.VSDebugEngine\bin\debug\Cosmos.Debug.VSDebugEngine.* .
REM xcopy /Y ..\..\source\Cosmos\Cosmos.Hardware\bin\debug\Cosmos.Hardware2.* .
REM xcopy /Y ..\..\source\Cosmos.Hardware.Plugs\bin\Debug\Cosmos.Hardware.Plugs.* .
REM xcopy /Y ..\..\source2\IL2CPU\Cosmos.IL2CPU\bin\debug\Cosmos.IL2CPU.* .
REM xcopy /Y ..\..\source2\IL2CPU\Cosmos.IL2CPU.x86\bin\debug\Cosmos.IL2CPU.X86.* .
REM xcopy /Y ..\..\source\Cosmos\Cosmos.Kernel\bin\debug\Cosmos.Kernel.* .
REM xcopy /Y ..\..\source\Cosmos\Cosmos.Kernel.Plugs\bin\debug\Cosmos.Kernel.Plugs.* .
REM xcopy /Y ..\..\source\Cosmos\Cosmos.System\bin\debug\Cosmos.Sys.* .
REM xcopy /Y ..\..\source\Cosmos\Cosmos.Sys.Plugs\bin\Debug\Cosmos.Sys.Plugs.* .
REM xcopy /Y ..\..\source\Cosmos.Kernel.FileSystems\bin\debug\Cosmos.Sys.FileSystem.* .
REM xcopy /Y ..\..\source2\VSIP\Cosmos.VS.Package\bin\Debug\Cosmos.VS.Package.* .
REM xcopy /Y ..\..\source2\VSIP\Cosmos.VS.Wizards\bin\Debug\Cosmos.VS.Wizards.* .

REM xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosProject (VB).zip" .
REM xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosKernel (VB).zip" .
REM splitup compiler:
REM xcopy /Y ..\..\source2\Compiler\Cosmos.Compiler.Assembler\bin\debug\Cosmos.Compiler.Assembler.* .
REM xcopy /Y ..\..\source2\Compiler\Cosmos.Compiler.Assembler.X86\bin\debug\Cosmos.Compiler.Assembler.X86.* .
REM xcopy /Y ..\..\source2\Compiler\Cosmos.Compiler.DebugStub\bin\debug\Cosmos.Compiler.DebugStub.* .
REM xcopy /Y ..\..\source2\Compiler\Cosmos.Compiler.XSharp\bin\debug\Cosmos.Compiler.XSharp.* .

REM Copy templates
IF EXIST "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (C#).zip" (
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (C#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosKernel (C#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\Cosmos.zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (VB).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosKernel (VB).zip" .
) ELSE (
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosProject (C#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosKernel (C#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\Cosmos.zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosProject (VB).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosKernel (VB).zip" .
)

echo .
echo .
echo .
IF EXIST ..\..\Setup2\Output\CosmosUserKit.exe del /F ..\..\Setup2\Output\CosmosUserKit.exe

IF EXIST "%ProgFiles%\Inno Setup 5\ISCC.exe" (
	echo Creating setup.exe
	"%ProgFiles%\Inno Setup 5\ISCC" /Q ..\..\Setup2\Cosmos.iss /dBuildConfiguration=Devkit
)
pause
..\..\Setup2\Output\CosmosUserKit.exe /SILENT

rem Relaunch VS
IF EXIST "%ProgFiles%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe" (
	start "%ProgFiles%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe" ..\..\source\Cosmos.sln
)