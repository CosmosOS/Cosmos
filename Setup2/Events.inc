[Code]

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
	xResultCode: Integer;
begin
	EnableFsRedirection(true);
	try
		Exec(ExpandConstant('{code:VSNET2008_PATH|}\devenv.exe'), '/setup', '', SW_HIDE, ewWaitUntilTerminated, xResultCode);
		if xResultCode <> 0 then
		begin
			RaiseException('An error occurred while unregistering with Visual Studio!');
		end;	
	finally
		EnableFsRedirection(false);
	end;
end;

//procedure CurStepChanged(CurStep: TSetupStep);
//begin
//  if (CurStep=ssInstall) then
//  begin
//    if (IsUpgrade()) then
//    begin
//      UnInstallOldVersion();
//    end;
//  end;
//end;
