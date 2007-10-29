
; flat assembler interface for DOS
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

go32:
	use16
	call	modes:real32
	use32
	retw

init_memory:
	mov	[stack_limit],0
	cmp	[mode],dpmi
	je	init_dpmi_memory
	mov	ax,4300h		; check for XMS
	int	2Fh
	cmp	al,80h			; XMS present?
	je	xms_init
	mov	ax,0E801h		; check for large free extended memory
	int	15h
	jnc	large_raw_memory
	mov	ah,88h			; how much extended memory free?
	int	15h
	or	ax,ax
	jz	no_extended_memory
	movzx	eax,ax			; convert AX kilobytes to pointer
	shl	eax,10
	jmp	use_raw_memory
    large_raw_memory:
	movzx	ecx,cx
	shl	ecx,10
	movzx	edx,dx
	shl	edx,16
	mov	eax,ecx
	add	eax,edx
    use_raw_memory:
	add	eax,100000h
	sub	eax,[program_base]
	mov	[memory_end],eax
	push	ds
	push	0			; DS := 0
	pop	ds
	call	enable_a20		; enable A20
	call	test_a20		; is A20 enabled?
	jz	a20_ok
	pop	ds
	jmp	no_extended_memory
    a20_ok:
	lds	bx,dword [4*19h]
	mov	eax,100000h		; initial free extended memory base
	cmp	dword [bx+12h],'VDIS'	; VDISK memory allocation?
	jne	short no_vdisk		; if present, get base of free memory
	mov	eax,dword [bx+2Ch]	; get first free extended memory byte
	add	eax,0Fh 		; align on paragraph
	and	eax,0FFFFF0h		; address is only 24bit
    no_vdisk:
	push	0FFFFh			; DS := FFFFh for ext mem addressing
	pop	ds
	cmp	dword [13h],'VDIS'	; VDISK memory allocation?
	jne	short vdisk_ok		; if present, get base of free memory
	movzx	ebx,word [2Eh]		; get first free kilobyte
	shl	ebx,10
	cmp	eax,ebx 		; pick larger of 2 addresses
	ja	short vdisk_ok
	mov	eax,ebx
    vdisk_ok:
	pop	ds
	sub	eax,[program_base]
	mov	[memory_start],eax
	mov	edx,[memory_setting]
	shl	edx,10
	jz	extended_memory_ok
	mov	eax,[memory_end]
	sub	eax,[memory_start]
	sub	eax,edx
	jbe	extended_memory_ok
	sub	[memory_end],eax
	jmp	extended_memory_ok
enable_a20:
	call	test_a20		; is A20 already enabled?
	jz	a20_enabled		; if yes, done
	in	al,92h			; PS/2 A20 enable
	or	al,2
	out	92h,al
	call	test_a20		; is A20 enabled?
	jz	a20_enabled		; if yes, done
	call	kb_wait 		; AT A20 enable
	jnz	a20_enabled
	mov	al,0D1h
	out	64h,al
	call	kb_wait
	jnz	a20_enabled
	mov	al,0DFh
	out	60h,al
	call	kb_wait
    a20_enabled:
	ret
    kb_wait:				; wait for safe to write to 8042
	xor	cx,cx
      .loop:
	in	al,64h			; read 8042 status
	test	al,2			; buffer full?
	loopnz	.loop			; if yes, loop
	ret
    test_a20:				; test for enabled A20
	mov	al,[0]			; get byte from 0:0
	mov	ah,al			; preserve old byte
	not	al			; modify byte
	xchg	al,[100000h]		; put modified byte to 0FFFFh:10h
	cmp	ah,[0]			; set zero if byte at 0:0 not modified
	mov	[100000h],al		; restore byte at 0FFFFh:10h
	ret				; return, zero if A20 enabled
xms_init:
	push	es
	mov	ax,4310h		; get XMS driver address
	int	2Fh
	mov	word [xms_proc],bx	; store XMS driver address
	mov	word [xms_proc+2],es
	pop	es
	mov	ah,3			; enable A20
	call	call_xms
	cmp	ax,1			; error enabling A20?
	jne	no_extended_memory
	mov	ah,88h			; get free extended memory size (XMS 3.0)
	xor	bl,bl
	call	call_xms
	or	bl,bl
	jz	xms_large_init
	mov	ah,8			; get free extended memory size
	xor	bl,bl
	call	call_xms
	or	bl,bl
	jnz	no_extended_memory
	mov	dx,ax
	movzx	eax,ax
	shl	eax,10
	mov	[memory_end],eax
	mov	ah,9			; allocate largest memory block
    xms_allocate:
	mov	ecx,[memory_setting]
	shl	ecx,10
	jz	xms_size_ok
	cmp	ecx,[memory_end]
	jae	xms_size_ok
	mov	[memory_end],ecx
	mov	edx,[memory_setting]
    xms_size_ok:
	call	call_xms
	mov	[xms_handle],dx
	cmp	ax,1
	jne	no_extended_memory
	mov	ah,0Ch			; lock extended memory block
	call	call_xms
	cmp	ax,1
	jne	no_extended_memory
	shl	edx,16
	mov	dx,bx
	sub	edx,[program_base]
	mov	[memory_start],edx	; store memory block address
	add	[memory_end],edx
	jmp	extended_memory_ok
    xms_large_init:
	mov	edx,eax
	shl	eax,10
	mov	[memory_end],eax
	mov	ah,89h			; allocate largest memory block (XMS 3.0)
	jmp	xms_allocate
    call_xms:
	call	modes:switch_real16
	use16
	call	far dword [xms_proc]
	call	modes:switch_real32
	use32
	ret
no_extended_memory:
	xor	eax,eax
	mov	[memory_start],eax
extended_memory_ok:
	mov	ah,48h			; get free conventional memory size
	mov	bx,-1
	int	21h
	movzx	ecx,bx
	shl	ecx,4
	mov	ah,48h			; allocate all conventional memory
	int	21h
	movzx	edi,ax
	shl	edi,4
	sub	edi,[program_base]
	mov	[additional_memory],edi
	mov	[additional_memory_end],edi
	add	[additional_memory_end],ecx
	cmp	[memory_start],0
	je	only_conventional_memory
	mov	eax,[memory_end]
	sub	eax,[memory_start]
	shr	eax,2
	cmp	eax,ecx
	ja	no_conventional_memory
	ret
    only_conventional_memory:
	shr	ecx,2			; use part of conventional memory
	add	edi,ecx 		; as a substitute for extended memory
	mov	[memory_start],edi
	xchg	[additional_memory_end],edi
	mov	[memory_end],edi
	ret

init_dpmi_memory:
	mov	ax,500h 		; get free memory information
	mov	edi,[buffer_address]
	int	31h
	mov	ebx,[edi]
    allocate_dpmi_memory:
	mov	edx,[memory_setting]
	shl	edx,10
	jz	dpmi_memory_size_ok
	cmp	ebx,edx
	jbe	dpmi_memory_size_ok
	mov	ebx,edx
    dpmi_memory_size_ok:
	mov	[memory_end],ebx
	mov	ecx,ebx
	shr	ebx,16
	mov	ax,501h
	int	31h
	jnc	dpmi_memory_ok
	mov	ebx,[memory_end]
	shr	ebx,1
	cmp	ebx,4000h
	jb	out_of_memory
	jmp	allocate_dpmi_memory
    dpmi_memory_ok:
	shl	ebx,16
	mov	bx,cx
	sub	ebx,[program_base]
	jc	out_of_memory
	mov	[memory_start],ebx
	add	[memory_end],ebx
	mov	ax,100h 		; get free conventional memory size
	mov	bx,-1
	int	31h
	movzx	ecx,bx
	shl	ecx,4
	jecxz	no_conventional_memory
	mov	ax,100h 		; allocate all conventional memory
	int	31h
	movzx	edi,ax
	shl	edi,4
	sub	edi,[program_base]
	jc	no_conventional_memory
	mov	[additional_memory],edi
	mov	[additional_memory_end],edi
	add	[additional_memory_end],ecx
	mov	eax,[memory_end]
	sub	eax,[memory_start]
	shr	eax,2
	cmp	eax,ecx
	ja	no_conventional_memory
	ret
    no_conventional_memory:
	mov	eax,[memory_end]
	mov	ebx,[memory_start]
	sub	eax,ebx
	shr	eax,2
	mov	[additional_memory],ebx
	add	ebx,eax
	mov	[additional_memory_end],ebx
	mov	[memory_start],ebx
	ret

dos_int:
	cmp	[mode],dpmi
	je	dpmi_dos_int
	stc
	int	21h
	ret
    dpmi_dos_int:
	mov	[real_mode_segment],main
    simulate_real_mode:
	push	0			; SS:SP (DPMI will allocate stack)
	push	0			; CS:IP (ignored)
	push	0
	push	[real_mode_segment]	; DS
	push	[real_mode_segment]	; ES
	stc
	pushfw
	push	eax
	push	ecx
	push	edx
	push	ebx
	push	0
	push	ebp
	push	esi
	push	edi
	mov	ax,300h
	mov	bx,21h
	xor	cx,cx
	mov	edi,esp
	push	es ss
	pop	es
	int	31h
	pop	es
	mov	edi,[esp]
	mov	esi,[esp+4]
	mov	ebp,[esp+8]
	mov	ebx,[esp+10h]
	mov	edx,[esp+14h]
	mov	ecx,[esp+18h]
	mov	ah,[esp+20h]
	add	esp,32h
	sahf
	mov	eax,[esp-32h+1Ch]
	ret
dos_int_with_buffer:
	cmp	[mode],dpmi
	je	dpmi_dos_int_with_buffer
	push	ds buffer
	pop	ds
	stc
	int	21h
	pop	ds
	ret
    dpmi_dos_int_with_buffer:
	mov	[real_mode_segment],buffer
	jmp	simulate_real_mode

exit_program:
	cmp	[mode],dpmi
	je	.exit
	cmp	[xms_handle],0
	je	.exit
	push	ax
	mov	ah,0Dh			; unlock extended memory block
	mov	dx,[xms_handle]
	call	call_xms
	mov	ah,0Ah			; free extended memory block
	call	call_xms
	pop	ax
      .exit:
	mov	ah,4Ch
	int	21h

xms_proc dd ?				; XMS driver pointer
xms_handle dw ? 			; handle of XMS memory block

get_environment_variable:
	mov	ebx,esi
	push	ds
	mov	ds,[environment_segment]
	xor	esi,esi
      compare_variable_names:
	mov	edx,ebx
      compare_character:
	lodsb
	mov	ah,[es:edx]
	inc	edx
	cmp	al,'='
	je	end_of_variable_name
	or	ah,ah
	jz	next_variable
	sub	ah,al
	jz	compare_character
	cmp	ah,20h
	jne	next_variable
	cmp	al,41h
	jb	next_variable
	cmp	al,5Ah
	jna	compare_character
      next_variable:
	lodsb
	or	al,al
	jnz	next_variable
	cmp	byte [esi],0
	jne	compare_variable_names
	pop	ds
	ret
      end_of_variable_name:
	or	ah,ah
	jnz	next_variable
      copy_variable_value:
	lodsb
	cmp	edi,[es:memory_end]
	jae	out_of_memory
	stosb
	or	al,al
	jnz	copy_variable_value
	dec	edi
	pop	ds
	ret

open:
	push	esi edi
	call	adapt_path
	mov	ax,716Ch
	mov	bx,100000b
	mov	dx,1
	xor	cx,cx
	xor	si,si
	call	dos_int_with_buffer
	jnc	open_done
	cmp	ax,7100h
	je	old_open
	stc
	jmp	open_done
      old_open:
	mov	ax,3D00h
	xor	dx,dx
	call	dos_int_with_buffer
      open_done:
	mov	bx,ax
	pop	edi esi
	ret
    adapt_path:
	mov	esi,edx
	mov	edi,[buffer_address]
      copy_path:
	lodsb
	cmp	al,'/'
	jne	path_char_ok
	mov	al,'\'
      path_char_ok:
	stosb
	or	al,al
	jnz	copy_path
	ret
create:
	push	esi edi
	call	adapt_path
	mov	ax,716Ch
	mov	bx,100001b
	mov	dx,10010b
	xor	cx,cx
	xor	si,si
	xor	di,di
	call	dos_int_with_buffer
	jnc	create_done
	cmp	ax,7100h
	je	old_create
	stc
	jmp	create_done
      old_create:
	mov	ah,3Ch
	xor	cx,cx
	xor	dx,dx
	call	dos_int_with_buffer
      create_done:
	mov	bx,ax
	pop	edi esi
	ret
write:
	push	edx esi edi ebp
	mov	ebp,ecx
	mov	esi,edx
      .loop:
	mov	ecx,1000h
	sub	ebp,1000h
	jnc	.write
	add	ebp,1000h
	mov	ecx,ebp
	xor	ebp,ebp
      .write:
	push	ecx
	mov	edi,[buffer_address]
	shr	ecx,2
	rep	movsd
	mov	ecx,[esp]
	and	ecx,11b
	rep	movsb
	pop	ecx
	mov	ah,40h
	xor	dx,dx
	call	dos_int_with_buffer
	or	ebp,ebp
	jnz	.loop
	pop	ebp edi esi edx
	ret
read:
	push	edx esi edi ebp
	mov	ebp,ecx
	mov	edi,edx
      .loop:
	mov	ecx,1000h
	sub	ebp,1000h
	jnc	.read
	add	ebp,1000h
	mov	ecx,ebp
	xor	ebp,ebp
      .read:
	push	ecx
	mov	ah,3Fh
	xor	dx,dx
	call	dos_int_with_buffer
	cmp	ax,cx
	jne	.eof
	mov	esi,[buffer_address]
	mov	ecx,[esp]
	shr	ecx,2
	rep	movsd
	pop	ecx
	and	ecx,11b
	rep	movsb
	or	ebp,ebp
	jnz	.loop
      .exit:
	pop	ebp edi esi edx
	ret
      .eof:
	pop	ecx
	stc
	jmp	.exit
close:
	mov	ah,3Eh
	int	21h
	ret
lseek:
	mov	ah,42h
	mov	ecx,edx
	shr	ecx,16
	int	21h
	pushf
	shl	edx,16
	popf
	mov	dx,ax
	mov	eax,edx
	ret

display_string:
	lods	byte [esi]
	or	al,al
	jz	string_end
	mov	dl,al
	mov	ah,2
	int	21h
	jmp	display_string
      string_end:
	ret
display_number:
	push	ebx
	mov	ecx,1000000000
	xor	edx,edx
	xor	bl,bl
      display_loop:
	div	ecx
	push	edx
	cmp	ecx,1
	je	display_digit
	or	bl,bl
	jnz	display_digit
	or	al,al
	jz	digit_ok
	not	bl
      display_digit:
	mov	dl,al
	add	dl,30h
	mov	ah,2
	int	21h
      digit_ok:
	mov	eax,ecx
	xor	edx,edx
	mov	ecx,10
	div	ecx
	mov	ecx,eax
	pop	eax
	or	ecx,ecx
	jnz	display_loop
	pop	ebx
	ret

display_user_messages:
	mov	[displayed_count],0
	call	flush_display_buffer
	cmp	[displayed_count],1
	jb	line_break_ok
	je	make_line_break
	mov	ax,word [last_displayed]
	cmp	ax,0A0Dh
	je	line_break_ok
	cmp	ax,0D0Ah
	je	line_break_ok
      make_line_break:
	mov	ah,2
	mov	dl,0Dh
	int	21h
	mov	dl,0Ah
	int	21h
      line_break_ok:
	ret
display_block:
	add	[displayed_count],ecx
	cmp	ecx,1
	ja	take_last_two_characters
	jb	display_character
	mov	al,[last_displayed+1]
	mov	ah,[esi]
	mov	word [last_displayed],ax
	jmp	display_character
      take_last_two_characters:
	mov	ax,[esi+ecx-2]
	mov	word [last_displayed],ax
      display_character:
	lods	byte [esi]
	mov	dl,al
	mov	ah,2
	int	21h
	loopd	display_character
	ret

fatal_error:
	mov	dx,error_prefix
	mov	ah,9
	call	dos_int
	pop	esi
	call	display_string
	mov	dx,error_suffix
	mov	ah,9
	call	dos_int
	mov	al,0FFh
	jmp	exit_program
assembler_error:
	call	display_user_messages
	push	dword 0
	mov	ebx,[current_line]
      get_error_lines:
	push	ebx
	test	byte [ebx+7],80h
	jz	display_error_line
	mov	edx,ebx
      find_definition_origin:
	mov	edx,[edx+12]
	test	byte [edx+7],80h
	jnz	find_definition_origin
	push	edx
	mov	ebx,[ebx+8]
	jmp	get_error_lines
      display_error_line:
	mov	esi,[ebx]
	call	display_string
	mov	dx,line_number_start
	mov	ah,9
	call	dos_int
	mov	eax,[ebx+4]
	and	eax,7FFFFFFFh
	call	display_number
	mov	dl,']'
	mov	ah,2
	int	21h
	pop	esi
	cmp	ebx,esi
	je	line_number_ok
	mov	dl,20h
	mov	ah,2
	int	21h
	push	esi
	mov	esi,[esi]
	movzx	ecx,byte [esi]
	inc	esi
	call	display_block
	mov	dx,line_number_start
	mov	ah,9
	call	dos_int
	pop	esi
	mov	eax,[esi+4]
	and	eax,7FFFFFFFh
	call	display_number
	mov	dl,']'
	mov	ah,2
	int	21h
      line_number_ok:
	mov	dx,line_data_start
	mov	ah,9
	call	dos_int
	mov	esi,ebx
	mov	edx,[esi]
	call	open
	mov	al,2
	xor	edx,edx
	call	lseek
	mov	edx,[esi+8]
	sub	eax,edx
	mov	[counter],eax
	xor	al,al
	call	lseek
	mov	esi,[additional_memory]
      read_line_data:
	mov	ecx,100h
	cmp	ecx,[counter]
	jbe	bytes_count_ok
	mov	ecx,[counter]
      bytes_count_ok:
	sub	[counter],ecx
	lea	eax,[esi+ecx]
	cmp	eax,[additional_memory_end]
	ja	out_of_memory
	push	ecx
	mov	edx,esi
	call	read
	pop	ecx
      get_line_data:
	mov	al,[esi]
	cmp	al,0Ah
	je	display_line_data
	cmp	al,0Dh
	je	display_line_data
	cmp	al,1Ah
	je	display_line_data
	or	al,al
	jz	display_line_data
	inc	esi
	loop	get_line_data
	cmp	[counter],0
	ja	read_line_data
      display_line_data:
	call	close
	mov	ecx,esi
	mov	esi,[additional_memory]
	sub	ecx,esi
	call	display_block
	mov	dx,cr_lf
	mov	ah,9
	call	dos_int
	pop	ebx
	or	ebx,ebx
	jnz	display_error_line
	mov	dx,error_prefix
	call	dos_int
	pop	esi
	call	display_string
	mov	dx,error_suffix
	mov	ah,9
	call	dos_int
	mov	al,2
	jmp	exit_program

make_timestamp:
	mov	ah,2Ah
	int	21h
	push	dx cx
	movzx	ecx,cx
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
	movzx	ecx,byte [esp+3]
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
	pop	cx
	jbe	day_correction_ok
	sub	ebp,2
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
	pop	dx
	movzx	eax,dl
	dec	eax
	add	eax,ebp
	mov	ebx,24
	mul	ebx
	push	eax
	mov	ah,2Ch
	int	21h
	pop	eax
	push	dx
	movzx	ebx,ch
	add	eax,ebx
	mov	ebx,60
	mul	ebx
	movzx	ebx,cl
	add	eax,ebx
	mov	ebx,60
	mul	ebx
	pop	dx
	movzx	ebx,dh
	add	eax,ebx
	ret

program_base dd ?
buffer_address dd ?
psp_segment dw ?
environment_segment dw ?
mode dw ?
real_mode_segment dw ?
displayed_count dd ?
last_displayed rb 2

error_prefix db 'error: ',24h
error_suffix db '.'
cr_lf db 0Dh,0Ah,24h
line_number_start db ' [',24h
line_data_start db ':',0Dh,0Ah,24h
