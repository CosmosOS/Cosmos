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

#if BuildConfiguration == "Devkit"
	; devkit releases are not compressed
	#pragma warning "Building Devkit release"
#else
	; userkit releases get compressed, and get languages included
	#pragma message "Building Userkit release"
	#define Compress true
	#define IncludeUILanguages true
#endif

; Cosmos Registry key
; Install assemblies
; Delete old user kit installer and task asm

[Setup]
AppId=CosmosUserKit
AppName=Cosmos User Kit
AppVerName=Cosmos User Kit v{#ChangeSetVersion}
AppCopyright=Copyright © 2007-2015 The Cosmos Project
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

; If you want all languages to be listed in the "Select Setup Language"
; dialog, even those that can't be displayed in the active code page,
; uncomment the following two lines.
UninstallLogMode=overwrite

; we're changing file associations, so we need to set this
ChangesAssociations=yes

#include "Events.inc"
#include "Localization.inc"

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
;Source: ".\Build\VSIP\*.dll"; DestDir: "{app}\Build\IL2CPU"; Flags: ignoreversion uninsremovereadonly
;Source: ".\Build\VSIP\*.exe"; DestDir: "{app}\Build\IL2CPU"; Flags: ignoreversion uninsremovereadonly
;Source: ".\Build\VSIP\*.pdb"; DestDir: "{app}\Build\IL2CPU"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\IL2CPU\*"; DestDir: "{app}\Build\IL2CPU"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\NASM\*"; DestDir: "{app}\Build\NASM"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\XSharp\*"; DestDir: "{app}\Build\XSharp"; Flags: ignoreversion uninsremovereadonly

;
Source: ".\Build\VSIP\Cosmos.Deploy.USB.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Deploy.Pixie.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Build.Common.dll"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly

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

; VSIP stuff
;Source: ".\Build\VSIP\Cosmos.targets"; DestDir: "{pf32}\MSBuild\Cosmos"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Assembler.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Build.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Debug.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
;Source: ".\Build\VSIP\Cosmos.VS.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
;Source: ".\Build\VSIP\XSharp.Nasm.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
;Source: ".\Build\VSIP\XSharp.VS.pdb"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
;Source: ".\Build\VSIP\XSharp.VS.dll"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly


; ?? If we have this do we need the others?
;Source: ".\Build\VSIP\Cosmos.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly

; VMware
Source: ".\Build\VMware\*"; DestDir: "{app}\Build\VMware"; Flags: ignoreversion uninsremovereadonly overwritereadonly recursesubdirs
; ISO
Source: ".\Build\ISO\*"; DestDir: "{app}\Build\ISO"
Source: ".\Build\mboot.c32"; DestDir: "{app}\Build\ISO\"
Source: ".\Build\syslinux.cfg"; DestDir: "{app}\Build\ISO\"
; USB
;Source: ".\Build\USB\*"; DestDir: "{app}\Build\USB"
Source: ".\Build\mboot.c32"; DestDir: "{app}\Build\USB\"
Source: ".\Build\syslinux.cfg"; DestDir: "{app}\Build\USB\"
; PXE
Source: ".\Build\PXE\*"; DestDir: "{app}\Build\PXE"
Source: ".\Build\mboot.c32"; DestDir: "{app}\Build\PXE\"
Source: ".\Build\syslinux.cfg"; DestDir: "{app}\Build\PXE\pxelinux.cfg"; DestName: "default"

[Registry]

; Register .Cosmos file association:
Root: HKCR; SubKey: ".Cosmos"; ValueType: string; ValueName: ; ValueData: "Cosmos.ProjectFile"; Flags: uninsdeletevalue
Root: HKCR; SubKey: "Cosmos.ProjectFile"; ValueType: string; ValueName: ; ValueData: "Cosmos Project File"; Flags: uninsdeletekey
Root: HKCR; SubKey: "Cosmos.ProjectFile\DefaultIcon"; ValueType: string; ValueName: ; ValueData: "{app}\Cosmos.ico"
Root: HKCR; SubKey: "Cosmos.ProjectFile\shell\open\command"; ValueType: string; ValueName: ; ValueData: """{pf32}\Common Files\Microsoft Shared\MSEnv\VSLauncher.exe"" ""%1"""

; Regiter .xs Extension
Root: HKCR; Subkey: ".xs"; ValueType: string; ValueName: ""; ValueData: "XSharp"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "XSharp"; ValueType: string; ValueName: ""; ValueData: "X# source file"; Flags: uninsdeletekey
Root: HKCR; Subkey: "XSharp\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Build\XSharp\XSC.exe,0"
Root: HKCR; Subkey: "XSharp\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\Build\XSharp\XSC.exe"" ""%1"""

;Root: HKLM; Subkey: Software\Microsoft\.NETFramework\v4.5\AssemblyFoldersEx\Cosmos; ValueType: none; Flags: uninsdeletekey
;Root: HKLM; Subkey: Software\Microsoft\.NETFramework\v4.5\AssemblyFoldersEx\Cosmos; ValueType: string; ValueName: ; ValueData: {app}\Kernel\; Flags: uninsdeletevalue

; User Kit Folder
Root: HKLM; SubKey: Software\Cosmos; ValueType: string; ValueName: "UserKit"; ValueData: {app}; Flags: uninsdeletekey

; Dev Kit Folder - Set by builder only, but we delete it here. See comments in builder.
; HKCU because Builder doesn't run as admin
; Note HKCU is not part of registry redirection
Root: HKCU; SubKey: Software\Cosmos; ValueType: none; ValueName: "DevKit"; Flags: deletekey
