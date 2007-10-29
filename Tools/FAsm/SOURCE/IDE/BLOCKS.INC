
; Assembly Editor core
; Copyright (c) 2001-2007, Tomasz Grysztar.
; All rights reserved.

insert_block:
	test	[editor_mode],AEMODE_VERTICALSEL
	jz	block_to_insert_ok
	push	esi
	or	edx,-1
	xor	ecx,ecx
    count_line_characters:
	lodsb
	cmp	al,9
	je	cannot_insert
	cmp	al,0Dh
	je	count_next_line
	or	al,al
	jz	end_of_line
	inc	ecx
	jmp	count_line_characters
    end_of_line:
	dec	esi
	jmp	check_block_width
    count_next_line:
	lodsb
	cmp	al,0Ah
	je	check_block_width
	dec	esi
    check_block_width:
	cmp	edx,-1
	je	line_to_insert_ok
	cmp	edx,ecx
	je	line_to_insert_ok
    cannot_insert:
	pop	esi
	stc
	retn
    line_to_insert_ok:
	mov	edx,ecx
	xor	ecx,ecx
	cmp	byte [esi],0
	jne	count_line_characters
	pop	esi
    block_to_insert_ok:
	mov	eax,[caret_line]
	mov	ecx,[caret_line_number]
	mov	edx,[caret_position]
	mov	[selection_line],eax
	mov	[selection_line_number],ecx
	mov	[selection_position],edx
	mov	ebx,esi
    get_line_to_insert:
	lodsb
	or	al,al
	jz	insert_full_line
	cmp	al,0Dh
	je	insert_full_line
	cmp	al,0Ah
	je	insert_full_line
	cmp	al,9
	jne	get_line_to_insert
	push	esi
	dec	esi
	mov	ecx,esi
	sub	ecx,ebx
	mov	esi,ebx
	push	ecx
	call	insert_into_line
	pop	ecx
	add	[caret_position],ecx
	mov	ecx,[caret_position]
	and	ecx,not 111b
	sub	ecx,[caret_position]
	add	ecx,8
	xor	esi,esi
	push	ecx
	call	insert_into_line
	pop	ecx
	add	[caret_position],ecx
	pop	esi
	mov	ebx,esi
	jmp	get_line_to_insert
    insert_full_line:
	dec	esi
	push	esi
	mov	ecx,esi
	sub	ecx,ebx
	mov	esi,ebx
	push	ecx
	call	insert_into_line
	pop	ecx
	add	[caret_position],ecx
	pop	esi
	lodsb
	or	al,al
	jz	last_line_inserted
	cmp	al,0Ah
	je	lf_first
	lodsb
	cmp	al,0Ah
	je	next_line_to_insert
	dec	esi
	jmp	next_line_to_insert
    lf_first:
	lodsb
	cmp	al,0Dh
	je	next_line_to_insert
	dec	esi
    next_line_to_insert:
	mov	ebx,[selection_position]
	test	[editor_mode],AEMODE_VERTICALSEL
	jnz	insert_in_next_line
	test	[editor_mode],AEMODE_OVERWRITE
	jz	insert_new_line
	push	esi
	call	clear_rest_of_line
	pop	esi
	xor	ebx,ebx
    insert_in_next_line:
	push	esi ebx
	mov	esi,[caret_line]
	call	check_line_length
	pop	ebx
	call	go_to_next_line
	pop	esi
	mov	ebx,esi
	jmp	get_line_to_insert
    last_line_inserted:
	mov	esi,[caret_line]
	call	check_line_length
	clc
	retn
    insert_new_line:
	push	esi
	push	[caret_line]
	push	[caret_line_number]
	xor	ebx,ebx
	call	break_line
	pop	[caret_line_number] ebx esi
	push	[caret_line]
	mov	[caret_line],ebx
    go_to_end_of_first_line:
	test	byte [ebx],1
	jz	insert_full_lines
	mov	ebx,[ebx]
	dec	ebx
	jmp	go_to_end_of_first_line
    insert_full_lines:
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	[ebx],eax
	mov	ebx,eax
	mov	eax,[caret_line]
	mov	[ebx+4],eax
	mov	[caret_line],ebx
	inc	[caret_line_number]
	inc	[lines_count]
	call	set_line
	jnc	insert_full_lines
	pop	edi
	mov	eax,[caret_line]
	mov	[edi+4],eax
	mov	[ebx],edi
	call	cut_line_break
	mov	esi,[caret_line]
	call	check_line_length
	clc
	retn
  set_line:
	mov	edi,ebx
	add	edi,SEGMENT_HEADER_LENGTH
	mov	ecx,SEGMENT_DATA_LENGTH
	mov	[caret_position],0
	push	ebx
    copy_line:
	lodsb
	or	al,al
	jz	last_line_set
	cmp	al,0Ah
	je	copy_lf
	cmp	al,0Dh
	je	copy_cr
	cmp	al,9
	je	copy_tab
    set_character:
	stosb
	loop	copy_line
    extra_segment:
	call	allocate_segment
	jc	memory_shortage
	call	store_free_segment_for_undo
	mov	edi,eax
	or	eax,1
	mov	[ebx],eax
	or	ebx,1
	mov	[edi+4],ebx
	mov	ebx,edi
	add	edi,SEGMENT_HEADER_LENGTH
	mov	ecx,SEGMENT_DATA_LENGTH
	add	[caret_position],ecx
	jmp	copy_line
    copy_tab:
	mov	al,20h
	mov	edx,SEGMENT_DATA_LENGTH
	sub	edx,ecx
	add	edx,[caret_position]
	and	edx,111b
	cmp	edx,111b
	je	set_character
	dec	esi
	jmp	set_character
    copy_lf:
	cmp	byte [esi],0Dh
	jne	copy_new_line
	inc	esi
	jmp	copy_new_line
    copy_cr:
	cmp	byte [esi],0Ah
	jne	copy_new_line
	inc	esi
    copy_new_line:
	pop	edx
	call	finish_line
	clc
	retn
    last_line_set:
	pop	edx
	call	finish_line
	stc
	retn
    finish_line:
	mov	eax,SEGMENT_DATA_LENGTH
	sub	eax,ecx
	add	eax,[caret_position]
	mov	[caret_position],eax
	mov	[edx+8],eax
	call	register_length
	mov	al,20h
	rep	stosb
	retn

delete_block:
	test	[editor_mode],AEMODE_VERTICALSEL
	jnz	delete_vertical_block
	mov	eax,[caret_line_number]
	cmp	eax,[selection_line_number]
	je	delete_vertical_block
	mov	esi,[caret_line]
	mov	ecx,[caret_line_number]
	mov	edx,[caret_position]
	cmp	ecx,[selection_line_number]
	jbe	position_for_deleting_ok
	xchg	esi,[selection_line]
	xchg	ecx,[selection_line_number]
	xchg	edx,[selection_position]
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
	mov	[caret_position],edx
    position_for_deleting_ok:
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	clear_block
	call	get_caret_segment
	cmp	edx,SEGMENT_DATA_LENGTH
	jb	first_line_of_block
	call	attach_empty_segments
    first_line_of_block:
	mov	ecx,[caret_position]
	sub	ecx,edx
    skip_rest_of_first_line:
	add	ecx,SEGMENT_DATA_LENGTH
	mov	eax,[esi]
	btr	eax,0
	jnc	end_of_first_line
	mov	esi,eax
	jmp	skip_rest_of_first_line
    end_of_first_line:
	call	store_segment_for_undo
	mov	edi,esi
	mov	esi,eax
    remove_middle_lines:
	cmp	esi,[selection_line]
	je	middle_lines_removed
	call	store_segment_for_undo
	or	ebx,-1
	xchg	[esi],ebx
	inc	[released_segments]
	call	cancel_line
	mov	esi,ebx
	btr	esi,0
	jnc	remove_middle_lines
    remove_middle_line_segments:
	call	store_segment_for_undo
	or	eax,-1
	xchg	[esi],eax
	inc	[released_segments]
	mov	esi,eax
	btr	esi,0
	jc	remove_middle_line_segments
	jmp	remove_middle_lines
    middle_lines_removed:
	call	store_segment_for_undo
	mov	eax,esi
	or	eax,1
	mov	[edi],eax
	mov	eax,edi
	or	eax,1
	mov	[esi+4],eax
	call	cancel_line
	add	ecx,[selection_position]
	sub	ecx,[caret_position]
	call	delete_from_line
	mov	esi,[caret_line]
	call	check_line_length
	mov	esi,[esi]
	or	esi,esi
	jz	block_deleted
	call	store_segment_for_undo
	mov	eax,[caret_line]
	mov	[esi+4],eax
    block_deleted:
	retn
    clear_block:
	push	[caret_line] [caret_position]
    clear_lines:
	call	clear_rest_of_line
	mov	[caret_line],esi
	mov	[caret_position],0
	cmp	esi,[selection_line]
	jne	clear_lines
	mov	ecx,[selection_position]
	call	clear_in_line
	pop	[caret_position] [caret_line]
	retn
    delete_vertical_block:
	push	[caret_line]
	push	[caret_line_number]
	mov	eax,[caret_position]
	cmp	eax,[selection_position]
	jbe	delete_vertical_block_line
	xchg	eax,[selection_position]
	mov	[caret_position],eax
    delete_vertical_block_line:
	mov	ecx,[selection_position]
	sub	ecx,[caret_position]
	call	delete_from_line
	mov	esi,[caret_line]
	call	check_line_length
	mov	esi,[caret_line]
	mov	eax,[caret_line_number]
	cmp	eax,[selection_line_number]
	je	vertical_block_deleted
	ja	delete_in_previous_line
    delete_in_next_line:
	mov	esi,[esi]
	btr	esi,0
	jc	delete_in_next_line
	mov	[caret_line],esi
	inc	[caret_line_number]
	jmp	delete_vertical_block_line
    delete_in_previous_line:
	mov	esi,[esi+4]
	mov	[caret_line],esi
	dec	[caret_line_number]
	jmp	delete_vertical_block_line
    vertical_block_deleted:
	pop	[caret_line_number]
	pop	[caret_line]
	mov	[selection_line],0
	retn

get_block_length:
	test	[editor_mode],AEMODE_VERTICALSEL
	jnz	get_length_of_vertical_block
	mov	esi,[caret_line]
	mov	edx,[caret_position]
	mov	ebx,[selection_line]
	mov	ecx,[selection_position]
	mov	eax,[caret_line_number]
	cmp	eax,[selection_line_number]
	je	get_length_of_vertical_block
	jb	get_length_of_standard_block
	xchg	esi,ebx
	xchg	ecx,edx
    get_length_of_standard_block:
	push	ecx
	mov	ecx,[esi+8]
	sub	ecx,edx
	jae	add_length_of_line
	xor	ecx,ecx
    add_length_of_line:
	add	ecx,2
	add	[esp],ecx
    skip_standard_block_line:
	mov	esi,[esi]
	btr	esi,0
	jc	skip_standard_block_line
	cmp	esi,ebx
	je	length_of_block_ok
	mov	ecx,[esi+8]
	jmp	add_length_of_line
    length_of_block_ok:
	pop	ecx
	retn
    get_length_of_vertical_block:
	mov	edx,[caret_line_number]
	sub	edx,[selection_line_number]
	jae	vertical_dimension_ok
	neg	edx
    vertical_dimension_ok:
	mov	eax,[caret_position]
	sub	eax,[selection_position]
	jae	horizontal_dimension_ok
	neg	eax
    horizontal_dimension_ok:
	mov	ecx,eax
	add	eax,2
	mul	edx
	add	ecx,eax
	retn

copy_block:
	test	[editor_mode],AEMODE_VERTICALSEL
	jnz	copy_vertical_block
	mov	esi,[caret_line]
	mov	edx,[caret_position]
	mov	ebx,[selection_line]
	mov	ecx,[selection_position]
	mov	eax,[caret_line_number]
	cmp	eax,[selection_line_number]
	je	copy_vertical_block
	jb	copy_standard_block
	xchg	esi,ebx
	xchg	ecx,edx
    copy_standard_block:
	push	ecx
	push	ebx
	mov	ecx,[esi+8]
	sub	ecx,edx
	jb	block_line_copied
	call	copy_from_line
    block_line_copied:
	pop	ebx
    copy_next_line:
	mov	ax,0A0Dh
	stosw
	cmp	esi,ebx
	je	copy_from_last_line
	push	ebx
	mov	ecx,[esi+8]
	xor	edx,edx
	call	copy_from_line
	pop	ebx
	jmp	copy_next_line
    copy_from_last_line:
	pop	ecx
	xor	edx,edx
	call	copy_from_line
	xor	al,al
	stosb
	retn
    copy_vertical_block:
	mov	esi,[caret_line]
	mov	ebx,[selection_line]
	mov	edx,[caret_position]
	mov	ecx,[selection_position]
	mov	eax,[caret_line_number]
	cmp	eax,[selection_line_number]
	jbe	vertical_block_starting_line_ok
	xchg	esi,ebx
    vertical_block_starting_line_ok:
	cmp	edx,ecx
	jbe	vertical_block_starting_position_ok
	xchg	edx,ecx
    vertical_block_starting_position_ok:
	sub	ecx,edx
    copy_line_from_vertical_block:
	mov	eax,ebx
	sub	eax,esi
	push	eax ebx ecx edx
	call	copy_from_line
	pop	edx ecx ebx eax
	or	eax,eax
	jz	vertical_block_copied
	mov	ax,0A0Dh
	stosw
	jmp	copy_line_from_vertical_block
    vertical_block_copied:
	xor	al,al
	stosb
	retn
  copy_from_line:
	mov	ebx,ecx
    find_copying_origin:
	cmp	edx,SEGMENT_DATA_LENGTH
	jb	copy_line_segment
	mov	esi,[esi]
	btr	esi,0
	jnc	line_data_ended
	sub	edx,SEGMENT_DATA_LENGTH
	jmp	find_copying_origin
    copy_line_segment:
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	cmp	ebx,ecx
	jae	line_segment_length_ok
	mov	ecx,ebx
    line_segment_length_ok:
	sub	ebx,ecx
	mov	eax,[esi]
	lea	esi,[esi+SEGMENT_HEADER_LENGTH+edx]
	rep	movsb
	mov	esi,eax
	btr	esi,0
	jnc	line_data_ended
	xor	edx,edx
	jmp	copy_line_segment
    line_data_ended:
	or	ebx,ebx
	jz	line_copy_done
	mov	ecx,ebx
	mov	al,20h
	rep	stosb
    line_copy_done:
	retn
