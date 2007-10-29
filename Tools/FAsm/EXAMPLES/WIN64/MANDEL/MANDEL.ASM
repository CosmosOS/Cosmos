
format PE64 GUI 5.0
entry start

include 'win64a.inc'
include 'ddraw64.inc'

section '.data' data readable writeable

  _title db 'flat assembler DirectDraw application',0
  _class db 'FDDRAW64',0

  _error db 'Error',0
  _startup_error db 'Startup failed',0
  _ddraw_error db 'Direct Draw initialization failed (error code 0x%x).',0

  align 16				; SSE data follows

  label origin dqword
   x_left dq -2.2
   y_top dq 1.25

  label step dqword
   x_step dq 0.0045
   y_step dq 0.0052

  label zoom dqword
   dq 1.2,1.2

  label negate dqword
   dq 8000000000000000h,0

  limit dq 2.5

section '.bss' readable writeable

  hinstance dq ?
  hwnd dq ?
  wc WNDCLASSEX sizeof.WNDCLASSEX,0,WindowProc,0,0,NULL,NULL,NULL,NULL,NULL,_class,NULL
  msg MSG

  ddsd DDSURFACEDESC
  ddscaps DDSCAPS

  DDraw DirectDraw
  DDSPrimary DirectDrawSurface
  DDSBack DirectDrawSurface

  DDSPicture DirectDrawSurface
  DDPalette DirectDrawPalette

  rect RECT

  refresh_needed dd ?

  buffer rb 100h

section '.code' code readable executable

  start:

	sub	rsp,8

	invoke	GetModuleHandle,NULL
	mov	[hinstance],rax
	mov	[wc.hInstance],rax

	invoke	LoadIcon,NULL,IDI_APPLICATION
	mov	[wc.hIcon],rax
	invoke	LoadCursor,NULL,IDC_ARROW
	mov	[wc.hCursor],rax
	invoke	RegisterClassEx,wc
	test	rax,rax
	jz	startup_error

	invoke	CreateWindowEx,\
		0,_class,_title,WS_POPUP+WS_VISIBLE,0,0,0,0,NULL,NULL,[hinstance],NULL
	test	rax,rax
	jz	startup_error
	mov	[hwnd],rax

	invoke	DirectDrawCreate,NULL,DDraw,NULL
	test	rax,rax
	jnz	ddraw_error

	cominvk DDraw,SetCooperativeLevel,\
		[hwnd],DDSCL_EXCLUSIVE+DDSCL_FULLSCREEN
	test	rax,rax
	jnz	ddraw_error

	cominvk DDraw,SetDisplayMode,\
		640,480,16
	test	rax,rax
	jnz	ddraw_error

	mov	[ddsd.dwSize],sizeof.DDSURFACEDESC
	mov	[ddsd.dwFlags],DDSD_CAPS
	mov	[ddsd.ddsCaps.dwCaps],DDSCAPS_PRIMARYSURFACE
	cominvk DDraw,CreateSurface,\
		ddsd,DDSPrimary,NULL
	test	rax,rax
	jnz	ddraw_error

refresh:

	cominvk DDSPrimary,IsLost
	test	rax,rax
	jz	paint
	cmp	eax,DDERR_SURFACELOST
	jne	end_loop
	cominvk DDSPrimary,Restore

    paint:
	mov	[ddsd.dwSize],sizeof.DDSURFACEDESC
	mov	[ddsd.dwFlags],0
	cominvk DDSPrimary,Lock,NULL,ddsd,DDLOCK_SURFACEMEMORYPTR,NULL
	test	rax,rax
	jnz	main_loop
	mov	rdi,[ddsd.lpSurface]

	xor	edx,edx
	movsd	xmm8,[y_top]
    screen:
	xor	ebx,ebx
	movsd	xmm7,[x_left]
	unpcklpd xmm7,xmm8
    row:
	mov	rcx,255
	xorpd	xmm1,xmm1
    iterate:

	movapd	xmm3,xmm1
	unpckhpd xmm3,xmm3
	mulsd	xmm3,xmm1
	addsd	xmm3,xmm3

	mulpd	xmm1,xmm1
	movapd	xmm2,xmm1		; for SSE3-capable processor
	unpckhpd xmm2,xmm2		; these three instructions can be
	subsd	xmm1,xmm2		; replaced with HSUBPD XMM0,XMM0

	unpcklpd xmm1,xmm3
	addpd	xmm1,xmm7

	movapd	xmm0,xmm1
	mulpd	xmm0,xmm0
	movapd	xmm2,xmm0		; for SSE3-capable processor
	shufpd	xmm2,xmm2,1		; these three instructions can be
	addsd	xmm0,xmm2		; replaced with HADDPD XMM0,XMM0
	sqrtpd	xmm0,xmm0
	comisd	xmm0,[limit]
	ja	over

	loop	iterate
    over:
	xor	al,al
	stosb
	mov	al,cl
	stosb

	movsd	xmm0,[x_step]
	addpd	xmm7,xmm0
	sub	rdi,640*2
	add	rdi,[ddsd.lPitch]
	inc	ebx
	cmp	ebx,640
	jb	row

	subsd	xmm8,[y_step]

	inc	edx
	cmp	edx,480
	jb	screen

	cominvk DDSPrimary,Unlock,NULL
	mov	[refresh_needed],0

main_loop:

	invoke	PeekMessage,msg,NULL,0,0,PM_NOREMOVE
	or	eax,eax
	jz	no_message
	invoke	GetMessage,msg,NULL,0,0
	cmp	eax,1
	jb	end_loop
	jne	no_message
	invoke	TranslateMessage,msg
	invoke	DispatchMessage,msg

	cmp	[refresh_needed],0
	jne	refresh
	jmp	main_loop

    no_message:
	invoke	WaitMessage
	jmp	main_loop

ddraw_error:
	invoke	wsprintf,buffer,_ddraw_error,rax
	invoke	MessageBox,[hwnd],buffer,_error,MB_OK+MB_ICONERROR
	invoke	DestroyWindow,[hwnd]
	invoke	PostQuitMessage,1
	jmp	main_loop

startup_error:
	invoke	MessageBox,[hwnd],_startup_error,_error,MB_OK+MB_ICONERROR

end_loop:
	invoke	ExitProcess,[msg.wParam]

proc WindowProc uses rbx rsi rdi, hwnd,wmsg,wparam,lparam
	cmp	edx,WM_CREATE
	je	.wmcreate
	cmp	edx,WM_DESTROY
	je	.wmdestroy
	cmp	edx,WM_LBUTTONDOWN
	je	.wmlbuttondown
	cmp	edx,WM_RBUTTONDOWN
	je	.wmrbuttondown
	cmp	edx,WM_KEYDOWN
	je	.wmkeydown
	cmp	edx,WM_ACTIVATE
	je	.wmactivate
    .defwindowproc:
	invoke	DefWindowProc,rcx,rdx,r8,r9
	jmp	.finish
    .wmcreate:
	xor	eax,eax
	jmp	.finish
    .wmactivate:
	test	r8,r8
	jz	.finish
	or	[refresh_needed],1
	jmp	.finish
    .wmlbuttondown:
	movapd	xmm0,[step]
	divpd	xmm0,[zoom]
	movapd	xmm1,xmm0
	subpd	xmm1,[step]
	movapd	[step],xmm0
	movzx	eax,r9w
	cvtsi2sd xmm3,eax
	shr	r9,16
	movzx	eax,r9w
	cvtsi2sd xmm4,eax
	unpcklpd xmm3,xmm4
	mulpd	xmm1,xmm3
	xorpd	xmm1,[negate]
	addpd	xmm1,[origin]
	movapd	[origin],xmm1
	or	[refresh_needed],1
	jmp	.finish
    .wmrbuttondown:
	movapd	xmm0,[step]
	mulpd	xmm0,[zoom]
	movapd	xmm1,xmm0
	subpd	xmm1,[step]
	movapd	[step],xmm0
	movzx	eax,r9w
	cvtsi2sd xmm3,eax
	shr	r9,16
	movzx	eax,r9w
	cvtsi2sd xmm4,eax
	unpcklpd xmm3,xmm4
	mulpd	xmm1,xmm3
	xorpd	xmm1,[negate]
	addpd	xmm1,[origin]
	movapd	[origin],xmm1
	or	[refresh_needed],1
	jmp	.finish
    .wmkeydown:
	cmp	r8d,VK_ESCAPE
	jne	.finish
    .wmdestroy:
	cominvk DDraw,RestoreDisplayMode
	cominvk DDraw,Release
	invoke	PostQuitMessage,0
	xor	eax,eax
    .finish:
	ret
endp

section '.idata' import data readable

  library kernel32,'KERNEL32.DLL',\
	  user32,'USER32.DLL',\
	  ddraw,'DDRAW.DLL'

  include 'api\kernel32.inc'
  include 'api\user32.inc'

  import ddraw,\
	 DirectDrawCreate,'DirectDrawCreate'
