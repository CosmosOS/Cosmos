; Do NOT change this next line in Dev Kit
#ifndef ChangeSetVersion
#define ChangeSetVersion "20150130"
#endif

#ifndef BuildConfiguration
; Currently we dont use "UserKit" but this allows us to test/compile from Inno
; IDE so that we don't get an undefined error.
; We default to devkit so we dont have to wait on compression.
#define BuildConfiguration "DevKit"
;#define BuildConfiguration "UserKit"
#endif

#ifndef VSVersion
  #define VSVersion "vs2017"
#endif

#if BuildConfiguration == "DevKit"
	; devkit releases are not compressed
	#pragma warning "Building Dev Kit release"
#else
	; userkit releases get compressed, and get languages included
	#pragma message "Building User Kit release"
	#define Compress true
	#define IncludeUILanguages true
#endif

[Setup]
AppId=CosmosUserKit
AppName=Cosmos User Kit
AppVerName=Cosmos User Kit v{#ChangeSetVersion}
AppCopyright=Copyright (c) 2007-2017 The Cosmos Project
AppPublisher=Cosmos Project
AppPublisherURL=http://www.goCosmos.org/
AppSupportURL=http://www.goCosmos.org/
AppUpdatesURL=http://www.goCosmos.org/
AppVersion={#ChangeSetVersion}
SetupMutex=CosmosSetupMutexName,Global\CosmoSetupMutexName
DefaultDirName={userappdata}\Cosmos User Kit
DefaultGroupName=Cosmos User Kit
OutputDir=.\Setup\Output
OutputBaseFilename=CosmosUserKit-{#ChangeSetVersion}-{#VSVersion}
#ifdef Compress
Compression=lzma2/ultra64
InternalCompressLevel=ultra64
SolidCompression=true
#else
Compression=none
InternalCompressLevel=none
#endif
SourceDir=..
;Left Image should be 164x314
WizardImageFile=.\setup\images\cosmos.bmp
;Small Image should be 55x55
WizardSmallImageFile=.\setup\images\cosmos_small.bmp
AllowCancelDuringInstall=false
UninstallLogMode=overwrite
ChangesAssociations=yes
DisableWelcomePage=True
DisableReadyPage=True
DisableReadyMemo=True
FlatComponentsList=False
AlwaysShowComponentsList=False
ShowComponentSizes=False
LicenseFile=LICENSE.txt
DisableDirPage=no

[Messages]
SelectDirDesc=If the user installing the Cosmos User Kit is not the admin. Please choose the corresponding AppData/Roaming directory.

[Dirs]
Name: {app}; Flags: uninsalwaysuninstall

[InstallDelete]
Type: filesandordirs; Name: "{app}"

[Files]
; Tools
Source: ".\Build\Tools\*.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\Tools\NAsm\*.exe"; DestDir: "{app}\Build\Tools\NAsm"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\Tools\Cygwin\*"; DestDir: "{app}\Build\Tools\cygwin"; Flags: ignoreversion uninsremovereadonly overwritereadonly
Source: ".\Build\Tools\mkisofs\*"; DestDir: "{app}\Build\Tools\mkisofs"; Flags: ignoreversion uninsremovereadonly overwritereadonly
Source: ".\Build\VSIP\IL2CPU\*"; DestDir: "{app}\Build\IL2CPU"; Flags: ignoreversion uninsremovereadonly
;Source: ".\Build\VSIP\XSharp\*"; DestDir: "{app}\Build\XSharp"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Deploy.USB.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Deploy.Pixie.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Build.Common.dll"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Debug.GDB.exe"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
; Kernel assemblies
Source: ".\Build\VSIP\Cosmos.Debug.Kernel*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Core*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.HAL*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.System*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Common*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\IL2CPU.API*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
; Packages
Source: ".\Build\VSIP\Packages\*.nupkg"; DestDir: "{app}\packages\"; Flags: ignoreversion uninsremovereadonly
; Icon
Source: ".\Artwork\Cosmos.ico"; DestDir: "{app}"; Flags: ignoreversion uninsremovereadonly
; XSharp
Source: ".\Artwork\XSharp\XSharp.ico"; DestDir: "{app}\XSharp\"; Flags: ignoreversion uninsremovereadonly
Source: "..\IL2CPU\source\Cosmos.Core.DebugStub\*.xs"; DestDir: "{app}\XSharp\DebugStub\"; Flags: ignoreversion uninsremovereadonly
; VMware
Source: ".\Build\HyperV\*"; DestDir: "{app}\Build\HyperV"; Flags: ignoreversion uninsremovereadonly overwritereadonly recursesubdirs
; VMware
Source: ".\Build\VMware\*"; DestDir: "{app}\Build\VMware"; Flags: ignoreversion uninsremovereadonly overwritereadonly recursesubdirs
; ISO
Source: ".\Build\syslinux\isolinux.bin"; DestDir: "{app}\Build\ISO\"
Source: ".\Build\syslinux\ldlinux.c32"; DestDir: "{app}\Build\ISO\"
Source: ".\Build\syslinux\libcom32.c32"; DestDir: "{app}\Build\ISO\"
Source: ".\Build\syslinux\mboot.c32"; DestDir: "{app}\Build\ISO\"
Source: ".\Build\syslinux\syslinux.cfg"; DestDir: "{app}\Build\ISO\"
; USB
Source: ".\Build\syslinux\ldlinux.c32"; DestDir: "{app}\Build\USB\"
Source: ".\Build\syslinux\libcom32.c32"; DestDir: "{app}\Build\USB\"
Source: ".\Build\syslinux\mboot.c32"; DestDir: "{app}\Build\USB\"
Source: ".\Build\syslinux\syslinux.cfg"; DestDir: "{app}\Build\USB\"
; PXE
Source: ".\Build\syslinux\pxelinux.0"; DestDir: "{app}\Build\PXE"
Source: ".\Build\syslinux\mboot.c32"; DestDir: "{app}\Build\PXE\"
Source: ".\Build\syslinux\ldlinux.c32"; DestDir: "{app}\Build\PXE\"
Source: ".\Build\syslinux\libcom32.c32"; DestDir: "{app}\Build\PXE\"
Source: ".\Build\syslinux\syslinux.cfg"; DestDir: "{app}\Build\PXE\pxelinux.cfg"; DestName: "default"
; VSIP
Source: ".\Build\Tools\VSIXBootstrapper.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.VS.DebugEngine.vsix"; DestDir: "{app}\VSIX\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.VS.ProjectSystem.vsix"; DestDir: "{app}\VSIX\"; Flags: ignoreversion uninsremovereadonly

[Registry]
; Regiter .xs Extension
Root: HKCR; Subkey: ".xs"; ValueType: string; ValueName: ""; ValueData: "XSharp"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "XSharp"; ValueType: string; ValueName: ""; ValueData: "X# source file"; Flags: uninsdeletekey
Root: HKCR; Subkey: "XSharp\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\XSharp\XSharp.ico,0"
Root: HKCR; Subkey: "XSharp\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\Build\XSharp\XSC.exe"" ""%1"""
; User Kit Folder
Root: HKLM; SubKey: Software\Cosmos; ValueType: string; ValueName: "UserKit"; ValueData: {app}; Flags: uninsdeletekey
; Dev Kit Folder - Set by builder only, but we delete it here. See comments in builder.
; HKCU because Builder doesn't run as admin
; Note HKCU is not part of registry redirection
Root: HKCU; SubKey: Software\Cosmos; ValueType: none; ValueName: "DevKit"; Flags: deletekey

[ThirdParty]
UseRelativePaths=True

[Run]
Filename: "{app}\Build\Tools\nuget.exe"; Parameters: "sources Remove -Name ""Cosmos Local Package Feed"""; WorkingDir: "{app}"; Description: "Uninstall Kernel Packages"; StatusMsg: "Uninstalling Kernel Packages"
Filename: "{app}\Build\Tools\nuget.exe"; Parameters: "sources Add -Name ""Cosmos Local Package Feed"" -Source ""{app}\packages\\"""; WorkingDir: "{app}"; Description: "Install Kernel Packages"; StatusMsg: "Installing Kernel Packages"

Filename: "{app}\Build\Tools\VSIXBootstrapper.exe"; Parameters: "/q /u:Cosmos.VS.ProjectSystem"; Description: "Remove Cosmos Project System"; StatusMsg: "Removing Visual Studio Extension: Cosmos Project System"
Filename: "{app}\Build\Tools\VSIXBootstrapper.exe"; Parameters: "/q /u:Cosmos.VS.DebugEngine"; Description: "Remove Cosmos Debug Engine"; StatusMsg: "Removing Visual Studio Extension: Cosmos Debug Engine"

Filename: "{app}\Build\Tools\VSIXBootstrapper.exe"; Parameters: "/q Cosmos.VS.DebugEngine.vsix"; WorkingDir: "{app}\VSIX\"; Description: "Install Cosmos Debug Engine"; StatusMsg: "Installing Visual Studio Extension: Cosmos Debug Engine"
Filename: "{app}\Build\Tools\VSIXBootstrapper.exe"; Parameters: "/q Cosmos.VS.ProjectSystem.vsix"; WorkingDir: "{app}\VSIX\"; Description: "Install Cosmos Project System"; StatusMsg: "Installing Visual Studio Extension: Cosmos Project System"

[UninstallRun]
Filename: "{app}\Build\Tools\nuget.exe"; Parameters: "sources Remove -Name ""Cosmos Local Package Feed"""; WorkingDir: "{app}"; StatusMsg: "Uninstalling Kernel Packages"

Filename: "{app}\Build\Tools\VSIXBootstrapper.exe"; Parameters: "/q /u:Cosmos.VS.ProjectSystem"; StatusMsg: "Removing Visual Studio Extension: Cosmos Project System"
Filename: "{app}\Build\Tools\VSIXBootstrapper.exe"; Parameters: "/q /u:Cosmos.VS.DebugEngine"; StatusMsg: "Removing Visual Studio Extension: Cosmos Debug Engine"

[Code]
function IsAppRunning(const FileName: string): Boolean;
var
  FWMIService: Variant;
  FSWbemLocator: Variant;
  FWbemObjectSet: Variant;
begin
  Result := false;
  FSWbemLocator := CreateOleObject('WBEMScripting.SWBEMLocator');
  FWMIService := FSWbemLocator.ConnectServer('', 'root\CIMV2', '', '');
  FWbemObjectSet := FWMIService.ExecQuery(Format('SELECT Name FROM Win32_Process Where Name="%s"',[FileName]));
  Result := (FWbemObjectSet.Count > 0);
  FWbemObjectSet := Unassigned;
  FWMIService := Unassigned;
  FSWbemLocator := Unassigned;
end;

function IsAppRunningWithResponse(const FileName: string): Boolean;
var
  Retry: Integer;
begin
  Result := IsAppRunning(FileName);
  if Result then
    MsgBox(FileName  + ' is running. Please close the application before running the installer.', mbError, MB_OK);
end;

function InitializeSetup: boolean;
var
  Retry: Integer;
begin
for Retry := 0 to 2 do
  begin
    Result := not IsAppRunningWithResponse('devenv.exe');
    if not Result then
    begin
      continue;
    end;
    Result := not IsAppRunningWithResponse('VSIXInstaller.exe');
    if not Result then
    begin
      continue;
    end;
    Result := not IsAppRunningWithResponse('ServiceHub.IdentityHost.exe');
    if not Result then
    begin
      continue;
    end;
    Result := not IsAppRunningWithResponse('ServiceHub.VSDetouredHost.exe');
    if not Result then
    begin
      continue;
    end;
    Result := not IsAppRunningWithResponse('ServiceHub.Host.Node.x86.exe');
    if not Result then
    begin
      continue;
    end;
    Result := not IsAppRunningWithResponse('ServiceHub.SettingsHost.exe');
    if not Result then
    begin
      continue;
    end;
    Result := not IsAppRunningWithResponse('ServiceHub.Host.CLR.x86.exe');
    if not Result then
    begin
      continue;
    end
    else
    begin
      break;
    end;
  end;
end;

function ExecWithResult(const Filename, Params, WorkingDir: String; const ShowCmd: Integer;
  const Wait: TExecWait; var ResultCode: Integer; var ResultString: AnsiString): Boolean;
var
  TempFilename: AnsiString;
  Command: String;
begin
  TempFilename :=
    ExpandConstant('{tmp}\~execwithresult.txt');
  Command :=
    Format('"%s" /S /C ""%s" %s > "%s""', [ExpandConstant('{cmd}'), Filename, Params, TempFilename]);
  Result :=
    Exec(ExpandConstant('{cmd}'), Command, WorkingDir, ShowCmd, Wait, ResultCode);
  if not Result then
    Exit;
  LoadStringFromFile(TempFilename, ResultString);
  DeleteFile(TempFilename);
  if (Length(ResultString) >= 2) and (ResultString[Length(ResultString) - 1] = #13) and (ResultString[Length(ResultString)] = #10) then
    Delete(ResultString, Length(ResultString) - 1, 2);
end;

function ExecWithoutResult(const Filename, Params, WorkingDir: String; const ShowCmd: Integer;
  const Wait: TExecWait; var ResultCode: Integer): Boolean;
var
  Command: String;
begin
  Command :=
    Format('"%s" /S /C ""%s" %s"', [ExpandConstant('{cmd}'), Filename, Params]);
  Result :=
    Exec(ExpandConstant('{cmd}'), Command, WorkingDir, ShowCmd, Wait, ResultCode);
  if not Result then
    Exit;
end;

function GetVSPath(): String;
var
  Command: AnsiString;
  Params: AnsiString;
  Success: Boolean;
  ResultCode: Integer;
  ResultText: AnsiString;
begin
  Command := ExpandConstant('{app}\Build\Tools\vswhere.exe');
  Params := '-latest -version "[15.0,16.0)" -requires Microsoft.Component.MSBuild -property installationPath';
  Success :=
    ExecWithResult(Command, Params, '', SW_HIDE, ewWaitUntilTerminated, ResultCode, ResultText)
      or (ResultCode <> 0);
  if Success then
    Result := ResultText;
end;

function GetVsixInstallerPath(): String;
var
  VsPath: String;
begin
  VsPath := GetVSPath();
  Result := VsPath + '\Common7\IDE\vsixinstaller.exe';
end;

function GetVsixInstallCommand(Param: String): String;
var
  Command: String;
begin
  Command := GetVsixInstallerPath();
  Result := Command;
end;

function GetVsixInstallParams(const Filename: String): String;
var
  Params: String;
begin
  Params := ' "' + ExpandConstant('{app}\') + Filename + '"';
  Result := Params;
end;

function GetVsixUninstallParams(const Filename: String): String;
var
  Params: String;
begin
  Params := ' /uninstall:"' + Filename + '"';
  Result := Params;
end;

function GetMSBuildDirectory(Param: String): String;
var
  VSPath: String;
begin
  VSPath := GetVSPath();
  Result := VsPath + '\MSBuild';
end;

function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

/////////////////////////////////////////////////////////////////////
// Uninstall previously installed application.
/////////////////////////////////////////////////////////////////////
function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
// Return Values:
// 1 - uninstall string is empty
// 2 - error executing the UnInstallString
// 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  // get the uninstall string of the old app
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

[Languages]
Name: en; MessagesFile: compiler:Default.isl; InfoBeforeFile: .\setup\Readme.txt
#ifdef IncludeUILanguages
Name: eu; MessagesFile: .\setup\Languages\Basque-1-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: ca; MessagesFile: .\setup\Languages\Catalan-4-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: cs; MessagesFile: .\setup\Languages\Czech-5-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: da; MessagesFile: .\setup\Languages\Danish-4-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: nl; MessagesFile: .\setup\Languages\Dutch-8-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: fi; MessagesFile: .\setup\Languages\Finnish-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: fr; MessagesFile: .\setup\Languages\French-15-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: de; MessagesFile: .\setup\Languages\German-2-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: hu; MessagesFile: .\setup\Languages\Hungarian-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: it; MessagesFile: .\setup\Languages\Italian-14-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: no; MessagesFile: .\setup\Languages\Norwegian-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: pl; MessagesFile: .\setup\Languages\Polish-8-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: pt; MessagesFile: .\setup\Languages\PortugueseStd-1-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: ru; MessagesFile: .\setup\Languages\Russian-19-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: sk; MessagesFile: .\setup\Languages\Slovak-6-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: sl; MessagesFile: .\setup\Languages\Slovenian-3-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
;Unofficial:
Name: bg; MessagesFile: .\setup\Languages\InOfficial\Bulgarian-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: el; MessagesFile: .\setup\Languages\InOfficial\Greek-4-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: is; MessagesFile: .\setup\Languages\InOfficial\Icelandic-1-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: id; MessagesFile: .\setup\Languages\InOfficial\Indonesian-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: ja; MessagesFile: .\setup\Languages\InOfficial\Japanese-5-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: ko; MessagesFile: .\setup\Languages\InOfficial\Korean-5-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: ms; MessagesFile: .\setup\Languages\InOfficial\Malaysian-2-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: es; MessagesFile: .\setup\Languages\InOfficial\SpanishStd-2-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: sv; MessagesFile: .\setup\Languages\InOfficial\Swedish-8-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: tr; MessagesFile: .\setup\Languages\InOfficial\Turkish-3-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: uk; MessagesFile: .\setup\Languages\InOfficial\Ukrainian-5-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: cn; MessagesFile: .\setup\Languages\InOfficial\ChineseSimp-11-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
Name: tw; MessagesFile: .\setup\Languages\InOfficial\ChineseTrad-2-5.1.0.isl; InfoBeforeFile: .\setup\Readme.txt
#endif

[Messages]
en.BeveledLabel=English
#ifdef IncludeUILanguages
eu.BeveledLabel=Basque
ca.BeveledLabel=Catalan
cs.BeveledLabel=Czech
da.BeveledLabel=Danish
nl.BeveledLabel=Dutch
fi.BeveledLabel=Finnish
fr.BeveledLabel=French
de.BeveledLabel=German
hu.BeveledLabel=Hungarian
it.BeveledLabel=Italian
no.BeveledLabel=Norwegian
pl.BeveledLabel=Polish
pt.BeveledLabel=Portuguese
ru.BeveledLabel=Russian
sk.BeveledLabel=Slovak
sl.BeveledLabel=Slovenian
;Unofficial:
bg.BeveledLabel=Bulgarian
el.BeveledLabel=Greek
is.BeveledLabel=Icelandic
id.BeveledLabel=Indonesian
ja.BeveledLabel=Japanese
ko.BeveledLabel=Korean
ms.BeveledLabel=Malaysian
es.BeveledLabel=Spanish
sv.BeveledLabel=Swedish
tr.BeveledLabel=Turkish
uk.BeveledLabel=Ukrainian
cn.BeveledLabel=Chinese Simplified
tw.BeveledLabel=Chinese Traditional
#endif
