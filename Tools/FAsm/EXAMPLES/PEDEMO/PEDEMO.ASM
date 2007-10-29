
; Example of making 32-bit PE program as raw code and data

format PE GUI
entry start

section '.code' code readable executable

  start:

	push	0
	push	_caption
	push	_message
	push	0
	call	[MessageBoxA]

	push	0
	call	[ExitProcess]

section '.data' data readable writeable

  _caption db 'Win32 assembly program',0
  _message db 'Hello World!',0

section '.idata' import data readable writeable

  dd 0,0,0,RVA kernel_name,RVA kernel_table
  dd 0,0,0,RVA user_name,RVA user_table
  dd 0,0,0,0,0

  kernel_table:
    ExitProcess dd RVA _ExitProcess
    dd 0
  user_table:
    MessageBoxA dd RVA _MessageBoxA
    dd 0

  kernel_name db 'KERNEL32.DLL',0
  user_name db 'USER32.DLL',0

  _ExitProcess dw 0
    db 'ExitProcess',0
  _MessageBoxA dw 0
    db 'MessageBoxA',0

section '.reloc' fixups data readable discardable	; needed for Win32s
