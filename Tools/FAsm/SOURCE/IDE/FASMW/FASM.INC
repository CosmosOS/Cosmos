
; flat assembler interface for Win32 IDE
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

flat_assembler:

	mov	edx,[esp+4]
	mov	[input_file],edx
	invoke	GetFullPathName,edx,1000h,path_buffer,param_buffer
	mov	edi,[param_buffer]
	xor	al,al
	stosb
	invoke	SetCurrentDirectory,path_buffer
	mov	[hfile],0
	invoke	GlobalAlloc,GMEM_MOVEABLE,1
	mov	[hmem_display],eax
	invoke	GlobalLock,[hmem_display]
	mov	byte [eax],0
	invoke	GlobalUnlock,[hmem_display]
	mov	[display_size],1
	mov	[error_data_size],0
	mov	eax,esp
	and	eax,not 0FFFh
	add	eax,1000h-10000h
	mov	[stack_limit],eax
	mov	[allocated_memory],0
	mov	eax,[compiler_memory]
	shl	eax,10
	jz	out_of_memory
      allocate_memory:
	mov	edx,eax
	shr	edx,2
	mov	ecx,eax
	sub	ecx,edx
	mov	[memory_end],ecx
	mov	[additional_memory_end],edx
	invoke	VirtualAlloc,0,eax,MEM_COMMIT,PAGE_READWRITE
	or	eax,eax
	jnz	memory_allocated
	mov	eax,[additional_memory_end]
	shl	eax,1
	cmp	eax,4000h
	jb	out_of_memory
	jmp	allocate_memory
      memory_allocated:
	mov	[allocated_memory],eax
	mov	[memory_start],eax
	mov	[code_start],eax
	add	eax,[memory_end]
	mov	[memory_end],eax
	mov	[additional_memory],eax
	add	[additional_memory_end],eax
	mov	[display_buffer],0
	mov	[passes_limit],100

	invoke	SendMessage,[hwnd_progress],PBM_SETPOS,0,0
	invoke	SetThreadPriority,[hthread],[compiler_priority]
	invoke	GetTickCount
	mov	[start_time],eax
	mov	[preprocessing_done],0
	call	preprocessor
	invoke	SendMessage,[hwnd_progress],PBM_SETPOS,1,0
	or	[preprocessing_done],-1
	call	parser
	invoke	SendMessage,[hwnd_progress],PBM_SETPOS,2,0
	call	assembler
	invoke	SendMessage,[hwnd_progress],PBM_SETPOS,3,0
	call	formatter
	invoke	SendMessage,[hwnd_progress],PBM_SETPOS,4,0
	call	flush_display_buffer
	invoke	GetTickCount
	sub	eax,[start_time]
	mov	[total_time],eax
	mov	esi,[output_file]
	mov	edi,path_buffer
      copy_executable_name:
	lodsb
	stosb
	or	al,al
	jnz	copy_executable_name
	xor	al,al

exit_program:
	movzx	eax,al
	push	eax
	mov	eax,[allocated_memory]
	or	eax,eax
	jz	memory_ok
	invoke	VirtualFree,eax,0,MEM_RELEASE
	mov	[allocated_memory],0
      memory_ok:
	mov	eax,[hfile]
	or	eax,eax
	jz	handle_ok
	invoke	CloseHandle,eax
      handle_ok:
	invoke	PostMessage,[hwnd_compiler],WM_COMMAND,IDOK,0
	call	[ExitThread]

get_environment_variable:
	invoke	GetEnvironmentVariable,esi,path_buffer,1000h
	mov	ecx,[memory_end]
	sub	ecx,edi
	cmp	ecx,1000h
	jbe	get_local_variable
	mov	ecx,1000h
      get_local_variable:
	invoke	GetPrivateProfileString,_section_environment,esi,path_buffer,edi,ecx,ini_path
	add	edi,eax
	cmp	edi,[memory_end]
	jae	out_of_memory
	retn

open:
	invoke	CreateFile,edx,GENERIC_READ,FILE_SHARE_READ,0,OPEN_EXISTING,0,0
	cmp	eax,-1
	je	file_error
	mov	[hfile],eax
	mov	ebx,eax
	clc
	retn
    file_error:
	stc
	retn
create:
	invoke	CreateFile,edx,GENERIC_WRITE,0,0,CREATE_ALWAYS,0,0
	cmp	eax,-1
	je	file_error
	mov	ebx,eax
	clc
	retn
write:
	invoke	WriteFile,ebx,edx,ecx,bytes_count,0
	or	eax,eax
	jz	file_error
	clc
	retn
read:
	mov	ebp,ecx
	invoke	ReadFile,ebx,edx,ecx,bytes_count,0
	or	eax,eax
	jz	file_error
	cmp	ebp,[bytes_count]
	jne	file_error
	clc
	retn
close:
	cmp	ebx,[hfile]
	jne	close_handle
	mov	[hfile],0
      close_handle:
	invoke	CloseHandle,ebx
	retn
lseek:
	movzx	eax,al
	invoke	SetFilePointer,ebx,edx,0,eax
	cmp	eax,-1
	je	file_error
	retn

display_block:
	push	edi
	push	ecx
	add	ecx,[display_size]
	invoke	GlobalReAlloc,[hmem_display],ecx,GMEM_MOVEABLE
	or	eax,eax
	jz	out_of_memory
	mov	[hmem_display],eax
	invoke	GlobalLock,[hmem_display]
	add	eax,[display_size]
	lea	edi,[eax-1]
	pop	ecx
	add	[display_size],ecx
	rep	movsb
	xor	al,al
	stosb
	invoke	GlobalUnlock,[hmem_display]
	pop	edi
	retn
fatal_error:
	pop	[error_message]
	mov	al,0FFh
	jmp	exit_program
assembler_error:
	call	flush_display_buffer
	pop	[error_message]
	mov	ebx,[current_line]
	xor	ecx,ecx
      get_error_lines:
	test	byte [ebx+7],80h
	jz	error_lines_ok
	inc	ecx
	mov	edx,ebx
      find_definition_origin:
	mov	edx,[edx+12]
	test	byte [edx+7],80h
	jnz	find_definition_origin
	mov	eax,[edx+4]
	and	eax,7FFFFFFFh
	push	eax
	mov	edx,[edx]
	push	edx
	mov	ebx,[ebx+8]
	jmp	get_error_lines
      error_lines_ok:
	inc	ecx
	mov	eax,[ebx+4]
	and	eax,7FFFFFFFh
	push	eax
	mov	edx,[ebx]
	push	edx
	mov	ebx,ecx
	inc	ecx
	shl	ecx,3
	mov	[error_data_size],ecx
	invoke	GlobalAlloc,GMEM_MOVEABLE,ecx
	mov	[hmem_error_data],eax
	invoke	GlobalLock,[hmem_error_data]
	mov	[eax],ebx
	invoke	GlobalUnlock,[hmem_error_data]
	xor	ebx,ebx
      store_error_lines:
	pop	edx
	invoke	GetFullPathName,edx,1000h,path_buffer,param_buffer
	inc	eax
	mov	esi,eax
	add	eax,[error_data_size]
	invoke	GlobalReAlloc,[hmem_error_data],eax,GMEM_MOVEABLE
	invoke	GlobalLock,[hmem_error_data]
	mov	edi,eax
	add	edi,[error_data_size]
	mov	ecx,esi
	mov	esi,path_buffer
	rep	movsb
	pop	edx
	mov	[eax+8+ebx*8+4],edx
	sub	edi,eax
	xchg	[error_data_size],edi
	mov	[eax+8+ebx*8],edi
	mov	esi,[eax]
	invoke	GlobalUnlock,[hmem_error_data]
	inc	ebx
	cmp	ebx,esi
	jb	store_error_lines
	mov	edi,[additional_memory]
	cmp	[preprocessing_done],0
	jne	error_in_preprocessed
	xor	al,al
	stosb
	jmp	instruction_converted
      error_in_preprocessed:
	mov	esi,[current_line]
	add	esi,16
	xor	dl,dl
      convert_instruction:
	lodsb
	cmp	al,1Ah
	je	copy_symbol
	cmp	al,22h
	je	copy_symbol
	cmp	al,3Bh
	je	ignore_preprocessor_symbols
	stosb
	or	al,al
	jz	instruction_converted
	xor	dl,dl
	jmp	convert_instruction
      copy_symbol:
	or	dl,dl
	jz	space_ok
	mov	byte [edi],20h
	inc	edi
      space_ok:
	cmp	al,22h
	je	quoted
	lodsb
	movzx	ecx,al
	rep	movsb
	or	dl,-1
	jmp	convert_instruction
      quoted:
	mov	al,27h
	stosb
	lodsd
	mov	ecx,eax
	jecxz	quoted_copied
      copy_quoted:
	lodsb
	stosb
	cmp	al,27h
	jne	quote_ok
	stosb
      quote_ok:
	loop	copy_quoted
      quoted_copied:
	mov	al,27h
	stosb
	or	dl,-1
	jmp	convert_instruction
      ignore_preprocessor_symbols:
	xor	al,al
	stosb
      instruction_converted:
	sub	edi,[additional_memory]
	mov	ebx,[error_data_size]
	lea	eax,[ebx+edi]
	invoke	GlobalReAlloc,[hmem_error_data],eax,GMEM_MOVEABLE
	invoke	GlobalLock,[hmem_error_data]
	mov	ecx,edi
	mov	[eax+4],ebx
	lea	edi,[eax+ebx]
	mov	esi,[additional_memory]
	rep	movsb
	invoke	GlobalUnlock,[hmem_error_data]
	mov	al,2
	jmp	exit_program

make_timestamp:
	invoke	GetSystemTime,systime
	movzx	ecx,[systime.wYear]
	mov	eax,ecx
	sub	eax,1970
	mov	ebx,365
	mul	ebx
	mov	ebp,eax
	mov	eax,ecx
	sub	eax,1969
	shr	eax,2
	add	ebp,eax
	mov	eax,ecx
	sub	eax,1901
	mov	ebx,100
	div	ebx
	sub	ebp,eax
	mov	eax,ecx
	xor	edx,edx
	sub	eax,1601
	mov	ebx,400
	div	ebx
	add	ebp,eax
	movzx	ecx,[systime.wMonth]
	mov	eax,ecx
	dec	eax
	mov	ebx,30
	mul	ebx
	add	ebp,eax
	cmp	ecx,8
	jbe	months_correction
	mov	eax,ecx
	sub	eax,7
	shr	eax,1
	add	ebp,eax
	mov	ecx,8
      months_correction:
	mov	eax,ecx
	shr	eax,1
	add	ebp,eax
	cmp	ecx,2
	jbe	day_correction_ok
	sub	ebp,2
	movzx	ecx,word [systime.wYear]
	test	ecx,11b
	jnz	day_correction_ok
	xor	edx,edx
	mov	eax,ecx
	mov	ebx,100
	div	ebx
	or	edx,edx
	jnz	day_correction
	mov	eax,ecx
	mov	ebx,400
	div	ebx
	or	edx,edx
	jnz	day_correction_ok
      day_correction:
	inc	ebp
      day_correction_ok:
	movzx	eax,[systime.wDay]
	dec	eax
	add	eax,ebp
	mov	ebx,24
	mul	ebx
	movzx	ecx,[systime.wHour]
	add	eax,ecx
	mov	ebx,60
	mul	ebx
	movzx	ecx,[systime.wMinute]
	add	eax,ecx
	mov	ebx,60
	mul	ebx
	movzx	ecx,[systime.wSecond]
	add	eax,ecx
	retn

include '..\..\errors.inc'
include '..\..\expressi.inc'
include '..\..\preproce.inc'
include '..\..\parser.inc'
include '..\..\assemble.inc'
include '..\..\formats.inc'
include '..\..\x86_64.inc'
include '..\..\tables.inc'

align 4

include '..\..\variable.inc'

allocated_memory dd ?
start_time dd ?
total_time dd ?
display_size dd ?
error_message dd ?
error_data_size dd ?
preprocessing_done db ?
