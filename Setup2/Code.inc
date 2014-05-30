[Code]
var
  __NET_FRAMEWORK_INSTALLED,
  __NET_FRAMEWORK2_INSTALLED,
  __NET_FRAMEWORK3_INSTALLED,
  __NET_FRAMEWORK4_INSTALLED,
  __NET_FRAMEWORK_SDK_INSTALLED,
  __NET_FRAMEWORK_64BIT_INSTALLED,
  __VSNET2013,
  __PathsInitialized: Boolean;
  __FRAMEWORK_INSTALL_ROOT,
  __FRAMEWORK64_INSTALL_ROOT,
  __SDK_INSTALL_ROOT,
  __SDK2_INSTALL_ROOT,
  __REGASM,
  __REGASM64,
  __VSNET2013_PATH,
  __VSNET2013_BASE_PATH,
	__CSharpExpress2013ProjectTemplatePath,
	__VBExpress2013ProjectTemplatePath: string;
  
procedure InitializePaths;
var __VisualStudioLocation : string;
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
  __NET_FRAMEWORK4_INSTALLED := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework\policy\v4.0,30128|}') <> '';


  __SDK_INSTALL_ROOT := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,sdkInstallRootv1.1|}');
  __SDK2_INSTALL_ROOT := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,sdkInstallRootv2.0|}');
                     

  __NET_FRAMEWORK_SDK_INSTALLED := (__SDK2_INSTALL_ROOT <> '') or (__SDK_INSTALL_ROOT <> '');
  if __SDK2_INSTALL_ROOT <> '' then
    __SDK_INSTALL_ROOT := __SDK2_INSTALL_ROOT;

  __NET_FRAMEWORK_64BIT_INSTALLED := ExpandConstant('{reg:HKLM\SOFTWARE\Microsoft\.NETFramework,Enable64Bit|0}') = '1';
  __NET_FRAMEWORK_64BIT_INSTALLED := __NET_FRAMEWORK_64BIT_INSTALLED or ((__RegAsm64<> '') and FileExists(__RegAsm64));


  __VSNET2013_PATH := ExpandConstant('{reg:HKLM\Software\Microsoft\VisualStudio\12.0,InstallDir|}');
  __VSNET2013 := __VSNET2013_PATH <> '';
    
  if not __VSNET2013 then begin
    __VSNET2013_PATH := ExpandConstant('{reg:HKLM\Software\Microsoft\VNS\12.0,InstallDir|}');
    __VSNET2013 := __VSNET2013_PATH <> '';
  end;
  if not __VSNET2013 then begin
    __VSNET2013_PATH := ExpandConstant('{reg:HKLM\Software\Microsoft\WinExpress\12.0,InstallDir|}');
    __VSNET2013 := __VSNET2013_PATH <> '';
  end;
  if not __VSNET2013 then begin
    __VSNET2013_PATH := ExpandConstant('{reg:HKLM\Software\Microsoft\WDExpress\12.0,InstallDir|}');
    __VSNET2013 := __VSNET2013_PATH <> '';
  end;

  if __VSNET2013 then
    __VSNET2013_BASE_PATH := ExpandFilename(AddBackslash(__VSNET2013_PATH)+'..\..');
 
  if RegQueryStringValue(HKCU, 'Software\Microsoft\VisualStudio\12.0', 'UserProjectTemplatesLocation', __CSharpExpress2013ProjectTemplatePath) then
  begin
    StringChangeEx(__CSharpExpress2013ProjectTemplatePath, '%USERPROFILE%',  GetEnv('USERPROFILE'), False);
  end
  else
  begin
    if RegQueryStringValue(HKCU, 'Software\Microsoft\VisualStudio\12.0', 'VisualStudioLocation', __VisualStudioLocation) then
    begin
      StringChangeEx(__VisualStudioLocation, '%USERPROFILE%',  GetEnv('USERPROFILE'), False);
	  __CSharpExpress2013ProjectTemplatePath := __VisualStudioLocation + '\Templates\ProjectTemplates';
    end
    else
    begin
      __CSharpExpress2013ProjectTemplatePath := ExpandConstant('{userdocs}\Visual Studio 2013\Templates\ProjectTemplates');
    end;
  end;

  __VBExpress2013ProjectTemplatePath := __CSharpExpress2013ProjectTemplatePath;
end;

procedure DeInitializePaths;
begin
  __PathsInitialized := False;
end;

function VSNET2013: Boolean;
begin
  Result := __VSNET2013;
end;

function VSNET2013UP: Boolean;
begin
  Result := VSNET2013;
end;

function VSNET2013_PATH(dummy: string): string;
begin
  Result := __VSNET2013_PATH;
end;

function FRAMEWORK_INSTALL_ROOT(dummy: string): string;
begin
  Result := __FRAMEWORK_INSTALL_ROOT;
end;

function FRAMEWORK64_INSTALL_ROOT(dummy: string): string;
begin
  Result := __FRAMEWORK64_INSTALL_ROOT;
end;

function booltostr(value: Boolean) : string;
begin
	if value then
		Result := 'true'
	else
		Result := 'false';
end;

function Csharp2013_Installed(dummy: string): Boolean;
begin
  Result := RegKeyExists(HKLM,'SOFTWARE\Microsoft\VisualStudio\12.0\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}')
       or RegKeyExists(HKLM,'SOFTWARE\Microsoft\VNS\12.0\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}')
       or RegKeyExists(HKLM,'SOFTWARE\Microsoft\WinExpress\12.0\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}')
       or RegKeyExists(HKLM,'SOFTWARE\Microsoft\WDExpress\12.0\Projects\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}');
end;

function Fsharp2013_Installed(dummy: string): Boolean;
begin
  Result := RegKeyExists(HKLM,'SOFTWARE\Microsoft\VisualStudio\12.0\Projects\{f2a71f9b-5d33-465a-a702-920d77279786}')
       or RegKeyExists(HKLM,'SOFTWARE\Microsoft\VNS\12.0\Projects\{f2a71f9b-5d33-465a-a702-920d77279786}')
       or RegKeyExists(HKLM,'SOFTWARE\Microsoft\WinExpress\12.0\Projects\{f2a71f9b-5d33-465a-a702-920d77279786}')
       or RegKeyExists(HKLM,'SOFTWARE\Microsoft\WDExpress\12.0\Projects\{f2a71f9b-5d33-465a-a702-920d77279786}');
end;

function Vb2013_Installed(dummy: string): Boolean;
begin
  Result := RegKeyExists(HKLM,'SOFTWARE\Microsoft\VisualStudio\12.0\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}')
        or RegKeyExists(HKLM,'SOFTWARE\Microsoft\VNS\12.0\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}')
        or RegKeyExists(HKLM,'SOFTWARE\Microsoft\WinExpress\12.0\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}')
        or RegKeyExists(HKLM,'SOFTWARE\Microsoft\WDExpress\12.0\Projects\{F184B08F-C81C-45f6-A57F-5ABD9991F28F}');
end;

function VS2013_REGISTRY_HIVE(dummy: string): string;
begin
  Result := '12.0';
end;

function VSNET2013_BASE_PATH(dummy: string): string;
begin
  Result := __VSNET2013_BASE_PATH;
end;

function VS13_BASE_PATH(dummy: string): string;
begin
  Result := __VSNET2013_BASE_PATH;
end;

function Get_SDK_INSTALL_ROOT(dummy: string): string;
begin
  Result := __SDK_INSTALL_ROOT;
end;

function Get_SDK2_INSTALL_ROOT(dummy: string): string;
begin
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

function IsCSharpExpress2013Installed(dummy: string): Boolean;
begin
	Result := RegValueExists(HKLM,'SOFTWARE\Microsoft\VNS\12.0','InstallDir')
		or RegValueExists(HKLM,'SOFTWARE\Wow6432Node\Microsoft\WinExpress\12.0','InstallDir')
		or RegValueExists(HKLM,'SOFTWARE\Wow6432Node\Microsoft\WDExpress\12.0','InstallDir');
end;

function GetCSharpExpress2013ProjectTemplatePath(dummy: string): string;
begin
	Result := __CSharpExpress2013ProjectTemplatePath;
end;

function IsVBExpress2013Installed(dummy: string) : Boolean;
begin
	Result := IsCSharpExpress2013Installed('dummy');
end;

function GetVBExpress2013ProjectTemplatePath(dummy: string) : string;
begin
	Result := __CSharpExpress2013ProjectTemplatePath;
end;

function InitializeSetup: Boolean;
begin
  InitializePaths;
  if not VSNET2013 then begin
    MsgBox(ExpandConstant('Visual Studio 2013 not installed! Please see CodePlex docs for supported editions/versions.'), MbError, MB_OK);
    Result := false;
    exit;
  end;
  Result := true;
end;