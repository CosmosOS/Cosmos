@echo off

REM Necessary to set dir when running as admin
cd /D %~dp0

rem Prob not needed at all anymore, can delete
rem echo Killing old stuff - Need to modify not to be user specific
rem del /S /Q "C:\Users\Atmoic\AppData\Local\Microsoft\VisualStudio\9.0\ProjectAssemblies\*.*"
rem rmdir /S /Q "C:\Users\Atmoic\AppData\Local\Microsoft\VisualStudio\9.0\ProjectAssemblies"
rem del /S /Q "C:\Users\Atmoic\AppData\Roaming\Cosmos User Kit\Build\VSIP\*.*"

echo Compiling cosmos
cd ..\Build\VSIP
set "THE_OUTPUT_PATH=%CD%"
cd "..\..\source"
%windir%\Microsoft.NET\Framework\V3.5\msbuild Cosmos.sln /maxcpucount /verbosity:normal /nologo /p:Configuration=Bootstrap /p:Platform=x86 /t:Rebuild "/p:OutputPath=%THE_OUTPUT_PATH%"
rem /t:Rebuild
cd ..\Build\VSIP\

echo Copying files
REM xcopy /Y ..\..\Source\libraries\MDbg\raw.* .
REM xcopy /Y ..\..\Source\libraries\MDbg\corapi.* .
REM xcopy /Y ..\..\source2\Build\Cosmos.Build.MSBuild\Cosmos.targets .
REM xcopy /Y ..\..\source2\Build\Cosmos.Build.Common\bin\Debug\Cosmos.Build.Common.* .
REM xcopy /Y ..\..\source2\Build\Cosmos.Build.MSBuild\bin\Debug\Cosmos.Build.MSBuild.* .
REM xcopy /Y ..\..\source2\IL2PCU\Cosmos.IL2CPU.Debug\bin\Debug\Cosmos.Compiler.Debug.* .
REM xcopy /Y ..\..\source2\debug\Cosmos.Debug.Common\bin\debug\Cosmos.Debug.Common.* .
REM xcopy /Y ..\..\source2\debug\Cosmos.Debug.HostProcess\bin\debug\Cosmos.Debug.HostProcess.* .
REM xcopy /Y ..\..\source2\debug\Cosmos.Debug.VSDebugEngine\bin\debug\Cosmos.Debug.VSDebugEngine.* .
REM xcopy /Y ..\..\source\Cosmos\Cosmos.Hardware\bin\debug\Cosmos.Hardware2.* .
REM xcopy /Y ..\..\source\Cosmos.Hardware.Plugs\bin\Debug\Cosmos.Hardware.Plugs.* .
REM xcopy /Y ..\..\source2\IL2PCU\Cosmos.IL2CPU\bin\debug\Cosmos.IL2CPU.* .
REM xcopy /Y ..\..\source2\IL2PCU\Cosmos.IL2CPU.x86\bin\debug\Cosmos.IL2CPU.X86.* .
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
xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (C#).zip" .
xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosKernel (C#).zip" .
xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\Cosmos.zip" .

echo . 
echo .
echo .
echo Creating setup.exe
REM Try one, then if not there the other for x64
IF EXIST "C:\Program Files\Inno Setup 5\ISCC.exe" (
	"C:\Program Files\Inno Setup 5\ISCC" /Q ..\..\Setup2\Cosmos.iss /dBuildConfiguration=Userkit
) ELSE (
	"C:\Program Files (x86)\Inno Setup 5\ISCC" /Q ..\..\Setup2\Cosmos.iss /dBuildConfiguration=Userkit
)
cd ..\..\Setup2
