
; flat assembler core
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

preprocessor:
	mov	edi,characters
	xor	al,al
      make_characters_table:
	stosb
	inc	al
	jnz	make_characters_table
	mov	esi,characters+'a'
	mov	edi,characters+'A'
	mov	ecx,26
	rep	movsb
	mov	edi,characters
	mov	esi,symbol_characters+1
	movzx	ecx,byte [esi-1]
	xor	eax,eax
      mark_symbol_characters:
	lodsb
	mov	byte [edi+eax],0
	loop	mark_symbol_characters
	mov	edi,locals_counter
	mov	al,7
	stos	byte [edi]
	movzx	ecx,al
	mov	al,'0'
	rep	stos byte [edi]
	mov	edi,[memory_start]
	mov	[include_paths],edi
	mov	esi,include_variable
	call	get_environment_variable
	xor	al,al
	stosb
	mov	[memory_start],edi
	mov	eax,[additional_memory]
	mov	[free_additional_memory],eax
	mov	eax,[additional_memory_end]
	mov	[labels_list],eax
	xor	eax,eax
	mov	[display_buffer],eax
	mov	[hash_tree],eax
	mov	[macro_status],al
	mov	esi,[input_file]
	mov	edx,esi
	call	open
	jc	main_file_not_found
	mov	edi,[memory_start]
	call	preprocess_file
	mov	eax,[error_line]
	mov	[current_line],eax
	cmp	[macro_status],0
	jne	incomplete_macro
	mov	[source_start],edi
	ret

preprocess_file:
	push	[memory_end]
	push	esi
	mov	al,2
	xor	edx,edx
	call	lseek
	push	eax
	xor	al,al
	xor	edx,edx
	call	lseek
	pop	ecx
	mov	edx,[memory_end]
	dec	edx
	mov	byte [edx],1Ah
	sub	edx,ecx
	jc	out_of_memory
	mov	esi,edx
	cmp	edx,edi
	jbe	out_of_memory
	mov	[memory_end],edx
	call	read
	call	close
	pop	edx
	xor	ecx,ecx
	mov	ebx,esi
      preprocess_source:
	inc	ecx
	mov	[current_line],edi
	mov	eax,edx
	stos	dword [edi]
	mov	eax,ecx
	stos	dword [edi]
	mov	eax,esi
	sub	eax,ebx
	stos	dword [edi]
	xor	eax,eax
	stos	dword [edi]
	push	ebx edx
	call	convert_line
	call	preprocess_line
	pop	edx ebx
      next_line:
	cmp	byte [esi-1],1Ah
	jne	preprocess_source
      file_end:
	pop	[memory_end]
	clc
	ret

convert_line:
	push	ecx
	test	[macro_status],0Fh
	jz	convert_line_data
	mov	ax,3Bh
	stos	word [edi]
      convert_line_data:
	cmp	edi,[memory_end]
	jae	out_of_memory
	lods	byte [esi]
	cmp	al,20h
	je	convert_line_data
	cmp	al,9
	je	convert_line_data
	mov	ah,al
	mov	ebx,characters
	xlat	byte [ebx]
	or	al,al
	jz	convert_separator
	cmp	ah,27h
	je	convert_string
	cmp	ah,22h
	je	convert_string
	mov	byte [edi],1Ah
	scas	word [edi]
	xchg	al,ah
	stos	byte [edi]
	mov	ebx,characters
	xor	ecx,ecx
      convert_symbol:
	lods	byte [esi]
	stos	byte [edi]
	xlat	byte [ebx]
	or	al,al
	loopnzd convert_symbol
	neg	ecx
	cmp	ecx,255
	ja	name_too_long
	mov	ebx,edi
	sub	ebx,ecx
	mov	byte [ebx-2],cl
      found_separator:
	dec	edi
	mov	ah,[esi-1]
      convert_separator:
	xchg	al,ah
	cmp	al,20h
	jb	control_character
	je	convert_line_data
      symbol_character:
	cmp	al,3Bh
	je	ignore_comment
	cmp	al,5Ch
	je	backslash_character
	stos	byte [edi]
	jmp	convert_line_data
      control_character:
	cmp	al,1Ah
	je	line_end
	cmp	al,0Dh
	je	cr_character
	cmp	al,0Ah
	je	lf_character
	cmp	al,9
	je	convert_line_data
	or	al,al
	jnz	symbol_character
	jmp	line_end
      lf_character:
	lods	byte [esi]
	cmp	al,0Dh
	je	line_end
	dec	esi
	jmp	line_end
      cr_character:
	lods	byte [esi]
	cmp	al,0Ah
	je	line_end
	dec	esi
	jmp	line_end
      convert_string:
	mov	al,22h
	stos	byte [edi]
	scas	dword [edi]
	mov	ebx,edi
      copy_string:
	lods	byte [esi]
	stos	byte [edi]
	cmp	al,0Ah
	je	missing_end_quote
	cmp	al,0Dh
	je	missing_end_quote
	or	al,al
	jz	missing_end_quote
	cmp	al,1Ah
	je	missing_end_quote
	cmp	al,ah
	jne	copy_string
	lods	byte [esi]
	cmp	al,ah
	je	copy_string
	dec	esi
	dec	edi
	mov	eax,edi
	sub	eax,ebx
	mov	[ebx-4],eax
	jmp	convert_line_data
      backslash_character:
	mov	byte [edi],0
	lods	byte [esi]
	cmp	al,20h
	je	concatenate_lines
	cmp	al,9
	je	concatenate_lines
	cmp	al,1Ah
	je	unexpected_end_of_file
	cmp	al,0Ah
	je	concatenate_lf
	cmp	al,0Dh
	je	concatenate_cr
	cmp	al,3Bh
	je	find_concatenated_line
	mov	al,1Ah
	stos	byte [edi]
	mov	ecx,edi
	mov	ax,5C01h
	stos	word [edi]
	dec	esi
      group_backslashes:
	lods	byte [esi]
	cmp	al,5Ch
	jne	backslashed_symbol
	stos	byte [edi]
	inc	byte [ecx]
	jmp	group_backslashes
      backslashed_symbol:
	cmp	al,1Ah
	je	unexpected_end_of_file
	cmp	al,0Ah
	je	extra_characters_on_line
	cmp	al,0Dh
	je	extra_characters_on_line
	cmp	al,20h
	je	extra_characters_on_line
	cmp	al,9
	je	extra_characters_on_line
	cmp	al,22h
	je	extra_characters_on_line
	cmp	al,27h
	je	extra_characters_on_line
	cmp	al,3Bh
	je	extra_characters_on_line
	mov	ah,al
	mov	ebx,characters
	xlat	byte [ebx]
	or	al,al
	jz	backslashed_symbol_character
	mov	al,ah
      convert_backslashed_symbol:
	stos	byte [edi]
	xlat	byte [ebx]
	or	al,al
	jz	found_separator
	inc	byte [ecx]
	jz	name_too_long
	lods	byte [esi]
	jmp	convert_backslashed_symbol
      backslashed_symbol_character:
	mov	al,ah
	stos	byte [edi]
	inc	byte [ecx]
	jmp	convert_line_data
      concatenate_lines:
	lods	byte [esi]
	cmp	al,20h
	je	concatenate_lines
	cmp	al,9
	je	concatenate_lines
	cmp	al,1Ah
	je	unexpected_end_of_file
	cmp	al,0Ah
	je	concatenate_lf
	cmp	al,0Dh
	je	concatenate_cr
	cmp	al,3Bh
	jne	extra_characters_on_line
      find_concatenated_line:
	lods	byte [esi]
	cmp	al,0Ah
	je	concatenate_lf
	cmp	al,0Dh
	je	concatenate_cr
	or	al,al
	jz	concatenate_ok
	cmp	al,1Ah
	jne	find_concatenated_line
	jmp	unexpected_end_of_file
      concatenate_lf:
	lods	byte [esi]
	cmp	al,0Dh
	je	concatenate_ok
	dec	esi
	jmp	concatenate_ok
      concatenate_cr:
	lods	byte [esi]
	cmp	al,0Ah
	je	concatenate_ok
	dec	esi
      concatenate_ok:
	inc	dword [esp]
	jmp	convert_line_data
      ignore_comment:
	lods	byte [esi]
	cmp	al,0Ah
	je	lf_character
	cmp	al,0Dh
	je	cr_character
	or	al,al
	jz	line_end
	cmp	al,1Ah
	jne	ignore_comment
      line_end:
	xor	al,al
	stos	byte [edi]
	pop	ecx
	ret

lower_case:
	mov	edi,converted
	mov	ebx,characters
      convert_case:
	lods	byte [esi]
	xlat	byte [ebx]
	stos	byte [edi]
	loop	convert_case
      case_ok:
	ret

get_directive:
	push	edi
	mov	edx,esi
	mov	ebp,ecx
	call	lower_case
	pop	edi
      scan_directives:
	mov	esi,converted
	movzx	eax,byte [edi]
	or	al,al
	jz	no_directive
	mov	ecx,ebp
	inc	edi
	mov	ebx,edi
	add	ebx,eax
	mov	ah,[esi]
	cmp	ah,[edi]
	jb	no_directive
	ja	next_directive
	cmp	cl,al
	jne	next_directive
	repe	cmps byte [esi],[edi]
	jb	no_directive
	je	directive_ok
      next_directive:
	mov	edi,ebx
	add	edi,2
	jmp	scan_directives
      no_directive:
	mov	esi,edx
	mov	ecx,ebp
	stc
	ret
      directive_ok:
	lea	esi,[edx+ebp]
	movzx	eax,word [ebx]
	add	eax,preprocessor
	clc
	ret


preprocess_line:
	mov	eax,esp
	sub	eax,100h
	jc	stack_overflow
	cmp	eax,[stack_limit]
	jb	stack_overflow
	push	ecx esi
      preprocess_current_line:
	mov	esi,[current_line]
	add	esi,16
	cmp	word [esi],3Bh
	jne	line_start_ok
	add	esi,2
      line_start_ok:
	test	[macro_status],0F0h
	jnz	macro_preprocessing
	cmp	byte [esi],1Ah
	jne	not_fix_constant
	movzx	edx,byte [esi+1]
	lea	edx,[esi+2+edx]
	cmp	word [edx],031Ah
	jne	not_fix_constant
	mov	ebx,characters
	movzx	eax,byte [edx+2]
	xlat	byte [ebx]
	ror	eax,8
	mov	al,[edx+3]
	xlat	byte [ebx]
	ror	eax,8
	mov	al,[edx+4]
	xlat	byte [ebx]
	ror	eax,16
	cmp	eax,'fix'
	je	define_fix_constant
      not_fix_constant:
	call	process_fix_constants
	jmp	initial_preprocessing_ok
      macro_preprocessing:
	call	process_macro_operators
      initial_preprocessing_ok:
	mov	esi,[current_line]
	add	esi,16
	mov	al,[macro_status]
	test	al,2
	jnz	skip_macro_block
	test	al,1
	jnz	find_macro_block
      preprocess_instruction:
	mov	[current_offset],esi
	lods	byte [esi]
	movzx	ecx,byte [esi]
	inc	esi
	cmp	al,1Ah
	jne	not_preprocessor_symbol
	cmp	cl,3
	jb	not_preprocessor_directive
	push	edi
	mov	edi,preprocessor_directives
	call	get_directive
	pop	edi
	jc	not_preprocessor_directive
	mov	byte [edx-2],3Bh
	jmp	near eax
      not_preprocessor_directive:
	xor	ch,ch
	call	get_preprocessor_symbol
	jc	not_macro
	mov	byte [ebx-2],3Bh
	mov	[struc_name],0
	jmp	use_macro
      not_macro:
	mov	[struc_name],esi
	add	esi,ecx
	lods	byte [esi]
	cmp	al,':'
	je	preprocess_label
	cmp	al,1Ah
	jne	not_preprocessor_symbol
	lods	byte [esi]
	cmp	al,3
	jne	not_symbolic_constant
	mov	ebx,characters
	movzx	eax,byte [esi]
	xlat	byte [ebx]
	ror	eax,8
	mov	al,[esi+1]
	xlat	byte [ebx]
	ror	eax,8
	mov	al,[esi+2]
	xlat	byte [ebx]
	ror	eax,16
	cmp	eax,'equ'
	je	define_equ_constant
	mov	al,3
      not_symbolic_constant:
	mov	ch,1
	mov	cl,al
	call	get_preprocessor_symbol
	jc	not_preprocessor_symbol
	push	edx esi
	mov	esi,[struc_name]
	mov	[struc_label],esi
	sub	[struc_label],2
	mov	cl,[esi-1]
	mov	ch,10b
	call	get_preprocessor_symbol
	jc	struc_name_ok
	mov	ecx,[edx+12]
	add	ecx,3
	lea	ebx,[edi+ecx]
	mov	ecx,edi
	sub	ecx,[struc_label]
	lea	esi,[edi-1]
	lea	edi,[ebx-1]
	std
	rep	movs byte [edi],[esi]
	cld
	mov	edi,[struc_label]
	mov	esi,[edx+8]
	mov	ecx,[edx+12]
	add	[struc_name],ecx
	add	[struc_name],3
	call	move_data
	mov	al,3Ah
	stos	byte [edi]
	mov	ax,3Bh
	stos	word [edi]
	mov	edi,ebx
	pop	esi
	add	esi,[edx+12]
	add	esi,3
	pop	edx
	jmp	use_macro
      struc_name_ok:
	mov	edx,[struc_name]
	movzx	eax,byte [edx-1]
	add	edx,eax
	mov	al,3Ah
	mov	[edx],al
	inc	al
	xchg	al,[edx+1]
	dec	al
	mov	[edx+2],al
	pop	esi edx
	jmp	use_macro
      preprocess_label:
	dec	esi
	sub	esi,ecx
	lea	ebp,[esi-2]
	mov	ch,10b
	call	get_preprocessor_symbol
	jnc	symbolic_constant_in_label
	lea	esi,[esi+ecx+1]
	jmp	preprocess_instruction
      symbolic_constant_in_label:
	mov	ebx,[edx+8]
	mov	ecx,[edx+12]
	add	ecx,ebx
      check_for_broken_label:
	cmp	ebx,ecx
	je	label_broken
	cmp	byte [ebx],1Ah
	jne	label_broken
	movzx	eax,byte [ebx+1]
	lea	ebx,[ebx+2+eax]
	cmp	ebx,ecx
	je	label_constant_ok
	cmp	byte [ebx],':'
	jne	label_broken
	inc	ebx
	jmp	check_for_broken_label
      label_broken:
	push	line_preprocessed
	jmp	replace_symbolic_constant
      label_constant_ok:
	mov	ecx,edi
	sub	ecx,esi
	mov	edi,[edx+12]
	add	edi,ebp
	push	edi
	lea	eax,[edi+ecx]
	push	eax
	cmp	esi,edi
	je	replace_label
	jb	move_rest_of_line_up
	rep	movs byte [edi],[esi]
	jmp	replace_label
      move_rest_of_line_up:
	lea	esi,[esi+ecx-1]
	lea	edi,[edi+ecx-1]
	std
	rep	movs byte [edi],[esi]
	cld
      replace_label:
	mov	ecx,[edx+12]
	mov	edi,[esp+4]
	sub	edi,ecx
	mov	esi,[edx+8]
	rep	movs byte [edi],[esi]
	pop	edi esi
	inc	esi
	jmp	preprocess_instruction
      not_preprocessor_symbol:
	mov	esi,[current_offset]
	call	process_equ_constants
      line_preprocessed:
	pop	esi ecx
	ret

get_preprocessor_symbol:
	push	ebp edi esi
	mov	ebp,ecx
	shl	ebp,22
	movzx	ecx,cl
	mov	ebx,hash_tree
	mov	edi,10
      follow_hashes_roots:
	mov	edx,[ebx]
	or	edx,edx
	jz	preprocessor_symbol_not_found
	xor	eax,eax
	shl	ebp,1
	adc	eax,0
	lea	ebx,[edx+eax*4]
	dec	edi
	jnz	follow_hashes_roots
	mov	edi,ebx
	call	calculate_hash
	mov	ebp,eax
	and	ebp,3FFh
	shl	ebp,10
	xor	ebp,eax
	mov	ebx,edi
	mov	edi,22
      follow_hashes_tree:
	mov	edx,[ebx]
	or	edx,edx
	jz	preprocessor_symbol_not_found
	xor	eax,eax
	shl	ebp,1
	adc	eax,0
	lea	ebx,[edx+eax*4]
	dec	edi
	jnz	follow_hashes_tree
	mov	al,cl
	mov	edx,[ebx]
	or	edx,edx
	jz	preprocessor_symbol_not_found
      compare_with_preprocessor_symbol:
	mov	edi,[edx+4]
	cmp	edi,1
	jbe	next_equal_hash
	repe	cmps byte [esi],[edi]
	je	preprocessor_symbol_found
	mov	cl,al
	mov	esi,[esp]
      next_equal_hash:
	mov	edx,[edx]
	or	edx,edx
	jnz	compare_with_preprocessor_symbol
      preprocessor_symbol_not_found:
	pop	esi edi ebp
	stc
	ret
      preprocessor_symbol_found:
	pop	ebx edi ebp
	clc
	ret
      calculate_hash:
	xor	ebx,ebx
	mov	eax,2166136261
	mov	ebp,16777619
      fnv1a_hash:
	xor	al,[esi+ebx]
	mul	ebp
	inc	bl
	cmp	bl,cl
	jb	fnv1a_hash
	ret
add_preprocessor_symbol:
	push	edi esi
	call	calculate_hash
	mov	ebp,eax
	and	ebp,3FFh
	shr	eax,10
	xor	ebp,eax
	shl	ecx,22
	or	ebp,ecx
	mov	ebx,hash_tree
	mov	ecx,32
      find_leave_for_symbol:
	mov	edx,[ebx]
	or	edx,edx
	jz	extend_hashes_tree
	xor	eax,eax
	rol	ebp,1
	adc	eax,0
	lea	ebx,[edx+eax*4]
	dec	ecx
	jnz	find_leave_for_symbol
	mov	edx,[ebx]
	or	edx,edx
	jz	add_symbol_entry
	shr	ebp,30
	cmp	ebp,11b
	je	reuse_symbol_entry
	cmp	dword [edx+4],0
	jne	add_symbol_entry
      find_entry_to_reuse:
	mov	edi,[edx]
	or	edi,edi
	jz	reuse_symbol_entry
	cmp	dword [edi+4],0
	jne	reuse_symbol_entry
	mov	edx,edi
	jmp	find_entry_to_reuse
      add_symbol_entry:
	mov	eax,edx
	mov	edx,[labels_list]
	sub	edx,16
	cmp	edx,[free_additional_memory]
	jb	out_of_memory
	mov	[labels_list],edx
	mov	[edx],eax
	mov	[ebx],edx
      reuse_symbol_entry:
	pop	esi edi
	mov	[edx+4],esi
	ret
      extend_hashes_tree:
	mov	edx,[labels_list]
	sub	edx,8
	cmp	edx,[free_additional_memory]
	jb	out_of_memory
	mov	[labels_list],edx
	xor	eax,eax
	mov	[edx],eax
	mov	[edx+4],eax
	shl	ebp,1
	adc	eax,0
	mov	[ebx],edx
	lea	ebx,[edx+eax*4]
	dec	ecx
	jnz	extend_hashes_tree
	mov	edx,[labels_list]
	sub	edx,16
	cmp	edx,[free_additional_memory]
	jb	out_of_memory
	mov	[labels_list],edx
	mov	dword [edx],0
	mov	[ebx],edx
	pop	esi edi
	mov	[edx+4],esi
	ret

define_fix_constant:
	add	edx,5
	add	esi,2
	push	edx esi
	mov	esi,edx
	call	skip_parameters
	xchg	esi,[esp]
	mov	ch,11b
	jmp	define_preprocessor_constant
define_equ_constant:
	add	esi,3
	push	esi
	call	process_equ_constants
	push	esi
	mov	esi,[struc_name]
	mov	ch,10b
      define_preprocessor_constant:
	mov	byte [esi-2],3Bh
	mov	cl,[esi-1]
	call	add_preprocessor_symbol
	pop	esi ebx
	mov	ecx,edi
	dec	ecx
	sub	ecx,ebx
	mov	[edx+8],ebx
	mov	[edx+12],ecx
	jmp	line_preprocessed
define_symbolic_constant:
	lods	byte [esi]
	cmp	al,1Ah
	jne	invalid_name
	lods	byte [esi]
	mov	cl,al
	mov	ch,10b
	call	add_preprocessor_symbol
	movzx	eax,byte [esi-1]
	add	esi,eax
	push	esi edx
	call	skip_parameters
	pop	edx ebx
	lea	ecx,[esi-1]
	sub	ecx,ebx
	mov	[edx+8],ebx
	mov	[edx+12],ecx
	jmp	line_preprocessed
      skip_parameters:
	lods	byte [esi]
	or	al,al
	jz	parameters_skipped
	cmp	al,'{'
	je	parameters_skipped
	cmp	al,22h
	je	skip_quoted_parameter
	cmp	al,1Ah
	jne	skip_parameters
	lods	byte [esi]
	movzx	eax,al
	add	esi,eax
	jmp	skip_parameters
      skip_quoted_parameter:
	lods	dword [esi]
	add	esi,eax
	jmp	skip_parameters
      parameters_skipped:
	ret

define_struc:
	mov	ch,1
	jmp	make_macro
define_macro:
	xor	ch,ch
      make_macro:
	lods	byte [esi]
	cmp	al,1Ah
	jne	invalid_name
	lods	byte [esi]
	mov	cl,al
	call	add_preprocessor_symbol
	mov	eax,[current_line]
	mov	[edx+12],eax
	movzx	eax,byte [esi-1]
	add	esi,eax
	mov	[edx+8],esi
	mov	al,[macro_status]
	and	al,0F0h
	or	al,1
	mov	[macro_status],al
	mov	eax,[current_line]
	mov	[error_line],eax
	xor	bl,bl
	lods	byte [esi]
	or	al,al
	jz	line_preprocessed
	cmp	al,'{'
	je	found_macro_block
	dec	esi
      skip_macro_arguments:
	lods	byte [esi]
	cmp	al,1Ah
	je	skip_macro_argument
	cmp	al,'['
	jne	invalid_macro_arguments
	xor	bl,-1
	jz	invalid_macro_arguments
	lods	byte [esi]
	cmp	al,1Ah
	jne	invalid_macro_arguments
      skip_macro_argument:
	movzx	eax,byte [esi]
	inc	esi
	add	esi,eax
	lods	byte [esi]
	cmp	al,'*'
	jne	macro_argument_end
	lods	byte [esi]
      macro_argument_end:
	cmp	al,','
	je	skip_macro_arguments
	cmp	al,']'
	jne	end_macro_arguments
	lods	byte [esi]
	not	bl
      end_macro_arguments:
	or	bl,bl
	jnz	invalid_macro_arguments
	or	al,al
	jz	line_preprocessed
	cmp	al,'{'
	je	found_macro_block
	jmp	invalid_macro_arguments
      find_macro_block:
	add	esi,2
	lods	byte [esi]
	or	al,al
	jz	line_preprocessed
	cmp	al,'{'
	jne	unexpected_characters
      found_macro_block:
	or	[macro_status],2
      skip_macro_block:
	lods	byte [esi]
	cmp	al,1Ah
	je	skip_macro_symbol
	cmp	al,3Bh
	je	skip_macro_symbol
	cmp	al,22h
	je	skip_macro_string
	or	al,al
	jz	line_preprocessed
	cmp	al,'}'
	jne	skip_macro_block
	mov	al,[macro_status]
	and	[macro_status],0F0h
	test	al,8
	jnz	use_instant_macro
	cmp	byte [esi],0
	je	line_preprocessed
	mov	ecx,edi
	sub	ecx,esi
	mov	edx,esi
	lea	esi,[esi+ecx-1]
	lea	edi,[edi+1+16]
	mov	ebx,edi
	dec	edi
	std
	rep	movs byte [edi],[esi]
	cld
	mov	edi,edx
	xor	al,al
	stos	byte [edi]
	mov	esi,[current_line]
	mov	[current_line],edi
	mov	ecx,4
	rep	movs dword [edi],[esi]
	mov	edi,ebx
	jmp	initial_preprocessing_ok
      skip_macro_symbol:
	movzx	eax,byte [esi]
	inc	esi
	add	esi,eax
	jmp	skip_macro_block
      skip_macro_string:
	lods	dword [esi]
	add	esi,eax
	jmp	skip_macro_block
rept_directive:
	mov	[base_code],0
	jmp	define_instant_macro
irp_directive:
	mov	[base_code],1
	jmp	define_instant_macro
irps_directive:
	mov	[base_code],2
	jmp	define_instant_macro
match_directive:
	mov	[base_code],10h
define_instant_macro:
	mov	al,[macro_status]
	and	al,0F0h
	or	al,8+1
	mov	[macro_status],al
	mov	eax,[current_line]
	mov	[error_line],eax
	mov	[instant_macro_start],esi
	cmp	[base_code],10h
	je	prepare_match
	call	skip_parameters
      instant_macro_parameters_skipped:
	dec	esi
	mov	[parameters_end],esi
	lods	byte [esi]
	cmp	al,'{'
	je	found_macro_block
	or	al,al
	jnz	invalid_macro_arguments
	jmp	line_preprocessed
prepare_match:
	call	skip_pattern
	mov	[value_type],80h+10b
	call	process_symbolic_constants
	jmp	instant_macro_parameters_skipped
      skip_pattern:
	lods	byte [esi]
	or	al,al
	jz	invalid_macro_arguments
	cmp	al,','
	je	pattern_skipped
	cmp	al,22h
	je	skip_quoted_string_in_pattern
	cmp	al,1Ah
	je	skip_symbol_in_pattern
	cmp	al,'='
	jne	skip_pattern
	mov	al,[esi]
	cmp	al,1Ah
	je	skip_pattern
	cmp	al,22h
	je	skip_pattern
	inc	esi
	jmp	skip_pattern
      skip_symbol_in_pattern:
	lods	byte [esi]
	movzx	eax,al
	add	esi,eax
	jmp	skip_pattern
      skip_quoted_string_in_pattern:
	lods	dword [esi]
	add	esi,eax
	jmp	skip_pattern
      pattern_skipped:
	ret

purge_macro:
	xor	ch,ch
	jmp	restore_preprocessor_symbol
purge_struc:
	mov	ch,1
	jmp	restore_preprocessor_symbol
restore_equ_constant:
	mov	ch,10b
      restore_preprocessor_symbol:
	push	ecx
	lods	byte [esi]
	cmp	al,1Ah
	jne	invalid_name
	lods	byte [esi]
	mov	cl,al
	call	get_preprocessor_symbol
	jc	no_symbol_to_restore
	mov	dword [edx+4],0
	jmp	symbol_restored
      no_symbol_to_restore:
	add	esi,ecx
      symbol_restored:
	pop	ecx
	lods	byte [esi]
	cmp	al,','
	je	restore_preprocessor_symbol
	or	al,al
	jnz	extra_characters_on_line
	jmp	line_preprocessed

process_fix_constants:
	mov	[value_type],11b
	jmp	process_symbolic_constants
process_equ_constants:
	mov	[value_type],10b
      process_symbolic_constants:
	mov	ebp,esi
	lods	byte [esi]
	cmp	al,1Ah
	je	check_symbol
	cmp	al,22h
	je	ignore_string
	cmp	al,'{'
	je	check_brace
	or	al,al
	jnz	process_symbolic_constants
	ret
      ignore_string:
	lods	dword [esi]
	add	esi,eax
	jmp	process_symbolic_constants
      check_brace:
	test	[value_type],80h
	jz	process_symbolic_constants
	ret
      no_replacing:
	movzx	ecx,byte [esi-1]
	add	esi,ecx
	jmp	process_symbolic_constants
      check_symbol:
	mov	cl,[esi]
	inc	esi
	mov	ch,[value_type]
	call	get_preprocessor_symbol
	jc	no_replacing
	mov	[current_section],edi
      replace_symbolic_constant:
	mov	ecx,[edx+12]
	mov	edx,[edx+8]
	xchg	esi,edx
	call	move_data
	mov	esi,edx
      process_after_replaced:
	lods	byte [esi]
	cmp	al,1Ah
	je	symbol_after_replaced
	stos	byte [edi]
	cmp	al,22h
	je	string_after_replaced
	cmp	al,'{'
	je	brace_after_replaced
	or	al,al
	jnz	process_after_replaced
	mov	ecx,edi
	sub	ecx,esi
	mov	edi,ebp
	call	move_data
	mov	esi,edi
	ret
      move_data:
	lea	eax,[edi+ecx]
	cmp	eax,[memory_end]
	jae	out_of_memory
	shr	ecx,1
	jnc	movsb_ok
	movs	byte [edi],[esi]
      movsb_ok:
	shr	ecx,1
	jnc	movsw_ok
	movs	word [edi],[esi]
      movsw_ok:
	rep	movs dword [edi],[esi]
	ret
      string_after_replaced:
	lods	dword [esi]
	stos	dword [edi]
	mov	ecx,eax
	call	move_data
	jmp	process_after_replaced
      brace_after_replaced:
	test	[value_type],80h
	jz	process_after_replaced
	mov	edx,edi
	mov	ecx,[current_section]
	sub	edx,ecx
	sub	ecx,esi
	rep	movs byte [edi],[esi]
	mov	ecx,edi
	sub	ecx,esi
	mov	edi,ebp
	call	move_data
	lea	esi,[ebp+edx]
	ret
      symbol_after_replaced:
	mov	cl,[esi]
	inc	esi
	mov	ch,[value_type]
	call	get_preprocessor_symbol
	jnc	replace_symbolic_constant
	movzx	ecx,byte [esi-1]
	mov	al,1Ah
	mov	ah,cl
	stos	word [edi]
	call	move_data
	jmp	process_after_replaced
process_macro_operators:
	xor	dl,dl
	mov	ebp,edi
      before_macro_operators:
	mov	edi,esi
	lods	byte [esi]
	cmp	al,'`'
	je	symbol_conversion
	cmp	al,'#'
	je	concatenation
	cmp	al,1Ah
	je	symbol_before_macro_operators
	cmp	al,3Bh
	je	no_more_macro_operators
	cmp	al,22h
	je	string_before_macro_operators
	xor	dl,dl
	or	al,al
	jnz	before_macro_operators
	mov	edi,esi
	ret
      no_more_macro_operators:
	mov	edi,ebp
	ret
      symbol_before_macro_operators:
	mov	dl,1Ah
	mov	ebx,esi
	lods	byte [esi]
	movzx	ecx,al
	jecxz	symbol_before_macro_operators_ok
	mov	edi,esi
	cmp	byte [esi],'\'
	je	escaped_symbol
      symbol_before_macro_operators_ok:
	add	esi,ecx
	jmp	before_macro_operators
      string_before_macro_operators:
	mov	dl,22h
	mov	ebx,esi
	lods	dword [esi]
	add	esi,eax
	jmp	before_macro_operators
      escaped_symbol:
	dec	byte [edi-1]
	dec	ecx
	inc	esi
	cmp	ecx,1
	rep	movs byte [edi],[esi]
	jne	after_macro_operators
	mov	al,[esi-1]
	mov	ecx,ebx
	mov	ebx,characters
	xlat	byte [ebx]
	mov	ebx,ecx
	or	al,al
	jnz	after_macro_operators
	sub	edi,3
	mov	al,[esi-1]
	stos	byte [edi]
	xor	dl,dl
	jmp	after_macro_operators
      reduce_symbol_conversion:
	inc	esi
      symbol_conversion:
	mov	edx,esi
	mov	al,[esi]
	cmp	al,1Ah
	jne	symbol_character_conversion
	lods	word [esi]
	movzx	ecx,ah
	lea	ebx,[edi+3]
	jecxz	convert_to_quoted_string
	cmp	byte [esi],'\'
	jne	convert_to_quoted_string
	inc	esi
	dec	ecx
	dec	ebx
	jmp	convert_to_quoted_string
      symbol_character_conversion:
	cmp	al,22h
	je	after_macro_operators
	cmp	al,'`'
	je	reduce_symbol_conversion
	lea	ebx,[edi+5]
	xor	ecx,ecx
	or	al,al
	jz	convert_to_quoted_string
	cmp	al,'#'
	je	convert_to_quoted_string
	inc	ecx
      convert_to_quoted_string:
	sub	ebx,edx
	ja	shift_line_data
	mov	al,22h
	mov	dl,al
	stos	byte [edi]
	mov	ebx,edi
	mov	eax,ecx
	stos	dword [edi]
	rep	movs byte [edi],[esi]
	cmp	edi,esi
	je	before_macro_operators
	jmp	after_macro_operators
      shift_line_data:
	push	ecx
	mov	edx,esi
	lea	esi,[ebp-1]
	add	ebp,ebx
	lea	edi,[ebp-1]
	lea	ecx,[esi+1]
	sub	ecx,edx
	std
	rep	movs byte [edi],[esi]
	cld
	pop	eax
	sub	edi,3
	mov	dl,22h
	mov	[edi-1],dl
	mov	ebx,edi
	mov	[edi],eax
	lea	esi,[edi+4+eax]
	jmp	before_macro_operators
      concatenation:
	cmp	dl,1Ah
	je	symbol_concatenation
	cmp	dl,22h
	je	string_concatenation
      no_concatenation:
	cmp	esi,edi
	je	before_macro_operators
	jmp	after_macro_operators
      symbol_concatenation:
	cmp	byte [esi],1Ah
	jne	no_concatenation
	inc	esi
	lods	byte [esi]
	movzx	ecx,al
	jecxz	do_symbol_concatenation
	cmp	byte [esi],'\'
	je	concatenate_escaped_symbol
      do_symbol_concatenation:
	add	[ebx],cl
	jc	name_too_long
	rep	movs byte [edi],[esi]
	jmp	after_macro_operators
      concatenate_escaped_symbol:
	inc	esi
	dec	ecx
	jz	do_symbol_concatenation
	movzx	eax,byte [esi]
	cmp	byte [characters+eax],0
	jne	do_symbol_concatenation
	sub	esi,3
	jmp	no_concatenation
      string_concatenation:
	cmp	byte [esi],22h
	je	do_string_concatenation
	cmp	byte [esi],'`'
	jne	no_concatenation
      concatenate_converted_symbol:
	inc	esi
	mov	al,[esi]
	cmp	al,'`'
	je	concatenate_converted_symbol
	cmp	al,22h
	je	do_string_concatenation
	cmp	al,1Ah
	jne	concatenate_converted_symbol_character
	inc	esi
	lods	byte [esi]
	movzx	ecx,al
	jecxz	finish_concatenating_converted_symbol
	cmp	byte [esi],'\'
	jne	finish_concatenating_converted_symbol
	inc	esi
	dec	ecx
      finish_concatenating_converted_symbol:
	add	[ebx],ecx
	rep	movs byte [edi],[esi]
	jmp	after_macro_operators
      concatenate_converted_symbol_character:
	or	al,al
	jz	after_macro_operators
	cmp	al,'#'
	je	after_macro_operators
	inc	dword [ebx]
	movs	byte [edi],[esi]
	jmp	after_macro_operators
      do_string_concatenation:
	inc	esi
	lods	dword [esi]
	mov	ecx,eax
	add	[ebx],eax
	rep	movs byte [edi],[esi]
      after_macro_operators:
	lods	byte [esi]
	cmp	al,'`'
	je	symbol_conversion
	cmp	al,'#'
	je	concatenation
	stos	byte [edi]
	cmp	al,1Ah
	je	symbol_after_macro_operators
	cmp	al,3Bh
	je	no_more_macro_operators
	cmp	al,22h
	je	string_after_macro_operators
	xor	dl,dl
	or	al,al
	jnz	after_macro_operators
	ret
      symbol_after_macro_operators:
	mov	dl,1Ah
	mov	ebx,edi
	lods	byte [esi]
	stos	byte [edi]
	movzx	ecx,al
	jecxz	symbol_after_macro_operatorss_ok
	cmp	byte [esi],'\'
	je	escaped_symbol
      symbol_after_macro_operatorss_ok:
	rep	movs byte [edi],[esi]
	jmp	after_macro_operators
      string_after_macro_operators:
	mov	dl,22h
	mov	ebx,edi
	lods	dword [esi]
	stos	dword [edi]
	mov	ecx,eax
	rep	movs byte [edi],[esi]
	jmp	after_macro_operators

use_macro:
	push	[free_additional_memory]
	push	[macro_symbols]
	mov	[macro_symbols],0
	push	[counter_limit]
	push	dword [edx+4]
	mov	dword [edx+4],1
	push	edx
	mov	ebx,esi
	mov	esi,[edx+8]
	mov	eax,[edx+12]
	mov	[macro_line],eax
	mov	[counter_limit],0
      process_macro_arguments:
	mov	al,[esi]
	or	al,al
	jz	arguments_end
	cmp	al,'{'
	je	arguments_end
	inc	esi
	cmp	al,'['
	jne	get_macro_arguments
	mov	ebp,esi
	inc	esi
	inc	[counter_limit]
      get_macro_arguments:
	call	get_macro_argument
	lods	byte [esi]
	cmp	al,','
	je	next_argument
	cmp	al,']'
	je	next_arguments_group
	dec	esi
	jmp	arguments_end
      next_argument:
	cmp	byte [ebx],','
	jne	process_macro_arguments
	inc	ebx
	jmp	process_macro_arguments
      next_arguments_group:
	cmp	byte [ebx],','
	jne	arguments_end
	inc	ebx
	inc	[counter_limit]
	mov	esi,ebp
	jmp	process_macro_arguments
      get_macro_argument:
	lods	byte [esi]
	movzx	ecx,al
	mov	eax,[counter_limit]
	call	add_macro_symbol
	add	esi,ecx
	xchg	esi,ebx
	mov	[edx+12],esi
	cmp	byte [esi],'<'
	jne	simple_argument
	inc	esi
	mov	[edx+12],esi
	mov	ecx,1
      enclosed_argument:
	lods	byte [esi]
	or	al,al
	jz	invalid_macro_arguments
	cmp	al,1Ah
	je	enclosed_symbol
	cmp	al,22h
	je	enclosed_string
	cmp	al,'>'
	je	enclosed_argument_end
	cmp	al,'<'
	jne	enclosed_argument
	inc	ecx
	jmp	enclosed_argument
      enclosed_symbol:
	movzx	eax,byte [esi]
	inc	esi
	add	esi,eax
	jmp	enclosed_argument
      enclosed_string:
	lods	dword [esi]
	add	esi,eax
	jmp	enclosed_argument
      enclosed_argument_end:
	loop	enclosed_argument
	mov	al,[esi]
	or	al,al
	jz	enclosed_argument_ok
	cmp	al,','
	jne	invalid_macro_arguments
      enclosed_argument_ok:
	mov	eax,esi
	sub	eax,[edx+12]
	dec	eax
	or	eax,80000000h
	mov	[edx+8],eax
	jmp	argument_value_ok
      simple_argument:
	lods	byte [esi]
	or	al,al
	jz	argument_value_end
	cmp	al,','
	je	argument_value_end
	cmp	al,22h
	je	argument_string
	cmp	al,1Ah
	jne	simple_argument
	movzx	eax,byte [esi]
	inc	esi
	add	esi,eax
	jmp	simple_argument
      argument_string:
	lods	dword [esi]
	add	esi,eax
	jmp	simple_argument
      argument_value_end:
	dec	esi
	mov	eax,esi
	sub	eax,[edx+12]
	mov	[edx+8],eax
      argument_value_ok:
	xchg	esi,ebx
	cmp	byte [esi],'*'
	jne	macro_argument_ok
	cmp	dword [edx+8],0
	je	invalid_macro_arguments
	inc	esi
      macro_argument_ok:
	ret
      arguments_end:
	cmp	byte [ebx],0
	jne	invalid_macro_arguments
	mov	eax,[esp+4]
	dec	eax
	call	process_macro
	pop	edx
	pop	dword [edx+4]
	pop	[counter_limit]
	pop	[macro_symbols]
	pop	[free_additional_memory]
	jmp	line_preprocessed
use_instant_macro:
	push	edi [current_line] esi
	mov	eax,[error_line]
	mov	[current_line],eax
	mov	[macro_line],eax
	mov	esi,[instant_macro_start]
	cmp	[base_code],10h
	jae	do_match
	cmp	[base_code],0
	jne	do_irp
	call	get_number
	jc	invalid_value
	or	ebp,ebp
	jnz	invalid_value
	cmp	dword [edi+4],0
	jne	value_out_of_range
	mov	eax,[edi]
	or	eax,eax
	jz	instant_macro_done
	cmp	eax,80000000h
	jae	value_out_of_range
	push	[free_additional_memory]
	push	[macro_symbols]
	mov	[macro_symbols],0
	push	[counter_limit]
	mov	[struc_name],0
	mov	[counter_limit],eax
	lods	byte [esi]
	or	al,al
	jz	rept_counters_ok
	cmp	al,'{'
	je	rept_counters_ok
	cmp	al,1Ah
	jne	invalid_macro_arguments
      add_rept_counter:
	lods	byte [esi]
	movzx	ecx,al
	xor	eax,eax
	call	add_macro_symbol
	add	esi,ecx
	xor	eax,eax
	mov	dword [edx+12],eax
	inc	eax
	mov	dword [edx+8],eax
	lods	byte [esi]
	cmp	al,':'
	jne	rept_counter_added
	push	edx
	call	get_number
	jc	invalid_value
	or	ebp,ebp
	jnz	invalid_value
	cmp	dword [edi+4],0
	jne	value_out_of_range
	mov	eax,[edi]
	mov	edx,eax
	add	edx,[counter_limit]
	jc	value_out_of_range
	pop	edx
	mov	dword [edx+8],eax
	lods	byte [esi]
      rept_counter_added:
	cmp	al,','
	jne	rept_counters_ok
	lods	byte [esi]
	cmp	al,1Ah
	jne	invalid_macro_arguments
	jmp	add_rept_counter
      rept_counters_ok:
	dec	esi
      instant_macro_parameters_ok:
	xor	eax,eax
	call	process_macro
	pop	[counter_limit]
	pop	[macro_symbols]
	pop	[free_additional_memory]
      instant_macro_done:
	pop	ebx esi edx
	cmp	byte [ebx],0
	je	line_preprocessed
	mov	[current_line],edi
	mov	ecx,4
	rep	movs dword [edi],[esi]
	test	[macro_status],0Fh
	jz	instant_macro_attached_line
	mov	ax,3Bh
	stos	word [edi]
      instant_macro_attached_line:
	mov	esi,ebx
	sub	edx,ebx
	mov	ecx,edx
	call	move_data
	jmp	initial_preprocessing_ok
do_irp:
	cmp	byte [esi],1Ah
	jne	invalid_macro_arguments
	movzx	eax,byte [esi+1]
	lea	esi,[esi+2+eax]
	lods	byte [esi]
	cmp	[base_code],1
	ja	irps_name_ok
	cmp	al,'*'
	jne	irp_name_ok
	lods	byte [esi]
      irp_name_ok:
	cmp	al,','
	jne	invalid_macro_arguments
	jmp	irp_parameters_start
      irps_name_ok:
	cmp	al,','
	jne	invalid_macro_arguments
	mov	al,[esi]
	or	al,al
	jz	instant_macro_done
	cmp	al,'{'
	je	instant_macro_done
      irp_parameters_start:
	xor	eax,eax
	push	[free_additional_memory]
	push	[macro_symbols]
	mov	[macro_symbols],eax
	push	[counter_limit]
	mov	[counter_limit],eax
	mov	[struc_name],eax
	mov	ebx,esi
	cmp	[base_code],1
	ja	get_irps_parameter
	mov	edx,[parameters_end]
	mov	al,[edx]
	push	eax
	mov	byte [edx],0
      get_irp_parameter:
	inc	[counter_limit]
	mov	esi,[instant_macro_start]
	inc	esi
	call	get_macro_argument
	cmp	byte [ebx],','
	jne	irp_parameters_end
	inc	ebx
	jmp	get_irp_parameter
      irp_parameters_end:
	mov	esi,ebx
	pop	eax
	mov	[esi],al
	jmp	instant_macro_parameters_ok
      get_irps_parameter:
	mov	esi,[instant_macro_start]
	inc	esi
	lods	byte [esi]
	movzx	ecx,al
	inc	[counter_limit]
	mov	eax,[counter_limit]
	call	add_macro_symbol
	mov	[edx+12],ebx
	cmp	byte [ebx],1Ah
	je	irps_symbol
	cmp	byte [ebx],22h
	je	irps_quoted_string
	mov	eax,1
	jmp	irps_parameter_ok
      irps_quoted_string:
	mov	eax,[ebx+1]
	add	eax,1+4
	jmp	irps_parameter_ok
      irps_symbol:
	movzx	eax,byte [ebx+1]
	add	eax,1+1
      irps_parameter_ok:
	mov	[edx+8],eax
	add	ebx,eax
	cmp	byte [ebx],0
	je	irps_parameters_end
	cmp	byte [ebx],'{'
	jne	get_irps_parameter
      irps_parameters_end:
	mov	esi,ebx
	jmp	instant_macro_parameters_ok
do_match:
	mov	ebx,esi
	call	skip_pattern
	call	exact_match
	mov	edx,edi
	mov	al,[ebx]
	cmp	al,1Ah
	je	free_match
	cmp	al,','
	jne	instant_macro_done
	cmp	esi,[parameters_end]
	je	matched_pattern
	jmp	instant_macro_done
      free_match:
	add	edx,12
	cmp	edx,[memory_end]
	ja	out_of_memory
	mov	[edx-12],ebx
	mov	[edx-8],esi
	call	skip_match_element
	jc	try_different_matching
	mov	[edx-4],esi
	movzx	eax,byte [ebx+1]
	lea	ebx,[ebx+2+eax]
	cmp	byte [ebx],1Ah
	je	free_match
      find_exact_match:
	call	exact_match
	cmp	esi,[parameters_end]
	je	end_matching
	cmp	byte [ebx],1Ah
	je	free_match
	mov	ebx,[edx-12]
	movzx	eax,byte [ebx+1]
	lea	ebx,[ebx+2+eax]
	mov	esi,[edx-4]
	jmp	match_more_elements
      try_different_matching:
	sub	edx,12
	cmp	edx,edi
	je	instant_macro_done
	mov	ebx,[edx-12]
	movzx	eax,byte [ebx+1]
	lea	ebx,[ebx+2+eax]
	cmp	byte [ebx],1Ah
	je	try_different_matching
	mov	esi,[edx-4]
      match_more_elements:
	call	skip_match_element
	jc	try_different_matching
	mov	[edx-4],esi
	jmp	find_exact_match
      skip_match_element:
	cmp	esi,[parameters_end]
	je	cannot_match
	mov	al,[esi]
	cmp	al,1Ah
	je	skip_match_symbol
	cmp	al,22h
	je	skip_match_quoted_string
	add	esi,1
	ret
      skip_match_quoted_string:
	mov	eax,[esi+1]
	add	esi,5
	jmp	skip_match_ok
      skip_match_symbol:
	movzx	eax,byte [esi+1]
	add	esi,2
      skip_match_ok:
	add	esi,eax
	ret
      cannot_match:
	stc
	ret
      exact_match:
	cmp	esi,[parameters_end]
	je	exact_match_complete
	mov	ah,[esi]
	mov	al,[ebx]
	cmp	al,','
	je	exact_match_complete
	cmp	al,1Ah
	je	exact_match_complete
	cmp	al,'='
	je	match_verbatim
	call	match_elements
	je	exact_match
      exact_match_complete:
	ret
      match_verbatim:
	inc	ebx
	call	match_elements
	je	exact_match
	dec	ebx
	ret
      match_elements:
	mov	al,[ebx]
	cmp	al,1Ah
	je	match_symbols
	cmp	al,22h
	je	match_quoted_strings
	cmp	al,ah
	je	symbol_characters_matched
	ret
      symbol_characters_matched:
	lea	ebx,[ebx+1]
	lea	esi,[esi+1]
	ret
      match_quoted_strings:
	mov	ecx,[ebx+1]
	add	ecx,5
	jmp	compare_elements
      match_symbols:
	movzx	ecx,byte [ebx+1]
	add	ecx,2
      compare_elements:
	mov	eax,esi
	mov	ebp,edi
	mov	edi,ebx
	repe	cmps byte [esi],[edi]
	jne	elements_mismatch
	mov	ebx,edi
	mov	edi,ebp
	ret
      elements_mismatch:
	mov	esi,eax
	mov	edi,ebp
	ret
      end_matching:
	cmp	byte [ebx],','
	jne	instant_macro_done
      matched_pattern:
	xor	eax,eax
	push	[free_additional_memory]
	push	[macro_symbols]
	mov	[macro_symbols],eax
	push	[counter_limit]
	mov	[counter_limit],eax
	mov	[struc_name],eax
	push	esi edi edx
      add_matched_symbol:
	cmp	edi,[esp]
	je	matched_symbols_ok
	mov	esi,[edi]
	inc	esi
	lods	byte [esi]
	movzx	ecx,al
	xor	eax,eax
	call	add_macro_symbol
	mov	eax,[edi+4]
	mov	dword [edx+12],eax
	mov	ecx,[edi+8]
	sub	ecx,eax
	mov	dword [edx+8],ecx
	add	edi,12
	jmp	add_matched_symbol
      matched_symbols_ok:
	pop	edx edi esi
	jmp	instant_macro_parameters_ok

process_macro:
	push	dword [macro_status]
	or	[macro_status],10h
	push	[counter]
	push	[macro_block]
	push	[macro_block_line]
	push	[macro_block_line_number]
	push	[struc_label]
	push	[struc_name]
	push	eax
	push	[current_line]
	lods	byte [esi]
	cmp	al,'{'
	je	macro_instructions_start
	or	al,al
	jnz	unexpected_characters
      find_macro_instructions:
	mov	[macro_line],esi
	add	esi,16+2
	lods	byte [esi]
	or	al,al
	jz	find_macro_instructions
	cmp	al,'{'
	je	macro_instructions_start
	cmp	al,3Bh
	jne	unexpected_characters
	call	skip_foreign_symbol
	jmp	find_macro_instructions
      macro_instructions_start:
	mov	ecx,80000000h
	mov	[macro_block],esi
	mov	eax,[macro_line]
	mov	[macro_block_line],eax
	mov	[macro_block_line_number],ecx
	xor	eax,eax
	mov	[counter],eax
	cmp	[counter_limit],eax
	je	process_macro_line
	inc	[counter]
      process_macro_line:
	mov	[current_line],edi
	lea	eax,[edi+10h]
	cmp	eax,[memory_end]
	jae	out_of_memory
	mov	eax,[esp+4]
	or	eax,eax
	jz	instant_macro_line_header
	stos	dword [edi]
	mov	eax,ecx
	stos	dword [edi]
	mov	eax,[esp]
	stos	dword [edi]
	mov	eax,[macro_line]
	stos	dword [edi]
	jmp	macro_line_header_ok
      instant_macro_line_header:
	mov	edx,[macro_line]
	mov	eax,[edx]
	stos	dword [edi]
	mov	eax,[edx+4]
	stos	dword [edi]
	mov	eax,[edx+8]
	stos	dword [edi]
	mov	eax,[edx+12]
	stos	dword [edi]
      macro_line_header_ok:
	or	[macro_status],20h
	push	ebx ecx
	test	[macro_status],0Fh
	jz	process_macro_line_element
	mov	ax,3Bh
	stos	word [edi]
      process_macro_line_element:
	lea	eax,[edi+100h]
	cmp	eax,[memory_end]
	jae	out_of_memory
	lods	byte [esi]
	cmp	al,'}'
	je	macro_line_processed
	or	al,al
	jz	macro_line_processed
	cmp	al,1Ah
	je	process_macro_symbol
	cmp	al,3Bh
	je	macro_foreign_line
	and	[macro_status],not 20h
	stos	byte [edi]
	cmp	al,22h
	jne	process_macro_line_element
      copy_macro_string:
	mov	ecx,[esi]
	add	ecx,4
	call	move_data
	jmp	process_macro_line_element
      process_macro_symbol:
	push	esi edi
	test	[macro_status],20h
	jz	not_macro_directive
	movzx	ecx,byte [esi]
	inc	esi
	mov	edi,macro_directives
	call	get_directive
	jnc	process_macro_directive
	dec	esi
	jmp	not_macro_directive
      process_macro_directive:
	mov	edx,eax
	pop	edi eax
	mov	byte [edi],0
	inc	edi
	pop	ecx ebx
	jmp	near edx
      not_macro_directive:
	and	[macro_status],not 20h
	movzx	ecx,byte [esi]
	inc	esi
	mov	eax,[counter]
	call	get_macro_symbol
	jnc	group_macro_symbol
	xor	eax,eax
	cmp	[counter],eax
	je	multiple_macro_symbol_values
	call	get_macro_symbol
	jc	not_macro_symbol
      replace_macro_symbol:
	pop	edi eax
	mov	ecx,[edx+8]
	and	ecx,not 80000000h
	mov	edx,[edx+12]
	or	edx,edx
	jz	replace_macro_counter
	xchg	esi,edx
	call	move_data
	mov	esi,edx
	jmp	process_macro_line_element
      group_macro_symbol:
	xor	eax,eax
	cmp	[counter],eax
	je	replace_macro_symbol
	push	esi edx
	sub	esi,ecx
	call	get_macro_symbol
	mov	ebx,edx
	pop	edx esi
	jc	replace_macro_symbol
	cmp	edx,ebx
	ja	replace_macro_symbol
	mov	edx,ebx
	jmp	replace_macro_symbol
      multiple_macro_symbol_values:
	inc	eax
	push	eax
	call	get_macro_symbol
	pop	eax
	jc	not_macro_symbol
	pop	edi
	push	ecx
	mov	ecx,[edx+8]
	mov	edx,[edx+12]
	xchg	esi,edx
	btr	ecx,31
	jc	enclose_macro_symbol_value
	rep	movs byte [edi],[esi]
	jmp	macro_symbol_value_ok
      enclose_macro_symbol_value:
	mov	byte [edi],'<'
	inc	edi
	rep	movs byte [edi],[esi]
	mov	byte [edi],'>'
	inc	edi
      macro_symbol_value_ok:
	cmp	eax,[counter_limit]
	je	multiple_macro_symbol_values_ok
	mov	byte [edi],','
	inc	edi
	mov	esi,edx
	pop	ecx
	push	edi
	sub	esi,ecx
	jmp	multiple_macro_symbol_values
      multiple_macro_symbol_values_ok:
	pop	ecx eax
	mov	esi,edx
	jmp	process_macro_line_element
      replace_macro_counter:
	mov	eax,[counter]
	and	eax,not 80000000h
	jz	group_macro_counter
	add	ecx,eax
	dec	ecx
	call	store_number_symbol
	jmp	process_macro_line_element
      group_macro_counter:
	mov	edx,ecx
	xor	ecx,ecx
      multiple_macro_counter_values:
	push	ecx edx
	add	ecx,edx
	call	store_number_symbol
	pop	edx ecx
	inc	ecx
	cmp	ecx,[counter_limit]
	je	process_macro_line_element
	mov	byte [edi],','
	inc	edi
	jmp	multiple_macro_counter_values
      store_number_symbol:
	mov	ax,1Ah
	stos	word [edi]
	push	edi
	mov	eax,ecx
	mov	ecx,1000000000
	xor	edx,edx
	xor	bl,bl
      store_number_digits:
	div	ecx
	push	edx
	or	bl,bl
	jnz	store_number_digit
	cmp	ecx,1
	je	store_number_digit
	or	al,al
	jz	number_digit_ok
	not	bl
      store_number_digit:
	add	al,30h
	stos	byte [edi]
      number_digit_ok:
	mov	eax,ecx
	xor	edx,edx
	mov	ecx,10
	div	ecx
	mov	ecx,eax
	pop	eax
	or	ecx,ecx
	jnz	store_number_digits
	pop	ebx
	mov	eax,edi
	sub	eax,ebx
	mov	[ebx-1],al
	ret
      not_macro_symbol:
	pop	edi esi
	mov	al,1Ah
	stos	byte [edi]
	mov	al,[esi]
	inc	esi
	stos	byte [edi]
	cmp	byte [esi],'.'
	jne	copy_raw_symbol
	mov	ebx,[esp+8+8]
	or	ebx,ebx
	jz	copy_raw_symbol
	cmp	al,1
	je	copy_struc_name
	xchg	esi,ebx
	movzx	ecx,byte [esi-1]
	add	[edi-1],cl
	jc	name_too_long
	rep	movs byte [edi],[esi]
	xchg	esi,ebx
      copy_raw_symbol:
	movzx	ecx,al
	rep	movs byte [edi],[esi]
	jmp	process_macro_line_element
      copy_struc_name:
	inc	esi
	xchg	esi,ebx
	movzx	ecx,byte [esi-1]
	mov	[edi-1],cl
	rep	movs byte [edi],[esi]
	xchg	esi,ebx
	mov	eax,[esp+8+12]
	cmp	byte [eax],3Bh
	je	process_macro_line_element
	cmp	byte [eax],1Ah
	jne	disable_replaced_struc_name
	mov	byte [eax],3Bh
	jmp	process_macro_line_element
      disable_replaced_struc_name:
	mov	ebx,[esp+8+8]
	push	esi edi
	lea	edi,[ebx-3]
	lea	esi,[edi-2]
	lea	ecx,[esi+1]
	sub	ecx,eax
	std
	rep	movs byte [edi],[esi]
	cld
	mov	word [eax],3Bh
	pop	edi esi
	jmp	process_macro_line_element
      skip_foreign_symbol:
	lods	byte [esi]
	movzx	eax,al
	add	esi,eax
      skip_foreign_line:
	lods	byte [esi]
	cmp	al,1Ah
	je	skip_foreign_symbol
	cmp	al,3Bh
	je	skip_foreign_symbol
	cmp	al,22h
	je	skip_foreign_string
	or	al,al
	jnz	skip_foreign_line
	ret
      skip_foreign_string:
	lods	dword [esi]
	add	esi,eax
	jmp	skip_foreign_line
      macro_foreign_line:
	call	skip_foreign_symbol
      macro_line_processed:
	mov	byte [edi],0
	inc	edi
	push	eax
	call	preprocess_line
	pop	eax
	pop	ecx ebx
	cmp	al,'}'
	je	macro_block_processed
      process_next_line:
	inc	ecx
	mov	[macro_line],esi
	add	esi,16+2
	jmp	process_macro_line
      macro_block_processed:
	call	close_macro_block
	jc	process_macro_line
	pop	[current_line]
	add	esp,12
	pop	[macro_block_line_number]
	pop	[macro_block_line]
	pop	[macro_block]
	pop	[counter]
	pop	eax
	and	al,0F0h
	and	[macro_status],0Fh
	or	[macro_status],al
	ret

local_symbols:
	lods	byte [esi]
	cmp	al,1Ah
	jne	invalid_argument
	mov	byte [edi-1],3Bh
	xor	al,al
	stos	byte [edi]
      make_local_symbol:
	push	ecx
	lods	byte [esi]
	movzx	ecx,al
	mov	eax,[counter]
	call	add_macro_symbol
	mov	[edx+12],edi
	movzx	eax,[locals_counter]
	add	eax,ecx
	inc	eax
	cmp	eax,100h
	jae	name_too_long
	lea	ebp,[edi+2+eax]
	cmp	ebp,[memory_end]
	jae	out_of_memory
	mov	ah,al
	mov	al,1Ah
	stos	word [edi]
	rep	movs byte [edi],[esi]
	mov	al,'?'
	stos	byte [edi]
	push	esi
	mov	esi,locals_counter+1
	movzx	ecx,[locals_counter]
	rep	movs byte [edi],[esi]
	pop	esi
	mov	eax,edi
	sub	eax,[edx+12]
	mov	[edx+8],eax
	xor	al,al
	stos	byte [edi]
	mov	eax,locals_counter
	movzx	ecx,byte [eax]
      counter_loop:
	inc	byte [eax+ecx]
	cmp	byte [eax+ecx],':'
	jb	counter_ok
	jne	letter_digit
	mov	byte [eax+ecx],'A'
	jmp	counter_ok
      letter_digit:
	cmp	byte [eax+ecx],'F'
	jbe	counter_ok
	mov	byte [eax+ecx],'0'
	loop	counter_loop
      counter_ok:
	pop	ecx
	lods	byte [esi]
	cmp	al,'}'
	je	macro_block_processed
	or	al,al
	jz	process_next_line
	cmp	al,','
	jne	extra_characters_on_line
	dec	edi
	lods	byte [esi]
	cmp	al,1Ah
	je	make_local_symbol
	jmp	invalid_argument
common_block:
	call	close_macro_block
	jc	process_macro_line
	mov	[counter],0
	jmp	new_macro_block
forward_block:
	cmp	[counter_limit],0
	je	common_block
	call	close_macro_block
	jc	process_macro_line
	mov	[counter],1
	jmp	new_macro_block
reverse_block:
	cmp	[counter_limit],0
	je	common_block
	call	close_macro_block
	jc	process_macro_line
	mov	eax,[counter_limit]
	or	eax,80000000h
	mov	[counter],eax
      new_macro_block:
	mov	[macro_block],esi
	mov	eax,[macro_line]
	mov	[macro_block_line],eax
	mov	[macro_block_line_number],ecx
	jmp	process_macro_line
close_macro_block:
	cmp	[counter],0
	je	block_closed
	jl	reverse_counter
	mov	eax,[counter]
	cmp	eax,[counter_limit]
	je	block_closed
	inc	[counter]
	jmp	continue_block
      reverse_counter:
	mov	eax,[counter]
	dec	eax
	cmp	eax,80000000h
	je	block_closed
	mov	[counter],eax
      continue_block:
	mov	esi,[macro_block]
	mov	eax,[macro_block_line]
	mov	[macro_line],eax
	mov	ecx,[macro_block_line_number]
	stc
	ret
      block_closed:
	clc
	ret
get_macro_symbol:
	push	ecx
	call	find_macro_symbol_leaf
	jc	macro_symbol_not_found
	mov	edx,[ebx]
	mov	ebx,esi
      try_macro_symbol:
	or	edx,edx
	jz	macro_symbol_not_found
	mov	ecx,[esp]
	mov	edi,[edx+4]
	repe	cmps byte [esi],[edi]
	je	macro_symbol_found
	mov	esi,ebx
	mov	edx,[edx]
	jmp	try_macro_symbol
      macro_symbol_found:
	pop	ecx
	clc
	ret
      macro_symbol_not_found:
	pop	ecx
	stc
	ret
      find_macro_symbol_leaf:
	shl	eax,8
	mov	al,cl
	mov	ebp,eax
	mov	ebx,macro_symbols
      follow_macro_symbols_tree:
	mov	edx,[ebx]
	or	edx,edx
	jz	no_such_macro_symbol
	xor	eax,eax
	shr	ebp,1
	adc	eax,0
	lea	ebx,[edx+eax*4]
	or	ebp,ebp
	jnz	follow_macro_symbols_tree
	add	ebx,8
	clc
	ret
      no_such_macro_symbol:
	stc
	ret
add_macro_symbol:
	push	ebx ebp
	call	find_macro_symbol_leaf
	jc	extend_macro_symbol_tree
	mov	eax,[ebx]
      make_macro_symbol:
	mov	edx,[free_additional_memory]
	add	edx,16
	cmp	edx,[labels_list]
	ja	out_of_memory
	xchg	edx,[free_additional_memory]
	mov	[ebx],edx
	mov	[edx],eax
	mov	[edx+4],esi
	pop	ebp ebx
	ret
      extend_macro_symbol_tree:
	mov	edx,[free_additional_memory]
	add	edx,16
	cmp	edx,[labels_list]
	ja	out_of_memory
	xchg	edx,[free_additional_memory]
	xor	eax,eax
	mov	[edx],eax
	mov	[edx+4],eax
	mov	[edx+8],eax
	mov	[edx+12],eax
	shr	ebp,1
	adc	eax,0
	mov	[ebx],edx
	lea	ebx,[edx+eax*4]
	or	ebp,ebp
	jnz	extend_macro_symbol_tree
	add	ebx,8
	xor	eax,eax
	jmp	make_macro_symbol

include_file:
	lods	byte [esi]
	cmp	al,22h
	jne	invalid_argument
	lods	dword [esi]
	cmp	byte [esi+eax],0
	jne	extra_characters_on_line
	push	esi
	push	edi
	mov	ebx,[current_line]
      find_current_file_path:
	mov	esi,[ebx]
	test	byte [ebx+7],80h
	jz	copy_current_file_path
	mov	ebx,[ebx+8]
	jmp	find_current_file_path
      copy_current_file_path:
	lods	byte [esi]
	stos	byte [edi]
	or	al,al
	jnz	copy_current_file_path
      cut_current_file_name:
	cmp	edi,[esp]
	je	current_file_path_ok
	cmp	byte [edi-1],'\'
	je	current_file_path_ok
	cmp	byte [edi-1],'/'
	je	current_file_path_ok
	dec	edi
	jmp	cut_current_file_name
      current_file_path_ok:
	mov	esi,[esp+4]
	call	preprocess_path
	pop	edx
	mov	esi,edx
	call	open
	jnc	include_path_ok
	mov	ebp,[include_paths]
      try_include_directories:
	mov	edi,esi
	mov	esi,ebp
	cmp	byte [esi],0
	je	try_in_current_directory
	push	ebp
	push	edi
      copy_include_directory:
	lods	byte [esi]
	cmp	al,';'
	je	include_directory_ok
	stos	byte [edi]
	or	al,al
	jnz	copy_include_directory
	dec	esi
	dec	edi
      include_directory_ok:
	cmp	byte [edi-1],'/'
	je	path_separator_ok
	cmp	byte [edi-1],'\'
	je	path_separator_ok
	mov	al,'/'
	stos	byte [edi]
      path_separator_ok:
	mov	[esp+4],esi
	mov	esi,[esp+8]
	call	preprocess_path
	pop	edx
	mov	esi,edx
	call	open
	pop	ebp
	jnc	include_path_ok
	jmp	try_include_directories
	mov	edi,esi
      try_in_current_directory:
	mov	esi,[esp]
	push	edi
	call	preprocess_path
	pop	edx
	mov	esi,edx
	call	open
	jc	file_not_found
      include_path_ok:
	mov	edi,[esp]
      copy_preprocessed_path:
	lods	byte [esi]
	stos	byte [edi]
	or	al,al
	jnz	copy_preprocessed_path
	pop	esi
	lea	ecx,[edi-1]
	sub	ecx,esi
	mov	[esi-4],ecx
	push	dword [macro_status]
	and	[macro_status],0Fh
	call	preprocess_file
	pop	eax
	mov	[macro_status],al
	jmp	line_preprocessed
      preprocess_path:
	lods	byte [esi]
	cmp	al,'%'
	je	environment_variable
	stos	byte [edi]
	or	al,al
	jnz	preprocess_path
	cmp	edi,[memory_end]
	ja	out_of_memory
	ret
      environment_variable:
	mov	ebx,esi
      find_variable_end:
	lods	byte [esi]
	or	al,al
	jz	not_environment_variable
	cmp	al,'%'
	jne	find_variable_end
	mov	byte [esi-1],0
	push	esi
	mov	esi,ebx
	call	get_environment_variable
	pop	esi
	mov	byte [esi-1],'%'
	jmp	preprocess_path
      not_environment_variable:
	mov	al,'%'
	stos	byte [edi]
	mov	esi,ebx
	jmp	preprocess_path
