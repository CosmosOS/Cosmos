
; Example of 64-bit PE program

format PE64 GUI
entry start

section '.code' code readable executable

  start:
	sub	rsp,8*5 	; reserve stack for API use and make stack dqword aligned

	mov	r9d,0
	lea	r8,[_caption]
	lea	rdx,[_message]
	mov	rcx,0
	call	[MessageBoxA]

	mov	ecx,eax
	call	[ExitProcess]

section '.data' data readable writeable

  _caption db 'Win64 assembly program',0
  _message db 'Hello World!',0

section '.idata' import data readable writeable

  dd 0,0,0,RVA kernel_name,RVA kernel_table
  dd 0,0,0,RVA user_name,RVA user_table
  dd 0,0,0,0,0

  kernel_table:
    ExitProcess dq RVA _ExitProcess
    dq 0
  user_table:
    MessageBoxA dq RVA _MessageBoxA
    dq 0

  kernel_name db 'KERNEL32.DLL',0
  user_name db 'USER32.DLL',0

  _ExitProcess dw 0
    db 'ExitProcess',0
  _MessageBoxA dw 0
    db 'MessageBoxA',0
