@echo off

rem click on install.bat and run as admin the path in %CD% is c:\windows\system32
if "%CD%" == "%SystemRoot%\system32" goto adminClick
goto adminCalledFromACmdShellOrUnprivileged
:adminClick
REM Necessary to set dir when running as admin
cd /D %~dp0
:adminCalledFromACmdShellOrUnprivileged

echo Compiling cosmos
set THE_OUTPUT_PATH=%CD%
set ProgFiles=%ProgramFiles%
if "%PROCESSOR_ARCHITECTURE%"=="AMD64" set ProgFiles="%ProgramFiles(x86)%"

cd /D %~dp0
cd ..\..\source
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild Cosmos.sln /maxcpucount /verbosity:normal /nologo /p:Configuration=Bootstrap /p:Platform=x86 /p:OutputPath="%THE_OUTPUT_PATH%"
rem /t:Rebuild
cd ..\Build\VSIP\

echo Copying files

REM Copy templates
REM .iss does some of this as well.. why some here? And why is VB disabled in .iss?
IF EXIST "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (C#).zip" (
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (C#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosKernel (C#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (F#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\Cosmos.zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (VB).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosKernel (VB).zip" .
) ELSE (
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosProject (C#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosKernel (C#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosProject (F#).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\Cosmos.zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosProject (VB).zip" .
	xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\Debug\CosmosKernel (VB).zip" .
)

echo .
echo .
echo .

IF NOT EXIST ..\..\Setup2\Output\CosmosUserKit.exe goto afterSetupDelete
	ren ..\..\Setup2\Output\CosmosUserKit.exe tmp 2> nul
	if ERRORLEVEL 1 (
		echo Old COSMOS setup could not be removed, it is locked.
		pause
		exit /B 1
	)
	del /F ..\..\Setup2\Output\tmp
:afterSetupDelete

IF NOT EXIST "%ProgFiles%\Inno Setup 5\ISCC.exe" (
	echo Cannot find Inno Setup!
	pause
)

echo Creating setup.exe
"%ProgFiles%\Inno Setup 5\ISCC" /Q ..\..\Setup2\Cosmos.iss /dBuildConfiguration=Devkit

echo Running setup.exe
..\..\Setup2\Output\CosmosUserKit.exe /SILENT

echo Install Completed.

rem Relaunch VS
IF EXIST "%ProgFiles%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe" (
    IF "%1"=="HIVE" goto ResetHive
	goto ResetHiveAfter
:ResetHive
	echo Resetting hive keys
	"%ProgFiles%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe" /setup /rootsuffix Exp /ranu
:ResetHiveAfter
	echo Launching Visual Studio 2010
	REM - Emit thise before we actually launch VS, since the bat will stick there
	echo You can close this window now.
	"%ProgFiles%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe" ..\..\source\Cosmos.sln
)
