
; DialogBox example

format PE GUI 4.0
entry start

include 'win32a.inc'

ID_CAPTION	   = 101
ID_MESSAGE	   = 102
ID_ICONERROR	   = 201
ID_ICONINFORMATION = 202
ID_ICONQUESTION    = 203
ID_ICONWARNING	   = 204
ID_TOPMOST	   = 301

section '.data' data readable writeable

  flags dd ?
  caption rb 40h
  message rb 100h

section '.code' code readable executable

  start:

	invoke	GetModuleHandle,0
	invoke	DialogBoxParam,eax,37,HWND_DESKTOP,DialogProc,0
	or	eax,eax
	jz	exit
	invoke	MessageBox,HWND_DESKTOP,message,caption,[flags]
  exit:
	invoke	ExitProcess,0

proc DialogProc hwnddlg,msg,wparam,lparam
	push	ebx esi edi
	cmp	[msg],WM_INITDIALOG
	je	.wminitdialog
	cmp	[msg],WM_COMMAND
	je	.wmcommand
	cmp	[msg],WM_CLOSE
	je	.wmclose
	xor	eax,eax
	jmp	.finish
  .wminitdialog:
	invoke	CheckRadioButton,[hwnddlg],ID_ICONERROR,ID_ICONWARNING,ID_ICONINFORMATION
	jmp	.processed
  .wmcommand:
	cmp	[wparam],BN_CLICKED shl 16 + IDCANCEL
	je	.wmclose
	cmp	[wparam],BN_CLICKED shl 16 + IDOK
	jne	.processed
	invoke	GetDlgItemText,[hwnddlg],ID_CAPTION,caption,40h
	invoke	GetDlgItemText,[hwnddlg],ID_MESSAGE,message,100h
	mov	[flags],MB_OK
	invoke	IsDlgButtonChecked,[hwnddlg],ID_ICONERROR
	cmp	eax,BST_CHECKED
	jne	.iconerror_ok
	or	[flags],MB_ICONERROR
      .iconerror_ok:
	invoke	IsDlgButtonChecked,[hwnddlg],ID_ICONINFORMATION
	cmp	eax,BST_CHECKED
	jne	.iconinformation_ok
	or	[flags],MB_ICONINFORMATION
      .iconinformation_ok:
	invoke	IsDlgButtonChecked,[hwnddlg],ID_ICONQUESTION
	cmp	eax,BST_CHECKED
	jne	.iconquestion_ok
	or	[flags],MB_ICONQUESTION
      .iconquestion_ok:
	invoke	IsDlgButtonChecked,[hwnddlg],ID_ICONWARNING
	cmp	eax,BST_CHECKED
	jne	.iconwarning_ok
	or	[flags],MB_ICONWARNING
      .iconwarning_ok:
	invoke	IsDlgButtonChecked,[hwnddlg],ID_TOPMOST
	cmp	eax,BST_CHECKED
	jne	.topmost_ok
	or	[flags],MB_TOPMOST
      .topmost_ok:
	invoke	EndDialog,[hwnddlg],1
	jmp	.processed
  .wmclose:
	invoke	EndDialog,[hwnddlg],0
  .processed:
	mov	eax,1
  .finish:
	pop	edi esi ebx
	ret
endp

section '.idata' import data readable writeable

  library kernel,'KERNEL32.DLL',\
	  user,'USER32.DLL'

  import kernel,\
	 GetModuleHandle,'GetModuleHandleA',\
	 ExitProcess,'ExitProcess'

  import user,\
	 DialogBoxParam,'DialogBoxParamA',\
	 CheckRadioButton,'CheckRadioButton',\
	 GetDlgItemText,'GetDlgItemTextA',\
	 IsDlgButtonChecked,'IsDlgButtonChecked',\
	 MessageBox,'MessageBoxA',\
	 EndDialog,'EndDialog'

section '.rsrc' resource data readable

  directory RT_DIALOG,dialogs

  resource dialogs,\
	   37,LANG_ENGLISH+SUBLANG_DEFAULT,demonstration

  dialog demonstration,'Create message box',70,70,190,175,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'STATIC','&Caption:',-1,10,10,70,8,WS_VISIBLE
    dialogitem 'EDIT','',ID_CAPTION,10,20,170,13,WS_VISIBLE+WS_BORDER+WS_TABSTOP
    dialogitem 'STATIC','&Message:',-1,10,40,70,8,WS_VISIBLE
    dialogitem 'EDIT','',ID_MESSAGE,10,50,170,13,WS_VISIBLE+WS_BORDER+WS_TABSTOP+ES_AUTOHSCROLL
    dialogitem 'BUTTON','&Icon',-1,10,70,80,70,WS_VISIBLE+BS_GROUPBOX
    dialogitem 'BUTTON','&Error',ID_ICONERROR,20,82,60,13,WS_VISIBLE+BS_AUTORADIOBUTTON+WS_TABSTOP+WS_GROUP
    dialogitem 'BUTTON','I&nformation',ID_ICONINFORMATION,20,95,60,13,WS_VISIBLE+BS_AUTORADIOBUTTON
    dialogitem 'BUTTON','&Question',ID_ICONQUESTION,20,108,60,13,WS_VISIBLE+BS_AUTORADIOBUTTON
    dialogitem 'BUTTON','&Warning',ID_ICONWARNING,20,121,60,13,WS_VISIBLE+BS_AUTORADIOBUTTON
    dialogitem 'BUTTON','&Style',-1,100,70,80,70,WS_VISIBLE+BS_GROUPBOX
    dialogitem 'BUTTON','&Top most',ID_TOPMOST,110,82,60,13,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','OK',IDOK,85,150,45,15,WS_VISIBLE+WS_TABSTOP+BS_DEFPUSHBUTTON
    dialogitem 'BUTTON','C&ancel',IDCANCEL,135,150,45,15,WS_VISIBLE+WS_TABSTOP+BS_PUSHBUTTON
  enddialog
