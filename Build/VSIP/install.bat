@echo off

REM Necessary to set dir when running as admin
cd %~dp0

echo Killing old stuff - Need to modify not to be user specific
del /S /Q "C:\Users\Atmoic\AppData\Local\Microsoft\VisualStudio\9.0\ProjectAssemblies\*.*"
rmdir /S /Q "C:\Users\Atmoic\AppData\Local\Microsoft\VisualStudio\9.0\ProjectAssemblies"
del /S /Q "C:\Users\Atmoic\AppData\Roaming\Cosmos User Kit\Build\VSIP\Cosmos.VS.Package.*"

echo Compiling cosmos
%windir%\Microsoft.NET\Framework\V3.5\msbuild ..\..\source\Cosmos.sln /verbosity:quiet /nologo /p:Configuration=Debug /p:Platform=x86

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
xcopy /Y ..\..\Build\Tools\Cosmos.Hardware.Plugs\Cosmos.Hardware.Plugs.* .
xcopy /Y ..\..\source2\IL2PCU\Cosmos.IL2CPU\bin\debug\Cosmos.IL2CPU.* .
xcopy /Y ..\..\source2\IL2PCU\Cosmos.IL2CPU.x86\bin\debug\Cosmos.IL2CPU.X86.* .
xcopy /Y ..\..\source\Cosmos\Cosmos.Kernel\bin\debug\Cosmos.Kernel.* .
xcopy /Y ..\..\Build\Tools\Cosmos.Kernel.Plugs\Cosmos.Kernel.Plugs.* .
rem xcopy /Y ..\..\source\Cosmos\Cosmos.Kernel.Plugs\bin\debug\Cosmos.Kernel.Plugs.* .
xcopy /Y ..\..\source\Cosmos\Cosmos.System\bin\debug\Cosmos.Sys.* .
xcopy /Y ..\..\Build\Tools\Cosmos.Sys.Plugs\Cosmos.Sys.Plugs.* .
xcopy /Y ..\..\source\Cosmos.Kernel.FileSystems\bin\debug\Cosmos.Sys.FileSystem.* .
xcopy /Y ..\..\source2\VSIP\Cosmos.VS.Package\bin\debug\Cosmos.VS.Package.* .
xcopy /Y ..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosProject.zip .

echo .
echo .
echo .
echo Creating setup.exe
REM Try one, then if not there the other for x64
IF EXIST "C:\Program Files\Inno Setup 5\ISCC.exe" (
	"C:\Program Files\Inno Setup 5\ISCC" /Q ..\..\Setup2\Cosmos.iss
) ELSE (
	"C:\Program Files (x86)\Inno Setup 5\ISCC" /Q ..\..\Setup2\Cosmos.iss
)

..\..\Setup2\Output\CosmosUserKit5.exe /SILENT

rem Relaunch VS
..\..\source\Cosmos.sln
