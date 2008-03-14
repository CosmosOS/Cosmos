; Cosmos Registry key
; Install assemblies
; Delete old user kit installer and task asm

[Setup]
AppName=Cosmos User Kit
AppVerName=Cosmos User Kit 1.1
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

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Icons]
Name: "{group}\{cm:ProgramOnTheWeb,Cosmos}"; Filename: "http://www.gocosmos.org/"
Name: "{group}\{cm:UninstallProgram,Uninstall}"; Filename: "{uninstallexe}"

[Files]
Source: "..\setup\Cosmos.vsi"; DestDir: "{app}";
Source: "Tools\*"; DestDir: "{app}\Tools"; Excludes: "*.log;*.asm;output.bin"; Flags: recursesubdirs;
Source: "..\source\Cosmos.Shell.Guess\bin\Debug\*.dll"; DestDir: "{app}\Tools\GAC";
Source: "..\source\Cosmos.Shell.Guess\bin\Debug\IL2CPU.exe"; DestDir: "{app}\Tools\GAC";
Source: "..\setup\RegGac.exe"; DestDir: "{app}";

[Registry]
Root: HKCU; Subkey: "Software\Cosmos"; ValueType: string; ValueName: "Build Path"; ValueData: "{app}";
Root: HKLM; Subkey: "Software\Microsoft\.NETFramework\AssemblyFolders\Cosmos"; ValueType: string; ValueName: ""; ValueData: "{app}\Tools\GAC\";

[Run]
Filename: "{app}\Cosmos.vsi"; Verb: "open"; Flags: shellexec runascurrentuser;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Cosmos.Build.Windows.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Cosmos.Hardware.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Cosmos.Hardware.PC.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Cosmos.Kernel.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install IL2CPU.exe gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.Assembler.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.Assembler.X86.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.Assembler.X86.Win32.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.Assembler.X86.Native.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.IL.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.IL.X86.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.IL.X86.Native.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.IL.X86.Win32.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "install Indy.IL2CPU.Plugs.dll gac"; Flags: runhidden;

[UninstallRun]
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Cosmos.Build.Windows.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Cosmos.Hardware.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Cosmos.Hardware.PC.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Cosmos.Kernel.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall IL2CPU.exe gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.Assembler.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.Assembler.X86.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.Assembler.X86.Win32.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.Assembler.X86.Native.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.IL.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.IL.X86.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.IL.X86.Native.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.IL.X86.Win32.dll gac"; Flags: runhidden;
Filename: "{app}\RegGac.exe"; WorkingDir: "{app}"; Parameters: "uninstall Indy.IL2CPU.Plugs.dll gac"; Flags: runhidden;

