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
OutputDir=C:\source\Cosmos\Setup\Output
OutputBaseFilename=CosmosUserKit
Compression=lzma/ultra64
SolidCompression=yes
SourceDir=c:\source\Cosmos\Build

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Icons]
Name: "{group}\{cm:ProgramOnTheWeb,Cosmos}"; Filename: "http://www.gocosmos.org/"
Name: "{group}\{cm:UninstallProgram,Uninstall}"; Filename: "{uninstallexe}"

[Files]
Source: "Cosmos.vsi"; DestDir: "{app}";
Source: "GAC\*"; DestDir: "{app}\GAC"; Flags: recursesubdirs;
Source: "ISO\*"; DestDir: "{app}\ISO"; Excludes: "output.bin"; Flags: recursesubdirs;
Source: "PXE\*"; DestDir: "{app}\PXE"; Flags: recursesubdirs;
Source: "Tools\*"; DestDir: "{app}\Tools"; Flags: recursesubdirs;
Source: "VMWare\*"; DestDir: "{app}\VMWare"; Excludes: "*.log"; Flags: recursesubdirs;
Source: "VPC\*"; DestDir: "{app}\VPC"; Flags: recursesubdirs;

[Registry]
Root: HKLM; Subkey: "Software\Cosmos"; ValueType: string; ValueName: "Build Path"; ValueData: "{app}";

