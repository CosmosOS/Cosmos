@echo off

rem click on install.bat and run as admin the path in %CD% is c:\windows\system32
if "%CD%" == "%SystemRoot%\system32" goto adminClick
goto adminCalledFromACmdShellOrUnprivileged

:adminClick
REM Necessary to set dir when running as admin
cd /D %~dp0

:adminCalledFromACmdShellOrUnprivileged

rem Prob not needed at all anymore, can delete
rem echo Killing old stuff - Need to modify not to be user specific
rem del /S /Q "C:\Users\Atmoic\AppData\Local\Microsoft\VisualStudio\9.0\ProjectAssemblies\*.*"
rem rmdir /S /Q "C:\Users\Atmoic\AppData\Local\Microsoft\VisualStudio\9.0\ProjectAssemblies"
rem del /S /Q "C:\Users\Atmoic\AppData\Roaming\Cosmos User Kit\Build\VSIP\*.*"

echo Compiling cosmos
set "THE_OUTPUT_PATH=%CD%"

cd /D %~dp0
cd ..\source
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild Cosmos.sln /maxcpucount /verbosity:normal /nologo /p:Configuration=Bootstrap /p:Platform=x86 "/p:OutputPath=%THE_OUTPUT_PATH%" /t:Rebuild
cd ..\Build\VSIP\

del Cosmos.VS.Package.vsix

REM Copy templates
xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (C#).zip" .
xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosKernel (C#).zip" .
xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (F#).zip" .
xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosProject (VB).zip" .
xcopy /Y "..\..\source2\VSIP\Cosmos.VS.Package\obj\x86\Debug\CosmosKernel (VB).zip" .
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