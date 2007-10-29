
; Component Object Model usage demonstration

format PE64 GUI 5.0
entry start

include 'win64a.inc'

struc GUID def
 {
   match d1-d2-d3-d4-d5, def
    \{
      .Data1 dd 0x\#d1
      .Data2 dw 0x\#d2
      .Data3 dw 0x\#d3
      .Data4 db 0x\#d4 shr 8,0x\#d4 and 0FFh
      .Data5 db 0x\#d5 shr 40,0x\#d5 shr 32 and 0FFh,0x\#d5 shr 24 and 0FFh,0x\#d5 shr 16 and 0FFh,0x\#d5 shr 8 and 0FFh,0x\#d5 and 0FFh
    \}
 }

interface ITaskBarList,\
	   QueryInterface,\
	   AddRef,\
	   Release,\
	   HrInit,\
	   AddTab,\
	   DeleteTab,\
	   ActivateTab,\
	   SetActiveAlt

CLSCTX_INPROC_SERVER	    = 0x1
CLSCTX_INPROC_HANDLER	    = 0x2
CLSCTX_LOCAL_SERVER	    = 0x4
CLSCTX_INPROC_SERVER16	    = 0x8
CLSCTX_REMOTE_SERVER	    = 0x10
CLSCTX_INPROC_HANDLER16     = 0x20
CLSCTX_INPROC_SERVERX86     = 0x40
CLSCTX_INPROC_HANDLERX86    = 0x80
CLSCTX_ESERVER_HANDLER	    = 0x100
CLSCTX_NO_CODE_DOWNLOAD     = 0x400
CLSCTX_NO_CUSTOM_MARSHAL    = 0x1000
CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000
CLSCTX_NO_FAILURE_LOG	    = 0x4000
CLSCTX_DISABLE_AAA	    = 0x8000
CLSCTX_ENABLE_AAA	    = 0x10000
CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000

ID_EXIT = IDCANCEL
ID_SHOW = 100
ID_HIDE = 101

IDD_COMDEMO = 1

section '.data' data readable writeable

 CLSID_TaskbarList GUID 56FDF344-FD6D-11D0-958A-006097C9A090
 IID_ITaskbarList GUID 56FDF342-FD6D-11D0-958A-006097C9A090

 ShellTaskBar ITaskBarList

section '.code' code readable executable

 start:
	sub	rsp,8		; Make stack dqword aligned

	invoke	CoInitialize,NULL
	invoke	CoCreateInstance,CLSID_TaskbarList,NULL,CLSCTX_INPROC_SERVER,IID_ITaskbarList,ShellTaskBar

	invoke	GetModuleHandle,0
	invoke	DialogBoxParam,rax,IDD_COMDEMO,HWND_DESKTOP,COMDemo,0

	cominvk ShellTaskBar,Release

	invoke	ExitProcess,0

proc COMDemo uses rbx rsi rdi, hwnd,msg,wparam,lparam
	mov	[hwnd],rcx
	cmp	edx,WM_INITDIALOG
	je	.wminitdialog
	cmp	edx,WM_COMMAND
	je	.wmcommand
	cmp	edx,WM_CLOSE
	je	.wmclose
	xor	eax,eax
	jmp	.finish
  .wminitdialog:
	jmp	.processed
  .wmcommand:
	cmp	r8,BN_CLICKED shl 16 + ID_EXIT
	je	.wmclose
	cmp	r8,BN_CLICKED shl 16 + ID_SHOW
	je	.show
	cmp	r8,BN_CLICKED shl 16 + ID_HIDE
	jne	.processed
  .hide:
	cominvk ShellTaskBar,HrInit
	cominvk ShellTaskBar,DeleteTab,[hwnd]
	jmp	.processed
  .show:
	mov	rbx,[ShellTaskBar]
	comcall rbx,ITaskBarList,HrInit
	comcall rbx,ITaskBarList,AddTab,[hwnd]
	comcall rbx,ITaskBarList,ActivateTab,[hwnd]
	jmp	.processed
  .wmclose:
	invoke	EndDialog,[hwnd],0
  .processed:
	mov	eax,1
  .finish:
	ret
endp

section '.idata' import data readable

  library kernel,'KERNEL32.DLL',\
	  user,'USER32.DLL',\
	  ole,'OLE32.DLL'

  import kernel,\
	 GetModuleHandle,'GetModuleHandleA',\
	 ExitProcess,'ExitProcess'

  import user,\
	 DialogBoxParam,'DialogBoxParamA',\
	 EndDialog,'EndDialog'

  import ole,\
	 CoInitialize,'CoInitialize',\
	 CoCreateInstance,'CoCreateInstance'

section '.rsrc' resource data readable

  directory RT_DIALOG,dialogs

  resource dialogs,\
	   IDD_COMDEMO,LANG_ENGLISH+SUBLANG_DEFAULT,comdemo

  dialog comdemo,'Taskbar item control',70,70,170,24,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'BUTTON','Show',ID_SHOW,4,4,45,15,WS_VISIBLE+WS_TABSTOP
    dialogitem 'BUTTON','Hide',ID_HIDE,54,4,45,15,WS_VISIBLE+WS_TABSTOP
    dialogitem 'BUTTON','Exit',ID_EXIT,120,4,45,15,WS_VISIBLE+WS_TABSTOP
  enddialog
