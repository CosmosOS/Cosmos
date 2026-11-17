; Cosmos OS Development Kit - Gen3 Installer
; Inno Setup Script for Windows
; Requires Inno Setup 6.4+ (ExecAndCaptureOutput, CopyFile)

#define MyAppName "Cosmos OS Development Kit"
; Version comes from the COSMOS_VERSION environment variable. `cosmos install`
; sets this from the detected Cosmos.Sdk package version (see
; src/Cosmos.Tools/Commands/InstallCommand.cs). When invoking ISCC.exe directly,
; export COSMOS_VERSION first.
#define MyAppVersion GetEnv('COSMOS_VERSION')
#if MyAppVersion == ""
  #error "COSMOS_VERSION env var must be set when invoking ISCC (typically via 'cosmos install')"
#endif
#define MyAppPublisher "Cosmos Project"
#define MyAppURL "https://github.com/CosmosOS/Cosmos"

; IDE extensions. The ids and the Visual Studio range follow each extension's
; manifest: CosmosOS/CosmosVsCodeExtension, CosmosOS/CosmosVsExtension
; (source.extension.vsixmanifest) and CosmosOS/CosmosRiderExtension (plugin.xml).
#define VSCodeExtensionId "cosmosos.cosmos-vscode"
#define VisualStudioExtensionId "Cosmos.VisualStudio.77e2f7e6-2f56-45de-9e83-82ec60bed79b"
#define VisualStudioVersions "[17.14,19.0)"
#define RiderPluginDir "cosmos-rider-plugin"

[Setup]
AppId={{E5B3A550-47DB-4E3C-B714-C6D01F1E9F3C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
DefaultDirName={localappdata}\Cosmos
DefaultGroupName=Cosmos
DisableProgramGroupPage=no
OutputDir=output
OutputBaseFilename=CosmosSetup-{#MyAppVersion}-windows
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardImageFile=images\cosmos.bmp
WizardSmallImageFile=images\cosmos_small.bmp
SetupIconFile=images\Cosmos.ico
UninstallDisplayIcon={app}\Cosmos.ico
LicenseFile=..\LICENSE
Uninstallable=yes
UninstallDisplayName={#MyAppName}
ChangesEnvironment=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
; Each task shows only when its IDE was found (see InitializeSetup)
Name: "vscode"; Description: "Visual Studio Code"; GroupDescription: "Install the Cosmos extension for:"; Check: VSCodeInstalled
Name: "visualstudio"; Description: "Visual Studio (asks for administrator rights)"; GroupDescription: "Install the Cosmos extension for:"; Check: VisualStudioInstalled
Name: "rider"; Description: "JetBrains Rider"; GroupDescription: "Install the Cosmos extension for:"; Check: RiderInstalled

[InstallDelete]
; Drop the extensions an earlier version bundled, so only the new ones get installed
Type: filesandordirs; Name: "{app}\Extensions"

[Files]
; Icon
Source: "images\Cosmos.ico"; DestDir: "{app}"; Flags: ignoreversion

; NuGet packages
Source: "bundle\packages\*.nupkg"; DestDir: "{app}\Packages"; Flags: ignoreversion

; LLVM toolchain (clang + lld + freestanding headers — handles both x64 and ARM64)
Source: "bundle\tools\windows\llvm-tools\*"; DestDir: "{app}\Tools\llvm-tools"; Flags: ignoreversion recursesubdirs createallsubdirs

; Build tools
Source: "bundle\tools\windows\xorriso\*"; DestDir: "{app}\Tools\xorriso"; Flags: ignoreversion recursesubdirs createallsubdirs

; QEMU emulator (x64 and ARM64)
Source: "bundle\tools\windows\qemu\*"; DestDir: "{app}\Tools\qemu"; Flags: ignoreversion recursesubdirs createallsubdirs

; GDB multiarch debugger
Source: "bundle\tools\windows\gdb\*"; DestDir: "{app}\Tools\gdb"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist

; IDE extensions
Source: "bundle\extensions\vscode\*.vsix"; DestDir: "{app}\Extensions\VSCode"; Flags: ignoreversion skipifsourcedoesntexist
Source: "bundle\extensions\visualstudio\*.vsix"; DestDir: "{app}\Extensions\VisualStudio"; Flags: ignoreversion skipifsourcedoesntexist
Source: "bundle\extensions\rider\*"; DestDir: "{app}\Extensions\Rider"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist

[Icons]
Name: "{group}\Uninstall Cosmos"; Filename: "{uninstallexe}"

[Registry]
Root: HKCU; Subkey: "Software\Cosmos"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
; Add tool paths to user PATH via registry
Root: HKCU; Subkey: "Software\Cosmos"; ValueType: string; ValueName: "ToolsPath"; ValueData: "{app}\Tools"; Flags: uninsdeletekey

[Run]
; Register local NuGet feed for offline package restore
StatusMsg: "Registering Cosmos NuGet feed..."; \
  Filename: "dotnet"; \
  Parameters: "nuget add source ""{app}\Packages"" --name ""Cosmos Local Feed"""; \
  Flags: runhidden waituntilterminated; \
  Check: DotNetInstalled

; Uninstall previous Cosmos.Patcher (if any) then install new version
StatusMsg: "Updating Cosmos Patcher..."; \
  Filename: "dotnet"; \
  Parameters: "tool uninstall -g Cosmos.Patcher"; \
  Flags: runhidden waituntilterminated; \
  Check: DotNetInstalled
StatusMsg: "Installing Cosmos Patcher..."; \
  Filename: "dotnet"; \
  Parameters: "tool install -g Cosmos.Patcher --add-source ""{app}\Packages"""; \
  Flags: runhidden waituntilterminated; \
  Check: DotNetInstalled

; Uninstall previous Cosmos.Tools (if any) then install new version
StatusMsg: "Updating Cosmos Tools CLI..."; \
  Filename: "dotnet"; \
  Parameters: "tool uninstall -g Cosmos.Tools"; \
  Flags: runhidden waituntilterminated; \
  Check: DotNetInstalled
StatusMsg: "Installing Cosmos Tools CLI..."; \
  Filename: "dotnet"; \
  Parameters: "tool install -g Cosmos.Tools --add-source ""{app}\Packages"""; \
  Flags: runhidden waituntilterminated; \
  Check: DotNetInstalled

; Uninstall previous templates (if any) then install new version
StatusMsg: "Updating Cosmos project templates..."; \
  Filename: "dotnet"; \
  Parameters: "new uninstall Cosmos.Build.Templates"; \
  Flags: runhidden waituntilterminated; \
  Check: DotNetInstalled
StatusMsg: "Installing Cosmos project templates..."; \
  Filename: "dotnet"; \
  Parameters: "new install Cosmos.Build.Templates --add-source ""{app}\Packages"""; \
  Flags: runhidden waituntilterminated; \
  Check: DotNetInstalled

; Install VS Code extension (Visual Studio and Rider install in CurStepChanged)
StatusMsg: "Installing VS Code extension..."; \
  Filename: "cmd"; \
  Parameters: "/c for %f in (""{app}\Extensions\VSCode\*.vsix"") do code --install-extension ""%f"" --force"; \
  Flags: runhidden waituntilterminated; \
  Tasks: vscode

[UninstallRun]
Filename: "dotnet"; Parameters: "nuget remove source ""Cosmos Local Feed"""; Flags: runhidden waituntilterminated; RunOnceId: "RemoveNuGetFeed"
Filename: "dotnet"; Parameters: "tool uninstall -g Cosmos.Patcher"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallPatcher"
Filename: "dotnet"; Parameters: "tool uninstall -g Cosmos.Tools"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallTools"
Filename: "dotnet"; Parameters: "new uninstall Cosmos.Build.Templates"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallTemplates"
Filename: "cmd"; Parameters: "/c code --uninstall-extension {#VSCodeExtensionId}"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallVSCodeExt"

[Code]
const
  SMTO_ABORTIFHUNG = 2;
  WM_SETTINGCHANGE = $001A;

var
  VSCodeFound: Boolean;
  { Comma-separated ids of the Visual Studio instances found, and the VSIXInstaller of the newest }
  VisualStudioIds: string;
  VSIXInstallerPath: string;
  { Config folders (%APPDATA%\JetBrains\Rider<version>) of the Rider versions the plugin supports }
  RiderConfigDirs: TArrayOfString;

function SendMessageTimeoutW(hWnd: LongInt; Msg: LongInt; wParam: LongInt; lParam: string; fuFlags: LongInt; uTimeout: LongInt; var lpdwResult: LongInt): LongInt;
  external 'SendMessageTimeoutW@user32.dll stdcall';

procedure BroadcastEnvironmentChange;
var
  Dummy: LongInt;
begin
  { Notify all windows that environment variables have changed }
  SendMessageTimeoutW($FFFF {HWND_BROADCAST}, WM_SETTINGCHANGE, 0, 'Environment', SMTO_ABORTIFHUNG, 5000, Dummy);
end;

function DotNetInstalled: Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('dotnet', '--version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function VSCodeInstalled: Boolean;
begin
  Result := VSCodeFound;
end;

function VisualStudioInstalled: Boolean;
begin
  Result := (VisualStudioIds <> '') and (VSIXInstallerPath <> '');
end;

function RiderInstalled: Boolean;
begin
  Result := GetArrayLength(RiderConfigDirs) > 0;
end;

procedure FindVSCode;
var
  ResultCode: Integer;
begin
  VSCodeFound := Exec('cmd', '/c code --version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

procedure FindVisualStudio;
var
  Output: TExecOutput;
  ResultCode, I: Integer;
  Line, Installer: string;
begin
  VisualStudioIds := '';
  VSIXInstallerPath := '';
  { -sort lists the newest instance first; the text format gives one "name: value" line per property }
  if not ExecAndCaptureOutput(ExpandConstant('{commonpf32}\Microsoft Visual Studio\Installer\vswhere.exe'),
    '-nologo -sort -prerelease -version "{#VisualStudioVersions}"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode, Output) or (ResultCode <> 0) then
    Exit;
  for I := 0 to GetArrayLength(Output.StdOut) - 1 do
  begin
    Line := Output.StdOut[I];
    if Pos('instanceId: ', Line) = 1 then
    begin
      if VisualStudioIds <> '' then
        VisualStudioIds := VisualStudioIds + ',';
      VisualStudioIds := VisualStudioIds + Copy(Line, Length('instanceId: ') + 1, MaxInt);
    end
    else if (VSIXInstallerPath = '') and (Pos('installationPath: ', Line) = 1) then
    begin
      Installer := Copy(Line, Length('installationPath: ') + 1, MaxInt) + '\Common7\IDE\VSIXInstaller.exe';
      if FileExists(Installer) then
        VSIXInstallerPath := Installer;
    end;
  end;
end;

{ Name is a Rider config folder such as Rider2025.3; the plugin's sinceBuild 253 is 2025.3 }
function RiderVersionSupported(Name: string): Boolean;
var
  Dot, Year, Minor: Integer;
begin
  Result := False;
  if Pos('Rider', Name) <> 1 then
    Exit;
  Delete(Name, 1, Length('Rider'));
  Dot := Pos('.', Name);
  if Dot = 0 then
    Exit;
  Year := StrToIntDef(Copy(Name, 1, Dot - 1), 0);
  Minor := StrToIntDef(Copy(Name, Dot + 1, MaxInt), -1);
  Result := (Minor >= 0) and ((Year > 2025) or ((Year = 2025) and (Minor >= 3)));
end;

{ Rider creates its config folder on first launch, so a Rider never started is not found }
procedure FindRider;
var
  FindRec: TFindRec;
  Root: string;
  Count: Integer;
begin
  SetArrayLength(RiderConfigDirs, 0);
  Root := ExpandConstant('{userappdata}\JetBrains\');
  if FindFirst(Root + 'Rider*', FindRec) then
  try
    repeat
      if ((FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0) and RiderVersionSupported(FindRec.Name) then
      begin
        Count := GetArrayLength(RiderConfigDirs);
        SetArrayLength(RiderConfigDirs, Count + 1);
        RiderConfigDirs[Count] := Root + FindRec.Name;
      end;
    until not FindNext(FindRec);
  finally
    FindClose(FindRec);
  end;
end;

{ Returns the first file matching Pattern, or '' }
function FindBundledFile(Pattern: string): string;
var
  FindRec: TFindRec;
begin
  Result := '';
  if FindFirst(Pattern, FindRec) then
  begin
    Result := ExtractFilePath(Pattern) + FindRec.Name;
    FindClose(FindRec);
  end;
end;

procedure CopyTree(const Source, Dest: string);
var
  FindRec: TFindRec;
begin
  ForceDirectories(Dest);
  if FindFirst(Source + '\*', FindRec) then
  try
    repeat
      if (FindRec.Name <> '.') and (FindRec.Name <> '..') then
      begin
        if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
          CopyTree(Source + '\' + FindRec.Name, Dest + '\' + FindRec.Name)
        else
          CopyFile(Source + '\' + FindRec.Name, Dest + '\' + FindRec.Name, False);
      end;
    until not FindNext(FindRec);
  finally
    FindClose(FindRec);
  end;
end;

function VisualStudioRunning: Boolean;
var
  Output: TExecOutput;
  ResultCode, I: Integer;
begin
  Result := False;
  if not ExecAndCaptureOutput('tasklist', '/FI "IMAGENAME eq devenv.exe" /FO CSV /NH', '', SW_HIDE, ewWaitUntilTerminated, ResultCode, Output) then
    Exit;
  for I := 0 to GetArrayLength(Output.StdOut) - 1 do
    if Pos('"devenv.exe"', Lowercase(Output.StdOut[I])) = 1 then
      Result := True;
end;

{ VSIXInstaller cannot change the extensions of a running Visual Studio }
function WaitForVisualStudioToClose(const Action: string): Boolean;
begin
  Result := True;
  while VisualStudioRunning do
    if SuppressibleMsgBox('Close Visual Studio to ' + Action + ' the Cosmos extension, then click Retry.',
      mbInformation, MB_RETRYCANCEL, IDCANCEL) = IDCANCEL then
    begin
      Result := False;
      Exit;
    end;
end;

procedure InstallVisualStudioExtension;
var
  Vsix: string;
  ResultCode: Integer;
begin
  Vsix := FindBundledFile(ExpandConstant('{app}\Extensions\VisualStudio\*.vsix'));
  if Vsix = '' then
    Exit;
  if WaitForVisualStudioToClose('install') then
  begin
    { The extension installs for all users, which takes an elevated VSIXInstaller }
    if ShellExec('runas', VSIXInstallerPath, '/quiet /admin /instanceIds:' + VisualStudioIds + ' "' + Vsix + '"',
      '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0) then
    begin
      RegWriteStringValue(HKCU, 'Software\Cosmos', 'VisualStudioInstances', VisualStudioIds);
      Exit;
    end;
    Log(Format('VSIXInstaller failed with code %d', [ResultCode]));
  end;
  SuppressibleMsgBox('The Cosmos extension was not installed in Visual Studio.' + #13#10 + #13#10 +
    'To install it later, close Visual Studio and open:' + #13#10 + Vsix, mbError, MB_OK, IDOK);
end;

procedure UninstallVisualStudioExtension;
var
  Instances: string;
  ResultCode: Integer;
begin
  if not RegQueryStringValue(HKCU, 'Software\Cosmos', 'VisualStudioInstances', Instances) then
    Exit;
  FindVisualStudio;
  if (VSIXInstallerPath <> '') and WaitForVisualStudioToClose('uninstall') then
    ShellExec('runas', VSIXInstallerPath, '/quiet /instanceIds:' + Instances + ' /u:{#VisualStudioExtensionId}',
      '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

procedure InstallRiderPlugin;
var
  Source: string;
  I: Integer;
begin
  Source := ExpandConstant('{app}\Extensions\Rider\{#RiderPluginDir}');
  if not DirExists(Source) then
    Exit;
  { Rider loads every plugin folder under <config>\plugins at its next start; the old copy goes
    first so a renamed jar from an earlier version does not stay beside the new one }
  for I := 0 to GetArrayLength(RiderConfigDirs) - 1 do
  begin
    DelTree(RiderConfigDirs[I] + '\plugins\{#RiderPluginDir}', True, True, True);
    CopyTree(Source, RiderConfigDirs[I] + '\plugins\{#RiderPluginDir}');
  end;
end;

procedure UninstallRiderPlugin;
var
  I: Integer;
begin
  FindRider;
  for I := 0 to GetArrayLength(RiderConfigDirs) - 1 do
    DelTree(RiderConfigDirs[I] + '\plugins\{#RiderPluginDir}', True, True, True);
end;

procedure AddToUserPath(Dir: string);
var
  CurrentPath: string;
begin
  if RegQueryStringValue(HKCU, 'Environment', 'Path', CurrentPath) then
  begin
    if Pos(Uppercase(Dir), Uppercase(CurrentPath)) = 0 then
    begin
      if CurrentPath <> '' then
        CurrentPath := CurrentPath + ';';
      CurrentPath := CurrentPath + Dir;
      RegWriteStringValue(HKCU, 'Environment', 'Path', CurrentPath);
    end;
  end
  else
    RegWriteStringValue(HKCU, 'Environment', 'Path', Dir);
end;

procedure RemoveFromUserPath(Dir: string);
var
  CurrentPath, UpperDir, UpperPath: string;
  P: Integer;
begin
  if RegQueryStringValue(HKCU, 'Environment', 'Path', CurrentPath) then
  begin
    UpperDir := Uppercase(Dir);
    UpperPath := Uppercase(CurrentPath);
    P := Pos(UpperDir, UpperPath);
    if P > 0 then
    begin
      { Remove the directory and any trailing semicolon }
      Delete(CurrentPath, P, Length(Dir));
      if (P <= Length(CurrentPath)) and (CurrentPath[P] = ';') then
        Delete(CurrentPath, P, 1)
      else if (P > 1) and (CurrentPath[P - 1] = ';') then
        Delete(CurrentPath, P - 1, 1);
      RegWriteStringValue(HKCU, 'Environment', 'Path', CurrentPath);
    end;
  end;
end;

function InitializeSetup: Boolean;
begin
  if not DotNetInstalled then
  begin
    if MsgBox('.NET SDK is required but was not found.' + #13#10 + #13#10 +
              'Please install .NET 10.0 SDK from https://dot.net/download' + #13#10 + #13#10 +
              'Continue anyway?', mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
      Exit;
    end;
  end;
  { The IDE tasks show only for the IDEs found here }
  FindVSCode;
  FindVisualStudio;
  FindRider;
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    { Add tool directories to user PATH }
    AddToUserPath(ExpandConstant('{app}\Tools\xorriso'));
    AddToUserPath(ExpandConstant('{app}\Tools\llvm-tools\bin'));
    AddToUserPath(ExpandConstant('{app}\Tools\qemu'));
    { grumpycoder's gdb-multiarch zip extracts to gdb\bin — DLLs live there too }
    AddToUserPath(ExpandConstant('{app}\Tools\gdb\bin'));
    { Broadcast so new terminals pick up the PATH change immediately }
    BroadcastEnvironmentChange;

    if WizardIsTaskSelected('visualstudio') then
    begin
      WizardForm.StatusLabel.Caption := 'Installing Visual Studio extension...';
      InstallVisualStudioExtension;
    end;
    if WizardIsTaskSelected('rider') then
    begin
      WizardForm.StatusLabel.Caption := 'Installing Rider plugin...';
      InstallRiderPlugin;
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    { Before the uninstall deletes Software\Cosmos, which records the Visual Studio instances }
    UninstallVisualStudioExtension;
    UninstallRiderPlugin;
  end;
  if CurUninstallStep = usPostUninstall then
  begin
    { Remove tool directories from user PATH }
    RemoveFromUserPath(ExpandConstant('{app}\Tools\xorriso'));
    RemoveFromUserPath(ExpandConstant('{app}\Tools\llvm-tools\bin'));
    RemoveFromUserPath(ExpandConstant('{app}\Tools\qemu'));
    RemoveFromUserPath(ExpandConstant('{app}\Tools\gdb\bin'));
    { Broadcast so terminals pick up the PATH removal }
    BroadcastEnvironmentChange;
  end;
end;
