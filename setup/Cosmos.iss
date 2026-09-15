; Cosmos OS Development Kit - Gen3 Installer
; Inno Setup Script for Windows
; Requires Inno Setup 6.x+

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

; VS Code extension
Source: "bundle\extensions\*.vsix"; DestDir: "{app}\Extensions"; Flags: ignoreversion skipifsourcedoesntexist

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

; Install VS Code extension
StatusMsg: "Installing VS Code extension..."; \
  Filename: "cmd"; \
  Parameters: "/c for %f in (""{app}\Extensions\*.vsix"") do code --install-extension ""%f"" --force"; \
  Flags: runhidden waituntilterminated; \
  Check: VSCodeInstalled

[UninstallRun]
Filename: "dotnet"; Parameters: "nuget remove source ""Cosmos Local Feed"""; Flags: runhidden waituntilterminated; RunOnceId: "RemoveNuGetFeed"
Filename: "dotnet"; Parameters: "tool uninstall -g Cosmos.Patcher"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallPatcher"
Filename: "dotnet"; Parameters: "tool uninstall -g Cosmos.Tools"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallTools"
Filename: "dotnet"; Parameters: "new uninstall Cosmos.Build.Templates"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallTemplates"
Filename: "cmd"; Parameters: "/c code --uninstall-extension cosmosos.cosmos-vscode"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallVSCodeExt"

[Code]
const
  SMTO_ABORTIFHUNG = 2;
  WM_SETTINGCHANGE = $001A;

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
var
  ResultCode: Integer;
begin
  Result := Exec('cmd', '/c code --version', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
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
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
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
