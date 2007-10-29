
; flat assembler interface for Unix/libc
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

	format	ELF
	public	main

macro ccall proc,[arg]
 { common
    local size
    size = 0
   reverse
    pushd arg
    size = size+4
   common
    call proc
    add esp,size }

extrn gettimeofday

section '.text' executable align 16

main:
	mov	ecx,[esp+4]
	mov	[argc],ecx
	mov	ebx,[esp+8]
	mov	[argv],ebx
	push	ebp
	mov	[stack_frame],esp

	mov	esi,_logo
	call	display_string

	call	get_params
	jc	information

	call	init_memory

	mov	esi,_memory_prefix
	call	display_string
	mov	eax,[memory_end]
	sub	eax,[memory_start]
	add	eax,[additional_memory_end]
	sub	eax,[additional_memory]
	shr	eax,10
	call	display_number
	mov	esi,_memory_suffix
	call	display_string

	ccall	gettimeofday,buffer,0
	mov	eax,dword [buffer]
	mov	ecx,1000
	mul	ecx
	mov	ebx,eax
	mov	eax,dword [buffer+4]
	div	ecx
	add	eax,ebx
	mov	[start_time],eax

	call	preprocessor
	call	parser
	call	assembler
	call	formatter

	call	display_user_messages
	movzx	eax,[current_pass]
	inc	eax
	call	display_number
	mov	esi,_passes_suffix
	call	display_string
	ccall	gettimeofday,buffer,0
	mov	eax,dword [buffer]
	mov	ecx,1000
	mul	ecx
	mov	ebx,eax
	mov	eax,dword [buffer+4]
	div	ecx
	add	eax,ebx
	sub	eax,[start_time]
	jnc	time_ok
	add	eax,3600000
      time_ok:
	xor	edx,edx
	mov	ebx,100
	div	ebx
	or	eax,eax
	jz	display_bytes_count
	xor	edx,edx
	mov	ebx,10
	div	ebx
	push	edx
	call	display_number
	mov	dl,'.'
	call	display_character
	pop	eax
	call	display_number
	mov	esi,_seconds_suffix
	call	display_string
      display_bytes_count:
	mov	eax,[written_size]
	call	display_number
	mov	esi,_bytes_suffix
	call	display_string
	xor	al,al
	jmp	exit_program

information:
	mov	esi,_usage
	call	display_string
	mov	al,1
	jmp	exit_program

get_params:
	mov	[input_file],0
	mov	[output_file],0
	mov	[memory_setting],0
	mov	[passes_limit],100
	mov	ecx,[argc]
	mov	ebx,[argv]
	add	ebx,4
	dec	ecx
	jz	bad_params
      get_param:
	mov	esi,[ebx]
	mov	al,[esi]
	cmp	al,'-'
	je	option_param
	cmp	[input_file],0
	jne	get_output_file
	mov	[input_file],esi
	jmp	next_param
      get_output_file:
	cmp	[output_file],0
	jne	bad_params
	mov	[output_file],esi
	jmp	next_param
      option_param:
	inc	esi
	lodsb
	cmp	al,'m'
	je	memory_option
	cmp	al,'M'
	je	memory_option
	cmp	al,'p'
	je	passes_option
	cmp	al,'P'
	je	passes_option
      bad_params:
	stc
	ret
      memory_option:
	cmp	byte [esi],0
	jne	get_memory_setting
	dec	ecx
	jz	bad_params
	add	ebx,4
	mov	esi,[ebx]
      get_memory_setting:
	call	get_option_value
	or	edx,edx
	jz	bad_params
	cmp	edx,1 shl (32-10)
	jae	bad_params
	mov	[memory_setting],edx
	jmp	next_param
      passes_option:
	cmp	byte [esi],0
	jne	get_passes_setting
	dec	ecx
	jz	bad_params
	add	ebx,4
	mov	esi,[ebx]
      get_passes_setting:
	call	get_option_value
	or	edx,edx
	jz	bad_params
	cmp	edx,10000h
	ja	bad_params
	mov	[passes_limit],dx
      next_param:
	add	ebx,4
	dec	ecx
	jnz	get_param
	cmp	[input_file],0
	je	bad_params
	clc
	ret
      get_option_value:
	xor	eax,eax
	mov	edx,eax
      get_option_digit:
	lodsb
	cmp	al,20h
	je	option_value_ok
	cmp	al,0Dh
	je	option_value_ok
	or	al,al
	jz	option_value_ok
	sub	al,30h
	jc	invalid_option_value
	cmp	al,9
	ja	invalid_option_value
	imul	edx,10
	jo	invalid_option_value
	add	edx,eax
	jc	invalid_option_value
	jmp	get_option_digit
      option_value_ok:
	dec	esi
	clc
	ret
      invalid_option_value:
	stc
	ret

include 'system.inc'

include '..\version.inc'

_copyright db 'Copyright (c) 1999-2007, Tomasz Grysztar',0xA,0

_logo db 'flat assembler  version ',VERSION_STRING,0
_usage db 0xA
       db 'usage: fasm <source> [output]',0xA
       db 'optional settings:',0xA
       db ' -m <limit>  set the limit in kilobytes for the memory available to assembler',0xA
       db ' -p <limit>  set the maximum allowed number of passes',0xA
       db 0
_memory_prefix db '  (',0
_memory_suffix db ' kilobytes memory)',0xA,0
_passes_suffix db ' passes, ',0
_seconds_suffix db ' seconds, ',0
_bytes_suffix db ' bytes.',0xA,0

include '..\errors.inc'
include '..\expressi.inc'
include '..\preproce.inc'
include '..\parser.inc'
include '..\assemble.inc'
include '..\formats.inc'
include '..\x86_64.inc'
include '..\tables.inc'

section '.bss' writeable align 4

include '..\variable.inc'

argc dd ?
argv dd ?
stack_frame dd ?
memory_setting dd ?
start_time dd ?
timestamp dd ?
displayed_count dd ?
last_displayed db ?

buffer rb 1000h
