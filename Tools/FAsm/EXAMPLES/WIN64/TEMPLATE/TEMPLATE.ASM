
format PE64 GUI 5.0
entry start

include 'win64a.inc'

section '.data' data readable writeable

  _title TCHAR 'Win64 program template',0
  _class TCHAR 'FASMWIN64',0
  _error TCHAR 'Startup failed.',0

  wc WNDCLASSEX sizeof.WNDCLASSEX,0,WindowProc,0,0,NULL,NULL,NULL,COLOR_BTNFACE+1,NULL,_class,NULL

  msg MSG

section '.code' code readable executable

  start:
	sub	rsp,8		; Make stack dqword aligned

	invoke	GetModuleHandle,0
	mov	[wc.hInstance],rax
	invoke	LoadIcon,0,IDI_APPLICATION
	mov	[wc.hIcon],rax
	mov	[wc.hIconSm],rax
	invoke	LoadCursor,0,IDC_ARROW
	mov	[wc.hCursor],rax
	invoke	RegisterClassEx,wc
	test	rax,rax
	jz	error

	invoke	CreateWindowEx,0,_class,_title,WS_VISIBLE+WS_DLGFRAME+WS_SYSMENU,128,128,256,192,NULL,NULL,[wc.hInstance],NULL
	test	rax,rax
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

proc WindowProc uses rbx rsi rdi, hwnd,wmsg,wparam,lparam

; Note that first four parameters are passed in registers,
; while names given in the declaration of procedure refer to the stack
; space reserved for them - you may store them there to be later accessible
; if the contents of registers gets destroyed. This may look like:
;       mov     [hwnd],rcx
;       mov     [wmsg],edx
;       mov     [wparam],r8
;       mov     [lparam],r9

	cmp	edx,WM_DESTROY
	je	.wmdestroy
  .defwndproc:
	invoke	DefWindowProc,rcx,rdx,r8,r9
	jmp	.finish
  .wmdestroy:
	invoke	PostQuitMessage,0
	xor	eax,eax
  .finish:
	ret

endp

section '.idata' import data readable writeable

  library kernel32,'KERNEL32.DLL',\
	  user32,'USER32.DLL'

  include 'api\kernel32.inc'
  include 'api\user32.inc'
