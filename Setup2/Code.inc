[Code]
var
  __NET_FRAMEWORK_INSTALLED,
  __NET_FRAMEWORK2_INSTALLED,
  __NET_FRAMEWORK3_INSTALLED,
  __NET_FRAMEWORK4_INSTALLED,
  __NET_FRAMEWORK_SDK_INSTALLED,
  __NET_FRAMEWORK_64BIT_INSTALLED,
  __VSNET2008,
  __VSNET2010,
  __PathsInitialized: Boolean;
  __FRAMEWORK_INSTALL_ROOT,
  __FRAMEWORK64_INSTALL_ROOT,
  __SDK_INSTALL_ROOT,
  __SDK2_INSTALL_ROOT,
  __REGASM,
  __REGASM64,
  __VSNET2008_PATH,
  __VSNET2010_PATH,
  __VSNET2008_BASE_PATH,
  __VSNET2010_BASE_PATH,
	__CSharpExpress2008ProjectTemplatePath: string;
  
procedure InitializePaths;
begin
  if __PathsInitialized then exit;
  __PathsInitialized := true;
  __NET_FRAMEWORK_SDK_INSTALLED := true;

  __FRAMEWORK_INSTALL_ROOT := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,InstallRoot|}');
  __NET_FRAMEWORK_INSTALLED := __FRAMEWORK_INSTALL_ROOT <> '';
  if __NET_FRAMEWORK_INSTALLED then begin
    __FRAMEWORK64_INSTALL_ROOT := ExpandFileName(AddBackslash(__FRAMEWORK_INSTALL_ROOT)+'..\Framework64');
    if not DirExists(__FRAMEWORK64_INSTALL_ROOT) then __FRAMEWORK64_INSTALL_ROOT := '';
  end;
  __NET_FRAMEWORK2_INSTALLED := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework\policy\v2.0,50727|}') <> '';
  if __NET_FRAMEWORK2_INSTALLED then begin
    __REGASM64 := AddBackslash(__FRAMEWORK64_INSTALL_ROOT)+'v2.0.50727\regasm.exe';
    if not FileExists(__REGASM64) then __REGASM64 := '';
    __REGASM := AddBackslash(__FRAMEWORK_INSTALL_ROOT)+'v2.0.50727\regasm.exe';
  end;
  __NET_FRAMEWORK3_INSTALLED := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework\AssemblyFolders\v3.5,All Assemblies In|}') <> '';
  __SDK_INSTALL_ROOT := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,sdkInstallRootv1.1|}');
  __SDK2_INSTALL_ROOT := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,sdkInstallRootv2.0|}');
  
__NET_FRAMEWORK4_INSTALLED := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework\policy\v4.0,30128|}') <> '';

  __NET_FRAMEWORK_SDK_INSTALLED := (__SDK2_INSTALL_ROOT <> '') or (__SDK_INSTALL_ROOT <> '');
  if __SDK2_INSTALL_ROOT<> '' then
    __SDK_INSTALL_ROOT := __SDK2_INSTALL_ROOT;

  __NET_FRAMEWORK_64BIT_INSTALLED := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,Enable64Bit|0}') = '1';
  __NET_FRAMEWORK_64BIT_INSTALLED := __NET_FRAMEWORK_64BIT_INSTALLED or ((__RegAsm64<> '') and FileExists(__RegAsm64));

  __VSNET2008_PATH := ExpandConstant('{reg:HKLM\Software\Microsoft\VisualStudio\9.0,InstallDir|}');
  __VSNET2010_PATH := ExpandConstant('{reg:HKLM\Software\Microsoft\VisualStudio\10.0,InstallDir|}');
  __VSNET2008 := __VSNET2008_PATH <> '';
  __VSNET2010 := __VSNET2010_PATH <> '';
  if __VSNET2008 then
  __VSNET2008_BASE_PATH := ExpandFilename(AddBackslash(__VSNET2008_PATH)+'..\..');
  if __VSNET2010 then
  __VSNET2010_BASE_PATH := ExpandFilename(AddBackslash(__VSNET2010_PATH)+'..\..');
  
  RegQueryStringValue(HKCU, 'SOFTWARE\Microsoft\VCSExpress\9.0', 'UserProjectTemplatesLocation', __CSharpExpress2008ProjectTemplatePath);
  
  StringChangeEx(__CSharpExpress2008ProjectTemplatePath, '%USERPROFILE%',  GetEnv('USERPROFILE'), False);

end;

procedure DeInitializePaths;
begin
  __PathsInitialized := False;
end;

function VSNET2008: Boolean;
begin
  InitializePaths;
  Result := __VSNET2008;
end;

function VSNET2010: Boolean;
begin
  InitializePaths;
  Result := __VSNET2010;
end;

function VSNET2008UP: Boolean;
begin
  Result := VSNET2008 or VSNET2010;
end;

function VSNET2010UP: Boolean;
begin
  Result := VSNET2010;
end;

function VSNET2008_PATH(dummy: string): string;
begin
  InitializePaths;
  result := __VSNET2008_PATH;
end;

function VSNET2010_PATH(dummy: string): string;
begin
  InitializePaths;
  result := __VSNET2010_PATH;
end;

function FRAMEWORK_INSTALL_ROOT(dummy: string): string;
begin
  InitializePaths;
  result := __FRAMEWORK_INSTALL_ROOT;
end;

function FRAMEWORK64_INSTALL_ROOT(dummy: string): string;
begin
  InitializePaths;
  result := __FRAMEWORK64_INSTALL_ROOT;
end;

function Vb2008_Installed: Boolean;
begin
  result := RegValueExists(HKLM,'SOFTWARE\Microsoft\VisualStudio\9.0\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}','Package');
end;

function Vb2010_Installed: Boolean;
begin
	result := RegValueExists(HKLM,'SOFTWARE\Microsoft\VisualStudio\10.0\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}','Package');
end;

function booltostr(value: Boolean) : string;
begin
	if value then
		Result := 'true'
	else
		Result := 'false';
end;

function csharp2008_Installed(test: string): Boolean;
begin
	result := RegValueExists(HKLM,'SOFTWARE\Microsoft\VisualStudio\9.0\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}','Package');
end;

function csharp2010_Installed: Boolean;
begin
  result := RegValueExists(HKLM,'SOFTWARE\Microsoft\VisualStudio\10.0\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}','Package');
end;

function VS2008_REGISTRY_HIVE(dummy: string): string;
begin
  Result := '9.0';
end;

function VS2010_REGISTRY_HIVE(dummy: string): string;
begin
  Result := '10.0';
end;

function ORCAS_REGISTRY_HIVE(dummy: string): string;
begin
  Result := '9.0';
end;

function VS10_REGISTRY_HIVE(dummy: string): string;
begin
  Result := '10.0';
end;

function VSNET2008_BASE_PATH(dummy: string): string;
begin
  InitializePaths;
  result := __VSNET2008_BASE_PATH;
end;

function VSNET2010_BASE_PATH(dummy: string): string;
begin
  InitializePaths;
  result := __VSNET2010_BASE_PATH;
end;

function ORCAS_BASE_PATH(dummy: string): string;
begin
  InitializePaths;
  result := __VSNET2008_BASE_PATH;
end;

function VS10_BASE_PATH(dummy: string): string;
begin
  InitializePaths;
  result := __VSNET2010_BASE_PATH;
end;


function Get_SDK_INSTALL_ROOT(dummy: string): string;
begin
  InitializePaths;
  Result := __SDK_INSTALL_ROOT;
end;

function Get_SDK2_INSTALL_ROOT(dummy: string): string;
begin
  InitializePaths;
  Result := __SDK2_INSTALL_ROOT;
end;

function GET_NET_FRAMEWORK3_INSTALLED:Boolean;
begin
  Result := __NET_FRAMEWORK3_INSTALLED;
end;

function GET_NET_FRAMEWORK2_INSTALLED:Boolean;
begin
  Result := __NET_FRAMEWORK2_INSTALLED;
end;

function GET_NET_FRAMEWORK_INSTALLED:Boolean;
begin
  Result := __NET_FRAMEWORK_INSTALLED;
end;

function GET_NET_FRAMEWORK4_INSTALLED:Boolean;
begin
  Result := __NET_FRAMEWORK4_INSTALLED;
end;

function IsCSharpExpress2008Installed(dummy: string): boolean;
begin
	if (Length(Trim(__CSharpExpress2008ProjectTemplatePath)) > 0) then
		Result := True
	else
		Result := False;
end;

function GetCSharpExpress2008ProjectTemplatePath(dummy: string) : string;
begin
	InitializePaths;
	Result := __CSharpExpress2008ProjectTemplatePath;
	SaveStringToFile('c:\setup.log', Format('C# Express TemplatePath: "%s"'#13#10, [Result]), true);
end;

function InitializeSetup: Boolean;
begin
  if not VSNET2008 then begin
    MsgBox(ExpandConstant('Visual Studio 2008 not installed!'), MbError, MB_OK);
    result := false;
    exit;
   end;
  result := true;
end;
