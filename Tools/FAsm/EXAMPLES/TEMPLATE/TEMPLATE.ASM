
; Template for program using standard Win32 headers

format PE GUI 4.0
entry start

include 'win32w.inc'

section '.data' data readable writeable

  _class TCHAR 'FASMWIN32',0
  _title TCHAR 'Win32 program template',0
  _error TCHAR 'Startup failed.',0

  wc WNDCLASS 0,WindowProc,0,0,NULL,NULL,NULL,COLOR_BTNFACE+1,NULL,_class

  msg MSG

section '.code' code readable executable

  start:

	invoke	GetModuleHandle,0
	mov	[wc.hInstance],eax
	invoke	LoadIcon,0,IDI_APPLICATION
	mov	[wc.hIcon],eax
	invoke	LoadCursor,0,IDC_ARROW
	mov	[wc.hCursor],eax
	invoke	RegisterClass,wc
	test	eax,eax
	jz	error

	invoke	CreateWindowEx,0,_class,_title,WS_VISIBLE+WS_DLGFRAME+WS_SYSMENU,128,128,256,192,NULL,NULL,[wc.hInstance],NULL
	test	eax,eax
	jz	error

  msg_loop:
	invoke	GetMessage,msg,NULL,0,0
	cmp	eax,1
	jb	end_loop
	jne	msg_loop
	invoke	TranslateMessage,msg
	invoke	DispatchMessage,msg
	jmp	msg_loop

  error:
	invoke	MessageBox,NULL,_error,NULL,MB_ICONERROR+MB_OK

  end_loop:
	invoke	ExitProcess,[msg.wParam]

proc WindowProc hwnd,wmsg,wparam,lparam
	push	ebx esi edi
	cmp	[wmsg],WM_DESTROY
	je	.wmdestroy
  .defwndproc:
	invoke	DefWindowProc,[hwnd],[wmsg],[wparam],[lparam]
	jmp	.finish
  .wmdestroy:
	invoke	PostQuitMessage,0
	xor	eax,eax
  .finish:
	pop	edi esi ebx
	ret
endp

section '.idata' import data readable writeable

  library kernel32,'KERNEL32.DLL',\
	  user32,'USER32.DLL'

  include 'api\kernel32.inc'
  include 'api\user32.inc'
