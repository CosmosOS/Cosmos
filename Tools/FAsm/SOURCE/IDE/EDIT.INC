
; Assembly Editor core
; Copyright (c) 2001-2007, Tomasz Grysztar.
; All rights reserved.

set_text:
	push	esi
	call	reset_editor_memory
	pop	esi
	mov	ebx,[first_line]
	or	esi,esi
	jz	new_text_ok
    set_text_line:
	call	set_line
	jc	new_text_ok
	call	allocate_segment
	jc	memory_shortage
	mov	[ebx],eax
	mov	ebx,eax
	mov	eax,[caret_line]
	mov	[ebx+4],eax
	mov	[caret_line],ebx
	inc	[lines_count]
	inc	[caret_line_number]
	jmp	set_text_line
    new_text_ok:
	xor	eax,eax
	mov	dword [ebx],eax
	inc	eax
	mov	[caret_line_number],eax
	mov	eax,[first_line]
	mov	[caret_line],eax
	mov	[caret_position],0
	retn

put_character:
	call	get_caret_segment
	inc	[caret_position]
    put_here:
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	put_after_line_end
	call	store_segment_for_undo
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	overwrite
	mov	ah,[esi+SEGMENT_HEADER_LENGTH+edx]
	shl	eax,8
	mov	al,ah
    insert_in_segment:
	lea	ebx,[esi+SEGMENT_HEADER_LENGTH+edx]
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
    shift_data_right:
	xchg	al,[ebx]
	inc	ebx
	loop	shift_data_right
	test	byte [esi],1
	jnz	shift_next_segment_right
	cmp	al,20h
	je	put_ok
    add_segment:
	push	eax
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	edi,eax
	or	eax,1
	xchg	eax,[esi]
	stosd
	mov	eax,esi
	or	eax,1
	stosd
	add	edi,SEGMENT_HEADER_LENGTH - 8
	pop	eax
	stosb
	mov	ecx,SEGMENT_DATA_LENGTH - 1
	mov	al,20h
	rep	stosb
	jmp	put_ok
    shift_next_segment_right:
	mov	esi,[esi]
	dec	esi
	call	store_segment_for_undo
	xor	edx,edx
	jmp	insert_in_segment
    put_after_line_end:
	push	eax
	call	attach_empty_segments
	pop	eax
	mov	[esi+SEGMENT_HEADER_LENGTH+edx],al
	mov	ah,20h
	shl	eax,8
    put_ok:
	test	[editor_style],AES_AUTOBRACKETS
	jz	insert_done
	shr	eax,8
	xchg	ah,al
	call	recognize_character
	jnc	insert_done
	cmp	ah,'('
	je	round
	cmp	ah,'['
	je	square
	cmp	ah,'{'
	je	brace
    insert_done:
	retn
    round:
	mov	al,')'
	jmp	auto_bracket
    square:
	mov	al,']'
	jmp	auto_bracket
    brace:
	mov	al,'}'
    auto_bracket:
	mov	edx,[caret_position]
	mov	esi,[caret_line]
	call	find_segment
	jmp	put_here
    overwrite:
	mov	[esi+SEGMENT_HEADER_LENGTH+edx],al
	retn
    attach_empty_segments:
	call	store_segment_for_undo
    attach_segment:
	push	edx
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	pop	edx
	mov	ebx,esi
	mov	esi,eax
	mov	edi,eax
	or	eax,1
	xchg	eax,[ebx]
	stosd
	mov	eax,ebx
	or	eax,1
	stosd
	add	edi,SEGMENT_HEADER_LENGTH - 8
	mov	ecx,SEGMENT_DATA_LENGTH shr 2
	mov	eax,20202020h
	rep	stosd
	sub	edx,SEGMENT_DATA_LENGTH
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	attach_segment
	retn
  recognize_character:
	cmp	al,20h
	jb	neutral_character
	cmp	al,30h
	jb	separator_character
	cmp	al,3Ah
	jb	neutral_character
	cmp	al,40h
	jb	separator_character
	cmp	al,5Bh
	jb	neutral_character
	cmp	al,5Fh
	jb	separator_character
	cmp	al,7Bh
	jb	neutral_character
	cmp	al,7Fh
	jb	separator_character
    neutral_character:
	clc
	retn
    separator_character:
	stc
	retn

delete_character:
	call	get_caret_segment
    delete_here:
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	delete_done
	call	store_segment_for_undo
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	blank
	lea	ebx,[esi+SEGMENT_LENGTH]
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	mov	al,20h
	mov	edi,[esi]
	test	edi,1
	jz	shift_data_left
	mov	al,[edi-1+SEGMENT_HEADER_LENGTH]
    shift_data_left:
	dec	ebx
	xchg	al,[ebx]
	loop	shift_data_left
	lea	esi,[edi-1]
	xor	edx,edx
	test	edi,1
	jnz	delete_here
    delete_done:
	retn
    blank:
	mov	byte [esi+SEGMENT_HEADER_LENGTH+edx],20h
	retn

tabulate:
	test	[editor_style],AES_SMARTTABS
	jz	standard_tab
	mov	esi,[caret_line]
	mov	esi,[esi+4]
	or	esi,esi
	jz	standard_tab
	mov	edx,[caret_position]
	call	find_segment
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	standard_tab
	push	dword 0
    find_space:
	lea	edi,[esi+SEGMENT_HEADER_LENGTH+edx]
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	mov	ebx,ecx
	mov	al,20h
	repne	scasb
	jne	no_space_in_this_segment
	sub	ebx,ecx
	dec	ebx
	add	edx,ebx
	add	[esp],ebx
	call	get_indent
	pop	ecx
	or	ebx,ebx
	jz	standard_tab
	add	ecx,ebx
	jmp	insert_tab_spaces
    no_space_in_this_segment:
	pop	ecx
	mov	esi,[esi]
	btr	esi,0
	jnc	standard_tab
	add	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	xor	edx,edx
	push	ecx
	jmp	find_space
    standard_tab:
	mov	ecx,[caret_position]
	and	ecx,not 111b
	sub	ecx,[caret_position]
	add	ecx,8
    insert_tab_spaces:
	xor	esi,esi
	push	ecx
	call	insert_into_line
	pop	ecx
	add	[caret_position],ecx
	retn

carriage_return:
	xor	ebx,ebx
	test	[editor_style],AES_AUTOINDENT
	jz	auto_indent_ok
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	auto_indent_ok
	call	get_caret_segment
	call	get_indent
	add	[caret_position],ebx
	mov	esi,[caret_line]
	xor	edx,edx
	call	get_indent
    auto_indent_ok:
	call	break_line
	retn
    get_indent:
	xor	ebx,ebx
    find_indent_origin:
	cmp	edx,SEGMENT_DATA_LENGTH
	jb	find_indent
	mov	esi,[esi]
	btr	esi,0
	jnc	no_indent
	sub	edx,SEGMENT_DATA_LENGTH
	jmp	find_indent_origin
    find_indent:
	lea	edi,[esi+SEGMENT_HEADER_LENGTH+edx]
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	add	ebx,ecx
	mov	al,20h
	repe	scasb
	je	segment_indent
	sub	ebx,ecx
	dec	ebx
	retn
    segment_indent:
	xor	edx,edx
	mov	esi,[esi]
	btr	esi,0
	jc	find_indent
    no_indent:
	xor	ebx,ebx
	retn

break_line:
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	go_to_next_line
	push	ebx
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	edi,eax
	mov	eax,[caret_line]
	mov	[edi+4],eax
	mov	ebx,[esp]
	push	edi
	add	edi,SEGMENT_HEADER_LENGTH
	mov	ecx,SEGMENT_DATA_LENGTH shr 2
	mov	eax,20202020h
	rep	stosd
	call	get_caret_segment
	call	store_segment_for_undo
	mov	edi,[esp]
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	empty_new_line
	push	esi edx
    new_line_segment:
	cmp	ebx,SEGMENT_DATA_LENGTH
	jb	new_line_segments_ok
	push	edi ebx
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	esi,eax
	lea	edi,[esi+SEGMENT_HEADER_LENGTH]
	mov	ecx,SEGMENT_DATA_LENGTH shr 2
	mov	eax,20202020h
	rep	stosd
	pop	ebx edi
	sub	ebx,SEGMENT_DATA_LENGTH
	mov	eax,esi
	or	eax,1
	mov	[edi],eax
	or	edi,1
	mov	[esi+4],edi
	mov	edi,esi
	jmp	new_line_segment
    new_line_segments_ok:
	pop	edx esi
    split_data:
	cmp	ebx,edx
	jbe	indent_data
	push	esi edi
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,ebx
	lea	esi,[esi+SEGMENT_HEADER_LENGTH+edx]
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+ebx]
	add	edx,ecx
	mov	eax,ecx
	rep	movsb
	mov	edi,esi
	sub	edi,eax
	mov	ecx,eax
	mov	al,20h
	rep	stosb
	push	edx
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	pop	edx edi esi
	mov	ebx,eax
	or	eax,1
	mov	[edi],eax
	or	edi,1
	mov	[ebx+4],edi
	mov	edi,ebx
	xor	ebx,ebx
	jmp	split_data
    indent_data:
	push	edi
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	mov	eax,ecx
	lea	esi,[esi+SEGMENT_HEADER_LENGTH+edx]
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+ebx]
	rep	movsb
	add	ebx,eax
	mov	edi,esi
	sub	edi,eax
	mov	ecx,eax
	mov	al,20h
	rep	stosb
	pop	edi
	mov	eax,[esp]
	xchg	eax,[esi-SEGMENT_LENGTH]
	btr	eax,0
	jnc	finish_new_line
    shift_splitted_data:
	mov	esi,eax
	or	eax,1
	mov	[edi],eax
	call	store_segment_for_undo
	mov	eax,edi
	or	eax,1
	mov	[esi+4],eax
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,ebx
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+ebx]
	add	esi,SEGMENT_HEADER_LENGTH
	mov	eax,esi
	rep	movsb
	mov	edi,eax
	mov	ecx,ebx
	rep	movsb
	sub	edi,ebx
	sub	edi,SEGMENT_HEADER_LENGTH
	mov	eax,[edi]
	btr	eax,0
	jc	shift_splitted_data
    finish_new_line:
	mov	esi,edi
	call	store_segment_for_undo
	mov	[edi],eax
	pop	esi
	or	eax,eax
	jz	new_line_pointers_ok
	xchg	esi,eax
	call	store_segment_for_undo
	xchg	esi,eax
	mov	[eax+4],esi
    new_line_pointers_ok:
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,ebx
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+ebx]
	mov	al,20h
	rep	stosb
	xchg	[caret_line],esi
	inc	[caret_line_number]
	call	check_line_length
	mov	esi,[caret_line]
	mov	dword [esi+8],0
	call	check_line_length
	inc	[lines_count]
	pop	[caret_position]
	retn
    empty_new_line:
	pop	edi
	mov	eax,[esi]
	mov	[esi],edi
	mov	[edi],eax
	or	eax,eax
	jz	empty_line_pointers_ok
	mov	[eax+4],edi
    empty_line_pointers_ok:
	mov	dword [edi+8],0
	mov	[caret_line],edi
	inc	[caret_line_number]
	inc	[lines_count]
	pop	[caret_position]
	retn
  go_to_next_line:
	mov	[caret_position],ebx
	mov	esi,[caret_line]
    skip_line_segments:
	mov	eax,[esi]
	btr	eax,0
	jnc	line_segments_skipped
	mov	esi,eax
	jmp	skip_line_segments
    line_segments_skipped:
	or	eax,eax
	jz	no_next_line
	mov	[caret_line],eax
    next_line_ok:
	inc	[caret_line_number]
	retn
    no_next_line:
	call	store_segment_for_undo
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	[esi],eax
	mov	edi,eax
	mov	[caret_line],edi
	xor	eax,eax
	stosd
	mov	[edi],esi
	add	edi,4
	stosd
	add	edi,SEGMENT_HEADER_LENGTH - 12
	mov	ecx,SEGMENT_DATA_LENGTH shr 2
	mov	eax,20202020h
	rep	stosd
	inc	[lines_count]
	jmp	next_line_ok

cut_line_break:
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	do_nothing
	call	get_caret_segment
	cmp	edx,SEGMENT_DATA_LENGTH
	jbe	segment_for_deleting_ok
	call	store_segment_for_undo
    add_empty_segment:
	push	edx esi
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	pop	ebx edx
	mov	esi,eax
	mov	edi,eax
	or	eax,1
	xchg	eax,[ebx]
	stosd
	mov	eax,ebx
	or	eax,1
	stosd
	add	edi,SEGMENT_HEADER_LENGTH - 8
	mov	ecx,SEGMENT_DATA_LENGTH shr 2
	mov	eax,20202020h
	rep	stosd
	sub	edx,SEGMENT_DATA_LENGTH
	cmp	edx,SEGMENT_DATA_LENGTH
	ja	add_empty_segment
    segment_for_deleting_ok:
	call	store_segment_for_undo
	mov	edi,esi
	mov	esi,[esi]
	btr	esi,0
	jc	append_segment
	call	cancel_line
    append_segment:
	or	esi,esi
	jz	fill_rest_with_spaces
	call	store_segment_for_undo
	or	byte [edi],1
	lea	eax,[edi+1]
	mov	[esi+4],eax
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	mov	eax,ecx
	add	esi,SEGMENT_HEADER_LENGTH
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+edx]
	rep	movsb
	mov	edi,esi
	sub	edi,eax
	mov	ecx,edx
	rep	movsb
	sub	esi,SEGMENT_LENGTH
	mov	edi,esi
	mov	esi,[esi]
	btr	esi,0
	jc	append_segment
    fill_rest_with_spaces:
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+edx]
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	mov	al,20h
	rep	stosb
	or	esi,esi
	jz	delete_done
	call	store_segment_for_undo
	mov	eax,[caret_line]
	mov	[esi+4],eax
    do_nothing:
	retn

cancel_line:
	dec	[lines_count]
	mov	eax,[esi+8]
	call	unregister_length
	cmp	esi,[window_line]
	jne	window_line_ok
	mov	eax,[caret_line]
	mov	[window_line],eax
	mov	eax,[caret_line_number]
	mov	[window_line_number],eax
    window_line_ok:
	cmp	esi,[first_line]
	jne	first_line_ok
	mov	eax,[caret_line]
	mov	[first_line],eax
    first_line_ok:
	retn

insert_into_line:
	or	ecx,ecx
	jz	void_insert
	push	ecx esi
	call	get_caret_segment
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	overwrite_in_line
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	attach_after_line_end
	mov	eax,[esp+4]
	push	edx
	xor	edx,edx
	mov	ecx,SEGMENT_DATA_LENGTH
	div	ecx
	mov	ebx,esi
	push	esi
	or	edx,edx
	jnz	find_last_segment_to_shift
	call	store_segment_for_undo
	mov	esi,[esi]
	btr	esi,0
	jnc	following_segments_shifted
	call	store_segment_for_undo
	jmp	following_segments_shifted
    find_last_segment_to_shift:
	test	byte [ebx],1
	jz	shift_following_segments
	mov	ebx,[ebx]
	dec	ebx
	jmp	find_last_segment_to_shift
    shift_following_segments:
	push	edx
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	pop	edx
	mov	edi,eax
	mov	esi,[ebx]
	mov	[edi],esi
	mov	eax,ebx
	or	eax,1
	mov	[edi+4],eax
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+edx]
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	mov	al,20h
	rep	stosb
	sub	edi,SEGMENT_LENGTH
    carry_to_next_segment:
	mov	esi,ebx
	call	store_segment_for_undo
	mov	eax,edi
	or	eax,1
	mov	[esi],eax
	add	esi,SEGMENT_LENGTH
	sub	esi,edx
	mov	ecx,edx
	add	edi,SEGMENT_HEADER_LENGTH
	rep	movsb
	cmp	ebx,[esp]
	je	following_segments_shifted
	mov	edi,ebx
	mov	ebx,[edi+4]
	btr	ebx,0
	push	edi
	add	edi,SEGMENT_LENGTH-1
	mov	esi,edi
	sub	esi,edx
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	std
	rep	movsb
	cld
	pop	edi
	jmp	carry_to_next_segment
    following_segments_shifted:
	pop	esi edx
    insert_more_segments:
	mov	edi,esi
	mov	ecx,[esp+4]
    make_inserted_segment:
	sub	ecx,SEGMENT_DATA_LENGTH
	jb	fill_inserted_segments
	push	ecx edx
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	pop	edx ecx
	mov	ebx,[edi]
	btr	ebx,0
	jnc	make_attached_segment
	or	eax,1
	mov	[edi],eax
	mov	[ebx+4],eax
	and	eax,not 1
	or	ebx,1
	mov	[eax],ebx
	or	edi,1
	mov	[eax+4],edi
	mov	edi,eax
	jmp	make_inserted_segment
    make_attached_segment:
	or	eax,1
	mov	[edi],eax
	and	eax,not 1
	mov	[eax],ebx
	or	edi,1
	mov	[eax+4],edi
	mov	edi,eax
	jmp	make_inserted_segment
    fill_inserted_segments:
	mov	eax,SEGMENT_DATA_LENGTH
	add	ecx,eax
	sub	eax,edx
	sub	eax,ecx
	jbe	all_shifts_done
	mov	ecx,eax
	lea	edi,[edi+SEGMENT_LENGTH-1]
	push	esi
	lea	esi,[esi+SEGMENT_HEADER_LENGTH+ecx-1]
	add	esi,edx
	std
	rep	movsb
	cld
	pop	esi
    all_shifts_done:
	mov	edi,esi
	pop	esi ebx
    fill_new_segment:
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	cmp	ecx,ebx
	jbe	length_for_inserting_ok
	mov	ecx,ebx
    length_for_inserting_ok:
	sub	ebx,ecx
	push	edi
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+edx]
	or	esi,esi
	jz	insert_blank_string
	rep	movsb
	jmp	string_inserted
    insert_blank_string:
	mov	al,20h
	rep	stosb
    string_inserted:
	pop	edi
	mov	edi,[edi]
	and	edi,not 1
	xor	edx,edx
	or	ebx,ebx
	jnz	fill_new_segment
	retn
    attach_after_line_end:
	call	attach_empty_segments
	mov	ebx,esi
	pop	esi
	or	esi,esi
	jnz	attach_string
	pop	ecx
	retn
    attach_string:
	mov	ecx,[esp]
	mov	eax,SEGMENT_DATA_LENGTH
	sub	eax,edx
	cmp	eax,ecx
	jae	length_to_attach_ok
	mov	ecx,eax
    length_to_attach_ok:
	sub	[esp],ecx
	lea	edi,[ebx+SEGMENT_HEADER_LENGTH+edx]
	rep	movsb
	mov	ecx,[esp]
	jecxz	attach_ok
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	edi,eax
	or	eax,1
	xchg	eax,[ebx]
	mov	[edi],eax
	or	ebx,1
	mov	[edi+4],eax
	mov	ebx,edi
	xor	edx,edx
	jmp	attach_string
    attach_ok:
	pop	eax
	lea	ecx,[ebx+SEGMENT_LENGTH]
	sub	ecx,edi
	mov	al,20h
	rep	stosb
    void_insert:
	retn
    overwrite_in_line:
	cmp	edx,SEGMENT_DATA_LENGTH
	jb	position_for_overwrite_ok
	call	attach_empty_segments
    position_for_overwrite_ok:
	mov	edi,esi
	pop	esi ebx
    overwrite_segment:
	xchg	esi,edi
	call	store_segment_for_undo
	xchg	esi,edi
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	cmp	ecx,ebx
	jbe	length_to_overwrite_ok
	mov	ecx,ebx
    length_to_overwrite_ok:
	sub	ebx,ecx
	push	edi
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+edx]
	or	esi,esi
	jz	overwrite_with_blank
	rep	movsb
	jmp	overwritten_ok
    overwrite_with_blank:
	mov	al,20h
	rep	stosb
    overwritten_ok:
	pop	edi
	or	ebx,ebx
	jz	overwrite_done
	push	esi
	mov	esi,[edi]
	btr	esi,0
	jc	overwrite_existing_segment
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	edx,eax
	or	edx,1
	mov	[edi],edx
	mov	[eax],esi
	or	edi,1
	mov	[eax+4],edi
	mov	edi,eax
	pop	esi
	xor	edx,edx
	cmp	ebx,SEGMENT_DATA_LENGTH
	jae	overwrite_segment
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,ebx
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+ebx]
	mov	al,20h
	rep	stosb
	sub	edi,SEGMENT_LENGTH
	jmp	overwrite_segment
    overwrite_existing_segment:
	call	store_segment_for_undo
	mov	edi,esi
	xor	edx,edx
	pop	esi
	jmp	overwrite_segment
    overwrite_done:
	retn

delete_from_line:
	or	ecx,ecx
	jz	nothing_to_delete
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	clear_in_line
	push	ecx
	add	[caret_position],ecx
	call	get_caret_segment
	pop	ecx
	sub	[caret_position],ecx
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	clear_rest_of_line
	push	esi edx
	call	get_caret_segment
	call	store_segment_for_undo
	mov	edi,esi
	mov	ebx,edx
	pop	edx esi
    shift_over_deleted:
	mov	ecx,SEGMENT_DATA_LENGTH
	mov	eax,ecx
	sub	ecx,ebx
	sub	eax,edx
	cmp	eax,ecx
	jae	size_to_shift_ok
	mov	ecx,eax
    size_to_shift_ok:
	push	esi edi
	lea	esi,[esi+SEGMENT_HEADER_LENGTH+edx]
	lea	edi,[edi+SEGMENT_HEADER_LENGTH+ebx]
	add	edx,ecx
	add	ebx,ecx
	rep	movsb
	pop	edi
	cmp	ebx,SEGMENT_DATA_LENGTH
	je	next_destination_segment
	pop	esi
	cmp	edx,SEGMENT_DATA_LENGTH
	jne	shift_over_deleted
	mov	esi,[esi]
	btr	esi,0
	jnc	cut_line
	xor	edx,edx
	jmp	shift_over_deleted
    next_destination_segment:
	mov	esi,[edi]
	btr	esi,0
	call	store_segment_for_undo
	mov	edi,esi
	pop	esi
	xor	ebx,ebx
	jmp	shift_over_deleted
    cut_line:
	mov	esi,edi
	mov	edx,ebx
	jmp	clear_from_here
    clear_in_line:
	xor	esi,esi
	jmp	insert_into_line
  clear_rest_of_line:
	call	get_caret_segment
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	no_line_tail_to_clear
	call	store_segment_for_undo
    clear_from_here:
	lea	edi,[esi+SEGMENT_HEADER_LENGTH+edx]
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	mov	al,20h
	rep	stosb
	mov	ebx,esi
	mov	esi,[ebx]
    remove_following_segments:
	btr	esi,0
	jnc	rest_of_line_cleared
	call	store_segment_for_undo
	or	eax,-1
	xchg	eax,[esi]
	mov	esi,eax
	inc	[released_segments]
	jmp	remove_following_segments
    rest_of_line_cleared:
	mov	[ebx],esi
    nothing_to_delete:
	retn
    no_line_tail_to_clear:
	mov	esi,[esi]
	retn

remove_line:
	mov	esi,[caret_line]
	mov	esi,[esi]
	or	esi,esi
	jnz	do_remove
	mov	edi,[caret_line]
	add	edi,SEGMENT_HEADER_LENGTH
	mov	ecx,SEGMENT_DATA_LENGTH
	mov	al,20h
	repe	scasb
	je	line_removed
    do_remove:
	call	store_status_for_undo
    cut_extra_segments:
	btr	esi,0
	jnc	replace_with_next_line
	call	store_segment_for_undo
	or	eax,-1
	xchg	eax,[esi]
	mov	esi,eax
	inc	[released_segments]
	jmp	cut_extra_segments
    replace_with_next_line:
	or	esi,esi
	jz	clear_current_line
	call	store_segment_for_undo
	mov	ebx,esi
	xchg	esi,[caret_line]
	mov	eax,[esi+4]
	mov	[ebx+4],eax
	call	store_segment_for_undo
	or	dword [esi],-1
	inc	[released_segments]
	call	cancel_line
	mov	esi,[esi+4]
	or	esi,esi
	jz	line_removed
    find_last_segment_of_previous_line:
	mov	eax,[esi]
	btr	eax,0
	jnc	link_to_new_next_line
	mov	esi,eax
	jmp	find_last_segment_of_previous_line
    link_to_new_next_line:
	call	store_segment_for_undo
	mov	[esi],ebx
    line_removed:
	mov	[caret_position],0
	mov	[selection_line],0
	retn
    clear_current_line:
	mov	esi,[caret_line]
	call	store_segment_for_undo
	mov	dword [esi],0
	lea	edi,[esi+SEGMENT_HEADER_LENGTH]
	mov	ecx,SEGMENT_DATA_LENGTH
	mov	al,20h
	rep	stosb
	jmp	line_removed

duplicate_line:
	call	store_status_for_undo
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	esi,[caret_line]
	mov	edi,eax
	mov	[edi+4],esi
	mov	eax,[esi+8]
	mov	[edi+8],eax
	call	register_length
	push	edi
    duplicate_segment:
	mov	ebx,edi
	add	edi,SEGMENT_HEADER_LENGTH
	add	esi,SEGMENT_HEADER_LENGTH
	mov	ecx,SEGMENT_DATA_LENGTH
	rep	movsb
	sub	esi,SEGMENT_LENGTH
	mov	eax,[esi]
	btr	eax,0
	jnc	all_segments_duplicated
	mov	esi,eax
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	edi,eax
	or	eax,1
	mov	[ebx],eax
	or	ebx,1
	mov	[edi+4],ebx
	jmp	duplicate_segment
    all_segments_duplicated:
	inc	[lines_count]
	call	store_segment_for_undo
	mov	[ebx],eax
	pop	ebx
	mov	[esi],ebx
	mov	esi,eax
	or	esi,esi
	jz	duplicate_done
	call	store_segment_for_undo
	mov	[esi+4],ebx
    duplicate_done:
	retn
