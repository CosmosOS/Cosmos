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
function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
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

/////////////////////////////////////////////////////////////////////
// Uninstall previously installed application.
/////////////////////////////////////////////////////////////////////
procedure CleanupInterferringWithPyToolsGuid;
begin
  if not PythonTools2013_Installed then
  begin
    // Remove interferring registry string.
    RegDeleteKeyIncludingSubkeys(HKLM, 'SOFTWARE\Wow6432Node\Microsoft\VisualStudio\12.0\CLSID\{8355452D-6D2F-41B0-89B8-BB2AA2529E94}')
    RegDeleteKeyIncludingSubkeys(HKLM, 'Software\Microsoft\VisualStudio\12.0\CLSID\{8355452D-6D2F-41B0-89B8-BB2AA2529E94}');
  end;
end;

/////////////////////////////////////////////////////////////////////
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    if (IsUpgrade()) then
    begin
      CleanupInterferringWithPyToolsGuid();
    end;
  end;
end;

[Run]
  Filename: {code:VSNET2013_PATH}\devenv.exe; Parameters: /setup; Flags: waituntilterminated;
