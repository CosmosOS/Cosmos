; Do NOT change this next line in Dev Kit
#define ChangeSetVersion "20150130"

#ifndef BuildConfiguration   
; Currently we dont use "UserKit" but this allows us to test/compile from Inno
; IDE so that we don't get an undefined error.
; We default to devkit so we dont have to wait on compression.
#define BuildConfiguration "devkit"
;#define BuildConfiguration "userkit"
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
AppCopyright=Copyright © 2007-2014 The Cosmos Project
AppPublisher=Cosmos Project
AppPublisherURL=http://www.goCosmos.org/
AppSupportURL=http://www.goCosmos.org/
AppUpdatesURL=http://www.goCosmos.org/
AppVersion={#ChangeSetVersion}
DefaultDirName={userappdata}\Cosmos User Kit
DefaultGroupName=Cosmos User Kit
OutputDir=.\Setup\Output
OutputBaseFilename=CosmosUserKit-{#ChangeSetVersion}
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

#include "Code.inc"
#include "Events.inc"

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

[Dirs]
Name: {code:VSNET2013_PATH}\ProjectTemplates\Cosmos; Flags: uninsalwaysuninstall
Name: {app}; Flags: uninsalwaysuninstall

[InstallDelete]
Type: filesandordirs; Name: "{app}"
Type: files; Name: "{code:VSNET2013_PATH}\PrivateAssemblies\Cosmos.*"
Type: filesandordirs; Name: "{code:VSNET2013_PATH}\ProjectTemplates\Cosmos"
Type: filesandordirs; Name: "{userdocs}\Visual Studio 2013\Templates\ProjectTemplates\Cosmos"; 
Type: filesandordirs; Name: "{userdocs}\Visual Studio 2013\Templates\ItemTemplates\Visual C#\Cosmos";
Type: filesandordirs; Name: "{code:GetCSharpExpress2013ProjectTemplatePath}\*Cosmos*.*"; Check: IsCSharpExpress2013Installed('dummy')

[Files]
; Cecil dll, pdb
Source: ".\Resources\Dependencies\cecil\Mono.Cecil.dll"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Resources\Dependencies\cecil\Mono.Cecil.pdb"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Resources\Dependencies\cecil\Mono.Cecil.Pdb.dll"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Resources\Dependencies\cecil\Mono.Cecil.Pdb.pdb"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly

; Tools
Source: ".\Build\Tools\*.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\Tools\NAsm\*.exe"; DestDir: "{app}\Build\Tools\NAsm"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\Tools\Cygwin\*"; DestDir: "{app}\Build\Tools\cygwin"; Flags: ignoreversion uninsremovereadonly overwritereadonly
Source: ".\Build\Tools\mkisofs\*"; DestDir: "{app}\Build\Tools\mkisofs"; Flags: ignoreversion uninsremovereadonly overwritereadonly
Source: ".\Build\VSIP\*.dll"; DestDir: "{app}\Build\IL2CPU"; Flags: ignoreversion ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\*.exe"; DestDir: "{app}\Build\IL2CPU"; Flags: ignoreversion ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\*.pdb"; DestDir: "{app}\Build\IL2CPU"; Flags: ignoreversion ignoreversion uninsremovereadonly

;
Source: ".\Build\VSIP\Cosmos.Deploy.USB.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Build.Common.dll"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Resources\Dependencies\Dapper\*.*"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\System.Data.SQLite.dll"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly
;
Source: ".\Build\VSIP\Cosmos.Deploy.Pixie.exe"; DestDir: "{app}\Build\Tools"; Flags: ignoreversion uninsremovereadonly

; Kernel assemblies
Source: ".\Build\VSIP\Cosmos.Debug.Kernel.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Core.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.HAL.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.System.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Common.*"; DestDir: "{app}\Kernel"; Flags: ignoreversion uninsremovereadonly

; Icon
Source: ".\source2\Cosmos.ico"; DestDir: "{app}"; Flags: ignoreversion uninsremovereadonly

; XSharp Source
Source: ".\source\Cosmos.Debug.DebugStub\*.xs"; DestDir: "{app}\XSharp\DebugStub\"; Flags: ignoreversion uninsremovereadonly

; System.Data.SQLite library
Source: ".\Resources\Dependencies\Dapper\*.*"; DestDir: "{app}\Build\VSIP"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\System.Data.SQLite.dll"; DestDir: "{app}\Build\VSIP"; Flags: ignoreversion uninsremovereadonly
Source: ".\Resources\Dependencies\Dapper\*.*"; DestDir: "{app}\MSBuild\Cosmos"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\System.Data.SQLite.dll"; DestDir: "{app}\MSBuild\Cosmos"; Flags: ignoreversion uninsremovereadonly

; VSIP stuff
Source: ".\Build\VSIP\Cosmos.targets"; DestDir: "{pf32}\MSBuild\Cosmos"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.VS.Package.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.VS.Windows.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.VS.Wizards.*"; DestDir: "{code:VSNET2013_PATH}\PrivateAssemblies"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Debug.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Build.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.Assembler.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\XSC.exe"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\XSharp.Nasm.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\XSharp.VS.pdb"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\XSharp.VS.dll"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly

Source: ".\Build\VSIP\Cosmos.Debug.VSDebugEngine.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Vestris.VMWareLib.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Interop.VixCOM.dll"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly

; ?? If we have this do we need the others?
Source: ".\Build\VSIP\Cosmos.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\Cosmos.IL2CPU.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\corapi.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
Source: ".\Build\VSIP\raw.*"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly
;Source: ".\Resources\Dependencies\Mosa\Mosa.Utility.IsoImage.dll"; DestDir: "{app}\Build\VSIP\"; Flags: ignoreversion uninsremovereadonly

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

; Templates
; Templates use the user dir and not global. Global requires devenve /setup (items) or /installvstemplates (project) which is VERY slow and kills during devkit

; Install files into VS Integrated Shell (which includes full Visual Studio)
Source: ".\Build\VSIP\CosmosProject (C#).zip"; DestDir: "{userdocs}\Visual Studio 2013\Templates\ProjectTemplates\Cosmos"; Flags: ignoreversion uninsremovereadonly; Check: Csharp2013_Installed('dummy')
Source: ".\Build\VSIP\CosmosProject (F#).zip"; DestDir: "{userdocs}\Visual Studio 2013\Templates\ProjectTemplates\Cosmos"; Flags: ignoreversion uninsremovereadonly; Check: Fsharp2013_Installed('dummy')
Source: ".\Build\VSIP\CosmosProject (VB).zip"; DestDir: "{userdocs}\Visual Studio 2013\Templates\ProjectTemplates\Cosmos"; Flags: ignoreversion uninsremovereadonly; Check: Vb2013_Installed('dummy')
; X# File Item Template
Source: ".\Build\VSIP\XSharpFileItem.zip"; DestDir: "{userdocs}\Visual Studio 2013\Templates\ItemTemplates\Visual C#\Cosmos"; DestName: "XSharpFileItem.zip"; Flags: ignoreversion uninsremovereadonly; Check: Csharp2013_Installed('dummy')

; Install templates into Shell
Source: ".\Build\VSIP\Cosmos.zip"; DestDir: "{code:VSNET2013_PATH}\ProjectTemplates\Cosmos"; Flags: ignoreversion uninsremovereadonly; Check: IsCSharpExpress2013Installed('dummy') or IsVBExpress2013Installed('dummy')
; Install templates into C# Express
Source: ".\Build\VSIP\CosmosKernel (C#).zip"; DestDir: "{code:GetCSharpExpress2013ProjectTemplatePath}"; DestName: "CosmosKernel (C#).zip"; Flags: ignoreversion uninsremovereadonly; Check: IsCSharpExpress2013Installed('dummy')
; Install templates into VB Express
Source: ".\Build\VSIP\CosmosKernel (VB).zip"; DestDir: "{code:GetVBExpress2013ProjectTemplatePath}"; DestName: "CosmosKernel (VB).zip"; Flags: ignoreversion uninsremovereadonly; Check: IsVBExpress2013Installed('dummy')

[Registry]

; Regiter .xs Extension
Root: HKCR; Subkey: ".xs"; ValueType: string; ValueName: ""; ValueData: "XSharp"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "XSharp"; ValueType: string; ValueName: ""; ValueData: "X# source file"; Flags: uninsdeletekey
Root: HKCR; Subkey: "XSharp\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Build\Tools\XSC.exe,0"
Root: HKCR; Subkey: "XSharp\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\Build\Tools\XSC.exe"" ""%1"""

Root: HKLM; Subkey: Software\Microsoft\.NETFramework\v4.5\AssemblyFoldersEx\Cosmos; ValueType: none; Flags: uninsdeletekey
Root: HKLM; Subkey: Software\Microsoft\.NETFramework\v4.5\AssemblyFoldersEx\Cosmos; ValueType: string; ValueName: ; ValueData: {app}\Kernel\; Flags: uninsdeletevalue

; User Kit Folder
Root: HKLM; SubKey: Software\Cosmos; ValueType: string; ValueName: "UserKit"; ValueData: {app}; Flags: uninsdeletekey

; Dev Kit Folder - Set by builder only, but we delete it here. See comments in builder.
; HKCU because Builder doesn't run as admin
; Note HKCU is not part of registry redirection
Root: HKCU; SubKey: Software\Cosmos; ValueType: none; ValueName: "DevKit"; Flags: deletekey

; VSIP package
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos Visual Studio Integration Package; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos Visual Studio Integration Package; ValueType: string; ValueName: Package; ValueData: {{4cae44ed-88b9-4b7c-822b-b040f18fcee3}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos Visual Studio Integration Package; ValueType: dword; ValueName: UseInterface; ValueData: 1
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: string; ValueName: ; ValueData: Cosmos.VS.Package.VSProject, Cosmos.VS.Package, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f4d94ac959d59ec3
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: string; ValueName: InprocServer32; ValueData: {sys}\MSCOREE.DLL
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: string; ValueName: Class; ValueData: Cosmos.VS.Package.VSProject
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: string; ValueName: CodeBase; ValueData: {app}\build\vsip\cosmos.vs.package.dll
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: dword; ValueName: ID; ValueData: $000003E9
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: string; ValueName: MinEdition; ValueData: Standard
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: string; ValueName: ProductVersion; ValueData: 1.0
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: string; ValueName: ProductName; ValueData: Cosmos Visual Studio Integration Package
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}; ValueType: string; ValueName: CompanyName; ValueData: Cosmos

; Project Properties Cosmos Page configuration page
; If we use the vsinstaller - do we still need these?
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{d33a2d29-c4fd-4e12-a510-4c01a14d63e1}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{d33a2d29-c4fd-4e12-a510-4c01a14d63e1}; ValueType: string; ValueName: ; ValueData: Cosmos.VS.Package.CosmosPage
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{d33a2d29-c4fd-4e12-a510-4c01a14d63e1}; ValueType: string; ValueName: InprocServer32; ValueData: {sys}\MSCOREE.DLL
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{d33a2d29-c4fd-4e12-a510-4c01a14d63e1}; ValueType: string; ValueName: Class; ValueData: Cosmos.VS.Package.CosmosPage
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{d33a2d29-c4fd-4e12-a510-4c01a14d63e1}; ValueType: string; ValueName: CodeBase; ValueData: {app}\build\vsip\cosmos.vs.package.dll
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{d33a2d29-c4fd-4e12-a510-4c01a14d63e1}; ValueType: string; ValueName: ThreadingModel; ValueData: Both

; Old configuration pages
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{39801176-289f-405f-9425-2931a2c03912}; ValueType: none; Flags: deletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{3b72bb68-7363-45a2-9eba-55c8d5f36e36}; ValueType: none; Flags: deletekey

; project type factory
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Projects\{{471ec4bb-e47e-4229-a789-d1f5f83b52d4}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Projects\{{471ec4bb-e47e-4229-a789-d1f5f83b52d4}; ValueType: string; ValueName: ; ValueData: VSProjectFactory
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Projects\{{471ec4bb-e47e-4229-a789-d1f5f83b52d4}; ValueType: string; ValueName: DisplayName; ValueData: Cosmos
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Projects\{{471ec4bb-e47e-4229-a789-d1f5f83b52d4}; ValueType: string; ValueName: DisplayProjectFileExtensions; ValueData: "Cosmos Project Files (*.Cosmos);*.Cosmos"
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Projects\{{471ec4bb-e47e-4229-a789-d1f5f83b52d4}; ValueType: string; ValueName: Package; ValueData: {{4cae44ed-88b9-4b7c-822b-b040f18fcee3}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Projects\{{471ec4bb-e47e-4229-a789-d1f5f83b52d4}; ValueType: string; ValueName: DefaultProjectExtension; ValueData: Cosmos
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Projects\{{471ec4bb-e47e-4229-a789-d1f5f83b52d4}; ValueType: string; ValueName: PossibleProjectExtensions; ValueData: Cosmos
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Projects\{{471ec4bb-e47e-4229-a789-d1f5f83b52d4}; ValueType: string; ValueName: ProjectTemplatesDir; ValueData: {app}\build\vsip\..\Templates\Projects\CosmosProject
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Projects\{{471ec4bb-e47e-4229-a789-d1f5f83b52d4}; ValueType: string; ValueName: Language(VsTemplate); ValueData: Cosmos
;Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\NewProjectTemplates\TemplateDirs\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}\/1; ValueType: string; ValueName: ; ValueData: Cosmos
;Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\NewProjectTemplates\TemplateDirs\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}\/1; ValueType: dword; ValueName: SortPriority; ValueData: $00000064
;Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\NewProjectTemplates\TemplateDirs\{{4cae44ed-88b9-4b7c-822b-b040f18fcee3}\/1; ValueType: string; ValueName: TemplatesDir; ValueData: {app}\build\Templates\Projects\CosmosProject

; Debug engine
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\AD7Metrics\Engine\{{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\AD7Metrics\Engine\{{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}; ValueType: string; ValueName: ; ValueData: guidCosmosDebugEngine
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\AD7Metrics\Engine\{{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}; ValueType: string; ValueName: CLSID; ValueData: {{8355452D-6D2F-41B0-89B8-BB2AA2529E94}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\AD7Metrics\Engine\{{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}; ValueType: string; ValueName: ProgramProvider; ValueData: {{B4DE9307-C062-45F1-B1AF-9A5FB25402D5}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\AD7Metrics\Engine\{{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}; ValueType: string; ValueName: Name; ValueData: Cosmos Debug Engine
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\AD7Metrics\Engine\{{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}\AutoSelectIncompatibleList; ValueType: string; ValueName: guidCOMPlusNativeEng; ValueData: {{92EF0900-2251-11D2-B72E-0000F87572EF}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\AD7Metrics\Engine\{{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}\AutoSelectIncompatibleList; ValueType: string; ValueName: guidCOMPlusOnlyEng; ValueData: {{449EC4CC-30D2-4032-9256-EE18EB41B62B}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\AD7Metrics\Engine\{{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}\AutoSelectIncompatibleList; ValueType: string; ValueName: guidNativeOnlyEng; ValueData: {{3B476D35-A401-11D2-AAD4-00C04F990171}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\AD7Metrics\Engine\{{FA1DA3A6-66FF-4c65-B077-E65F7164EF83}\AutoSelectIncompatibleList; ValueType: string; ValueName: guidScriptEng; ValueData: {{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}

; Debug engine
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{8355452D-6D2F-41B0-89B8-BB2AA2529E94}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{8355452D-6D2F-41B0-89B8-BB2AA2529E94}; ValueType: string; ValueName: Assembly; ValueData: Cosmos.Debug.VSDebugEngine
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{8355452D-6D2F-41B0-89B8-BB2AA2529E94}; ValueType: string; ValueName: Class; ValueData: Cosmos.Debug.VSDebugEngine.AD7Engine
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{8355452D-6D2F-41B0-89B8-BB2AA2529E94}; ValueType: string; ValueName: InprocServer32; ValueData: {sys}\mscoree.dll
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{8355452D-6D2F-41B0-89B8-BB2AA2529E94}; ValueType: string; ValueName: CodeBase; ValueData: {app}\build\vsip\Cosmos.Debug.VSDebugEngine.dll

; X# custom tool registration
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Generators\{{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}\CosmosXSharpGenerator; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Generators\{{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}\CosmosXSharpGenerator; ValueType: string; ValueName: CLSID; ValueData: {{D6F57DE8-E50E-4C91-99E1-FA2E262BC4EA}; 
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Generators\{{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}\CosmosXSharpGenerator; ValueType: dword; ValueName: GeneratesDesignTimeSource; ValueData: 1
; X# file generator type
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{D6F57DE8-E50E-4C91-99E1-FA2E262BC4EA}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{D6F57DE8-E50E-4C91-99E1-FA2E262BC4EA}; ValueType: string; ValueName: Assembly; ValueData: XSharp.VS
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{D6F57DE8-E50E-4C91-99E1-FA2E262BC4EA}; ValueType: string; ValueName: Class; ValueData: XSharp.VS.XsToAsmFileGenerator
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{D6F57DE8-E50E-4C91-99E1-FA2E262BC4EA}; ValueType: string; ValueName: InprocServer32; ValueData: {sys}\mscoree.dll
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{D6F57DE8-E50E-4C91-99E1-FA2E262BC4EA}; ValueType: string; ValueName: CodeBase; ValueData: {app}\build\vsip\XSharp.VS.dll

; program provider
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{B4DE9307-C062-45F1-B1AF-9A5FB25402D5}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{B4DE9307-C062-45F1-B1AF-9A5FB25402D5}; ValueType: string; ValueName: Assembly; ValueData: Cosmos.Debug.VSDebugEngine
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{B4DE9307-C062-45F1-B1AF-9A5FB25402D5}; ValueType: string; ValueName: Class; ValueData: Cosmos.Debug.VSDebugEngine.AD7ProgramProvider
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{B4DE9307-C062-45F1-B1AF-9A5FB25402D5}; ValueType: string; ValueName: InprocServer32; ValueData: {sys}\mscoree.dll
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\CLSID\{{B4DE9307-C062-45F1-B1AF-9A5FB25402D5}; ValueType: string; ValueName: CodeBase; ValueData: {app}\build\vsip\Cosmos.Debug.VSDebugEngine.dll

; debug window
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_WindowsPackage; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_WindowsPackage; ValueType: string; ValueData: #110
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_WindowsPackage; ValueName: Package; ValueType: string; ValueData: {{a82b45e9-2a89-43bd-925d-c7f0edd212aa}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_WindowsPackage; ValueName: PID; ValueType: string; ValueData: 1.0
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_WindowsPackage; ValueName: ProductDetails; ValueType: string; ValueData: #112
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_WindowsPackage; ValueName: LogoID; ValueType: string; ValueData: #400
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{a82b45e9-2a89-43bd-925d-c7f0edd212aa}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{a82b45e9-2a89-43bd-925d-c7f0edd212aa}; ValueType: string; ValueData: Cosmos.VS.Windows.Cosmos_VS_WindowsPackage, Cosmos.VS.Windows, Version=1.0.0.0, Culture=neutral, PublicKeyToken=fe99f43f2598173f
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{a82b45e9-2a89-43bd-925d-c7f0edd212aa}; ValueName: InprocServer32; ValueType: string; ValueData: {sys}\MSCOREE.DLL
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{a82b45e9-2a89-43bd-925d-c7f0edd212aa}; ValueName: Class; ValueType: string; ValueData: Cosmos.VS.Windows.Cosmos_VS_WindowsPackage
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Packages\{{a82b45e9-2a89-43bd-925d-c7f0edd212aa}; ValueName: CodeBase; ValueType: string; ValueData: {app}\Build\VSIP\Cosmos.VS.Windows.dll
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\Menus; ValueName: {{a82b45e9-2a89-43bd-925d-c7f0edd212aa}; ValueType: string; ValueData: , Menus.ctmenu, 1
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{f019fb29-c2c2-4d27-9abf-739533c939be}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{f019fb29-c2c2-4d27-9abf-739533c939be}; ValueType: string; ValueData: {{a82b45e9-2a89-43bd-925d-c7f0edd212aa}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{f019fb29-c2c2-4d27-9abf-739533c939be}; ValueName: Name; ValueType: string; ValueData: Cosmos.VS.Windows.AssemblyTW
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{ce2a2d0f-0f1b-4a1f-a9ac-5a5f2a5e2c25}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{ce2a2d0f-0f1b-4a1f-a9ac-5a5f2a5e2c25}; ValueType: string; ValueData: {{a82b45e9-2a89-43bd-925d-c7f0edd212aa}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{ce2a2d0f-0f1b-4a1f-a9ac-5a5f2a5e2c25}; ValueName: Name; ValueType: string; ValueData: Cosmos.VS.Windows.RegistersTW
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{a64d0fcc-8dcc-439a-9b16-3c43128aad51}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{a64d0fcc-8dcc-439a-9b16-3c43128aad51}; ValueType: string; ValueData: {{a82b45e9-2a89-43bd-925d-c7f0edd212aa}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{a64d0fcc-8dcc-439a-9b16-3c43128aad51}; ValueName: Name; ValueType: string; ValueData: Cosmos.VS.Windows.StackTW
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{681a4da7-ba11-4c26-80a9-b39734a95b1c}; ValueType: none; Flags: uninsdeletekey
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{681a4da7-ba11-4c26-80a9-b39734a95b1c}; ValueType: string; ValueData: {{a82b45e9-2a89-43bd-925d-c7f0edd212aa}
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\ToolWindows\{{681a4da7-ba11-4c26-80a9-b39734a95b1c}; ValueName: Name; ValueType: string; ValueData: Cosmos.VS.Windows.ConsoleTW

; mark our msbuild file as safe
Root: HKLM; SubKey: Software\Microsoft\VisualStudio\12.0\MSBuild\SafeImports; ValueType: string; ValueName: Cosmos1; ValueData: {pf32}\MSBuild\Cosmos\Cosmos.targets; Flags: uninsdeletevalue

; Register .cosmos file association:
Root: HKCR; SubKey: .Cosmos; ValueType: string; ValueName: ; ValueData: Cosmos.ProjectFile; Flags: uninsdeletevalue
Root: HKCR; SubKey: Cosmos.ProjectFile; ValueType: string; ValueName: ; ValueData: Cosmos Project File; Flags: uninsdeletekey
Root: HKCR; SubKey: Cosmos.ProjectFile\DefaultIcon; ValueType: string; ValueName: ; ValueData: {app}\Cosmos.ico
Root: HKCR; SubKey: Cosmos.ProjectFile\shell\open\command; ValueType: string; ValueName: ; ValueData: """{pf32}\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe"" ""%1"""

; X# Registration:
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_XSharpPackage; ValueType: string; ValueData: #110;
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_XSharpPackage; ValueName: Package; ValueType: string; ValueData: {{e2ce86d3-fb0b-43ad-938a-5bcdd087ea2d}
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_XSharpPackage; ValueName: PID; ValueType: string; ValueData: 1.0
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_XSharpPackage; ValueName: ProductDetails; ValueType: string; ValueData: #112
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\InstalledProducts\Cosmos_VS_XSharpPackage; ValueName: LogoID; ValueType: string; ValueData: #400

Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Packages\{{e2ce86d3-fb0b-43ad-938a-5bcdd087ea2d}; ValueType: string; ValueData: Cosmos_VS_XSharpPackage
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Packages\{{e2ce86d3-fb0b-43ad-938a-5bcdd087ea2d}; ValueName: InprocServer32; ValueType: string; ValueData: {sys}\MSCOREE.DLL
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Packages\{{e2ce86d3-fb0b-43ad-938a-5bcdd087ea2d}; ValueName: Class; ValueType: string; ValueData: XSharp.VS.Cosmos_VS_XSharpPackage
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Packages\{{e2ce86d3-fb0b-43ad-938a-5bcdd087ea2d}; ValueName: CodeBase; ValueType: string; ValueData: {app}\Build\VSIP\XSharp.VS.dll

Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Languages\File Extensions\.xs; ValueType: string; ValueData: {{3fb852ed-3562-3da4-98dc-55759744328c}

Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Languages\Language Services\X#; ValueType: string; ValueData: {{3fb852ed-3562-3da4-98dc-55759744328c}
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Languages\Language Services\X#; ValueName: Package; ValueType: string; ValueData: {{e2ce86d3-fb0b-43ad-938a-5bcdd087ea2d}
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Languages\Language Services\X#; ValueName: LangResID; ValueType: dword; ValueData: $6B
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Languages\Language Services\X#; ValueName: RequestStockColors; ValueType: dword; ValueData: 1
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Languages\Language Services\X#; ValueName: EnableAsyncCompletion; ValueType: dword; ValueData: 1
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Languages\Language Services\X#; ValueName: EnableCommenting; ValueType: dword; ValueData: 0
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Languages\Language Services\X#; ValueName: CodeSense; ValueType: dword; ValueData: 0

Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Services\{{3fb852ed-3562-3da4-98dc-55759744328c}; ValueType: string; ValueData: {{e2ce86d3-fb0b-43ad-938a-5bcdd087ea2d}
Root: HKLM; Subkey: Software\Microsoft\VisualStudio\12.0\Services\{{3fb852ed-3562-3da4-98dc-55759744328c}; ValueName: Name; ValueType: string; ValueData: X# Language Service


[Run]
; Forces VS to merge the resource metadata that describes menus, toolbars, and command groups from all VSPackages available.
#if BuildConfiguration == "Devkit"
	; /setup without nosetupvstemplates takes a LONG time... so we dont run it every time.. for DevKit users, they will need to run it one time first as user kit - see new note above in X# template
	; Filename: {code:VSNET2013_PATH}\devenv.exe; Parameters: /setup Flags: waituntilterminated
	Filename: {code:VSNET2013_PATH}\devenv.exe; Parameters: /setup /nosetupvstemplates; Flags: waituntilterminated
#else 
	Filename: {code:VSNET2013_PATH}\devenv.exe; Parameters: /setup; Flags: waituntilterminated
#endif