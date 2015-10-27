@REM These are the registry locations that contain the info we need
set FrameworkLookup=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework
set VersionLookup=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\policy\v2.0

@REM Test for if reg.exe is on this machine or not.
@set PATH=%PATH%;"%ProgramFiles%\Support Tools"
@reg.exe /? > NUL
@if not errorlevel 1 goto setvars
@echo Windows 2000 did not come with reg.exe by default.  It comes with the Support Tools.  See installation instructions here: 
@echo http://support.microsoft.com/kb/301423/
@echo Or you can download these tools from here:
@echo http://www.microsoft.com/downloads/details.aspx?FamilyID=f08d28f3-b835-4847-b810-bb6539362473
@goto cleanup

:setvars
@REM Get the Framework install directory and version.  Combine to get Version directory
@for /F "tokens=3* skip=2" %%p IN ('reg QUERY %FrameworkLookup% /v InstallRoot') DO (set FrameworkDir=%%p %%q& if "%%q"=="" set FrameworkDir=%%p)
@for /F "tokens=1 skip=2" %%p IN ('reg QUERY %VersionLookup%') DO set FrameworkVersion=%%p
set VersionDir=%FrameworkDir%v2.0.%FrameworkVersion%

@REM Get the SDK directory
@for /F "tokens=3* skip=2" %%p IN ('reg QUERY %FrameworkLookup% /v SDKInstallRootv2.0') DO (set FrameworkSDKDir=%%p %%q& if "%%q"=="" set FrameworkSDKDir=%%p)

@REM Modify the path
set PATH=%PATH%;"%VersionDir%";"%FrameworkSDKDir%\bin"

@REM UnSet temporary variables
:cleanup
@set FrameworkLookup=
@set VersionLookup=
@set FrameworkVersion=
@set FrameworkDir=
@set VersionDir=
@set FrameworkDir=
