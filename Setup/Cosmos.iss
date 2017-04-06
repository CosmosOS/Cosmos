; Do NOT change this next line in Dev Kit
#define ChangeSetVersion "20150130"

#ifndef BuildConfiguration
; Currently we dont use "UserKit" but this allows us to test/compile from Inno
; IDE so that we don't get an undefined error.
; We default to devkit so we dont have to wait on compression.
#define BuildConfiguration "devkit"
;#define BuildConfiguration "userkit"
#endif

#ifndef VSVersion
  #define VSVersion "vs2017"
#endif

#ifndef VSPath
  #define VSPath ""
#endif

#ifndef VSInstance
  #define VSInstance ""
#endif

#if BuildConfiguration == "Devkit"
	; devkit releases are not compressed
	#pragma warning "Building Devkit release"
#else
	; userkit releases get compressed, and get languages included
	#pragma message "Building Userkit release"
	#define Compress true
	#define IncludeUILanguages true
#endif

[Setup]
AppId=CosmosUserKit
AppName=Cosmos User Kit
AppVerName=Cosmos User Kit v{#ChangeSetVersion}
AppCopyright=Copyright � 2007-2017 The Cosmos Project
AppPublisher=Cosmos Project
AppPublisherURL=http://www.goCosmos.org/
AppSupportURL=http://www.goCosmos.org/
AppUpdatesURL=http://www.goCosmos.org/
AppVersion={#ChangeSetVersion}
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
Source: ".\Build\VSIP\NASM\*"; DestDir: "{app}\Build\NASM"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\XSharp\*"; DestDir: "{app}\Build\XSharp"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Deploy.USB.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Deploy.Pixie.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Build.Common.dll"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Assembler.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Build.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Debug.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
; Kernel assemblies
Source: ".\Build\VSIP\Cosmos.Debug.Kernel.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Core.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.HAL.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.System.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Common.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
; Icon
Source: ".\Artwork\Cosmos.ico"; DestDir: "{app}"; Flags: ignoreversion uninsremovereadonly
; XSharp Source
Source: ".\source\Cosmos.Debug.DebugStub\*.xs"; DestDir: "{app}\XSharp\DebugStub\"; Flags: ignoreversion uninsremovereadonly
; VMware
Source: ".\Build\VMware\*"; DestDir: "{app}\Build\VMware"; Flags: ignoreversion uninsremovereadonly overwritereadonly recursesubdirs
; ISO
Source: ".\Build\ISO\*"; DestDir: "{app}\Build\ISO"
Source: ".\Build\mboot.c32"; DestDir: "{app}\Build\ISO\"
Source: ".\Build\syslinux.cfg"; DestDir: "{app}\Build\ISO\"
; USB
Source: ".\Build\mboot.c32"; DestDir: "{app}\Build\USB\"
Source: ".\Build\syslinux.cfg"; DestDir: "{app}\Build\USB\"
; PXE
Source: ".\Build\PXE\*"; DestDir: "{app}\Build\PXE"
Source: ".\Build\mboot.c32"; DestDir: "{app}\Build\PXE\"
Source: ".\Build\syslinux.cfg"; DestDir: "{app}\Build\PXE\pxelinux.cfg"; DestName: "default"
; VSIP
Source: ".\Build\VSIP\Cosmos.targets"; DestDir: "{param:VSPath}\MSBuild\Cosmos"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.VS.ProjectSystem.vsix"; DestDir: "{app}"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.VS.Windows.vsix"; DestDir: "{app}"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.VS.DebugEngine.vsix"; DestDir: "{app}"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\XSharp.VS.vsix"; DestDir: "{app}"; Flags: ignoreversion uninsremovereadonly

[Registry]
; User Kit Folder
Root: HKLM; SubKey: Software\Cosmos; ValueType: string; ValueName: "UserKit"; ValueData: {app}; Flags: uninsdeletekey
; Dev Kit Folder - Set by builder only, but we delete it here. See comments in builder.
; HKCU because Builder doesn't run as admin
; Note HKCU is not part of registry redirection
Root: HKCU; SubKey: Software\Cosmos; ValueType: none; ValueName: "DevKit"; Flags: deletekey

[ThirdParty]
UseRelativePaths=True

[Run]
Filename: "{param:VSPath}\Common7\IDE\VSIXInstaller.exe"; Parameters: "/q ""{app}\Cosmos.VS.ProjectSystem.vsix"""; StatusMsg: "Installing Visual Studio Project System"
Filename: "{param:VSPath}\Common7\IDE\VSIXInstaller.exe"; Parameters: "/q ""{app}\Cosmos.VS.DebugEngine.vsix"""; StatusMsg: "Installing Visual Studio Cosmos Debug Engine"
Filename: "{param:VSPath}\Common7\IDE\VSIXInstaller.exe"; Parameters: "/q ""{app}\Cosmos.VS.Windows.vsix"""; StatusMsg: "Installing Visual Studio Cosmos Tool Windows"
Filename: "{param:VSPath}\Common7\IDE\VSIXInstaller.exe"; Parameters: "/q ""{app}\XSharp.VS.vsix"""; StatusMsg: "Installing Visual Studio X# Language Service"

[UninstallRun]
Filename: "{param:VSPath}\Common7\IDE\VSIXInstaller.exe"; Parameters: "/q /u:Cosmos.VS.ProjectSystem"; StatusMsg: "Removing Visual Studio Project System"
Filename: "{param:VSPath}\Common7\IDE\VSIXInstaller.exe"; Parameters: "/q /u:Cosmos.VS.DebugEngine"; StatusMsg: "Removing Visual Studio Cosmos Debugger"
Filename: "{param:VSPath}\Common7\IDE\VSIXInstaller.exe"; Parameters: "/q /u:Cosmos.VS.Windows"; StatusMsg: "Removing Visual Studio Cosmos Tool Windows"
Filename: "{param:VSPath}\Common7\IDE\VSIXInstaller.exe"; Parameters: "/q /u:XSharp.VS"; StatusMsg: "Removing Visual Studio X# Language Service"

[Code]
/////////////////////////////////////////////////////////////////////
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
