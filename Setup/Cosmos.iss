; Cosmos Registry key
; Install assemblies
; Delete old user kit installer and task asm

[Setup]
AppName=Cosmos User Kit
AppVerName=Cosmos User Kit 1.1
AppCopyright=Copyright © 2007-2008 The Cosmos Project
AppPublisher=Cosmos Project
AppPublisherURL=http://www.gocosmos.org/
AppSupportURL=http://www.gocosmos.org/
AppUpdatesURL=http://www.gocosmos.org/
DefaultDirName={pf}\Cosmos User Kit
DefaultGroupName=Cosmos User Kit
OutputDir=.\Output
OutputBaseFilename=CosmosUserKit
Compression=lzma/ultra64
SolidCompression=yes
SourceDir=..\build
;Left Image should be 164x314
WizardImageFile=..\setup\images\cosmos.bmp
;Small Image should be 55x55
WizardSmallImageFile=..\setup\images\cosmos_small.bmp


; If you want all languages to be listed in the "Select Setup Language"
; dialog, even those that can't be displayed in the active code page,
; uncomment the following two lines.
[LangOptions]
LanguageCodePage=0

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: eu; MessagesFile: "..\setup\Languages\Basque-1-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: ca; MessagesFile: "..\setup\Languages\Catalan-4-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: cs; MessagesFile: "..\setup\Languages\Czech-5-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: da; MessagesFile: "..\setup\Languages\Danish-4-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: nl; MessagesFile: "..\setup\Languages\Dutch-8-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: fi; MessagesFile: "..\setup\Languages\Finnish-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: fr; MessagesFile: "..\setup\Languages\French-15-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: de; MessagesFile: "..\setup\Languages\German-2-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: hu; MessagesFile: "..\setup\Languages\Hungarian-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: it; MessagesFile: "..\setup\Languages\Italian-14-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: no; MessagesFile: "..\setup\Languages\Norwegian-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: pl; MessagesFile: "..\setup\Languages\Polish-8-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: pt; MessagesFile: "..\setup\Languages\PortugueseStd-1-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: ru; MessagesFile: "..\setup\Languages\Russian-19-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: sk; MessagesFile: "..\setup\Languages\Slovak-6-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: sl; MessagesFile: "..\setup\Languages\Slovenian-3-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
;InOffical:
Name: bg; MessagesFile: "..\setup\Languages\InOfficial\Bulgarian-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: el; MessagesFile: "..\setup\Languages\InOfficial\Greek-4-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: is; MessagesFile: "..\setup\Languages\InOfficial\Icelandic-1-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: id; MessagesFile: "..\setup\Languages\InOfficial\Indonesian-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: ja; MessagesFile: "..\setup\Languages\InOfficial\Japanese-5-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: ko; MessagesFile: "..\setup\Languages\InOfficial\Korean-5-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: ms; MessagesFile: "..\setup\Languages\InOfficial\Malaysian-2-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: es; MessagesFile: "..\setup\Languages\InOfficial\SpanishStd-2-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: sv; MessagesFile: "..\setup\Languages\InOfficial\Swedish-8-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: tr; MessagesFile: "..\setup\Languages\InOfficial\Turkish-3-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: uk; MessagesFile: "..\setup\Languages\InOfficial\Ukrainian-5-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: cn; MessagesFile: "..\setup\Languages\InOfficial\ChineseSimp-11-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"
Name: tw; MessagesFile: "..\setup\Languages\InOfficial\ChineseTrad-2-5.1.0.isl"; InfoBeforeFile: "..\setup\Readme.txt"

[Messages]
en.BeveledLabel=English
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
;InOffical:
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

[Icons]
Name: "{group}\{cm:ProgramOnTheWeb,Cosmos}"; Filename: "http://www.gocosmos.org/"
Name: "{group}\{cm:UninstallProgram,Uninstall}"; Filename: "{uninstallexe}"

[Files]
Source: "..\setup\Cosmos.vsi"; DestDir: "{app}";
Source: "Tools\*"; DestDir: "{app}\Tools"; Excludes: "*.log;*.asm;output.bin"; Flags: recursesubdirs;
Source: "..\source\Cosmos\Cosmos.Shell.Console\bin\Debug\*.dll"; DestDir: "{app}\Tools\GAC";
Source: "..\source\IL2CPU\bin\Debug\IL2CPU.exe"; DestDir: "{app}\Tools\GAC";
Source: "..\setup\RegGac.exe"; DestDir: "{app}";
Source: "..\Build\ISO\*.*"; DestDir: "{userappdata}\Cosmos User Kit\ISO"; Excludes: "output.bin";

[Registry]
Root: HKCU; Subkey: "Software\Cosmos"; ValueType: string; ValueName: "Build Path"; ValueData: "{app}";
Root: HKLM; Subkey: "Software\Microsoft\.NETFramework\AssemblyFolders\Cosmos"; ValueType: string; ValueName: ""; ValueData: "{app}\Tools\GAC\";

[Run]
Filename: "{app}\Cosmos.vsi"; Verb: "open"; Flags: shellexec runascurrentuser;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Cosmos.Build.Windows.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Cosmos.Hardware.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Cosmos.Hardware.PC.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Cosmos.Kernel.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install IL2CPU.exe gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.Assembler.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.Assembler.X86.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.Assembler.X86.Win32.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.Assembler.X86.Native.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.IL.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.IL.X86.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.IL.X86.Native.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.IL.X86.Win32.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "install Indy.IL2CPU.Plugs.dll gac"; Flags: runhidden;
Filename: "{sys}\cmd.exe"; WorkingDir: "{app}"; Parameters: "/C del temp*.bat"; Flags: runhidden;

[UninstallRun]
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Cosmos.Build.Windows.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Cosmos.Hardware.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Cosmos.Hardware.PC.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Cosmos.Kernel.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall IL2CPU.exe gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.Assembler.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.Assembler.X86.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.Assembler.X86.Win32.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.Assembler.X86.Native.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.IL.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.IL.X86.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.IL.X86.Native.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.IL.X86.Win32.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}\Tools\GAC"; Parameters: "uninstall Indy.IL2CPU.Plugs.dll gac"; Flags: runhidden;

