
; flat assembler core
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

formatter:
	cmp	[output_file],0
	jne	output_path_ok
	push	edi
	mov	esi,[input_file]
	mov	edi,[free_additional_memory]
      copy_output_path:
	lods	byte [esi]
	cmp	edi,[structures_buffer]
	jae	out_of_memory
	stos	byte [edi]
	or	al,al
	jnz	copy_output_path
	dec	edi
	mov	eax,edi
      find_extension:
	dec	eax
	cmp	eax,[free_additional_memory]
	jb	extension_found
	cmp	byte [eax],'\'
	je	extension_found
	cmp	byte [eax],'/'
	je	extension_found
	cmp	byte [eax],'.'
	jne	find_extension
	mov	edi,eax
      extension_found:
	lea	eax,[edi+9]
	cmp	eax,[structures_buffer]
	jae	out_of_memory
	cmp	[file_extension],0
	jne	extension_specified
	cmp	[output_format],2
	je	exe_extension
	jb	bin_extension
	cmp	[output_format],4
	je	obj_extension
	cmp	[output_format],5
	je	o_extension
	cmp	[output_format],3
	jne	no_extension
	cmp	[subsystem],1
	je	sys_extension
	bt	[format_flags],8
	jnc	exe_extension
	mov	eax,'.dll'
	jmp	make_extension
      sys_extension:
	mov	eax,'.sys'
	jmp	make_extension
      bin_extension:
	mov	eax,'.bin'
	bt	[format_flags],0
	jnc	make_extension
	mov	eax,'.com'
	jmp	make_extension
      obj_extension:
	mov	eax,'.obj'
	jmp	make_extension
      o_extension:
	mov	eax,'.o'
	bt	[format_flags],0
	jnc	make_extension
      no_extension:
	xor	eax,eax
	jmp	make_extension
      exe_extension:
	mov	eax,'.exe'
      make_extension:
	xchg	eax,[edi]
	scas	dword [edi]
	mov	byte [edi],0
	scas	byte [edi]
	mov	esi,edi
	stos	dword [edi]
	sub	edi,9
	xor	eax,eax
	mov	ebx,characters
      adapt_case:
	mov	al,[esi]
	or	al,al
	jz	adapt_next
	xlat	byte [ebx]
	cmp	al,[esi]
	je	adapt_ok
	sub	byte [edi],20h
      adapt_ok:
	inc	esi
      adapt_next:
	inc	edi
	cmp	byte [edi],0
	jne	adapt_case
	jmp	extension_ok
      extension_specified:
	mov	al,'.'
	stos	byte [edi]
	mov	esi,[file_extension]
      copy_extension:
	lods	byte [esi]
	stos	byte [edi]
	test	al,al
	jnz	copy_extension
	dec	edi
      extension_ok:
	mov	esi,edi
	lea	ecx,[esi+1]
	sub	ecx,[free_additional_memory]
	mov	edi,[structures_buffer]
	dec	edi
	std
	rep	movs byte [edi],[esi]
	cld
	inc	edi
	mov	[structures_buffer],edi
	mov	[output_file],edi
	pop	edi
      output_path_ok:
	cmp	[output_format],4
	je	coff_formatter
	cmp	[output_format],5
	jne	common_formatter
	bt	[format_flags],0
	jnc	elf_formatter
      common_formatter:
	mov	eax,edi
	sub	eax,[code_start]
	mov	[real_code_size],eax
	cmp	edi,[undefined_data_end]
	jne	calculate_code_size
	mov	edi,[undefined_data_start]
      calculate_code_size:
	sub	edi,[code_start]
	mov	[code_size],edi
	mov	[written_size],0
	mov	edx,[output_file]
	call	create
	jc	write_failed
	cmp	[output_format],3
	jne	stub_written
	mov	edx,[code_start]
	mov	ecx,[stub_size]
	sub	edx,ecx
	add	[written_size],ecx
	call	write
      stub_written:
	cmp	[output_format],2
	jne	write_output
	call	write_mz_header
      write_output:
	call	write_code
      output_written:
	call	close
	ret
      write_code:
	mov	eax,[written_size]
	mov	[headers_size],eax
	mov	edx,[code_start]
	mov	ecx,[code_size]
	add	[written_size],ecx
	call	write
	jc	write_failed
	ret
format_directive:
	cmp	edi,[code_start]
	jne	unexpected_instruction
	cmp	[virtual_data],0
	jne	unexpected_instruction
	cmp	[output_format],0
	jne	unexpected_instruction
	lods	byte [esi]
	cmp	al,17h
	je	format_prefix
	cmp	al,18h
	jne	invalid_argument
	lods	byte [esi]
      select_format:
	mov	dl,al
	shr	al,4
	mov	[output_format],al
	and	edx,0Fh
	or	[format_flags],edx
	cmp	al,2
	je	format_mz
	cmp	al,3
	je	format_pe
	cmp	al,4
	je	format_coff
	cmp	al,5
	je	format_elf
      format_defined:
	cmp	byte [esi],86h
	jne	instruction_assembled
	cmp	word [esi+1],'('
	jne	invalid_argument
	mov	eax,[esi+3]
	add	esi,3+4
	mov	[file_extension],esi
	lea	esi,[esi+eax+1]
	jmp	instruction_assembled
      format_prefix:
	lods	byte [esi]
	mov	ah,al
	lods	byte [esi]
	cmp	al,18h
	jne	invalid_argument
	lods	byte [esi]
	mov	edx,eax
	shr	dl,4
	shr	dh,4
	cmp	dl,dh
	jne	invalid_argument
	or	al,ah
	jmp	select_format
entry_directive:
	bts	[format_flags],10h
	jc	setting_already_specified
	mov	al,[output_format]
	cmp	al,2
	je	mz_entry
	cmp	al,3
	je	pe_entry
	cmp	al,5
	jne	illegal_instruction
	bt	[format_flags],0
	jc	elf_entry
	jmp	illegal_instruction
stack_directive:
	bts	[format_flags],11h
	jc	setting_already_specified
	mov	al,[output_format]
	cmp	al,2
	je	mz_stack
	cmp	al,3
	je	pe_stack
	jmp	illegal_instruction
heap_directive:
	bts	[format_flags],12h
	jc	setting_already_specified
	mov	al,[output_format]
	cmp	al,2
	je	mz_heap
	cmp	al,3
	je	pe_heap
	jmp	illegal_instruction
segment_directive:
	cmp	[virtual_data],0
	jne	illegal_instruction
	mov	al,[output_format]
	cmp	al,2
	je	mz_segment
	cmp	al,5
	je	elf_segment
	jmp	illegal_instruction
section_directive:
	cmp	[virtual_data],0
	jne	illegal_instruction
	mov	al,[output_format]
	cmp	al,3
	je	pe_section
	cmp	al,4
	je	coff_section
	cmp	al,5
	je	elf_section
	jmp	illegal_instruction
public_directive:
	mov	al,[output_format]
	cmp	al,4
	je	public_allowed
	cmp	al,5
	jne	illegal_instruction
	bt	[format_flags],0
	jc	illegal_instruction
      public_allowed:
	lods	byte [esi]
	cmp	al,2
	jne	invalid_argument
	lods	dword [esi]
	cmp	eax,0Fh
	jb	invalid_use_of_symbol
	je	reserved_word_used_as_symbol
	mov	dx,[current_pass]
	mov	[eax+18],dx
	or	byte [eax+8],8
	inc	esi
	mov	ebx,[free_additional_memory]
	lea	edx,[ebx+10h]
	cmp	edx,[structures_buffer]
	jae	out_of_memory
	mov	[free_additional_memory],edx
	mov	[ebx+8],eax
	mov	eax,[current_line]
	mov	[ebx+0Ch],eax
	lods	byte [esi]
	cmp	al,86h
	jne	invalid_argument
	lods	word [esi]
	cmp	ax,'('
	jne	invalid_argument
	mov	[ebx+4],esi
	lods	dword [esi]
	lea	esi,[esi+eax+1]
	mov	byte [ebx],80h
	jmp	instruction_assembled
extrn_directive:
	mov	al,[output_format]
	cmp	al,4
	je	extrn_allowed
	cmp	al,5
	jne	illegal_instruction
	bt	[format_flags],0
	jc	illegal_instruction
      extrn_allowed:
	lods	word [esi]
	cmp	ax,'('
	jne	invalid_argument
	mov	ebx,esi
	lods	dword [esi]
	lea	esi,[esi+eax+1]
	mov	edx,[free_additional_memory]
	lea	eax,[edx+0Ch]
	cmp	eax,[structures_buffer]
	jae	out_of_memory
	mov	[free_additional_memory],eax
	mov	byte [edx],81h
	mov	[edx+4],ebx
	lods	byte [esi]
	cmp	al,86h
	jne	invalid_argument
	lods	byte [esi]
	cmp	al,2
	jne	invalid_argument
	lods	dword [esi]
	cmp	eax,0Fh
	jb	invalid_use_of_symbol
	je	reserved_word_used_as_symbol
	inc	esi
	mov	ebx,eax
	xor	ah,ah
	lods	byte [esi]
	cmp	al,':'
	je	get_extrn_size
	dec	esi
	cmp	al,11h
	jne	extrn_size_ok
      get_extrn_size:
	lods	word [esi]
	cmp	al,11h
	jne	invalid_argument
      extrn_size_ok:
	mov	[address_symbol],edx
	movzx	ecx,ah
	mov	[edx+8],ecx
	xor	eax,eax
	xor	edx,edx
	xor	ebp,ebp
	mov	ch,2
	test	[format_flags],8
	jz	make_free_label
	mov	ch,4
	jmp	make_free_label
mark_relocation:
	cmp	[value_type],0
	je	relocation_ok
	cmp	[virtual_data],0
	jne	relocation_ok
	cmp	[output_format],2
	je	mark_mz_relocation
	cmp	[output_format],3
	je	mark_pe_relocation
	cmp	[output_format],4
	je	mark_coff_relocation
	cmp	[output_format],5
	je	mark_elf_relocation
      relocation_ok:
	ret
close_pass:
	mov	al,[output_format]
	cmp	al,3
	je	close_pe
	cmp	al,4
	je	close_coff
	cmp	al,5
	je	close_elf
	ret

format_mz:
	mov	edx,[additional_memory]
	push	edi
	mov	edi,edx
	mov	ecx,1Ch shr 2
	xor	eax,eax
	rep	stos dword [edi]
	mov	[free_additional_memory],edi
	pop	edi
	mov	word [edx+0Ch],0FFFFh
	mov	word [edx+10h],1000h
	mov	[code_type],16
	jmp	format_defined
mark_mz_relocation:
	push	eax ebx
	inc	[number_of_relocations]
	mov	ebx,[free_additional_memory]
	mov	eax,edi
	sub	eax,[code_start]
	mov	[ebx],ax
	shr	eax,16
	shl	ax,12
	mov	[ebx+2],ax
	cmp	word [ebx],0FFFFh
	jne	mz_relocation_ok
	inc	word [ebx+2]
	sub	word [ebx],10h
      mz_relocation_ok:
	add	ebx,4
	cmp	ebx,[structures_buffer]
	jae	out_of_memory
	mov	[free_additional_memory],ebx
	pop	ebx eax
	ret
mz_segment:
	lods	byte [esi]
	cmp	al,2
	jne	invalid_argument
	lods	dword [esi]
	cmp	eax,0Fh
	jb	invalid_use_of_symbol
	je	reserved_word_used_as_symbol
	inc	esi
	mov	ebx,eax
	mov	eax,edi
	sub	eax,[code_start]
	mov	ecx,0Fh
	add	eax,0Fh
	and	eax,1111b
	sub	ecx,eax
	mov	edx,edi
	xor	al,al
	rep	stos byte [edi]
	mov	dword [org_origin],edi
	mov	dword [org_origin+4],0
	mov	[org_registers],0
	mov	[org_start],edi
	mov	eax,edx
	call	undefined_data
	mov	eax,edi
	sub	eax,[code_start]
	shr	eax,4
	cmp	eax,10000h
	jae	value_out_of_range
	mov	edx,eax
	mov	al,16
	cmp	byte [esi],13h
	jne	segment_type_ok
	inc	esi
	lods	byte [esi]
      segment_type_ok:
	mov	[code_type],al
	mov	eax,edx
	mov	cx,0100h
	xor	edx,edx
	xor	ebp,ebp
	mov	[address_symbol],edx
	jmp	make_free_label
mz_entry:
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	call	get_word_value
	cmp	[value_type],1
	je	initial_cs_ok
	cmp	[error_line],0
	jne	initial_cs_ok
	mov	eax,[current_line]
	mov	[error_line],eax
	mov	[error],invalid_address
      initial_cs_ok:
	mov	edx,[additional_memory]
	mov	[edx+16h],ax
	lods	byte [esi]
	cmp	al,':'
	jne	invalid_argument
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	ja	invalid_address
	call	get_word_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,[additional_memory]
	mov	[edx+14h],ax
	jmp	instruction_assembled
mz_stack:
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	call	get_word_value
	cmp	byte [esi],':'
	je	stack_pointer
	cmp	ax,10h
	jb	invalid_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,[additional_memory]
	mov	[edx+10h],ax
	jmp	instruction_assembled
      stack_pointer:
	cmp	[value_type],1
	je	initial_ss_ok
	cmp	[error_line],0
	jne	initial_ss_ok
	mov	eax,[current_line]
	mov	[error_line],eax
	mov	[error],invalid_address
      initial_ss_ok:
	mov	edx,[additional_memory]
	mov	[edx+0Eh],ax
	lods	byte [esi]
	cmp	al,':'
	jne	invalid_argument
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	call	get_word_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,[additional_memory]
	mov	[edx+10h],ax
	bts	[format_flags],4
	jmp	instruction_assembled
mz_heap:
	cmp	[output_format],2
	jne	illegal_instruction
	lods	byte [esi]
	call	get_size_operator
	cmp	ah,1
	je	invalid_value
	cmp	ah,2
	ja	invalid_value
	cmp	al,'('
	jne	invalid_argument
	call	get_word_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,[additional_memory]
	mov	[edx+0Ch],ax
	jmp	instruction_assembled
write_mz_header:
	mov	edx,[additional_memory]
	bt	[format_flags],4
	jc	mz_stack_ok
	mov	eax,[real_code_size]
	dec	eax
	shr	eax,4
	inc	eax
	mov	[edx+0Eh],ax
	shl	eax,4
	movzx	ecx,word [edx+10h]
	add	eax,ecx
	mov	[real_code_size],eax
      mz_stack_ok:
	mov	edi,[free_additional_memory]
	mov	eax,[number_of_relocations]
	shl	eax,2
	add	eax,1Ch
	sub	edi,eax
	xchg	edi,[free_additional_memory]
	mov	ecx,0Fh
	add	eax,0Fh
	and	eax,1111b
	sub	ecx,eax
	xor	al,al
	rep	stos byte [edi]
	sub	edi,[free_additional_memory]
	mov	ecx,edi
	shr	edi,4
	mov	word [edx],'MZ' 	; signature
	mov	[edx+8],di		; header size in paragraphs
	mov	eax,[number_of_relocations]
	mov	[edx+6],ax		; number of relocation entries
	mov	eax,[code_size]
	add	eax,ecx
	mov	esi,eax
	shr	esi,9
	and	eax,1FFh
	inc	si
	or	ax,ax
	jnz	mz_size_ok
	mov	ax,200h
	dec	si
      mz_size_ok:
	mov	[edx+2],ax		; number of bytes in last page
	mov	[edx+4],si		; number of pages
	mov	eax,[real_code_size]
	dec	eax
	shr	eax,4
	inc	eax
	mov	esi,[code_size]
	dec	esi
	shr	esi,4
	inc	esi
	sub	eax,esi
	mov	[edx+0Ah],ax		; minimum memory in addition to code
	add	[edx+0Ch],ax		; maximum memory in addition to code
	salc
	mov	ah,al
	or	[edx+0Ch],ax
	mov	word [edx+18h],1Ch	; offset of relocation table
	add	[written_size],ecx
	call	write
	jc	write_failed
	ret

make_stub:
	mov	[stub_file],edx
	or	edx,edx
	jnz	stub_from_file
	push	esi
	mov	edx,edi
	xor	eax,eax
	mov	ecx,20h
	rep	stos dword [edi]
	mov	eax,40h+default_stub_end-default_stub
	mov	cx,100h+default_stub_end-default_stub
	mov	word [edx],'MZ'
	mov	word [edx+4],1
	mov	word [edx+2],ax
	mov	word [edx+8],4
	mov	word [edx+0Ah],10h
	mov	word [edx+0Ch],0FFFFh
	mov	word [edx+10h],cx
	mov	word [edx+3Ch],ax
	mov	word [edx+18h],40h
	lea	edi,[edx+40h]
	mov	esi,default_stub
	mov	ecx,default_stub_end-default_stub
	rep	movs byte [edi],[esi]
	pop	esi
	jmp	stub_ok
      default_stub:
	use16
	push	cs
	pop	ds
	mov	dx,stub_message-default_stub
	mov	ah,9
	int	21h
	mov	ax,4C01h
	int	21h
      stub_message db 'This program cannot be run in DOS mode.',0Dh,0Ah,24h
	rq	1
      default_stub_end:
	use32
      stub_from_file:
	push	esi
	mov	esi,edx
	call	open_binary_file
	mov	edx,edi
	mov	ecx,1Ch
	mov	esi,edx
	call	read
	jc	binary_stub
	cmp	word [esi],'MZ'
	jne	binary_stub
	add	edi,1Ch
	movzx	ecx,word [esi+6]
	add	ecx,11b
	and	ecx,not 11b
	add	ecx,(40h-1Ch) shr 2
	lea	eax,[edi+ecx*4]
	cmp	edi,[display_buffer]
	jae	out_of_memory
	xor	eax,eax
	rep	stos dword [edi]
	mov	edx,40h
	xchg	dx,[esi+18h]
	xor	al,al
	call	lseek
	movzx	ecx,word [esi+6]
	shl	ecx,2
	lea	edx,[esi+40h]
	call	read
	mov	edx,edi
	sub	edx,esi
	shr	edx,4
	xchg	dx,[esi+8]
	shl	edx,4
	xor	al,al
	call	lseek
	movzx	ecx,word [esi+4]
	dec	ecx
	shl	ecx,9
	movzx	edx,word [esi+2]
	add	ecx,edx
	mov	edx,edi
	sub	ecx,eax
	je	read_stub_code
	jb	stub_code_ok
	push	ecx
	dec	ecx
	shr	ecx,3
	inc	ecx
	shl	ecx,1
	lea	eax,[edi+ecx*4]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	xor	eax,eax
	rep	stos dword [edi]
	pop	ecx
     read_stub_code:
	call	read
     stub_code_ok:
	call	close
	mov	edx,edi
	sub	edx,esi
	mov	ax,dx
	and	ax,1FFh
	mov	[esi+2],ax
	dec	edx
	shr	edx,9
	inc	edx
	mov	[esi+4],dx
	mov	eax,edi
	sub	eax,esi
	mov	[esi+3Ch],eax
	pop	esi
      stub_ok:
	ret
      binary_stub:
	mov	esi,edi
	mov	ecx,40h shr 2
	xor	eax,eax
	rep	stos dword [edi]
	mov	al,2
	xor	edx,edx
	call	lseek
	push	eax
	xor	al,al
	xor	edx,edx
	call	lseek
	mov	ecx,[esp]
	add	ecx,40h+111b
	and	ecx,not 111b
	mov	ax,cx
	and	ax,1FFh
	mov	[esi+2],ax
	lea	eax,[ecx+1FFh]
	shr	eax,9
	mov	[esi+4],ax
	mov	[esi+3Ch],ecx
	sub	ecx,40h
	mov	eax,10000h
	sub	eax,ecx
	jbe	binary_heap_ok
	shr	eax,4
	mov	[esi+0Ah],ax
      binary_heap_ok:
	mov	word [esi],'MZ'
	mov	word [esi+8],4
	mov	ax,0FFFFh
	mov	[esi+0Ch],ax
	dec	ax
	mov	[esi+10h],ax
	sub	ax,0Eh
	mov	[esi+0Eh],ax
	mov	[esi+16h],ax
	mov	word [esi+14h],100h
	mov	word [esi+18h],40h
	mov	eax,[display_buffer]
	sub	eax,ecx
	cmp	edi,eax
	jae	out_of_memory
	mov	edx,edi
	shr	ecx,2
	xor	eax,eax
	rep	stos dword [edi]
	pop	ecx
	call	read
	call	close
	pop	esi
	ret

format_pe:
	xor	edx,edx
	mov	[machine],14Ch
	mov	[subsystem],3
	mov	[subsystem_version],3 + 10 shl 16
	mov	[image_base],400000h
	test	[format_flags],8
	jz	pe_settings
	mov	[machine],8664h
	mov	[subsystem_version],5 + 0 shl 16
	mov	[image_base_high],0
      pe_settings:
	cmp	byte [esi],84h
	je	get_stub_name
	cmp	byte [esi],80h
	je	get_pe_base
	cmp	byte [esi],1Bh
	jne	pe_settings_ok
	lods	byte [esi]
	lods	byte [esi]
	test	al,80h+40h
	jz	subsystem_setting
	cmp	al,80h
	je	dll_flag
	cmp	al,81h
	je	wdm_flag
	jmp	pe_settings
      dll_flag:
	bts	[format_flags],8
	jc	setting_already_specified
	jmp	pe_settings
      wdm_flag:
	bts	[format_flags],9
	jc	setting_already_specified
	jmp	pe_settings
      subsystem_setting:
	bts	[format_flags],7
	jc	setting_already_specified
	and	ax,3Fh
	mov	[subsystem],ax
	cmp	byte [esi],'('
	jne	pe_settings
	inc	esi
	cmp	byte [esi],'.'
	jne	invalid_value
	inc	esi
	push	edx
	cmp	byte [esi+11],0
	jne	invalid_value
	cmp	byte [esi+10],2
	ja	invalid_value
	mov	dx,[esi+8]
	cmp	dx,8000h
	je	zero_version
	mov	eax,[esi+4]
	cmp	dx,7
	jg	invalid_value
	mov	cx,7
	sub	cx,dx
	mov	eax,[esi+4]
	shr	eax,cl
	mov	ebx,eax
	shr	ebx,24
	cmp	bl,100
	jae	invalid_value
	and	eax,0FFFFFFh
	mov	ecx,100
	mul	ecx
	shrd	eax,edx,24
	jnc	version_value_ok
	inc	eax
      version_value_ok:
	shl	eax,16
	mov	ax,bx
	jmp	subsystem_version_ok
      zero_version:
	xor	eax,eax
      subsystem_version_ok:
	pop	edx
	add	esi,13
	mov	[subsystem_version],eax
	jmp	pe_settings
      get_pe_base:
	bts	[format_flags],10
	jc	setting_already_specified
	lods	word [esi]
	cmp	ah,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	push	edx edi
	add	edi,[stub_size]
	test	[format_flags],8
	jnz	get_pe64_base
	call	get_dword_value
	mov	[image_base],eax
	jmp	pe_base_ok
      get_pe64_base:
	call	get_qword_value
	mov	[image_base],eax
	mov	[image_base_high],edx
      pe_base_ok:
	pop	edi edx
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	cmp	byte [esi],84h
	jne	pe_settings_ok
      get_stub_name:
	lods	byte [esi]
	lods	word [esi]
	cmp	ax,'('
	jne	invalid_argument
	lods	dword [esi]
	mov	edx,esi
	add	esi,eax
	inc	esi
      pe_settings_ok:
	mov	ebp,[stub_size]
	or	ebp,ebp
	jz	make_pe_stub
	cmp	edx,[stub_file]
	je	pe_stub_ok
	sub	edi,[stub_size]
	mov	[code_start],edi
      make_pe_stub:
	call	make_stub
	mov	eax,edi
	sub	eax,[code_start]
	mov	[stub_size],eax
	mov	[code_start],edi
	mov	ebp,eax
      pe_stub_ok:
	mov	edx,edi
	mov	ecx,18h+0E0h
	test	[format_flags],8
	jz	zero_pe_header
	add	ecx,10h
      zero_pe_header:
	add	ebp,ecx
	shr	ecx,2
	xor	eax,eax
	rep	stos dword [edi]
	mov	word [edx],'PE' 	; signature
	mov	ax,[machine]
	mov	word [edx+4],ax
	mov	dword [edx+38h],1000h	; section alignment
	mov	dword [edx+3Ch],200h	; file alignment
	mov	word [edx+40h],1	; OS version
	mov	eax,[subsystem_version]
	mov	[edx+48h],eax
	mov	ax,[subsystem]
	mov	[edx+5Ch],ax
	cmp	ax,1
	jne	pe_alignment_ok
	mov	eax,20h
	mov	dword [edx+38h],eax
	mov	dword [edx+3Ch],eax
      pe_alignment_ok:
	mov	word [edx+1Ah],VERSION_MAJOR + VERSION_MINOR shl 8
	test	[format_flags],8
	jnz	init_pe64_specific
	mov	dword [edx+14h],0E0h	; size of optional header
	mov	dword [edx+16h],10B010Eh; flags and magic value
	mov	eax,[image_base]
	mov	dword [edx+34h],eax
	mov	dword [edx+60h],1000h	; stack reserve
	mov	dword [edx+64h],1000h	; stack commit
	mov	dword [edx+68h],10000h	; heap reserve
	mov	dword [edx+6Ch],0	; heap commit
	mov	dword [edx+74h],16	; number of directories
	jmp	pe_header_ok
      init_pe64_specific:
	mov	dword [edx+14h],0F0h	; size of optional header
	mov	dword [edx+16h],20B002Eh; flags and magic value
	mov	eax,[image_base]
	mov	dword [edx+30h],eax
	mov	eax,[image_base_high]
	mov	dword [edx+34h],eax
	mov	dword [edx+60h],1000h	; stack reserve
	mov	dword [edx+68h],1000h	; stack commit
	mov	dword [edx+70h],10000h	; heap reserve
	mov	dword [edx+78h],0	; heap commit
	mov	dword [edx+84h],16	; number of directories
      pe_header_ok:
	bsf	ecx,[edx+3Ch]
	imul	ebx,[number_of_sections],28h
	or	ebx,ebx
	jnz	reserve_space_for_section_headers
	mov	ebx,28h
      reserve_space_for_section_headers:
	add	ebx,ebp
	dec	ebx
	shr	ebx,cl
	inc	ebx
	shl	ebx,cl
	sub	ebx,ebp
	mov	ecx,ebx
	mov	eax,[display_buffer]
	sub	eax,ecx
	cmp	edi,eax
	jae	out_of_memory
	shr	ecx,2
	xor	eax,eax
	rep	stos dword [edi]
	mov	eax,edi
	sub	eax,[code_start]
	add	eax,[stub_size]
	mov	[edx+54h],eax		; size of headers
	mov	ecx,[edx+38h]
	dec	ecx
	add	eax,ecx
	not	ecx
	and	eax,ecx
	bt	[format_flags],8
	jc	pe_entry_init_ok
	mov	[edx+28h],eax		; entry point rva
      pe_entry_init_ok:
	mov	[number_of_sections],0
	movzx	ebx,word [edx+14h]
	lea	ebx,[edx+18h+ebx]
	mov	[current_section],ebx
	mov	dword [ebx],'.fla'
	mov	dword [ebx+4],'t'
	mov	[ebx+14h],edi
	mov	[ebx+0Ch],eax
	mov	dword [ebx+24h],0E0000060h
	xor	ecx,ecx
	not	eax
	not	ecx
	add	eax,1
	adc	ecx,0
	add	eax,edi
	adc	ecx,0
	test	[format_flags],8
	jnz	pe64_org
	sub	eax,[edx+34h]
	sbb	ecx,0
	mov	bl,2
	mov	[code_type],32
	jmp	pe_org_ok
      pe64_org:
	sub	eax,[edx+30h]
	sbb	ecx,[edx+34h]
	mov	bl,4
	mov	[code_type],64
      pe_org_ok:
	bt	[resolver_flags],0
	jc	pe_labels_type_ok
	xor	bl,bl
      pe_labels_type_ok:
	mov	[labels_type],bl
	mov	dword [org_origin],eax
	mov	dword [org_origin+4],ecx
	mov	[org_registers],0
	mov	[org_start],edi
	bt	[format_flags],8
	jnc	dll_flag_ok
	or	dword [edx+16h],2000h
      dll_flag_ok:
	bt	[format_flags],9
	jnc	wdm_flag_ok
	or	word [edx+5Eh],2000h
      wdm_flag_ok:
	jmp	format_defined
pe_section:
	call	close_pe_section
	bts	[format_flags],5
	lea	ecx,[ebx+28h]
	add	edx,[edx+54h]
	sub	edx,[stub_size]
	cmp	ecx,edx
	jbe	new_section
	lea	ebx,[edx-28h]
	or	[next_pass_needed],-1
	push	edi
	mov	edi,ebx
	mov	ecx,28h shr 4
	xor	eax,eax
	rep	stos dword [edi]
	pop	edi
      new_section:
	mov	[ebx+0Ch],eax
	lods	word [esi]
	cmp	ax,'('
	jne	invalid_argument
	lea	edx,[esi+4]
	mov	ecx,[esi]
	lea	esi,[esi+4+ecx+1]
	cmp	ecx,8
	ja	name_too_long
	xor	eax,eax
	mov	[ebx],eax
	mov	[ebx+4],eax
	push	esi edi
	mov	edi,ebx
	mov	esi,edx
	rep	movs byte [edi],[esi]
	pop	edi esi
	mov	dword [ebx+24h],0
	mov	[ebx+14h],edi
	mov	edx,[code_start]
	mov	eax,edi
	xor	ecx,ecx
	sub	eax,[ebx+0Ch]
	sbb	ecx,0
	test	[format_flags],8
	jnz	pe64_section_org
	sub	eax,[edx+34h]
	sbb	ecx,0
	mov	[labels_type],2
	mov	[code_type],32
	bt	[resolver_flags],0
	jc	pe_section_org_ok
	mov	[labels_type],0
	jmp	pe_section_org_ok
      pe64_section_org:
	sub	eax,[edx+30h]
	sbb	ecx,[edx+34h]
	mov	[labels_type],4
	mov	[code_type],64
	bt	[resolver_flags],0
	jc	pe_section_org_ok
	mov	[labels_type],0
      pe_section_org_ok:
	mov	dword [org_origin],eax
	mov	dword [org_origin+4],ecx
	mov	[org_registers],0
	mov	[org_start],edi
      get_section_flags:
	lods	byte [esi]
	cmp	al,1Ah
	je	set_directory
	cmp	al,19h
	je	section_flag
	dec	esi
	jmp	instruction_assembled
      set_directory:
	movzx	eax,byte [esi]
	inc	esi
	mov	ecx,ebx
	test	[format_flags],8
	jnz	pe64_directory
	xchg	ecx,[edx+78h+eax*8]
	mov	dword [edx+78h+eax*8+4],-1
	jmp	pe_directory_set
      pe64_directory:
	xchg	ecx,[edx+88h+eax*8]
	mov	dword [edx+88h+eax*8+4],-1
      pe_directory_set:
	or	ecx,ecx
	jnz	data_already_defined
	push	ebx edx
	call	generate_pe_data
	pop	edx ebx
	jmp	get_section_flags
      section_flag:
	lods	byte [esi]
	cmp	al,9
	je	invalid_argument
	cmp	al,11
	je	invalid_argument
	mov	cl,al
	mov	eax,1
	shl	eax,cl
	test	dword [ebx+24h],eax
	jnz	setting_already_specified
	or	dword [ebx+24h],eax
	jmp	get_section_flags
      close_pe_section:
	mov	ebx,[current_section]
	mov	edx,[code_start]
	mov	eax,edi
	sub	eax,[ebx+14h]
	jnz	finish_section
	bt	[format_flags],5
	jc	finish_section
	mov	eax,[ebx+0Ch]
	ret
      finish_section:
	mov	[ebx+8],eax
	cmp	edi,[undefined_data_end]
	jne	align_section
	cmp	dword [edx+38h],1000h
	jb	align_section
	mov	edi,[undefined_data_start]
      align_section:
	mov	[undefined_data_end],0
	mov	ebp,edi
	sub	ebp,[ebx+14h]
	mov	ecx,[edx+3Ch]
	dec	ecx
	lea	eax,[ebp+ecx]
	not	ecx
	and	eax,ecx
	mov	[ebx+10h],eax
	sub	eax,ebp
	mov	ecx,eax
	xor	al,al
	rep	stos byte [edi]
	mov	eax,[code_start]
	sub	eax,[stub_size]
	sub	[ebx+14h],eax
	mov	eax,[ebx+8]
	or	eax,eax
	jz	udata_ok
	cmp	dword [ebx+10h],0
	jne	udata_ok
	or	byte [ebx+24h],80h
      udata_ok:
	mov	ecx,[edx+38h]
	dec	ecx
	add	eax,ecx
	not	ecx
	and	eax,ecx
	add	eax,[ebx+0Ch]
	add	ebx,28h
	mov	[current_section],ebx
	inc	word [number_of_sections]
	jz	format_limitations_exceeded
	ret
data_directive:
	cmp	[output_format],3
	jne	illegal_instruction
	lods	byte [esi]
	cmp	al,1Ah
	je	predefined_data_type
	cmp	al,'('
	jne	invalid_argument
	call	get_byte_value
	cmp	al,16
	jb	data_type_ok
	jmp	invalid_value
      predefined_data_type:
	movzx	eax,byte [esi]
	inc	esi
      data_type_ok:
	mov	ebx,[current_section]
	mov	ecx,edi
	sub	ecx,[ebx+14h]
	add	ecx,[ebx+0Ch]
	mov	edx,[code_start]
	test	[format_flags],8
	jnz	pe64_data
	xchg	ecx,[edx+78h+eax*8]
	jmp	init_pe_data
      pe64_data:
	xchg	ecx,[edx+88h+eax*8]
      init_pe_data:
	or	ecx,ecx
	jnz	data_already_defined
	call	allocate_structure_data
	mov	word [ebx],data_directive-assembler
	mov	[ebx+2],al
	mov	edx,[current_line]
	mov	[ebx+4],edx
	call	generate_pe_data
	jmp	instruction_assembled
      end_data:
	cmp	[output_format],3
	jne	illegal_instruction
	call	find_structure_data
	jc	unexpected_instruction
	movzx	eax,byte [ebx+2]
	mov	edx,[current_section]
	mov	ecx,edi
	sub	ecx,[edx+14h]
	add	ecx,[edx+0Ch]
	mov	edx,[code_start]
	test	[format_flags],8
	jnz	end_pe64_data
	sub	ecx,[edx+78h+eax*8]
	mov	[edx+78h+eax*8+4],ecx
	jmp	remove_structure_data
      end_pe64_data:
	sub	ecx,[edx+88h+eax*8]
	mov	[edx+88h+eax*8+4],ecx
	jmp	remove_structure_data
pe_entry:
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	test	[format_flags],8
	jnz	pe64_entry
	call	get_dword_value
	mov	bl,2
	bt	[resolver_flags],0
	jc	check_pe_entry_label_type
	xor	bl,bl
      check_pe_entry_label_type:
	cmp	[value_type],bl
	je	pe_entry_ok
	cmp	[error_line],0
	jne	pe_entry_ok
	mov	edx,[current_line]
	mov	[error_line],edx
	mov	[error],invalid_address
      pe_entry_ok:
	mov	edx,[code_start]
	sub	eax,[edx+34h]
	mov	[edx+28h],eax
	jmp	instruction_assembled
      pe64_entry:
	call	get_qword_value
	mov	bl,4
	bt	[resolver_flags],0
	jc	check_pe64_entry_label_type
	xor	bl,bl
      check_pe64_entry_label_type:
	cmp	[value_type],bl
	je	pe64_entry_type_ok
	cmp	[error_line],0
	jne	pe64_entry_type_ok
	mov	edx,[current_line]
	mov	[error_line],edx
	mov	[error],invalid_address
      pe64_entry_type_ok:
	mov	ecx,[code_start]
	sub	eax,[ecx+30h]
	sbb	edx,[ecx+34h]
	jz	pe64_entry_range_ok
	mov	edx,[current_line]
	mov	[error_line],edx
	mov	[error],value_out_of_range
      pe64_entry_range_ok:
	mov	[ecx+28h],eax
	jmp	instruction_assembled
pe_stack:
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	test	[format_flags],8
	jnz	pe64_stack
	call	get_dword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,[code_start]
	mov	[edx+60h],eax
	cmp	byte [esi],','
	jne	default_stack_commit
	lods	byte [esi]
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	call	get_dword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,[code_start]
	mov	[edx+64h],eax
	cmp	eax,[edx+60h]
	ja	value_out_of_range
	jmp	instruction_assembled
      default_stack_commit:
	mov	dword [edx+64h],1000h
	mov	eax,[edx+60h]
	cmp	eax,1000h
	ja	instruction_assembled
	mov	dword [edx+64h],eax
	jmp	instruction_assembled
      pe64_stack:
	call	get_qword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	ecx,[code_start]
	mov	[ecx+60h],eax
	mov	[ecx+64h],edx
	cmp	byte [esi],','
	jne	default_pe64_stack_commit
	lods	byte [esi]
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	call	get_qword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	ecx,[code_start]
	mov	[ecx+68h],eax
	mov	[ecx+6Ch],edx
	cmp	edx,[ecx+64h]
	ja	value_out_of_range
	jb	instruction_assembled
	cmp	eax,[ecx+60h]
	ja	value_out_of_range
	jmp	instruction_assembled
      default_pe64_stack_commit:
	mov	dword [ecx+68h],1000h
	cmp	dword [ecx+64h],0
	jne	instruction_assembled
	mov	eax,[ecx+60h]
	cmp	eax,1000h
	ja	instruction_assembled
	mov	dword [ecx+68h],eax
	jmp	instruction_assembled
pe_heap:
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	test	[format_flags],8
	jnz	pe64_heap
	call	get_dword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,[code_start]
	mov	[edx+68h],eax
	cmp	byte [esi],','
	jne	instruction_assembled
	lods	byte [esi]
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	call	get_dword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,[code_start]
	mov	[edx+6Ch],eax
	cmp	eax,[edx+68h]
	ja	value_out_of_range
	jmp	instruction_assembled
      pe64_heap:
	call	get_qword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	ecx,[code_start]
	mov	[ecx+70h],eax
	mov	[ecx+74h],edx
	cmp	byte [esi],','
	jne	instruction_assembled
	lods	byte [esi]
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	call	get_qword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	ecx,[code_start]
	mov	[ecx+78h],eax
	mov	[ecx+7Ch],edx
	cmp	edx,[ecx+74h]
	ja	value_out_of_range
	jb	instruction_assembled
	cmp	eax,[edx+70h]
	ja	value_out_of_range
	jmp	instruction_assembled
mark_pe_relocation:
	push	eax ebx
	test	[format_flags],8
	jz	check_pe32_relocation_type
	cmp	[value_type],4
	je	pe_relocation_type_ok
      check_pe32_relocation_type:
	cmp	[value_type],2
	je	pe_relocation_type_ok
	cmp	[error_line],0
	jne	pe_relocation_type_ok
	mov	eax,[current_line]
	mov	[error_line],eax
	mov	[error],invalid_use_of_symbol
      pe_relocation_type_ok:
	mov	ebx,[current_section]
	mov	eax,edi
	sub	eax,[ebx+14h]
	add	eax,[ebx+0Ch]
	mov	ebx,[free_additional_memory]
	inc	[number_of_relocations]
	jz	invalid_use_of_symbol
	add	ebx,5
	cmp	ebx,[structures_buffer]
	jae	out_of_memory
	mov	[free_additional_memory],ebx
	mov	[ebx-5],eax
	cmp	[value_type],2
	je	fixup_32bit
	mov	byte [ebx-1],0Ah
	jmp	fixup_ok
      fixup_32bit:
	mov	byte [ebx-1],3
      fixup_ok:
	pop	ebx eax
	ret
generate_pe_data:
	cmp	al,2
	je	make_pe_resource
	cmp	al,5
	je	make_pe_fixups
	ret
make_pe_fixups:
	bts	[resolver_flags],0
	jc	pe_relocatable_ok
	or	[next_pass_needed],-1
     pe_relocatable_ok:
	push	esi
	mov	ecx,[number_of_relocations]
	mov	esi,[free_additional_memory]
	lea	eax,[ecx*5]
	sub	esi,eax
	mov	[free_additional_memory],esi
	or	[number_of_relocations],-1
	xor	edx,edx
	mov	ebp,edi
      make_fixups:
	cmp	[esi],edx
	jb	store_fixup
	mov	eax,edi
	sub	eax,ebp
	test	eax,11b
	jz	fixups_block
	xor	ax,ax
	stos	word [edi]
	add	dword [ebx],2
      fixups_block:
	mov	eax,edx
	add	edx,1000h
	cmp	[esi],edx
	jae	fixups_block
	stos	dword [edi]
	mov	ebx,edi
	mov	eax,8
	stos	dword [edi]
      store_fixup:
	jecxz	fixups_done
	add	dword [ebx],2
	mov	ah,[esi+1]
	and	ah,0Fh
	mov	al,[esi+4]
	shl	al,4
	or	ah,al
	mov	al,[esi]
	stos	word [edi]
	add	esi,5
	loop	make_fixups
      fixups_done:
	pop	esi
	ret
make_pe_resource:
	cmp	byte [esi],82h
	jne	resource_done
	inc	esi
	lods	word [esi]
	cmp	ax,'('
	jne	invalid_argument
	lods	dword [esi]
	mov	edx,esi
	lea	esi,[esi+eax+1]
	cmp	[next_pass_needed],0
	je	resource_from_file
	cmp	[current_pass],0
	jne	reserve_space_for_resource
	mov	[resource_size],0
      reserve_space_for_resource:
	add	edi,[resource_size]
	cmp	edi,[display_buffer]
	ja	out_of_memory
	jmp	resource_done
      resource_from_file:
	push	esi
	mov	esi,edx
	call	open_binary_file
	push	ebx
	mov	esi,[free_additional_memory]
	lea	eax,[esi+20h]
	cmp	eax,[structures_buffer]
	ja	out_of_memory
	mov	edx,esi
	mov	ecx,20h
	call	read
	jc	invalid_file_format
	xor	eax,eax
	cmp	[esi],eax
	jne	invalid_file_format
	mov	ax,0FFFFh
	cmp	[esi+8],eax
	jne	invalid_file_format
	cmp	[esi+12],eax
	jne	invalid_file_format
	mov	eax,20h
	cmp	[esi+4],eax
	jne	invalid_file_format
      read_resource_headers:
	test	eax,11b
	jz	resource_file_alignment_ok
	mov	edx,4
	and	eax,11b
	sub	edx,eax
	mov	al,1
	call	lseek
      resource_file_alignment_ok:
	mov	[esi],eax
	lea	edx,[esi+12]
	mov	ecx,8
	call	read
	jc	resource_headers_ok
	mov	ecx,[esi+16]
	add	[esi],ecx
	lea	edx,[esi+20]
	sub	ecx,8
	mov	[esi+16],ecx
	lea	eax,[edx+ecx]
	cmp	eax,[structures_buffer]
	ja	out_of_memory
	call	read
	jc	invalid_file_format
	mov	edx,[esi]
	add	edx,[esi+12]
	mov	eax,[esi+16]
	lea	ecx,[esi+20]
	lea	esi,[ecx+eax]
	add	ecx,2
	cmp	word [ecx-2],0FFFFh
	je	resource_header_type_ok
      check_resource_header_type:
	cmp	ecx,esi
	jae	invalid_file_format
	cmp	word [ecx],0
	je	resource_header_type_ok
	add	ecx,2
	jmp	check_resource_header_type
      resource_header_type_ok:
	add	ecx,2
	cmp	word [ecx],0FFFFh
	je	resource_header_name_ok
      check_resource_header_name:
	cmp	ecx,esi
	jae	invalid_file_format
	cmp	word [ecx],0
	je	resource_header_name_ok
	add	ecx,2
	jmp	check_resource_header_name
      resource_header_name_ok:
	xor	al,al
	call	lseek
	jmp	read_resource_headers
      resource_headers_ok:
	xor	eax,eax
	mov	[esi],eax
	mov	[resource_data],edi
	lea	eax,[edi+16]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	xor	eax,eax
	stos	dword [edi]
	call	make_timestamp
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	stos	dword [edi]
	xor	ebx,ebx
      make_type_name_directory:
	mov	esi,[free_additional_memory]
	xor	edx,edx
      find_type_name:
	cmp	dword [esi],0
	je	type_name_ok
	add	esi,20
	cmp	word [esi],0FFFFh
	je	check_next_type_name
	or	ebx,ebx
	jz	check_this_type_name
	xor	ecx,ecx
      compare_with_previous_type_name:
	mov	ax,[esi+ecx]
	cmp	ax,[ebx+ecx]
	ja	check_this_type_name
	jb	check_next_type_name
	add	ecx,2
	mov	ax,[esi+ecx]
	or	ax,[ebx+ecx]
	jnz	compare_with_previous_type_name
	jmp	check_next_type_name
      check_this_type_name:
	or	edx,edx
	jz	type_name_found
	xor	ecx,ecx
      compare_with_current_type_name:
	mov	ax,[esi+ecx]
	cmp	ax,[edx+ecx]
	ja	check_next_type_name
	jb	type_name_found
	add	ecx,2
	mov	ax,[esi+ecx]
	or	ax,[edx+ecx]
	jnz	compare_with_current_type_name
	jmp	same_type_name
      type_name_found:
	mov	edx,esi
      same_type_name:
	mov	[esi-16],edi
      check_next_type_name:
	mov	eax,[esi-4]
	add	esi,eax
	jmp	find_type_name
      type_name_ok:
	or	edx,edx
	jz	type_name_directory_done
	mov	ebx,edx
      make_type_name_entry:
	mov	eax,[resource_data]
	inc	word [eax+12]
	lea	eax,[edi+8]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	mov	eax,ebx
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	jmp	make_type_name_directory
      type_name_directory_done:
	mov	ebx,-1
      make_type_id_directory:
	mov	esi,[free_additional_memory]
	mov	edx,10000h
      find_type_id:
	cmp	dword [esi],0
	je	type_id_ok
	add	esi,20
	cmp	word [esi],0FFFFh
	jne	check_next_type_id
	movzx	eax,word [esi+2]
	cmp	eax,ebx
	jle	check_next_type_id
	cmp	eax,edx
	jg	check_next_type_id
	mov	edx,eax
	mov	[esi-16],edi
      check_next_type_id:
	mov	eax,[esi-4]
	add	esi,eax
	jmp	find_type_id
      type_id_ok:
	cmp	edx,10000h
	je	type_id_directory_done
	mov	ebx,edx
      make_type_id_entry:
	mov	eax,[resource_data]
	inc	word [eax+14]
	lea	eax,[edi+8]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	mov	eax,ebx
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	jmp	make_type_id_directory
      type_id_directory_done:
	mov	esi,[resource_data]
	add	esi,10h
	mov	ecx,[esi-4]
	or	cx,cx
	jz	resource_directories_ok
      make_resource_directories:
	push	ecx
	push	edi
	mov	edx,edi
	sub	edx,[resource_data]
	bts	edx,31
	mov	[esi+4],edx
	lea	eax,[edi+16]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	xor	eax,eax
	stos	dword [edi]
	call	make_timestamp
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	stos	dword [edi]
	mov	ebp,esi
	xor	ebx,ebx
      make_resource_name_directory:
	mov	esi,[free_additional_memory]
	xor	edx,edx
      find_resource_name:
	cmp	dword [esi],0
	je	resource_name_ok
	push	esi
	cmp	[esi+4],ebp
	jne	check_next_resource_name
	add	esi,20
	call	skip_resource_name
	cmp	word [esi],0FFFFh
	je	check_next_resource_name
	or	ebx,ebx
	jz	check_this_resource_name
	xor	ecx,ecx
      compare_with_previous_resource_name:
	mov	ax,[esi+ecx]
	cmp	ax,[ebx+ecx]
	ja	check_this_resource_name
	jb	check_next_resource_name
	add	ecx,2
	mov	ax,[esi+ecx]
	or	ax,[ebx+ecx]
	jnz	compare_with_previous_resource_name
	jmp	check_next_resource_name
      skip_resource_name:
	cmp	word [esi],0FFFFh
	jne	skip_unicode_string
	add	esi,4
	ret
      skip_unicode_string:
	add	esi,2
	cmp	word [esi-2],0
	jne	skip_unicode_string
	ret
      check_this_resource_name:
	or	edx,edx
	jz	resource_name_found
	xor	ecx,ecx
      compare_with_current_resource_name:
	mov	ax,[esi+ecx]
	cmp	ax,[edx+ecx]
	ja	check_next_resource_name
	jb	resource_name_found
	add	ecx,2
	mov	ax,[esi+ecx]
	or	ax,[edx+ecx]
	jnz	compare_with_current_resource_name
	jmp	same_resource_name
      resource_name_found:
	mov	edx,esi
      same_resource_name:
	mov	eax,[esp]
	mov	[eax+8],edi
      check_next_resource_name:
	pop	esi
	mov	eax,[esi+16]
	lea	esi,[esi+20+eax]
	jmp	find_resource_name
      resource_name_ok:
	or	edx,edx
	jz	resource_name_directory_done
	mov	ebx,edx
      make_resource_name_entry:
	mov	eax,[esp]
	inc	word [eax+12]
	lea	eax,[edi+8]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	mov	eax,ebx
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	jmp	make_resource_name_directory
      resource_name_directory_done:
	mov	ebx,-1
      make_resource_id_directory:
	mov	esi,[free_additional_memory]
	mov	edx,10000h
      find_resource_id:
	cmp	dword [esi],0
	je	resource_id_ok
	push	esi
	cmp	[esi+4],ebp
	jne	check_next_resource_id
	add	esi,20
	call	skip_resource_name
	cmp	word [esi],0FFFFh
	jne	check_next_resource_id
	movzx	eax,word [esi+2]
	cmp	eax,ebx
	jle	check_next_resource_id
	cmp	eax,edx
	jg	check_next_resource_id
	mov	edx,eax
	mov	eax,[esp]
	mov	[eax+8],edi
      check_next_resource_id:
	pop	esi
	mov	eax,[esi+16]
	lea	esi,[esi+20+eax]
	jmp	find_resource_id
      resource_id_ok:
	cmp	edx,10000h
	je	resource_id_directory_done
	mov	ebx,edx
      make_resource_id_entry:
	mov	eax,[esp]
	inc	word [eax+14]
	lea	eax,[edi+8]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	mov	eax,ebx
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	jmp	make_resource_id_directory
      resource_id_directory_done:
	pop	eax
	mov	esi,ebp
	pop	ecx
	add	esi,8
	dec	cx
	jnz	make_resource_directories
      resource_directories_ok:
	shr	ecx,16
	jnz	make_resource_directories
	mov	esi,[resource_data]
	add	esi,10h
	movzx	eax,word [esi-4]
	movzx	edx,word [esi-2]
	add	eax,edx
	lea	esi,[esi+eax*8]
	push	edi			; address of language directories
      update_resource_directories:
	cmp	esi,[esp]
	je	resource_directories_updated
	add	esi,10h
	mov	ecx,[esi-4]
	or	cx,cx
	jz	language_directories_ok
      make_language_directories:
	push	ecx
	push	edi
	mov	edx,edi
	sub	edx,[resource_data]
	bts	edx,31
	mov	[esi+4],edx
	lea	eax,[edi+16]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	xor	eax,eax
	stos	dword [edi]
	call	make_timestamp
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	stos	dword [edi]
	mov	ebp,esi
	mov	ebx,-1
      make_language_id_directory:
	mov	esi,[free_additional_memory]
	mov	edx,10000h
      find_language_id:
	cmp	dword [esi],0
	je	language_id_ok
	push	esi
	cmp	[esi+8],ebp
	jne	check_next_language_id
	add	esi,20
	mov	eax,esi
	call	skip_resource_name
	call	skip_resource_name
	neg	eax
	add	eax,esi
	and	eax,11b
	add	esi,eax
      get_language_id:
	movzx	eax,word [esi+6]
	cmp	eax,ebx
	jle	check_next_language_id
	cmp	eax,edx
	jge	check_next_language_id
	mov	edx,eax
	mov	eax,[esp]
	mov	[current_offset],eax
      check_next_language_id:
	pop	esi
	mov	eax,[esi+16]
	lea	esi,[esi+20+eax]
	jmp	find_language_id
      language_id_ok:
	cmp	edx,10000h
	je	language_id_directory_done
	mov	ebx,edx
      make_language_id_entry:
	mov	eax,[esp]
	inc	word [eax+14]
	lea	eax,[edi+8]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	mov	eax,ebx
	stos	dword [edi]
	mov	eax,[current_offset]
	stos	dword [edi]
	jmp	make_language_id_directory
      language_id_directory_done:
	pop	eax
	mov	esi,ebp
	pop	ecx
	add	esi,8
	dec	cx
	jnz	make_language_directories
      language_directories_ok:
	shr	ecx,16
	jnz	make_language_directories
	jmp	update_resource_directories
      resource_directories_updated:
	mov	esi,[resource_data]
	push	edi
      make_name_strings:
	add	esi,10h
	movzx	eax,word [esi-2]
	movzx	ecx,word [esi-4]
	add	eax,ecx
	lea	eax,[esi+eax*8]
	push	eax
	or	ecx,ecx
	jz	string_entries_processed
      process_string_entries:
	push	ecx
	mov	edx,edi
	sub	edx,[resource_data]
	bts	edx,31
	xchg	[esi],edx
	mov	ebx,edi
	xor	ax,ax
	stos	word [edi]
      copy_string_data:
	lea	eax,[edi+2]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	mov	ax,[edx]
	or	ax,ax
	jz	string_data_copied
	stos	word [edi]
	inc	word [ebx]
	add	edx,2
	jmp	copy_string_data
      string_data_copied:
	add	esi,8
	pop	ecx
	loop	process_string_entries
      string_entries_processed:
	pop	esi
	cmp	esi,[esp]
	jb	make_name_strings
	mov	eax,edi
	sub	eax,[resource_data]
	test	al,11b
	jz	resource_strings_alignment_ok
	xor	ax,ax
	stos	word [edi]
      resource_strings_alignment_ok:
	pop	edx
	pop	ebx			; address of language directories
	mov	ebp,edi
      update_language_directories:
	add	ebx,10h
	movzx	eax,word [ebx-2]
	movzx	ecx,word [ebx-4]
	add	ecx,eax
      make_data_records:
	push	ecx
	mov	esi,edi
	sub	esi,[resource_data]
	xchg	esi,[ebx+4]
	lea	eax,[edi+16]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	mov	eax,esi
	stos	dword [edi]
	mov	eax,[esi+12]
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	stos	dword [edi]
	pop	ecx
	add	ebx,8
	loop	make_data_records
	cmp	ebx,edx
	jb	update_language_directories
	pop	ebx			; file handle
	mov	esi,ebp
	mov	ebp,edi
      update_data_records:
	push	ebp
	mov	ecx,edi
	mov	eax,[current_section]
	sub	ecx,[eax+14h]
	add	ecx,[eax+0Ch]
	xchg	ecx,[esi]
	mov	edx,[ecx]
	xor	al,al
	call	lseek
	mov	edx,edi
	mov	ecx,[esi+4]
	add	edi,ecx
	cmp	edi,[display_buffer]
	ja	out_of_memory
	call	read
	mov	eax,edi
	sub	eax,[resource_data]
	and	eax,11b
	jz	resource_data_alignment_ok
	mov	ecx,4
	sub	ecx,eax
	xor	al,al
	rep	stos byte [edi]
      resource_data_alignment_ok:
	pop	ebp
	add	esi,16
	cmp	esi,ebp
	jb	update_data_records
	pop	esi
	call	close
	mov	eax,edi
	sub	eax,[resource_data]
	mov	[resource_size],eax
      resource_done:
	ret
close_pe:
	call	close_pe_section
	mov	edx,[code_start]
	mov	[edx+50h],eax
	call	make_timestamp
	mov	edx,[code_start]
	mov	[edx+8],eax
	mov	eax,[number_of_relocations]
	cmp	eax,-1
	je	pe_relocations_ok
	shl	eax,2
	sub	[free_additional_memory],eax
	btr	[resolver_flags],0
	jnc	pe_relocations_ok
	or	[next_pass_needed],-1
      pe_relocations_ok:
	mov	eax,[number_of_sections]
	mov	[edx+6],ax
	imul	eax,28h
	movzx	ecx,word [edx+14h]
	lea	eax,[eax+18h+ecx]
	add	eax,[stub_size]
	mov	ecx,[edx+3Ch]
	dec	ecx
	add	eax,ecx
	not	ecx
	and	eax,ecx
	cmp	eax,[edx+54h]
	je	pe_sections_ok
	or	[next_pass_needed],-1
      pe_sections_ok:
	xor	ecx,ecx
	add	edx,78h
	test	[format_flags],8
	jz	process_directories
	add	edx,10h
      process_directories:
	mov	eax,[edx+ecx*8]
	or	eax,eax
	jz	directory_ok
	cmp	dword [edx+ecx*8+4],-1
	jne	directory_ok
      section_data:
	mov	ebx,[edx+ecx*8]
	mov	eax,[ebx+0Ch]
	mov	[edx+ecx*8],eax 	; directory rva
	mov	eax,[ebx+8]
	mov	[edx+ecx*8+4],eax	; directory size
      directory_ok:
	inc	cl
	cmp	cl,10h
	jb	process_directories
	mov	ebx,[code_start]
	sub	ebx,[stub_size]
	mov	ecx,edi
	sub	ecx,ebx
	mov	ebp,ecx
	shr	ecx,1
	xor	eax,eax
	cdq
      calculate_checksum:
	mov	dx,[ebx]
	add	eax,edx
	mov	dx,ax
	shr	eax,16
	add	eax,edx
	add	ebx,2
	loop	calculate_checksum
	add	eax,ebp
	mov	ebx,[code_start]
	mov	[ebx+58h],eax
	ret

format_coff:
	mov	eax,[additional_memory]
	mov	[symbols_stream],eax
	mov	ebx,eax
	add	eax,20h
	cmp	eax,[structures_buffer]
	jae	out_of_memory
	mov	[free_additional_memory],eax
	xor	eax,eax
	mov	[ebx],al
	mov	[ebx+4],eax
	mov	[ebx+8],edi
	mov	al,4
	mov	[ebx+10h],eax
	mov	al,60h
	bt	[format_flags],0
	jnc	flat_section_flags_ok
	or	eax,0E0000000h
      flat_section_flags_ok:
	mov	dword [ebx+14h],eax
	mov	[current_section],ebx
	mov	[number_of_sections],0
	mov	dword [org_origin],edi
	mov	dword [org_origin+4],0
	mov	[org_registers],0
	mov	[org_start],edi
	mov	[org_symbol],ebx
	mov	[labels_type],2
	mov	[code_type],32
	test	[format_flags],8
	jz	format_defined
	mov	[labels_type],4
	mov	[code_type],64
	jmp	format_defined
coff_section:
	call	close_coff_section
	mov	ebx,[free_additional_memory]
	lea	eax,[ebx+20h]
	cmp	eax,[structures_buffer]
	jae	out_of_memory
	mov	[free_additional_memory],eax
	mov	[current_section],ebx
	inc	[number_of_sections]
	xor	eax,eax
	mov	[ebx],al
	mov	[ebx+8],edi
	mov	dword [org_origin],edi
	mov	dword [org_origin+4],0
	mov	[org_registers],0
	mov	[org_start],edi
	mov	[org_symbol],ebx
	mov	[labels_type],2
	test	[format_flags],8
	jz	coff_labels_type_ok
	mov	[labels_type],4
      coff_labels_type_ok:
	mov	[ebx+10h],eax
	mov	[ebx+14h],eax
	lods	word [esi]
	cmp	ax,'('
	jne	invalid_argument
	mov	[ebx+4],esi
	mov	ecx,[esi]
	lea	esi,[esi+4+ecx+1]
	cmp	ecx,8
	ja	name_too_long
      coff_section_flags:
	cmp	byte [esi],1Ch
	je	coff_section_alignment
	cmp	byte [esi],19h
	jne	coff_section_settings_ok
	inc	esi
	lods	byte [esi]
	bt	[format_flags],0
	jc	coff_section_flag_ok
	cmp	al,7
	ja	invalid_argument
      coff_section_flag_ok:
	mov	cl,al
	mov	eax,1
	shl	eax,cl
	test	dword [ebx+14h],eax
	jnz	setting_already_specified
	or	dword [ebx+14h],eax
	jmp	coff_section_flags
      coff_section_alignment:
	bt	[format_flags],0
	jnc	invalid_argument
	inc	esi
	lods	byte [esi]
	or	al,al
	jnz	invalid_argument
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	push	ebx
	call	get_dword_value
	pop	ebx
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,eax
	dec	edx
	test	eax,edx
	jnz	invalid_value
	or	eax,eax
	jz	invalid_value
	cmp	eax,2000h
	ja	invalid_value
	bsf	edx,eax
	inc	edx
	shl	edx,20
	or	[ebx+14h],edx
	xchg	[ebx+10h],eax
	or	eax,eax
	jnz	setting_already_specified
	jmp	coff_section_flags
      coff_section_settings_ok:
	cmp	dword [ebx+10h],0
	jne	instruction_assembled
	mov	dword [ebx+10h],4
	bt	[format_flags],0
	jnc	instruction_assembled
	or	dword [ebx+14h],300000h
	jmp	instruction_assembled
      close_coff_section:
	mov	ebx,[current_section]
	mov	eax,edi
	mov	edx,[ebx+8]
	sub	eax,edx
	mov	[ebx+0Ch],eax
	xor	eax,eax
	xchg	[undefined_data_end],eax
	cmp	eax,edi
	jne	coff_section_ok
	cmp	edx,[undefined_data_start]
	jne	coff_section_ok
	mov	edi,edx
	or	byte [ebx+14h],80h
      coff_section_ok:
	ret
mark_coff_relocation:
	cmp	[value_type],3
	je	coff_relocation_relative
	push	ebx eax
	test	[format_flags],8
	jnz	coff_64bit_relocation
	mov	al,6
	jmp	coff_relocation
      coff_64bit_relocation:
	mov	al,1
	cmp	[value_type],4
	je	coff_relocation
	mov	al,2
	jmp	coff_relocation
      coff_relocation_relative:
	push	ebx
	bt	[format_flags],0
	jnc	relative_ok
	mov	ebx,[current_section]
	mov	ebx,[ebx+8]
	sub	ebx,edi
	sub	eax,ebx
	add	eax,4
      relative_ok:
	push	eax
	mov	al,20
	test	[format_flags],8
	jnz	relative_coff_64bit_relocation
	cmp	[labels_type],2
	jne	invalid_use_of_symbol
	jmp	coff_relocation
      relative_coff_64bit_relocation:
	mov	al,4
	cmp	[labels_type],4
	jne	invalid_use_of_symbol
      coff_relocation:
	mov	ebx,[free_additional_memory]
	add	ebx,0Ch
	cmp	ebx,[structures_buffer]
	jae	out_of_memory
	mov	[free_additional_memory],ebx
	mov	byte [ebx-0Ch],al
	mov	eax,[current_section]
	mov	eax,[eax+8]
	neg	eax
	add	eax,edi
	mov	[ebx-0Ch+4],eax
	mov	eax,[symbol_identifier]
	mov	[ebx-0Ch+8],eax
	pop	eax ebx
	ret
close_coff:
	call	close_coff_section
	cmp	[next_pass_needed],0
	je	coff_closed
	mov	eax,[symbols_stream]
	mov	[free_additional_memory],eax
      coff_closed:
	ret
coff_formatter:
	sub	edi,[code_start]
	mov	[code_size],edi
	call	prepare_default_section
	mov	edi,[free_additional_memory]
	mov	ebx,edi
	mov	ecx,28h shr 2
	imul	ecx,[number_of_sections]
	add	ecx,14h shr 2
	lea	eax,[edi+ecx*4]
	cmp	eax,[structures_buffer]
	jae	out_of_memory
	xor	eax,eax
	rep	stos dword [edi]
	mov	word [ebx],14Ch
	test	[format_flags],8
	jz	coff_magic_ok
	mov	word [ebx],8664h
      coff_magic_ok:
	mov	word [ebx+12h],104h
	bt	[format_flags],0
	jnc	coff_flags_ok
	or	byte [ebx+12h],80h
      coff_flags_ok:
	push	ebx
	call	make_timestamp
	pop	ebx
	mov	[ebx+4],eax
	mov	eax,[number_of_sections]
	mov	[ebx+2],ax
	mov	esi,[symbols_stream]
	xor	eax,eax
	xor	ecx,ecx
      enumerate_symbols:
	cmp	esi,[free_additional_memory]
	je	symbols_enumerated
	mov	dl,[esi]
	or	dl,dl
	jz	enumerate_section
	cmp	dl,80h
	je	enumerate_public
	ja	enumerate_extrn
	add	esi,0Ch
	jmp	enumerate_symbols
      enumerate_section:
	mov	edx,eax
	shl	edx,8
	mov	[esi],edx
	inc	eax
	inc	ecx
	mov	[esi+1Eh],cx
	add	esi,20h
	jmp	enumerate_symbols
      enumerate_public:
	mov	edx,eax
	shl	edx,8
	mov	dl,80h
	mov	[esi],edx
	mov	edx,[esi+8]
	add	esi,10h
	inc	eax
	cmp	byte [edx+11],2
	jne	enumerate_symbols
	mov	edx,[edx+20]
	cmp	byte [edx],81h
	jne	enumerate_symbols
	inc	eax
	jmp	enumerate_symbols
      enumerate_extrn:
	mov	edx,eax
	shl	edx,8
	mov	dl,81h
	mov	[esi],edx
	add	esi,0Ch
	inc	eax
	jmp	enumerate_symbols
      prepare_default_section:
	mov	ebx,[symbols_stream]
	cmp	dword [ebx+0Ch],0
	jne	default_section_ok
	cmp	[number_of_sections],0
	je	default_section_ok
	mov	edx,ebx
      find_references_to_default_section:
	cmp	ebx,[free_additional_memory]
	jne	check_reference
	add	[symbols_stream],20h
	ret
      check_reference:
	mov	al,[ebx]
	or	al,al
	jz	skip_other_section
	cmp	al,80h
	je	check_public_reference
	ja	next_reference
	cmp	edx,[ebx+8]
	je	default_section_ok
      next_reference:
	add	ebx,0Ch
	jmp	find_references_to_default_section
      check_public_reference:
	mov	eax,[ebx+8]
	add	ebx,10h
	test	byte [eax+8],1
	jz	find_references_to_default_section
	mov	cx,[current_pass]
	cmp	cx,[eax+16]
	jne	find_references_to_default_section
	cmp	edx,[eax+20]
	je	default_section_ok
	jmp	find_references_to_default_section
      skip_other_section:
	add	ebx,20h
	jmp	find_references_to_default_section
      default_section_ok:
	inc	[number_of_sections]
	ret
      symbols_enumerated:
	mov	[ebx+0Ch],eax
	mov	ebp,edi
	sub	ebp,ebx
	push	ebp
	lea	edi,[ebx+14h]
	mov	esi,[symbols_stream]
      find_section:
	cmp	esi,[free_additional_memory]
	je	sections_finished
	mov	al,[esi]
	or	al,al
	jz	section_found
	add	esi,0Ch
	cmp	al,80h
	jne	find_section
	add	esi,4
	jmp	find_section
      section_found:
	push	esi edi
	mov	esi,[esi+4]
	or	esi,esi
	jz	default_section
	mov	ecx,[esi]
	add	esi,4
	rep	movs byte [edi],[esi]
	jmp	section_name_ok
      default_section:
	mov	al,'.'
	stos	byte [edi]
	mov	eax,'flat'
	stos	dword [edi]
      section_name_ok:
	pop	edi esi
	mov	eax,[esi+0Ch]
	mov	[edi+10h],eax
	mov	eax,[esi+14h]
	mov	[edi+24h],eax
	test	al,80h
	jnz	section_ptr_ok
	mov	eax,[esi+8]
	sub	eax,[code_start]
	add	eax,ebp
	mov	[edi+14h],eax
      section_ptr_ok:
	mov	ebx,[code_start]
	mov	edx,[code_size]
	add	ebx,edx
	add	edx,ebp
	xor	ecx,ecx
	add	esi,20h
      find_relocations:
	cmp	esi,[free_additional_memory]
	je	section_relocations_done
	mov	al,[esi]
	or	al,al
	jz	section_relocations_done
	cmp	al,80h
	jb	add_relocation
	ja	next_relocation
	add	esi,10h
	jmp	find_relocations
      add_relocation:
	lea	eax,[ebx+0Ah]
	cmp	eax,[display_buffer]
	ja	out_of_memory
	mov	eax,[esi+4]
	mov	[ebx],eax
	mov	eax,[esi+8]
	mov	eax,[eax]
	shr	eax,8
	mov	[ebx+4],eax
	movzx	ax,byte [esi]
	mov	[ebx+8],ax
	add	ebx,0Ah
	inc	ecx
      next_relocation:
	add	esi,0Ch
	jmp	find_relocations
      section_relocations_done:
	cmp	ecx,10000h
	jb	section_relocations_count_16bit
	bt	[format_flags],0
	jnc	format_limitations_exceeded
	mov	word [edi+20h],0FFFFh
	or	dword [edi+24h],1000000h
	mov	[edi+18h],edx
	push	esi edi
	push	ecx
	lea	esi,[ebx-1]
	add	ebx,0Ah
	lea	edi,[ebx-1]
	imul	ecx,0Ah
	std
	rep	movs byte [edi],[esi]
	cld
	pop	ecx
	inc	esi
	inc	ecx
	mov	[esi],ecx
	xor	eax,eax
	mov	[esi+4],eax
	mov	[esi+8],ax
	pop	edi esi
	jmp	section_relocations_ok
      section_relocations_count_16bit:
	mov	[edi+20h],cx
	jcxz	section_relocations_ok
	mov	[edi+18h],edx
      section_relocations_ok:
	sub	ebx,[code_start]
	mov	[code_size],ebx
	add	edi,28h
	jmp	find_section
      sections_finished:
	mov	edx,[free_additional_memory]
	mov	ebx,[code_size]
	add	ebp,ebx
	mov	[edx+8],ebp
	add	ebx,[code_start]
	mov	edi,ebx
	mov	ecx,[edx+0Ch]
	imul	ecx,12h shr 1
	xor	eax,eax
	shr	ecx,1
	jnc	zero_symbols_table
	stos	word [edi]
      zero_symbols_table:
	rep	stos dword [edi]
	mov	edx,edi
	stos	dword [edi]
	mov	esi,[symbols_stream]
      make_symbols_table:
	cmp	esi,[free_additional_memory]
	je	symbols_table_ok
	mov	al,[esi]
	cmp	al,80h
	je	add_public_symbol
	ja	add_extrn_symbol
	or	al,al
	jz	add_section_symbol
	add	esi,0Ch
	jmp	make_symbols_table
      add_section_symbol:
	call	store_symbol_name
	movzx	eax,word [esi+1Eh]
	mov	[ebx+0Ch],ax
	mov	byte [ebx+10h],3
	add	esi,20h
	add	ebx,12h
	jmp	make_symbols_table
      add_extrn_symbol:
	call	store_symbol_name
	mov	byte [ebx+10h],2
	add	esi,0Ch
	add	ebx,12h
	jmp	make_symbols_table
      add_public_symbol:
	call	store_symbol_name
	mov	eax,[esi+0Ch]
	mov	[current_line],eax
	mov	eax,[esi+8]
	test	byte [eax+8],1
	jz	undefined_symbol
	mov	cx,[current_pass]
	cmp	cx,[eax+16]
	jne	undefined_symbol
	mov	cl,[eax+11]
	or	cl,cl
	jz	public_constant
	test	[format_flags],8
	jnz	check_64bit_public_symbol
	cmp	cl,2
	je	public_symbol_type_ok
	jmp	invalid_use_of_symbol
      check_64bit_public_symbol:
	cmp	cl,4
	jne	invalid_use_of_symbol
      public_symbol_type_ok:
	mov	ecx,[eax+20]
	cmp	byte [ecx],81h
	je	alias_symbol
	cmp	byte [ecx],0
	jne	invalid_use_of_symbol
	mov	cx,[ecx+1Eh]
	mov	[ebx+0Ch],cx
      public_symbol_section_ok:
	cmp	dword [eax+4],0
	je	store_public_symbol
	cmp	dword [eax+4],-1
	jne	value_out_of_range
	bt	dword [eax],31
	jnc	value_out_of_range
      store_public_symbol:
	mov	eax,[eax]
	mov	[ebx+8],eax
	mov	byte [ebx+10h],2
	add	esi,10h
	add	ebx,12h
	jmp	make_symbols_table
      alias_symbol:
	bt	[format_flags],0
	jnc	invalid_use_of_symbol
	mov	ecx,[eax]
	or	ecx,[eax+4]
	jnz	invalid_use_of_symbol
	mov	byte [ebx+10h],69h
	mov	byte [ebx+11h],1
	add	ebx,12h
	mov	ecx,[eax+20]
	mov	ecx,[ecx]
	shr	ecx,8
	mov	[ebx],ecx
	mov	byte [ebx+4],3
	add	esi,10h
	add	ebx,12h
	jmp	make_symbols_table
      public_constant:
	mov	word [ebx+0Ch],0FFFFh
	jmp	public_symbol_section_ok
      symbols_table_ok:
	mov	eax,edi
	sub	eax,edx
	mov	[edx],eax
	sub	edi,[code_start]
	mov	[code_size],edi
	mov	[written_size],0
	mov	edx,[output_file]
	call	create
	jc	write_failed
	mov	edx,[free_additional_memory]
	pop	ecx
	add	[written_size],ecx
	call	write
	jc	write_failed
	jmp	write_output
      store_symbol_name:
	push	esi
	mov	esi,[esi+4]
	or	esi,esi
	jz	default_name
	lods	dword [esi]
	mov	ecx,eax
	cmp	ecx,8
	ja	add_string
	push	edi
	mov	edi,ebx
	rep	movs byte [edi],[esi]
	pop	edi esi
	ret
      default_name:
	mov	dword [ebx],'.fla'
	mov	dword [ebx+4],'t'
	pop	esi
	ret
      add_string:
	mov	eax,edi
	sub	eax,edx
	mov	[ebx+4],eax
	inc	ecx
	rep	movs byte [edi],[esi]
	pop	esi
	ret

format_elf:
	test	[format_flags],8
	jnz	format_elf64
	mov	edx,edi
	mov	ecx,34h shr 2
	lea	eax,[edi+ecx*4]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	xor	eax,eax
	rep	stos dword [edi]
	mov	dword [edx],7Fh + 'ELF' shl 8
	mov	al,1
	mov	[edx+4],al
	mov	[edx+5],al
	mov	[edx+6],al
	mov	[edx+14h],al
	mov	byte [edx+12h],3
	mov	byte [edx+28h],34h
	mov	byte [edx+2Eh],28h
	mov	[code_type],32
	cmp	word [esi],1D19h
	je	format_elf_exe
	mov	[labels_type],2
      elf_header_ok:
	mov	byte [edx+10h],1
	mov	eax,[additional_memory]
	mov	[symbols_stream],eax
	mov	ebx,eax
	add	eax,20h
	cmp	eax,[structures_buffer]
	jae	out_of_memory
	mov	[free_additional_memory],eax
	xor	eax,eax
	mov	[current_section],ebx
	mov	[number_of_sections],eax
	mov	dword [org_origin],edi
	mov	dword [org_origin+4],eax
	mov	[org_registers],eax
	mov	[org_start],edi
	mov	[org_symbol],ebx
	mov	[ebx],al
	mov	[ebx+4],eax
	mov	[ebx+8],edi
	mov	al,111b
	mov	[ebx+14h],eax
	mov	al,4
	mov	[ebx+10h],eax
	test	[format_flags],8
	jz	format_defined
	mov	byte [ebx+10h],8
	jmp	format_defined
      format_elf64:
	mov	edx,edi
	mov	ecx,40h shr 2
	lea	eax,[edi+ecx*4]
	cmp	eax,[display_buffer]
	jae	out_of_memory
	xor	eax,eax
	rep	stos dword [edi]
	mov	dword [edx],7Fh + 'ELF' shl 8
	mov	al,1
	mov	[edx+5],al
	mov	[edx+6],al
	mov	[edx+14h],al
	mov	byte [edx+4],2
	mov	byte [edx+12h],62
	mov	byte [edx+34h],40h
	mov	byte [edx+3Ah],40h
	mov	[code_type],64
	cmp	word [esi],1D19h
	je	format_elf64_exe
	mov	[labels_type],4
	jmp	elf_header_ok
elf_section:
	bt	[format_flags],0
	jc	illegal_instruction
	call	close_coff_section
	mov	ebx,[free_additional_memory]
	lea	eax,[ebx+20h]
	cmp	eax,[structures_buffer]
	jae	out_of_memory
	mov	[free_additional_memory],eax
	mov	[current_section],ebx
	inc	word [number_of_sections]
	jz	format_limitations_exceeded
	xor	eax,eax
	mov	[ebx],al
	mov	[ebx+8],edi
	mov	dword [org_origin],edi
	mov	dword [org_origin+4],0
	mov	[org_registers],0
	mov	[org_start],edi
	mov	[org_symbol],ebx
	test	[format_flags],8
	jnz	elf64_labels_type
	mov	[labels_type],2
	jmp	elf_labels_type_ok
      elf64_labels_type:
	mov	[labels_type],4
      elf_labels_type_ok:
	mov	[ebx+10h],eax
	mov	al,10b
	mov	[ebx+14h],eax
	lods	word [esi]
	cmp	ax,'('
	jne	invalid_argument
	mov	[ebx+4],esi
	mov	ecx,[esi]
	lea	esi,[esi+4+ecx+1]
      elf_section_flags:
	cmp	byte [esi],1Ch
	je	elf_section_alignment
	cmp	byte [esi],19h
	jne	elf_section_settings_ok
	inc	esi
	lods	byte [esi]
	sub	al,28
	xor	al,11b
	test	al,not 10b
	jnz	invalid_argument
	mov	cl,al
	mov	al,1
	shl	al,cl
	test	byte [ebx+14h],al
	jnz	setting_already_specified
	or	byte [ebx+14h],al
	jmp	elf_section_flags
      elf_section_alignment:
	inc	esi
	lods	byte [esi]
	or	al,al
	jnz	invalid_argument
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	push	ebx
	call	get_dword_value
	pop	ebx
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,eax
	dec	edx
	test	eax,edx
	jnz	invalid_value
	or	eax,eax
	jz	invalid_value
	xchg	[ebx+10h],eax
	or	eax,eax
	jnz	setting_already_specified
	jmp	elf_section_flags
      elf_section_settings_ok:
	cmp	dword [ebx+10h],0
	jne	instruction_assembled
	mov	dword [ebx+10h],4
	test	[format_flags],8
	jz	instruction_assembled
	mov	byte [ebx+10h],8
	jmp	instruction_assembled
mark_elf_relocation:
	cmp	[value_type],3
	je	elf_relocation_relative
	cmp	[value_type],7
	je	elf_relocation_relative
	push	ebx eax
	cmp	[value_type],5
	je	elf_gotoff_relocation
	ja	invalid_use_of_symbol
	mov	al,1			; R_386_32 / R_AMD64_64
	test	[format_flags],8
	jz	coff_relocation
	cmp	[value_type],4
	je	coff_relocation
	mov	al,11			; R_AMD64_32S
	jmp	coff_relocation
      elf_gotoff_relocation:
	test	[format_flags],8
	jnz	invalid_use_of_symbol
	mov	al,9			; R_386_GOTOFF
	jmp	coff_relocation
      elf_relocation_relative:
	cmp	[labels_type],0
	je	invalid_use_of_symbol
	push	ebx
	mov	ebx,[current_section]
	mov	ebx,[ebx+8]
	sub	ebx,edi
	sub	eax,ebx
	push	eax
	mov	al,2			; R_386_PC32 / R_AMD64_PC32
	cmp	[value_type],3
	je	coff_relocation
	mov	al,4			; R_386_PLT32 / R_AMD64_PLT32
	jmp	coff_relocation
close_elf:
	bt	[format_flags],0
	jc	close_elf_exe
	call	close_coff_section
	cmp	[next_pass_needed],0
	je	elf_closed
	mov	eax,[symbols_stream]
	mov	[free_additional_memory],eax
      elf_closed:
	ret
elf_formatter:
	push	edi
	call	prepare_default_section
	mov	esi,[symbols_stream]
	mov	edi,[free_additional_memory]
	xor	eax,eax
	mov	ecx,4
	rep	stos dword [edi]
	test	[format_flags],8
	jz	find_first_section
	mov	ecx,2
	rep	stos dword [edi]
      find_first_section:
	mov	al,[esi]
	or	al,al
	jz	first_section_found
	cmp	al,80h
	jne	skip_other_symbol
	add	esi,4
      skip_other_symbol:
	add	esi,0Ch
	jmp	find_first_section
      first_section_found:
	mov	ebx,esi
	mov	ebp,esi
	add	esi,20h
	xor	ecx,ecx
	xor	edx,edx
      find_next_section:
	cmp	esi,[free_additional_memory]
	je	make_section_symbol
	mov	al,[esi]
	or	al,al
	jz	make_section_symbol
	cmp	al,80h
	je	skip_public
	ja	skip_extrn
	or	byte [ebx+14h],40h
      skip_extrn:
	add	esi,0Ch
	jmp	find_next_section
      skip_public:
	add	esi,10h
	jmp	find_next_section
      make_section_symbol:
	mov	eax,edi
	xchg	eax,[ebx+4]
	stos	dword [edi]
	test	[format_flags],8
	jnz	elf64_section_symbol
	xor	eax,eax
	stos	dword [edi]
	stos	dword [edi]
	call	store_section_index
	jmp	section_symbol_ok
      store_section_index:
	inc	ecx
	mov	eax,ecx
	shl	eax,8
	mov	[ebx],eax
	inc	dx
	jz	format_limitations_exceeded
	mov	eax,edx
	shl	eax,16
	mov	al,3
	test	byte [ebx+14h],40h
	jz	section_index_ok
	or	ah,-1
	inc	dx
	jz	format_limitations_exceeded
      section_index_ok:
	stos	dword [edi]
	ret
      elf64_section_symbol:
	call	store_section_index
	xor	eax,eax
	stos	dword [edi]
	stos	dword [edi]
	stos	dword [edi]
	stos	dword [edi]
      section_symbol_ok:
	mov	ebx,esi
	add	esi,20h
	cmp	ebx,[free_additional_memory]
	jne	find_next_section
	inc	dx
	jz	format_limitations_exceeded
	mov	[current_section],edx
	mov	esi,[symbols_stream]
      find_other_symbols:
	cmp	esi,[free_additional_memory]
	je	elf_symbol_table_ok
	mov	al,[esi]
	or	al,al
	jz	skip_section
	cmp	al,80h
	je	make_public_symbol
	ja	make_extrn_symbol
	add	esi,0Ch
	jmp	find_other_symbols
      skip_section:
	add	esi,20h
	jmp	find_other_symbols
      make_public_symbol:
	mov	eax,[esi+0Ch]
	mov	[current_line],eax
	mov	ebx,[esi+8]
	test	byte [ebx+8],1
	jz	undefined_symbol
	mov	ax,[current_pass]
	cmp	ax,[ebx+16]
	jne	undefined_symbol
	mov	dl,[ebx+11]
	or	dl,dl
	jz	public_absolute
	mov	eax,[ebx+20]
	cmp	byte [eax],0
	jne	invalid_use_of_symbol
	mov	eax,[eax+4]
	test	[format_flags],8
	jnz	elf64_public
	cmp	dl,2
	jne	invalid_use_of_symbol
	mov	dx,[eax+0Eh]
	jmp	section_for_public_ok
      elf64_public:
	cmp	dl,4
	jne	invalid_use_of_symbol
	mov	dx,[eax+6]
	jmp	section_for_public_ok
      public_absolute:
	mov	dx,0FFF1h
      section_for_public_ok:
	mov	eax,[esi+4]
	stos	dword [edi]
	test	[format_flags],8
	jnz	elf64_public_symbol
	call	get_public_value
	stos	dword [edi]
	xor	eax,eax
	mov	al,[ebx+10]
	stos	dword [edi]
	mov	eax,edx
	shl	eax,16
	mov	al,10h
	cmp	byte [ebx+10],0
	je	elf_public_function
	or	al,1
	jmp	store_elf_public_info
      elf_public_function:
	or	al,2
      store_elf_public_info:
	stos	dword [edi]
	jmp	public_symbol_ok
      elf64_public_symbol:
	mov	eax,edx
	shl	eax,16
	mov	al,10h
	cmp	byte [ebx+10],0
	je	elf64_public_function
	or	al,1
	jmp	store_elf64_public_info
      elf64_public_function:
	or	al,2
      store_elf64_public_info:
	stos	dword [edi]
	call	get_public_value
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	mov	al,[ebx+10]
	stos	dword [edi]
	xor	al,al
	stos	dword [edi]
      public_symbol_ok:
	inc	ecx
	mov	eax,ecx
	shl	eax,8
	mov	al,80h
	mov	[esi],eax
	add	esi,10h
	jmp	find_other_symbols
      get_public_value:
	mov	eax,[ebx]
	cmp	dword [ebx+4],0
	je	public_value_ok
	cmp	dword [ebx+4],-1
	jne	value_out_of_range
	bt	eax,31
	jnc	value_out_of_range
      public_value_ok:
	ret
      make_extrn_symbol:
	mov	eax,[esi+4]
	stos	dword [edi]
	test	[format_flags],8
	jnz	elf64_extrn_symbol
	xor	eax,eax
	stos	dword [edi]
	mov	eax,[esi+8]
	stos	dword [edi]
	mov	eax,10h
	stos	dword [edi]
	jmp	extrn_symbol_ok
      elf64_extrn_symbol:
	mov	eax,10h
	stos	dword [edi]
	xor	al,al
	stos	dword [edi]
	stos	dword [edi]
	mov	eax,[esi+8]
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
      extrn_symbol_ok:
	inc	ecx
	mov	eax,ecx
	shl	eax,8
	mov	al,81h
	mov	[esi],eax
	add	esi,0Ch
	jmp	find_other_symbols
      elf_symbol_table_ok:
	mov	edx,edi
	mov	ebx,[free_additional_memory]
	xor	al,al
	stos	byte [edi]
	add	edi,16
	mov	[edx+1],edx
	add	ebx,10h
	test	[format_flags],8
	jz	make_string_table
	add	ebx,8
      make_string_table:
	cmp	ebx,edx
	je	elf_string_table_ok
	test	[format_flags],8
	jnz	make_elf64_string
	cmp	byte [ebx+0Dh],0
	je	rel_prefix_ok
	mov	byte [ebx+0Dh],0
	mov	eax,'.rel'
	stos	dword [edi]
      rel_prefix_ok:
	mov	esi,edi
	sub	esi,edx
	xchg	esi,[ebx]
	add	ebx,10h
      make_elf_string:
	or	esi,esi
	jz	default_string
	lods	dword [esi]
	mov	ecx,eax
	rep	movs byte [edi],[esi]
	xor	al,al
	stos	byte [edi]
	jmp	make_string_table
      make_elf64_string:
	cmp	byte [ebx+5],0
	je	elf64_rel_prefix_ok
	mov	byte [ebx+5],0
	mov	eax,'.rel'
	stos	dword [edi]
	mov	al,'a'
	stos	byte [edi]
      elf64_rel_prefix_ok:
	mov	esi,edi
	sub	esi,edx
	xchg	esi,[ebx]
	add	ebx,18h
	jmp	make_elf_string
      default_string:
	mov	eax,'.fla'
	stos	dword [edi]
	mov	ax,'t'
	stos	word [edi]
	jmp	make_string_table
      elf_string_table_ok:
	mov	[edx+1+8],edi
	mov	ebx,[code_start]
	mov	eax,edi
	sub	eax,[free_additional_memory]
	test	[format_flags],8
	jnz	finish_elf64_header
	mov	[ebx+20h],eax
	mov	eax,[current_section]
	inc	ax
	jz	format_limitations_exceeded
	mov	[ebx+32h],ax
	inc	ax
	jz	format_limitations_exceeded
	mov	[ebx+30h],ax
	jmp	elf_header_finished
      finish_elf64_header:
	mov	[ebx+28h],eax
	mov	eax,[current_section]
	inc	ax
	jz	format_limitations_exceeded
	mov	[ebx+3Eh],ax
	inc	ax
	jz	format_limitations_exceeded
	mov	[ebx+3Ch],ax
      elf_header_finished:
	xor	eax,eax
	mov	ecx,10
	rep	stos dword [edi]
	test	[format_flags],8
	jz	elf_null_section_ok
	mov	ecx,6
	rep	stos dword [edi]
      elf_null_section_ok:
	mov	esi,ebp
	xor	ecx,ecx
      make_section_entry:
	mov	ebx,edi
	mov	eax,[esi+4]
	mov	eax,[eax]
	stos	dword [edi]
	mov	eax,1
	cmp	dword [esi+0Ch],0
	je	bss_section
	test	byte [esi+14h],80h
	jz	section_type_ok
      bss_section:
	mov	al,8
      section_type_ok:
	stos	dword [edi]
	mov	eax,[esi+14h]
	and	al,3Fh
	call	store_elf_machine_word
	xor	eax,eax
	call	store_elf_machine_word
	mov	eax,[esi+8]
	mov	[image_base],eax
	sub	eax,[code_start]
	call	store_elf_machine_word
	mov	eax,[esi+0Ch]
	call	store_elf_machine_word
	xor	eax,eax
	stos	dword [edi]
	stos	dword [edi]
	mov	eax,[esi+10h]
	call	store_elf_machine_word
	xor	eax,eax
	call	store_elf_machine_word
	inc	ecx
	add	esi,20h
	xchg	edi,[esp]
	mov	ebp,edi
      convert_relocations:
	cmp	esi,[free_additional_memory]
	je	relocations_converted
	mov	al,[esi]
	or	al,al
	jz	relocations_converted
	cmp	al,80h
	jb	make_relocation_entry
	ja	relocation_entry_ok
	add	esi,10h
	jmp	convert_relocations
      make_relocation_entry:
	test	[format_flags],8
	jnz	make_elf64_relocation_entry
	mov	eax,[esi+4]
	stos	dword [edi]
	mov	eax,[esi+8]
	mov	eax,[eax]
	mov	al,[esi]
	stos	dword [edi]
	jmp	relocation_entry_ok
      make_elf64_relocation_entry:
	mov	eax,[esi+4]
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	movzx	eax,byte [esi]
	stos	dword [edi]
	mov	eax,[esi+8]
	mov	eax,[eax]
	shr	eax,8
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	stos	dword [edi]
      relocation_entry_ok:
	add	esi,0Ch
	jmp	convert_relocations
      store_elf_machine_word:
	stos	dword [edi]
	test	[format_flags],8
	jz	elf_machine_word_ok
	mov	dword [edi],0
	add	edi,4
      elf_machine_word_ok:
	ret
      relocations_converted:
	cmp	edi,ebp
	xchg	edi,[esp]
	je	rel_section_ok
	mov	eax,[ebx]
	sub	eax,4
	test	[format_flags],8
	jz	store_relocations_name_offset
	dec	eax
      store_relocations_name_offset:
	stos	dword [edi]
	test	[format_flags],8
	jnz	rela_section
	mov	eax,9
	jmp	store_relocations_type
      rela_section:
	mov	eax,4
      store_relocations_type:
	stos	dword [edi]
	xor	al,al
	call	store_elf_machine_word
	call	store_elf_machine_word
	mov	eax,ebp
	sub	eax,[code_start]
	call	store_elf_machine_word
	mov	eax,[esp]
	sub	eax,ebp
	call	store_elf_machine_word
	mov	eax,[current_section]
	stos	dword [edi]
	mov	eax,ecx
	stos	dword [edi]
	inc	ecx
	test	[format_flags],8
	jnz	finish_elf64_rela_section
	mov	eax,4
	stos	dword [edi]
	mov	al,8
	stos	dword [edi]
	jmp	rel_section_ok
      finish_elf64_rela_section:
	mov	eax,8
	stos	dword [edi]
	xor	al,al
	stos	dword [edi]
	mov	al,24
	stos	dword [edi]
	xor	al,al
	stos	dword [edi]
      rel_section_ok:
	cmp	esi,[free_additional_memory]
	jne	make_section_entry
	pop	eax
	mov	ebx,[code_start]
	sub	eax,ebx
	mov	[code_size],eax
	mov	ecx,20h
	test	[format_flags],8
	jz	adjust_elf_section_headers_offset
	mov	ecx,28h
      adjust_elf_section_headers_offset:
	add	[ebx+ecx],eax
	mov	eax,1
	stos	dword [edi]
	mov	al,2
	stos	dword [edi]
	xor	al,al
	call	store_elf_machine_word
	call	store_elf_machine_word
	mov	eax,[code_size]
	call	store_elf_machine_word
	mov	eax,[edx+1]
	sub	eax,[free_additional_memory]
	call	store_elf_machine_word
	mov	eax,[current_section]
	inc	eax
	stos	dword [edi]
	mov	eax,[number_of_sections]
	inc	eax
	stos	dword [edi]
	test	[format_flags],8
	jnz	finish_elf64_sym_section
	mov	eax,4
	stos	dword [edi]
	mov	al,10h
	stos	dword [edi]
	jmp	sym_section_ok
      finish_elf64_sym_section:
	mov	eax,8
	stos	dword [edi]
	xor	al,al
	stos	dword [edi]
	mov	al,18h
	stos	dword [edi]
	xor	al,al
	stos	dword [edi]
      sym_section_ok:
	mov	al,1+8
	stos	dword [edi]
	mov	al,3
	stos	dword [edi]
	xor	al,al
	call	store_elf_machine_word
	call	store_elf_machine_word
	mov	eax,[edx+1]
	sub	eax,[free_additional_memory]
	add	eax,[code_size]
	call	store_elf_machine_word
	mov	eax,[edx+1+8]
	sub	eax,[edx+1]
	call	store_elf_machine_word
	xor	eax,eax
	stos	dword [edi]
	stos	dword [edi]
	mov	al,1
	call	store_elf_machine_word
	xor	eax,eax
	call	store_elf_machine_word
	mov	eax,'tab'
	mov	dword [edx+1],'.sym'
	mov	[edx+1+4],eax
	mov	dword [edx+1+8],'.str'
	mov	[edx+1+8+4],eax
	mov	[written_size],0
	mov	edx,[output_file]
	call	create
	jc	write_failed
	call	write_code
	mov	ecx,edi
	mov	edx,[free_additional_memory]
	sub	ecx,edx
	add	[written_size],ecx
	call	write
	jc	write_failed
	jmp	output_written

format_elf_exe:
	add	esi,2
	or	[format_flags],1
	mov	[image_base],8048000h
	cmp	byte [esi],80h
	jne	elf_exe_base_ok
	lods	word [esi]
	cmp	ah,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	push	edx
	call	get_dword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	[image_base],eax
	pop	edx
      elf_exe_base_ok:
	mov	byte [edx+10h],2
	mov	byte [edx+2Ah],20h
	mov	ebx,edi
	mov	ecx,20h shr 2
	cmp	[current_pass],0
	je	init_elf_segments
	imul	ecx,[number_of_sections]
      init_elf_segments:
	xor	eax,eax
	rep	stos dword [edi]
	mov	[number_of_sections],0
	mov	byte [ebx],1
	mov	word [ebx+1Ch],1000h
	mov	byte [ebx+18h],111b
	mov	eax,edi
	sub	eax,[code_start]
	mov	[ebx+4],eax
	add	eax,[image_base]
	mov	[ebx+8],eax
	mov	[ebx+0Ch],eax
	mov	[edx+18h],eax
	xor	edx,edx
	not	eax
	not	edx
	add	eax,1
	adc	edx,0
	add	eax,edi
	adc	edx,0
	mov	dword [org_origin],eax
	mov	dword [org_origin+4],edx
	mov	[org_registers],0
	mov	[org_start],edi
	mov	[symbols_stream],edi
	jmp	format_defined
      format_elf64_exe:
	add	esi,2
	or	[format_flags],1
	mov	[image_base],400000h
	mov	[image_base_high],0
	cmp	byte [esi],80h
	jne	elf64_exe_base_ok
	lods	word [esi]
	cmp	ah,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	push	edx
	call	get_qword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	[image_base],eax
	mov	[image_base_high],edx
	pop	edx
      elf64_exe_base_ok:
	mov	byte [edx+10h],2
	mov	byte [edx+36h],38h
	mov	ebx,edi
	mov	ecx,38h shr 2
	cmp	[current_pass],0
	je	init_elf64_segments
	imul	ecx,[number_of_sections]
      init_elf64_segments:
	xor	eax,eax
	rep	stos dword [edi]
	mov	[number_of_sections],0
	mov	byte [ebx],1
	mov	word [ebx+30h],1000h
	mov	byte [ebx+4],111b
	push	edx
	mov	eax,edi
	sub	eax,[code_start]
	mov	[ebx+8],eax
	xor	edx,edx
	add	eax,[image_base]
	adc	edx,[image_base_high]
	mov	[ebx+10h],eax
	mov	[ebx+10h+4],edx
	mov	[ebx+18h],eax
	mov	[ebx+18h+4],edx
	pop	ebx
	mov	[ebx+18h],eax
	mov	[ebx+18h+4],edx
	not	eax
	not	edx
	add	eax,1
	adc	edx,0
	add	eax,edi
	adc	edx,0
	mov	dword [org_origin],eax
	mov	dword [org_origin+4],edx
	mov	[org_registers],0
	mov	[org_start],edi
	mov	[symbols_stream],edi
	jmp	format_defined
elf_entry:
	lods	byte [esi]
	cmp	al,'('
	jne	invalid_argument
	cmp	byte [esi],'.'
	je	invalid_value
	test	[format_flags],8
	jnz	elf64_entry
	call	get_dword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	edx,[code_start]
	mov	[edx+18h],eax
	jmp	instruction_assembled
      elf64_entry:
	call	get_qword_value
	cmp	[value_type],0
	jne	invalid_use_of_symbol
	mov	ebx,[code_start]
	mov	[ebx+18h],eax
	mov	[ebx+1Ch],edx
	jmp	instruction_assembled
elf_segment:
	bt	[format_flags],0
	jnc	illegal_instruction
	test	[format_flags],8
	jnz	elf64_segment
	call	close_elf_segment
	push	eax
	mov	ebx,[number_of_sections]
	shl	ebx,5
	add	ebx,[code_start]
	add	ebx,34h
	cmp	ebx,[symbols_stream]
	jb	new_elf_segment
	mov	ebx,[symbols_stream]
	sub	ebx,20h
	push	edi
	mov	edi,ebx
	mov	ecx,20h shr 2
	xor	eax,eax
	rep	stos dword [edi]
	pop	edi
	or	[next_pass_needed],-1
      new_elf_segment:
	mov	byte [ebx],1
	mov	word [ebx+1Ch],1000h
      elf_segment_flags:
	cmp	byte [esi],19h
	jne	elf_segment_flags_ok
	lods	word [esi]
	sub	ah,28
	jbe	invalid_argument
	cmp	ah,1
	je	mark_elf_segment_flag
	cmp	ah,3
	ja	invalid_argument
	xor	ah,1
	cmp	ah,2
	je	mark_elf_segment_flag
	inc	ah
      mark_elf_segment_flag:
	test	[ebx+18h],ah
	jnz	setting_already_specified
	or	[ebx+18h],ah
	jmp	elf_segment_flags
      elf_segment_flags_ok:
	mov	eax,edi
	sub	eax,[code_start]
	mov	[ebx+4],eax
	pop	edx
	and	eax,0FFFh
	add	edx,eax
	mov	[ebx+8],edx
	mov	[ebx+0Ch],edx
	mov	eax,edx
	xor	edx,edx
	not	eax
	not	edx
	add	eax,1
	adc	edx,0
	add	eax,edi
	adc	edx,0
	mov	dword [org_origin],eax
	mov	dword [org_origin+4],edx
	mov	[org_registers],0
	mov	[org_start],edi
	inc	[number_of_sections]
	jmp	instruction_assembled
      close_elf_segment:
	cmp	[number_of_sections],0
	jne	finish_elf_segment
	cmp	edi,[symbols_stream]
	jne	first_elf_segment_ok
	push	edi
	mov	edi,[code_start]
	add	edi,34h
	mov	ecx,20h shr 2
	xor	eax,eax
	rep	stos dword [edi]
	pop	edi
	mov	eax,[image_base]
	ret
      first_elf_segment_ok:
	inc	[number_of_sections]
      finish_elf_segment:
	mov	ebx,[number_of_sections]
	dec	ebx
	shl	ebx,5
	add	ebx,[code_start]
	add	ebx,34h
	mov	eax,edi
	sub	eax,[code_start]
	sub	eax,[ebx+4]
	mov	edx,edi
	cmp	edi,[undefined_data_end]
	jne	elf_segment_size_ok
	mov	edi,[undefined_data_start]
      elf_segment_size_ok:
	mov	[ebx+14h],eax
	add	eax,edi
	sub	eax,edx
	mov	[ebx+10h],eax
	mov	eax,[ebx+8]
	add	eax,[ebx+14h]
	add	eax,0FFFh
	and	eax,not 0FFFh
	ret
      elf64_segment:
	call	close_elf64_segment
	push	eax edx
	mov	ebx,[number_of_sections]
	imul	ebx,38h
	add	ebx,[code_start]
	add	ebx,40h
	cmp	ebx,[symbols_stream]
	jb	new_elf64_segment
	mov	ebx,[symbols_stream]
	sub	ebx,38h
	push	edi
	mov	edi,ebx
	mov	ecx,38h shr 2
	xor	eax,eax
	rep	stos dword [edi]
	pop	edi
	or	[next_pass_needed],-1
      new_elf64_segment:
	mov	byte [ebx],1
	mov	word [ebx+30h],1000h
      elf64_segment_flags:
	cmp	byte [esi],19h
	jne	elf64_segment_flags_ok
	lods	word [esi]
	sub	ah,28
	jbe	invalid_argument
	cmp	ah,1
	je	mark_elf64_segment_flag
	cmp	ah,3
	ja	invalid_argument
	xor	ah,1
	cmp	ah,2
	je	mark_elf64_segment_flag
	inc	ah
      mark_elf64_segment_flag:
	test	[ebx+4],ah
	jnz	setting_already_specified
	or	[ebx+4],ah
	jmp	elf64_segment_flags
      elf64_segment_flags_ok:
	mov	ecx,edi
	sub	ecx,[code_start]
	mov	[ebx+8],ecx
	pop	edx eax
	and	ecx,0FFFh
	add	eax,ecx
	adc	edx,0
	mov	[ebx+10h],eax
	mov	[ebx+10h+4],edx
	mov	[ebx+18h],eax
	mov	[ebx+18h+4],edx
	not	eax
	not	edx
	add	eax,1
	adc	edx,0
	add	eax,edi
	adc	edx,0
	mov	dword [org_origin],eax
	mov	dword [org_origin+4],edx
	mov	[org_registers],0
	mov	[org_start],edi
	inc	[number_of_sections]
	jmp	instruction_assembled
      close_elf64_segment:
	cmp	[number_of_sections],0
	jne	finish_elf64_segment
	cmp	edi,[symbols_stream]
	jne	first_elf64_segment_ok
	push	edi
	mov	edi,[code_start]
	add	edi,40h
	mov	ecx,38h shr 2
	xor	eax,eax
	rep	stos dword [edi]
	pop	edi
	mov	eax,[image_base]
	mov	edx,[image_base_high]
	ret
      first_elf64_segment_ok:
	inc	[number_of_sections]
      finish_elf64_segment:
	mov	ebx,[number_of_sections]
	dec	ebx
	imul	ebx,38h
	add	ebx,[code_start]
	add	ebx,40h
	mov	eax,edi
	sub	eax,[code_start]
	sub	eax,[ebx+8]
	mov	edx,edi
	cmp	edi,[undefined_data_end]
	jne	elf64_segment_size_ok
	mov	edi,[undefined_data_start]
      elf64_segment_size_ok:
	mov	[ebx+28h],eax
	add	eax,edi
	sub	eax,edx
	mov	[ebx+20h],eax
	mov	eax,[ebx+10h]
	mov	edx,[ebx+10h+4]
	add	eax,[ebx+28h]
	adc	edx,0
	sub	eax,1
	sbb	edx,0
	shrd	eax,edx,12
	shr	edx,12
	add	eax,1
	adc	edx,0
	shld	edx,eax,12
	shl	eax,12
	ret
close_elf_exe:
	test	[format_flags],8
	jnz	close_elf64_exe
	call	close_elf_segment
	mov	edx,[code_start]
	mov	eax,[number_of_sections]
	mov	byte [edx+1Ch],34h
	mov	[edx+2Ch],ax
	shl	eax,5
	add	eax,edx
	add	eax,34h
	cmp	eax,[symbols_stream]
	je	elf_exe_ok
	or	[next_pass_needed],-1
      elf_exe_ok:
	ret
      close_elf64_exe:
	call	close_elf64_segment
	mov	edx,[code_start]
	mov	eax,[number_of_sections]
	mov	byte [edx+20h],40h
	mov	[edx+38h],ax
	imul	eax,38h
	add	eax,edx
	add	eax,40h
	cmp	eax,[symbols_stream]
	je	elf64_exe_ok
	or	[next_pass_needed],-1
      elf64_exe_ok:
	ret
