
; flat assembler core
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

parser:
	mov	eax,[memory_end]
	mov	[labels_list],eax
	mov	eax,[additional_memory]
	mov	[free_additional_memory],eax
	xor	eax,eax
	mov	[current_locals_prefix],eax
	mov	[anonymous_reverse],eax
	mov	[anonymous_forward],eax
	mov	[hash_tree],eax
	mov	[blocks_stack],eax
	mov	[parsed_lines],eax
	mov	esi,[memory_start]
	mov	edi,[source_start]
      parser_loop:
	mov	[current_line],esi
	lea	eax,[edi+100h]
	cmp	eax,[labels_list]
	jae	out_of_memory
	cmp	byte [esi+16],0
	je	empty_line
	mov	al,0Fh
	stos	byte [edi]
	mov	eax,esi
	stos	dword [edi]
	inc	[parsed_lines]
	add	esi,16
      parse_line:
	cmp	byte [esi],1Ah
	jne	empty_instruction
	push	edi
	add	esi,2
	movzx	ecx,byte [esi-1]
	cmp	byte [esi+ecx],':'
	je	simple_label
	cmp	byte [esi+ecx],'='
	je	constant_label
	call	get_instruction
	jnc	main_instruction_identified
	cmp	byte [esi+ecx],1Ah
	jne	no_data_label
	push	esi ecx
	lea	esi,[esi+ecx+2]
	movzx	ecx,byte [esi-1]
	call	get_data_directive
	jnc	data_label
	pop	ecx esi
      no_data_label:
	call	get_data_directive
	jnc	main_instruction_identified
	pop	edi
	sub	esi,2
	xor	bx,bx
	call	parse_line_contents
	jmp	parse_next_line
      simple_label:
	pop	edi
	call	identify_label
	mov	byte [edi],2
	inc	edi
	stos	dword [edi]
	inc	esi
	xor	al,al
	stos	byte [edi]
	jmp	parse_line
      constant_label:
	pop	edi
	call	get_label_id
	mov	byte [edi],3
	inc	edi
	stos	dword [edi]
	xor	al,al
	stos	byte [edi]
	inc	esi
	xor	bx,bx
	call	parse_line_contents
	jmp	parse_next_line
      data_label:
	pop	ecx edx
	pop	edi
	push	eax ebx esi
	mov	esi,edx
	movzx	ecx,byte [esi-1]
	call	identify_label
	mov	byte [edi],2
	inc	edi
	stos	dword [edi]
	pop	esi ebx eax
	stos	byte [edi]
	push	edi
      main_instruction_identified:
	pop	edi
	mov	dl,al
	mov	al,1
	stos	byte [edi]
	mov	ax,bx
	stos	word [edi]
	mov	al,dl
	stos	byte [edi]
	cmp	bx,if_directive-assembler
	je	parse_block
	cmp	bx,repeat_directive-assembler
	je	parse_block
	cmp	bx,while_directive-assembler
	je	parse_block
	cmp	bx,end_directive-assembler
	je	parse_end_directive
	cmp	bx,else_directive-assembler
	je	parse_else
      common_parse:
	call	parse_line_contents
	jmp	parse_next_line
      empty_instruction:
	lods	byte [esi]
	or	al,al
	jz	parse_next_line
	cmp	al,':'
	je	invalid_name
	cmp	al,3Bh
	je	skip_preprocessed_symbol
	dec	esi
	call	parse_argument
	jmp	parse_next_line
      skip_preprocessed_symbol:
	lods	byte [esi]
	movzx	eax,al
	add	esi,eax
      skip_next:
	lods	byte [esi]
	or	al,al
	jz	parse_next_line
	cmp	al,1Ah
	je	skip_preprocessed_symbol
	cmp	al,3Bh
	je	skip_preprocessed_symbol
	cmp	al,22h
	je	skip_preprocessed_string
	jmp	skip_next
      skip_preprocessed_string:
	lods	dword [esi]
	add	esi,eax
	jmp	skip_next
      empty_line:
	add	esi,17
      parse_next_line:
	cmp	esi,[source_start]
	jb	parser_loop
      source_parsed:
	cmp	[blocks_stack],0
	je	blocks_stack_ok
	pop	eax
	pop	[current_line]
	jmp	missing_end_directive
      blocks_stack_ok:
	xor	al,al
	stos	byte [edi]
	;mov     eax,[error_line]
	;mov     [current_line],eax
	;cmp     [anonymous_forward],0
	;jne     invalid_value
	add	edi,0Fh
	and	edi,not 0Fh
	mov	[code_start],edi
	ret
      parse_block:
	mov	eax,esp
	sub	eax,100h
	jc	stack_overflow
	cmp	eax,[stack_limit]
	jb	stack_overflow
	push	[current_line]
	mov	ax,bx
	shl	eax,16
	push	eax
	inc	[blocks_stack]
	cmp	bx,if_directive-assembler
	je	parse_if
	cmp	bx,while_directive-assembler
	je	parse_while
	call	parse_line_contents
	jmp	parse_next_line
      parse_end_directive:
	cmp	byte [esi],1Ah
	jne	common_parse
	push	edi
	inc	esi
	movzx	ecx,byte [esi]
	inc	esi
	call	get_instruction
	pop	edi
	jnc	parse_end_block
	sub	esi,2
	jmp	common_parse
      parse_end_block:
	mov	dl,al
	mov	al,1
	stos	byte [edi]
	mov	ax,bx
	stos	word [edi]
	mov	al,dl
	stos	byte [edi]
	lods	byte [esi]
	or	al,al
	jnz	extra_characters_on_line
	cmp	bx,if_directive-assembler
	je	close_parsing_block
	cmp	bx,repeat_directive-assembler
	je	close_parsing_block
	cmp	bx,while_directive-assembler
	je	close_parsing_block
	jmp	parse_next_line
      close_parsing_block:
	cmp	[blocks_stack],0
	je	unexpected_instruction
	cmp	bx,[esp+2]
	jne	unexpected_instruction
	dec	[blocks_stack]
	pop	eax edx
	cmp	bx,if_directive-assembler
	jne	parse_next_line
	test	al,1100b
	jz	parse_next_line
	test	al,10000b
	jnz	parse_next_line
	sub	edi,8
	jmp	parse_next_line
      parse_if:
	push	edi
	call	parse_line_contents
	xor	al,al
	stos	byte [edi]
	xchg	esi,[esp]
	mov	edi,esi
	call	preevaluate_logical_expression
	pop	esi
	cmp	al,'0'
	je	parse_false_condition_block
	cmp	al,'1'
	je	parse_true_condition_block
	or	byte [esp],10000b
	jmp	parse_next_line
      parse_while:
	push	edi
	call	parse_line_contents
	xor	al,al
	stos	byte [edi]
	xchg	esi,[esp]
	mov	edi,esi
	call	preevaluate_logical_expression
	pop	esi
	cmp	al,'0'
	je	parse_false_condition_block
	cmp	al,'1'
	jne	parse_next_line
	stos	byte [edi]
	jmp	parse_next_line
      parse_false_condition_block:
	or	byte [esp],1
	sub	edi,4
	jmp	skip_parsing
      parse_true_condition_block:
	or	byte [esp],100b
	sub	edi,4
	jmp	parse_next_line
      parse_else:
	cmp	[blocks_stack],0
	je	unexpected_instruction
	cmp	word [esp+2],if_directive-assembler
	jne	unexpected_instruction
	lods	byte [esi]
	or	al,al
	jz	parse_pure_else
	cmp	al,1Ah
	jne	extra_characters_on_line
	push	edi
	movzx	ecx,byte [esi]
	inc	esi
	call	get_instruction
	jc	extra_characters_on_line
	pop	edi
	cmp	bx,if_directive-assembler
	jne	extra_characters_on_line
	test	byte [esp],100b
	jnz	skip_true_condition_else
	mov	dl,al
	mov	al,1
	stos	byte [edi]
	mov	ax,bx
	stos	word [edi]
	mov	al,dl
	stos	byte [edi]
	jmp	parse_if
      skip_true_condition_else:
	sub	edi,4
	or	byte [esp],1
	jmp	skip_parsing_contents
      parse_pure_else:
	bts	dword [esp],1
	jc	unexpected_instruction
	test	byte [esp],100b
	jz	parse_next_line
	sub	edi,4
	or	byte [esp],1
	jmp	skip_parsing
      skip_parsing:
	cmp	esi,[source_start]
	jae	source_parsed
	mov	[current_line],esi
	add	esi,16
      skip_parsing_line:
	cmp	byte [esi],1Ah
	jne	skip_parsing_contents
	inc	esi
	movzx	ecx,byte [esi]
	inc	esi
	cmp	byte [esi+ecx],':'
	je	skip_parsing_label
	push	edi
	call	get_instruction
	pop	edi
	jnc	skip_parsing_instruction
	add	esi,ecx
	jmp	skip_parsing_contents
      skip_parsing_label:
	lea	esi,[esi+ecx+1]
	jmp	skip_parsing_line
      skip_parsing_instruction:
	cmp	bx,if_directive-assembler
	je	skip_parsing_block
	cmp	bx,repeat_directive-assembler
	je	skip_parsing_block
	cmp	bx,while_directive-assembler
	je	skip_parsing_block
	cmp	bx,end_directive-assembler
	je	skip_parsing_end_directive
	cmp	bx,else_directive-assembler
	je	skip_parsing_else
      skip_parsing_contents:
	lods	byte [esi]
	or	al,al
	jz	skip_parsing
	cmp	al,1Ah
	je	skip_parsing_symbol
	cmp	al,3Bh
	je	skip_parsing_symbol
	cmp	al,22h
	je	skip_parsing_string
	jmp	skip_parsing_contents
      skip_parsing_symbol:
	lods	byte [esi]
	movzx	eax,al
	add	esi,eax
	jmp	skip_parsing_contents
      skip_parsing_string:
	lods	dword [esi]
	add	esi,eax
	jmp	skip_parsing_contents
      skip_parsing_block:
	mov	eax,esp
	sub	eax,100h
	jc	stack_overflow
	cmp	eax,[stack_limit]
	jb	stack_overflow
	push	[current_line]
	mov	ax,bx
	shl	eax,16
	push	eax
	inc	[blocks_stack]
	jmp	skip_parsing_contents
      skip_parsing_end_directive:
	cmp	byte [esi],1Ah
	jne	skip_parsing_contents
	push	edi
	inc	esi
	movzx	ecx,byte [esi]
	inc	esi
	call	get_instruction
	pop	edi
	jnc	skip_parsing_end_block
	add	esi,ecx
	jmp	skip_parsing_contents
      skip_parsing_end_block:
	lods	byte [esi]
	or	al,al
	jnz	extra_characters_on_line
	cmp	bx,if_directive-assembler
	je	close_skip_parsing_block
	cmp	bx,repeat_directive-assembler
	je	close_skip_parsing_block
	cmp	bx,while_directive-assembler
	je	close_skip_parsing_block
	jmp	skip_parsing
      close_skip_parsing_block:
	cmp	[blocks_stack],0
	je	unexpected_instruction
	cmp	bx,[esp+2]
	jne	unexpected_instruction
	dec	[blocks_stack]
	pop	eax edx
	test	al,1
	jz	skip_parsing
	cmp	bx,if_directive-assembler
	jne	parse_next_line
	test	al,10000b
	jz	parse_next_line
	mov	al,0Fh
	stos	byte [edi]
	mov	eax,[current_line]
	stos	dword [edi]
	inc	[parsed_lines]
	mov	eax,1 + (end_directive-assembler) shl 8
	stos	dword [edi]
	mov	eax,1 + (if_directive-assembler) shl 8
	stos	dword [edi]
	jmp	parse_next_line
      skip_parsing_else:
	cmp	[blocks_stack],0
	je	unexpected_instruction
	cmp	word [esp+2],if_directive-assembler
	jne	unexpected_instruction
	lods	byte [esi]
	or	al,al
	jz	skip_parsing_pure_else
	cmp	al,1Ah
	jne	extra_characters_on_line
	push	edi
	movzx	ecx,byte [esi]
	inc	esi
	call	get_instruction
	jc	extra_characters_on_line
	pop	edi
	cmp	bx,if_directive-assembler
	jne	extra_characters_on_line
	mov	al,[esp]
	test	al,1
	jz	skip_parsing_contents
	test	al,100b
	jnz	skip_parsing_contents
	test	al,10000b
	jnz	parse_else_if
	xor	al,al
	mov	[esp],al
	mov	al,0Fh
	stos	byte [edi]
	mov	eax,[current_line]
	stos	dword [edi]
	inc	[parsed_lines]
      parse_else_if:
	mov	eax,1 + (if_directive-assembler) shl 8
	stos	dword [edi]
	jmp	parse_if
      skip_parsing_pure_else:
	bts	dword [esp],1
	jc	unexpected_instruction
	mov	al,[esp]
	test	al,1
	jz	skip_parsing
	test	al,100b
	jnz	skip_parsing
	and	al,not 1
	or	al,1000b
	mov	[esp],al
	jmp	parse_next_line

parse_line_contents:
	mov	[parenthesis_stack],0
      parse_instruction_arguments:
	cmp	bx,prefix_instruction-assembler
	je	allow_embedded_instruction
	cmp	bx,times_directive-assembler
	je	parse_times_directive
	cmp	bx,end_directive-assembler
	je	allow_embedded_instruction
	cmp	bx,label_directive-assembler
	je	parse_label_directive
	cmp	bx,segment_directive-assembler
	je	parse_label_directive
	cmp	bx,load_directive-assembler
	je	parse_load_directive
	cmp	bx,extrn_directive-assembler
	je	parse_extrn_directive
	cmp	bx,public_directive-assembler
	je	parse_public_directive
      parse_argument:
	lea	eax,[edi+100h]
	cmp	eax,[labels_list]
	jae	out_of_memory
	lods	byte [esi]
	cmp	al,':'
	je	instruction_separator
	cmp	al,','
	je	separator
	cmp	al,'='
	je	separator
	cmp	al,'|'
	je	separator
	cmp	al,'&'
	je	separator
	cmp	al,'~'
	je	separator
	cmp	al,'>'
	je	greater
	cmp	al,'<'
	je	less
	cmp	al,')'
	je	close_parenthesis
	or	al,al
	jz	contents_parsed
	cmp	al,'['
	je	address_argument
	cmp	al,']'
	je	separator
	cmp	al,'{'
	je	unallowed_character
	cmp	al,'}'
	je	unallowed_character
	cmp	al,'#'
	je	unallowed_character
	cmp	al,'`'
	je	unallowed_character
	dec	esi
	cmp	al,1Ah
	jne	expression_argument
	push	edi
	mov	edi,directive_operators
	call	get_operator
	or	al,al
	jnz	operator_argument
	inc	esi
	movzx	ecx,byte [esi]
	inc	esi
	call	get_symbol
	jnc	symbol_argument
	cmp	ecx,1
	jne	check_argument
	cmp	byte [esi],'?'
	jne	check_argument
	pop	edi
	movs	byte [edi],[esi]
	jmp	argument_parsed
      symbol_argument:
	pop	edi
	stos	word [edi]
	jmp	argument_parsed
      operator_argument:
	pop	edi
	cmp	al,85h
	je	ptr_argument
	stos	byte [edi]
	cmp	al,80h
	je	forced_expression
	cmp	al,81h
	je	forced_parenthesis
	cmp	al,82h
	je	parse_from_operator
	cmp	al,89h
	je	parse_label_operator
	jmp	argument_parsed
      allow_embedded_instruction:
	cmp	byte [esi],1Ah
	jne	parse_argument
	push	edi
	inc	esi
	movzx	ecx,byte [esi]
	inc	esi
	call	get_instruction
	jnc	embedded_instruction
	call	get_data_directive
	jnc	embedded_instruction
	pop	edi
	sub	esi,2
	jmp	parse_argument
      embedded_instruction:
	pop	edi
	mov	dl,al
	mov	al,1
	stos	byte [edi]
	mov	ax,bx
	stos	word [edi]
	mov	al,dl
	stos	byte [edi]
	jmp	parse_instruction_arguments
      parse_times_directive:
	mov	al,'('
	stos	byte [edi]
	call	convert_expression
	mov	al,')'
	stos	byte [edi]
	cmp	byte [esi],':'
	jne	allow_embedded_instruction
	movs	byte [edi],[esi]
	jmp	allow_embedded_instruction
      parse_label_directive:
	cmp	byte [esi],1Ah
	jne	argument_parsed
	push	esi
	inc	esi
	movzx	ecx,byte [esi]
	inc	esi
	call	identify_label
	pop	ebx
	cmp	eax,0Fh
	je	non_label_identified
	mov	byte [edi],2
	inc	edi
	stos	dword [edi]
	xor	al,al
	stos	byte [edi]
	jmp	argument_parsed
      non_label_identified:
	mov	esi,ebx
	jmp	argument_parsed
      parse_load_directive:
	cmp	byte [esi],1Ah
	jne	argument_parsed
	push	esi
	inc	esi
	movzx	ecx,byte [esi]
	inc	esi
	call	get_label_id
	pop	ebx
	cmp	eax,0Fh
	je	non_label_identified
	mov	byte [edi],2
	inc	edi
	stos	dword [edi]
	xor	al,al
	stos	byte [edi]
	jmp	argument_parsed
      parse_public_directive:
	cmp	byte [esi],1Ah
	jne	parse_argument
	inc	esi
	push	esi
	movzx	ecx,byte [esi]
	inc	esi
	mov	al,2
	stos	byte [edi]
	call	get_label_id
	stos	dword [edi]
	mov	ax,8600h
	stos	word [edi]
	pop	ebx
	push	ebx esi edi
	mov	edi,directive_operators
	call	get_operator
	pop	edi edx ebx
	cmp	al,86h
	je	argument_parsed
	mov	esi,edx
	xchg	esi,ebx
	movzx	ecx,byte [esi]
	inc	esi
	mov	ax,'('
	stos	word [edi]
	mov	eax,ecx
	stos	dword [edi]
	rep	movs byte [edi],[esi]
	xor	al,al
	stos	byte [edi]
	xchg	esi,ebx
	jmp	argument_parsed
      parse_extrn_directive:
	cmp	byte [esi],22h
	je	parse_quoted_extrn
	cmp	byte [esi],1Ah
	jne	parse_argument
	push	esi
	movzx	ecx,byte [esi+1]
	add	esi,2
	mov	ax,'('
	stos	word [edi]
	mov	eax,ecx
	stos	dword [edi]
	rep	movs byte [edi],[esi]
	mov	ax,8600h
	stos	word [edi]
	pop	esi
      parse_label_operator:
	cmp	byte [esi],1Ah
	jne	argument_parsed
	inc	esi
	movzx	ecx,byte [esi]
	inc	esi
	mov	al,2
	stos	byte [edi]
	call	get_label_id
	stos	dword [edi]
	xor	al,al
	stos	byte [edi]
	jmp	argument_parsed
      parse_from_operator:
	cmp	byte [esi],22h
	jne	forced_expression
	jmp	argument_parsed
      parse_quoted_extrn:
	inc	esi
	mov	ax,'('
	stos	word [edi]
	lods	dword [esi]
	mov	ecx,eax
	stos	dword [edi]
	rep	movs byte [edi],[esi]
	xor	al,al
	stos	byte [edi]
	push	esi edi
	mov	edi,directive_operators
	call	get_operator
	mov	edx,esi
	pop	edi esi
	cmp	al,86h
	jne	argument_parsed
	stos	byte [edi]
	mov	esi,edx
	jmp	parse_label_operator
      ptr_argument:
	call	parse_address
	jmp	address_parsed
      check_argument:
	push	esi ecx
	sub	esi,2
	mov	edi,single_operand_operators
	call	get_operator
	pop	ecx esi
	or	al,al
	jnz	not_instruction
	call	get_instruction
	jnc	embedded_instruction
	call	get_data_directive
	jnc	embedded_instruction
      not_instruction:
	pop	edi
	sub	esi,2
      expression_argument:
	cmp	byte [esi],22h
	jne	not_string
	mov	eax,[esi+1]
	lea	ebx,[esi+5+eax]
	push	ebx ecx esi edi
	mov	al,'('
	stos	byte [edi]
	call	convert_expression
	mov	al,')'
	stos	byte [edi]
	pop	eax edx ecx ebx
	cmp	esi,ebx
	jne	expression_parsed
	mov	edi,eax
	mov	esi,edx
      string_argument:
	inc	esi
	mov	ax,'('
	stos	word [edi]
	lods	dword [esi]
	mov	ecx,eax
	stos	dword [edi]
	shr	ecx,1
	jnc	string_movsb_ok
	movs	byte [edi],[esi]
      string_movsb_ok:
	shr	ecx,1
	jnc	string_movsw_ok
	movs	word [edi],[esi]
      string_movsw_ok:
	rep	movs dword [edi],[esi]
	xor	al,al
	stos	byte [edi]
	jmp	expression_parsed
      not_string:
	cmp	byte [esi],'('
	jne	expression
	mov	eax,esp
	sub	eax,100h
	jc	stack_overflow
	cmp	eax,[stack_limit]
	jb	stack_overflow
	push	esi edi
	inc	esi
	mov	al,'{'
	stos	byte [edi]
	inc	[parenthesis_stack]
	jmp	parse_argument
      expression:
	mov	al,'('
	stos	byte [edi]
	call	convert_expression
	mov	al,')'
	stos	byte [edi]
	jmp	expression_parsed
      forced_expression:
	mov	al,'('
	stos	byte [edi]
	call	convert_expression
	mov	al,')'
	stos	byte [edi]
	jmp	argument_parsed
      address_argument:
	call	parse_address
	lods	byte [esi]
	cmp	al,']'
	je	address_parsed
	dec	esi
	mov	al,')'
	stos	byte [edi]
	jmp	argument_parsed
      address_parsed:
	mov	al,']'
	stos	byte [edi]
	jmp	argument_parsed
      parse_address:
	mov	al,'['
	stos	byte [edi]
	cmp	word [esi],021Ah
	jne	convert_address
	push	esi
	add	esi,4
	lea	ebx,[esi+1]
	cmp	byte [esi],':'
	pop	esi
	jne	convert_address
	add	esi,2
	mov	ecx,2
	push	ebx edi
	call	get_symbol
	pop	edi esi
	jc	unknown_segment_prefix
	cmp	al,10h
	jne	unknown_segment_prefix
	mov	al,ah
	and	ah,11110000b
	cmp	ah,60h
	jne	unknown_segment_prefix
	stos	byte [edi]
	jmp	convert_address
      unknown_segment_prefix:
	sub	esi,5
      convert_address:
	push	edi
	mov	edi,address_sizes
	call	get_operator
	pop	edi
	or	al,al
	jz	convert_expression
	add	al,70h
	stos	byte [edi]
	jmp	convert_expression
      forced_parenthesis:
	cmp	byte [esi],'('
	jne	argument_parsed
	inc	esi
	mov	al,'{'
	jmp	separator
      unallowed_character:
	mov	al,0FFh
	jmp	separator
      close_parenthesis:
	mov	al,'}'
      separator:
	stos	byte [edi]
	jmp	argument_parsed
      instruction_separator:
	stos	byte [edi]
	jmp	allow_embedded_instruction
      greater:
	cmp	byte [esi],'='
	jne	separator
	inc	esi
	mov	al,0F2h
	jmp	separator
      less:
	cmp	byte [edi-1],0F6h
	je	separator
	cmp	byte [esi],'>'
	je	not_equal
	cmp	byte [esi],'='
	jne	separator
	inc	esi
	mov	al,0F3h
	jmp	separator
      not_equal:
	inc	esi
	mov	al,0F1h
	jmp	separator
      argument_parsed:
	cmp	[parenthesis_stack],0
	je	parse_argument
	dec	[parenthesis_stack]
	add	esp,8
	jmp	argument_parsed
      expression_parsed:
	cmp	[parenthesis_stack],0
	je	parse_argument
	cmp	byte [esi],')'
	jne	argument_parsed
	dec	[parenthesis_stack]
	pop	edi esi
	jmp	expression
      contents_parsed:
	cmp	[parenthesis_stack],0
;        jne     invalid_expression
	je	contents_ok
	dec	[parenthesis_stack]
	add	esp,8
	jmp	contents_parsed
      contents_ok:
	ret

identify_label:
	cmp	byte [esi],'.'
	je	local_label_name
	call	get_label_id
	cmp	eax,10h
	jb	label_identified
	or	ebx,ebx
	jz	anonymous_label_name
	dec	ebx
	mov	[current_locals_prefix],ebx
      label_identified:
	ret
      anonymous_label_name:
	cmp	byte [esi-1],'@'
	je	anonymous_label_name_ok
	mov	eax,0Fh
      anonymous_label_name_ok:
	ret
      local_label_name:
	call	get_label_id
	ret

get_operator:
	cmp	byte [esi],1Ah
	jne	get_simple_operator
	mov	edx,esi
	push	ebp
	inc	esi
	lods	byte [esi]
	movzx	ebp,al
	push	edi
	mov	ecx,ebp
	call	lower_case
	pop	edi
      check_operator:
	mov	esi,converted
	movzx	ecx,byte [edi]
	jecxz	no_operator
	inc	edi
	mov	ebx,edi
	add	ebx,ecx
	cmp	ecx,ebp
	jne	next_operator
	repe	cmps byte [esi],[edi]
	je	operator_found
      next_operator:
	mov	edi,ebx
	inc	edi
	jmp	check_operator
      no_operator:
	mov	esi,edx
	mov	ecx,ebp
	pop	ebp
      no_simple_operator:
	xor	al,al
	ret
      operator_found:
	lea	esi,[edx+2+ebp]
	mov	ecx,ebp
	pop	ebp
	mov	al,[edi]
	ret
      get_simple_operator:
	mov	al,[esi]
	cmp	al,22h
	je	no_simple_operator
      simple_operator:
	cmp	byte [edi],1
	jb	no_simple_operator
	ja	simple_next_operator
	cmp	al,[edi+1]
	je	simple_operator_found
      simple_next_operator:
	movzx	ecx,byte [edi]
	lea	edi,[edi+1+ecx+1]
	jmp	simple_operator
      simple_operator_found:
	inc	esi
	mov	al,[edi+2]
	ret

get_symbol:
	push	esi
	mov	ebp,ecx
	call	lower_case
	mov	ecx,ebp
	cmp	cl,11
	ja	no_symbol
	sub	cl,2
	jc	no_symbol
	movzx	ebx,word [symbols+ecx*4]
	add	ebx,symbols
	movzx	edx,word [symbols+ecx*4+2]
      scan_symbols:
	or	edx,edx
	jz	no_symbol
	mov	eax,edx
	shr	eax,1
	lea	edi,[ebp+2]
	imul	eax,edi
	lea	edi,[ebx+eax]
	mov	esi,converted
	mov	ecx,ebp
	repe	cmps byte [esi],[edi]
	ja	symbols_up
	jb	symbols_down
	pop	esi
	add	esi,ebp
	mov	ax,[edi]
	clc
	ret
      no_symbol:
	pop	esi
	mov	ecx,ebp
	stc
	ret
      symbols_down:
	shr	edx,1
	jmp	scan_symbols
      symbols_up:
	lea	ebx,[edi+ecx+2]
	shr	edx,1
	adc	edx,-1
	jmp	scan_symbols

get_data_directive:
	push	esi
	mov	ebp,ecx
	call	lower_case
	mov	ecx,ebp
	cmp	cl,4
	ja	no_instruction
	sub	cl,2
	jc	no_instruction
	movzx	ebx,word [data_directives+ecx*4]
	add	ebx,data_directives
	movzx	edx,word [data_directives+ecx*4+2]
	jmp	scan_instructions

get_instruction:
	push	esi
	mov	ebp,ecx
	call	lower_case
	mov	ecx,ebp
	cmp	cl,11
	ja	no_instruction
	sub	cl,2
	jc	no_instruction
	movzx	ebx,word [instructions+ecx*4]
	add	ebx,instructions
	movzx	edx,word [instructions+ecx*4+2]
      scan_instructions:
	or	edx,edx
	jz	no_instruction
	mov	eax,edx
	shr	eax,1
	lea	edi,[ebp+3]
	imul	eax,edi
	lea	edi,[ebx+eax]
	mov	esi,converted
	mov	ecx,ebp
	repe	cmps byte [esi],[edi]
	ja	instructions_up
	jb	instructions_down
	pop	esi
	add	esi,ebp
	mov	al,[edi]
	mov	bx,[edi+1]
	clc
	ret
      no_instruction:
	pop	esi
	mov	ecx,ebp
	stc
	ret
      instructions_down:
	shr	edx,1
	jmp	scan_instructions
      instructions_up:
	lea	ebx,[edi+ecx+3]
	shr	edx,1
	adc	edx,-1
	jmp	scan_instructions

get_label_id:
	cmp	ecx,100h
	jae	name_too_long
	cmp	byte [esi],'@'
	je	anonymous_label
	cmp	byte [esi],'.'
	jne	standard_label
	cmp	byte [esi+1],'.'
	je	standard_label
	cmp	[current_locals_prefix],0
	je	standard_label
	push	edi
	mov	edi,[additional_memory_end]
	sub	edi,2
	sub	edi,ecx
	push	ecx esi
	mov	esi,[current_locals_prefix]
	lods	byte [esi]
	movzx	ecx,al
	sub	edi,ecx
	cmp	edi,[free_additional_memory]
	jb	out_of_memory
	mov	word [edi],0
	add	edi,2
	mov	ebx,edi
	rep	movs byte [edi],[esi]
	pop	esi ecx
	add	al,cl
	jc	name_too_long
	rep	movs byte [edi],[esi]
	pop	edi
	push	ebx esi
	movzx	ecx,al
	mov	byte [ebx-1],al
	mov	esi,ebx
	call	get_label_id
	pop	esi ebx
	cmp	ebx,[eax+24]
	jne	composed_label_id_ok
	lea	edx,[ebx-2]
	mov	[additional_memory_end],edx
      composed_label_id_ok:
	ret
      anonymous_label:
	cmp	ecx,2
	jne	standard_label
	mov	al,[esi+1]
	mov	ebx,characters
	xlat	byte [ebx]
	cmp	al,'@'
	je	new_anonymous
	cmp	al,'b'
	je	anonymous_back
	cmp	al,'r'
	je	anonymous_back
	cmp	al,'f'
	jne	standard_label
	add	esi,2
	mov	eax,[anonymous_forward]
	or	eax,eax
	jnz	anonymous_ok
	mov	eax,[current_line]
	mov	[error_line],eax
	call	allocate_label
	mov	[anonymous_forward],eax
      anonymous_ok:
	xor	ebx,ebx
	ret
      anonymous_back:
	mov	eax,[anonymous_reverse]
	or	eax,eax
	jz	new_anonymous
	add	esi,2
	jmp	anonymous_ok
      new_anonymous:
	add	esi,2
	mov	eax,[anonymous_forward]
	or	eax,eax
	jnz	new_anonymous_ok
	call	allocate_label
      new_anonymous_ok:
	mov	[anonymous_reverse],eax
	mov	[anonymous_forward],0
	jmp	anonymous_ok
      standard_label:
	cmp	byte [esi],'%'
	je	get_predefined_id
	cmp	byte [esi],'$'
	jne	find_label
	cmp	ecx,2
	ja	find_label
	inc	esi
	jb	get_current_offset_id
	inc	esi
	cmp	byte [esi-1],'$'
	je	get_org_origin_id
	sub	esi,ecx
	jmp	find_label
      get_current_offset_id:
	xor	eax,eax
	ret
      get_counter_id:
	mov	eax,1
	ret
      get_timestamp_id:
	mov	eax,2
	ret
      get_org_origin_id:
	mov	eax,3
	ret
      get_predefined_id:
	cmp	ecx,2
	ja	find_label
	inc	esi
	cmp	cl,1
	je	get_counter_id
	lods	byte [esi]
	mov	ebx,characters
	xlat	[ebx]
	cmp	al,'t'
	je	get_timestamp_id
	sub	esi,2
      find_label:
	xor	ebx,ebx
	mov	eax,2166136261
	mov	ebp,16777619
      hash_label:
	xor	al,[esi+ebx]
	mul	ebp
	inc	bl
	cmp	bl,cl
	jb	hash_label
	mov	ebp,eax
	shl	eax,8
	and	ebp,0FFh shl 24
	xor	ebp,eax
	or	ebp,ebx
	mov	[label_hash],ebp
	push	edi esi
	push	ecx
	mov	ecx,32
	mov	ebx,hash_tree
      follow_tree:
	mov	edx,[ebx]
	or	edx,edx
	jz	extend_tree
	xor	eax,eax
	shl	ebp,1
	adc	eax,0
	lea	ebx,[edx+eax*4]
	dec	ecx
	jnz	follow_tree
	mov	[label_leaf],ebx
	pop	edx
	mov	eax,[ebx]
	or	eax,eax
	jz	add_label
	mov	ebx,esi
	mov	ebp,[label_hash]
      compare_labels:
	mov	esi,ebx
	mov	ecx,edx
	mov	edi,[eax+4]
	mov	edi,[edi+24]
	repe	cmps byte [esi],[edi]
	je	label_found
	mov	eax,[eax]
	or	eax,eax
	jnz	compare_labels
	jmp	add_label
      label_found:
	add	esp,4
	pop	edi
	mov	eax,[eax+4]
	ret
      extend_tree:
	mov	edx,[free_additional_memory]
	lea	eax,[edx+8]
	cmp	eax,[additional_memory_end]
	ja	out_of_memory
	mov	[free_additional_memory],eax
	xor	eax,eax
	mov	[edx],eax
	mov	[edx+4],eax
	shl	ebp,1
	adc	eax,0
	mov	[ebx],edx
	lea	ebx,[edx+eax*4]
	dec	ecx
	jnz	extend_tree
	mov	[label_leaf],ebx
	pop	edx
      add_label:
	mov	ecx,edx
	pop	esi
	cmp	byte [esi-2],0
	je	label_name_ok
	mov	al,[esi]
	cmp	al,30h
	jb	name_first_char_ok
	cmp	al,39h
	jbe	invalid_name
      name_first_char_ok:
	cmp	al,'$'
	jne	check_for_reserved_word
	cmp	ecx,1
	jne	invalid_name
      reserved_word:
	mov	eax,0Fh
	pop	edi
	ret
      check_for_reserved_word:
	call	get_instruction
	jnc	reserved_word
	call	get_data_directive
	jnc	reserved_word
	call	get_symbol
	jnc	reserved_word
	sub	esi,2
	mov	edi,operators
	call	get_operator
	or	al,al
	jnz	reserved_word
	mov	edi,single_operand_operators
	call	get_operator
	or	al,al
	jnz	reserved_word
	mov	edi,directive_operators
	call	get_operator
	or	al,al
	jnz	reserved_word
	inc	esi
	movzx	ecx,byte [esi]
	inc	esi
      label_name_ok:
	mov	edx,[free_additional_memory]
	lea	eax,[edx+8]
	cmp	eax,[additional_memory_end]
	ja	out_of_memory
	mov	[free_additional_memory],eax
	mov	ebx,esi
	add	esi,ecx
	mov	eax,[label_leaf]
	mov	edi,[eax]
	mov	[edx],edi
	mov	[eax],edx
	call	allocate_label
	mov	[edx+4],eax
	mov	[eax+24],ebx
	pop	edi
	ret
      allocate_label:
	mov	eax,[labels_list]
	mov	ecx,LABEL_STRUCTURE_SIZE shr 2
      initialize_label:
	sub	eax,4
	mov	dword [eax],0
	loop	initialize_label
	mov	[labels_list],eax
	ret

LABEL_STRUCTURE_SIZE = 32
