
; flat assembler interface for DOS
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

	format	MZ
	heap	0
	stack	8000h
	entry	main:start

include 'modes.inc'

segment main use16

start:

	mov	ax,ds
	mov	dx,[2Ch]
	push	cs cs
	pop	ds es
	mov	[psp_segment],ax
	mov	[environment_segment],dx

	mov	dx,_logo
	mov	ah,9
	int	21h

	cld

	call	go32
	use32

	call	get_params
	jc	information

	call	init_memory

	mov	ah,9
	mov	dx,_memory_prefix
	call	dos_int
	mov	eax,[memory_end]
	sub	eax,[memory_start]
	add	eax,[additional_memory_end]
	sub	eax,[additional_memory]
	shr	eax,10
	call	display_number
	mov	ah,9
	mov	dx,_memory_suffix
	call	dos_int

	xor	ah,ah
	int	1Ah
	mov	ax,cx
	shl	eax,16
	mov	ax,dx
	mov	[start_time],eax

	call	preprocessor
	call	parser
	call	assembler
	call	formatter

	call	display_user_messages
	movzx	eax,[current_pass]
	inc	eax
	call	display_number
	mov	ah,9
	mov	dx,_passes_suffix
	call	dos_int
	xor	ah,ah
	int	1Ah
	mov	ax,cx
	shl	eax,16
	mov	ax,dx
	sub	eax,[start_time]
	mov	ebx,100
	mul	ebx
	mov	ebx,182
	div	ebx
	or	eax,eax
	jz	display_bytes_count
	xor	edx,edx
	mov	ebx,10
	div	ebx
	push	edx
	call	display_number
	mov	ah,2
	mov	dl,'.'
	int	21h
	pop	eax
	call	display_number
	mov	ah,9
	mov	dx,_seconds_suffix
	call	dos_int
      display_bytes_count:
	mov	eax,[written_size]
	call	display_number
	mov	ah,9
	mov	dx,_bytes_suffix
	call	dos_int
	xor	al,al
	jmp	exit_program

information:
	mov	dx,_usage
	mov	ah,9
	call	dos_int
	mov	al,1
	jmp	exit_program

get_params:
	mov	[memory_setting],0
	mov	[passes_limit],100
	push	ds
	mov	ds,[psp_segment]
	mov	esi,81h
	mov	edi,params
    find_param:
	lodsb
	cmp	al,20h
	je	find_param
	cmp	al,22h
	je	string_param
	cmp	al,'-'
	je	option_param
	cmp	al,0Dh
	je	all_params
	or	al,al
	jz	all_params
	inc	edi
	mov	ebx,edi
    copy_param:
	stosb
	lodsb
	cmp	al,20h
	je	param_end
	cmp	al,0Dh
	je	param_end
	or	al,al
	jz	param_end
	jmp	copy_param
    string_param:
	inc	edi
	mov	ebx,edi
    copy_string_param:
	lodsb
	cmp	al,22h
	je	string_param_end
	cmp	al,0Dh
	je	param_end
	or	al,al
	jz	param_end
	stosb
	jmp	copy_string_param
    option_param:
	lodsb
	cmp	al,'m'
	je	memory_option
	cmp	al,'M'
	je	memory_option
	cmp	al,'p'
	je	passes_option
	cmp	al,'P'
	je	passes_option
    invalid_option:
	pop	ds
	stc
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
	jc	bad_params_value
	cmp	al,9
	ja	bad_params_value
	imul	edx,10
	jo	bad_params_value
	add	edx,eax
	jc	bad_params_value
	jmp	get_option_digit
    option_value_ok:
	dec	esi
	clc
	ret
    bad_params_value:
	stc
	ret
    memory_option:
	lodsb
	cmp	al,20h
	je	memory_option
	cmp	al,0Dh
	je	invalid_option
	or	al,al
	jz	invalid_option
	dec	esi
	call	get_option_value
	jc	invalid_option
	or	edx,edx
	jz	invalid_option
	cmp	edx,1 shl (32-10)
	jae	invalid_option
	mov	[es:memory_setting],edx
	jmp	find_param
    passes_option:
	lodsb
	cmp	al,20h
	je	passes_option
	cmp	al,0Dh
	je	invalid_option
	or	al,al
	jz	invalid_option
	dec	esi
	call	get_option_value
	jc	bad_params
	or	edx,edx
	jz	invalid_option
	cmp	edx,10000h
	ja	invalid_option
	mov	[es:passes_limit],dx
	jmp	find_param
    param_end:
	dec	esi
    string_param_end:
	xor	al,al
	stosb
	mov	eax,edi
	sub	eax,ebx
	mov	[es:ebx-1],al
	jmp	find_param
    all_params:
	xor	al,al
	stosb
	pop	ds
	cmp	[params],0
	je	bad_params
	lea	eax,[params+1]
	mov	[input_file],eax
	mov	[output_file],0
	movzx	ecx,byte [eax-1]
	add	eax,ecx
	cmp	byte [eax],0
	je	params_ok
	inc	eax
	mov	[output_file],eax
	movzx	ecx,byte [eax-1]
	add	eax,ecx
	cmp	byte [eax],0
	jne	bad_params
    params_ok:
	clc
	ret
    bad_params:
	stc
	ret

include 'system.inc'

include '..\version.inc'

_copyright db 'Copyright (c) 1999-2007, Tomasz Grysztar',0Dh,0Ah,24h

_logo db 'flat assembler  version ',VERSION_STRING,24h
_usage db 0Dh,0Ah
       db 'usage: fasm <source> [output]',0Dh,0Ah
       db 'optional settings:',0Dh,0Ah
       db ' -m <limit>  set the limit in kilobytes for the memory available to assembler',0Dh,0Ah
       db ' -p <limit>  set the maximum allowed number of passes',0Dh,0Ah
       db 24h
_memory_prefix db '  (',24h
_memory_suffix db ' kilobytes memory)',0Dh,0Ah,24h
_passes_suffix db ' passes, ',24h
_seconds_suffix db ' seconds, ',24h
_bytes_suffix db ' bytes.',0Dh,0Ah,24h

include '..\errors.inc'
include '..\expressi.inc'
include '..\preproce.inc'
include '..\parser.inc'
include '..\assemble.inc'
include '..\formats.inc'
include '..\x86_64.inc'
include '..\tables.inc'

align 4

include '..\variable.inc'

memory_setting dd ?
start_time dd ?
params rb 100h

segment buffer

rb 1000h
