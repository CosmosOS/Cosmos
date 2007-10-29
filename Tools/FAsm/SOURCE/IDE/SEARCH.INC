
; Assembly Editor core
; Copyright (c) 2001-2007, Tomasz Grysztar.
; All rights reserved.

find_first:
	mov	[search_flags],eax
	call	release_search_data
	or	esi,esi
	jz	nothing_to_search_for
	mov	edi,esi
	xor	al,al
	xor	ecx,ecx
	sub	ecx,edi
	repne	scasb
	lea	ebx,[edi-1]
	sub	ebx,esi
	jz	nothing_to_search_for
	lea	ecx,[(256+ebx)*4+ebx]
	push	ebx
	call	get_memory
	or	eax,eax
	jz	not_enough_memory
	mov	[search_data],eax
	mov	[search_handle],ebx
	pop	ebx
	mov	edi,eax
	lea	ecx,[256+ebx]
	mov	eax,ebx
	rep	stosd
	mov	ecx,ebx
	mov	ebx,[search_data]
	mov	edx,ecx
	test	[search_flags],AEFIND_BACKWARD
	jnz	text_for_backward_search
	test	[search_flags],AEFIND_CASESENSITIVE
	jnz	copy_search_text
	push	ebx
	mov	ebx,case_table
    convert_search_text:
	lodsb
	xlatb
	stosb
	loop	convert_search_text
	pop	ebx
	jmp	make_character_shifts_table
    copy_search_text:
	rep	movsb
	jmp	make_character_shifts_table
    text_for_backward_search:
	add	edi,ecx
	mov	edx,ecx
	push	ebx
	mov	ebx,case_table
    reverse_search_text:
	lodsb
	test	[search_flags],AEFIND_CASESENSITIVE
	jnz	reverse_store_character
	xlatb
    reverse_store_character:
	dec	edi
	mov	[edi],al
	dec	ecx
	jnz	reverse_search_text
	pop	ebx
	add	edi,edx
	xor	ecx,ecx
    make_character_shifts_table:
	cmp	edx,ecx
	je	character_shifts_table_ok
	dec	edi
	jecxz	character_shift_ok
	mov	al,[edi]
	cmp	[ebx+eax*4],edx
	jne	character_shift_ok
	mov	[ebx+eax*4],ecx
    character_shift_ok:
	inc	ecx
	jmp	make_character_shifts_table
    character_shifts_table_ok:
	lea	edi,[ebx+(256+ecx)*4]
	push	edi
	lea	edi,[edi+ecx-2]
	movzx	eax,byte [edi+1]
	mov	edx,[ebx+eax*4]
	mov	[ebx+256*4],edx
	cmp	ecx,1
	je	suffix_match_shifts_table_done
	mov	ecx,2
	mov	esi,edi
	sub	esi,edx
    make_suffix_match_shifts_table:
	cmp	esi,[esp]
	jb	store_suffix_match_shift
	mov	al,[esi]
	cmp	al,[edi]
	je	store_suffix_match_shift
    find_suffix_match:
	dec	esi
	inc	edx
	cmp	esi,[esp]
	jb	match_part_of_suffix
	push	ecx esi edi
	repe	cmpsb
	pop	edi esi ecx
	jne	find_suffix_match
	jmp	store_suffix_match_shift
    match_part_of_suffix:
	mov	eax,[esp]
	push	ecx esi edi
	xchg	eax,esi
	sub	eax,esi
	sub	ecx,eax
	repe	cmpsb
	pop	edi esi ecx
	jne	suffix_match_shifts_table_done
    store_suffix_match_shift:
	mov	[ebx+256*4+(ecx-1)*4],edx
	dec	esi
	dec	edi
	inc	ecx
	cmp	ecx,[ebx]
	jbe	make_suffix_match_shifts_table
    suffix_match_shifts_table_done:
	pop	eax

find_next:
	mov	edi,[search_data]
	or	edi,edi
	jz	nothing_to_search_for
	push	[caret_line]
	push	[caret_line_number]
	push	[caret_position]
	push	[selection_position]
	test	[search_flags],AEFIND_BACKWARD
	jnz	search_backward
    search_for_text:
	mov	ecx,[edi]
	dec	ecx
	add	ecx,[caret_position]
	mov	esi,[caret_line]
	cmp	ecx,[peak_line_length]
	jae	text_not_in_this_line
	mov	[caret_position],ecx
	call	get_caret_segment
	mov	edi,[search_data]
	mov	eax,[edi]
	lea	ebx,[edi+(256+eax)*4]
	mov	ah,[ebx+eax-1]
	mov	ebx,case_table
    search_in_line:
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	text_not_in_this_line
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	test	[search_flags],AEFIND_CASESENSITIVE
	jnz	compare_last_character
	xlatb
    compare_last_character:
	cmp	al,ah
	je	partial_match
    mismatch_shift:
	movzx	ecx,al
	mov	ecx,[edi+ecx*4]
    shift_search_position:
	add	edx,ecx
	add	ecx,[caret_position]
	cmp	ecx,[peak_line_length]
	jae	text_not_in_this_line
	mov	[caret_position],ecx
    check_search_position:
	cmp	edx,SEGMENT_DATA_LENGTH
	jb	search_in_line
	mov	ecx,[esi]
	btr	ecx,0
	jnc	search_in_line
	sub	edx,SEGMENT_DATA_LENGTH
	mov	esi,ecx
	jmp	check_search_position
    partial_match:
	mov	ecx,[edi]
	dec	ecx
	jz	text_found
	push	edi
	lea	edi,[edi+(256+ecx+1)*4]
	lea	edi,[edi+ecx]
    compare_text:
	sub	edx,1
	jc	compare_in_previous_segment
	dec	edi
	mov	al,20h
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	compare_character
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	test	[search_flags],AEFIND_CASESENSITIVE
	jnz	compare_character
	xlatb
    compare_character:
	cmp	al,[edi]
	loope	compare_text
	pop	edi
	je	text_found
	neg	ecx
	add	ecx,[edi]
	dec	ecx
	add	edx,ecx
	mov	ecx,[edi+(256+ecx-1)*4]
	jmp	shift_search_position
    compare_in_previous_segment:
	mov	esi,[esi+4]
	and	esi,not 1
	mov	edx,SEGMENT_DATA_LENGTH
	jmp	compare_text
    text_not_in_this_line:
	mov	esi,[esi]
	or	esi,esi
	jz	text_not_found
	btr	esi,0
	jc	text_not_in_this_line
    search_in_next_line:
	mov	[caret_line],esi
	inc	[caret_line_number]
	mov	[caret_position],0
	mov	edi,[search_data]
	jmp	search_for_text
    text_found:
	mov	eax,[caret_position]
	inc	eax
	mov	[selection_position],eax
	sub	eax,[edi]
	mov	[caret_position],eax
	jz	left_text_edge_ok
	test	[search_flags],AEFIND_WHOLEWORDS
	jz	left_text_edge_ok
	mov	edi,[search_data]
	mov	ecx,[edi]
	mov	al,[edi+(256+ecx)*4]
	call	recognize_character
	jc	left_text_edge_ok
	dec	[caret_position]
	call	get_caret_segment
	inc	[caret_position]
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	left_text_edge_ok
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	call	recognize_character
	jnc	found_not_acceptable
    left_text_edge_ok:
	mov	eax,[caret_position]
	xchg	eax,[selection_position]
	mov	[caret_position],eax
	mov	edi,[search_data]
	mov	ecx,[edi]
	lea	edi,[edi+(256+ecx)*4]
	mov	al,[edi+ecx-1]
	cmp	al,20h
	je	right_text_edge_blank
	test	[search_flags],AEFIND_WHOLEWORDS
	jz	right_text_edge_ok
	call	recognize_character
	jc	right_text_edge_ok
	call	get_caret_segment
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	right_text_edge_ok
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	call	recognize_character
	jc	right_text_edge_ok
	mov	eax,[selection_position]
	mov	[caret_position],eax
    found_not_acceptable:
	mov	edi,[search_data]
	mov	ecx,[edi]
	mov	eax,[edi+(256+ecx-1)*4]
	add	[caret_position],eax
	jmp	search_for_text
    right_text_edge_blank:
	call	get_caret_segment
    check_for_blank_end:
	mov	ecx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	jz	blank_end_next_segment
	push	edi
	mov	al,20h
	lea	edi,[esi+SEGMENT_HEADER_LENGTH+edx]
	repe	scasb
	pop	edi
	jne	right_text_edge_ok
    blank_end_next_segment:
	mov	esi,[esi]
	or	esi,esi
	jz	text_not_found
	btr	esi,0
	jnc	search_in_next_line
	xor	edx,edx
	jmp	check_for_blank_end
    right_text_edge_ok:
	mov	eax,[caret_line]
	mov	ecx,[caret_line_number]
	mov	[selection_line],eax
	mov	[selection_line_number],ecx
	add	esp,16
	clc
	retn
    text_not_found:
	pop	[selection_position]
	pop	[caret_position]
	pop	[caret_line_number]
	pop	[caret_line]
	call	release_search_data
    nothing_to_search_for:
	stc
	retn
    search_backward:
	mov	ecx,[caret_position]
	sub	ecx,1
	jc	backward_text_not_in_this_line
	mov	[caret_position],ecx
	mov	edi,[search_data]
	mov	eax,[edi]
	add	ecx,eax
	mov	al,[edi+(256+eax)*4]
	cmp	al,20h
	jne	backward_search_starting_position_ok
	mov	esi,[caret_line]
	mov	ecx,[esi+8]
	mov	edi,[search_data]
	mov	eax,[edi]
	sub	ecx,eax
	jc	backward_text_not_in_this_line
	cmp	ecx,[caret_position]
	jae	backward_search_starting_position_ok
	mov	[caret_position],ecx
    backward_search_starting_position_ok:
	call	get_caret_segment
	mov	edi,[search_data]
	mov	eax,[edi]
	lea	ebx,[edi+(256+eax)*4]
	mov	ah,[ebx+eax-1]
	mov	ebx,case_table
	cmp	edx,SEGMENT_DATA_LENGTH
	jb	backward_search_in_line
	mov	eax,SEGMENT_DATA_LENGTH-1
	sub	edx,eax
	sub	[caret_position],edx
	mov	edx,eax
    backward_search_in_line:
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	test	[search_flags],AEFIND_CASESENSITIVE
	jnz	compare_first_character
	xlatb
    compare_first_character:
	cmp	al,ah
	je	backward_partial_match
    backward_mismatch_shift:
	movzx	ecx,al
	mov	ecx,[edi+ecx*4]
    shift_backward_search_position:
	sub	edx,ecx
	sub	[caret_position],ecx
	jc	backward_text_not_in_this_line
    check_backward_search_position:
	cmp	edx,0
	jge	backward_search_in_line
	mov	esi,[esi+4]
	and	esi,not 1
	add	edx,SEGMENT_DATA_LENGTH
	jmp	check_backward_search_position
    backward_partial_match:
	mov	ecx,[edi]
	dec	ecx
	jz	backward_text_found
	push	edi
	lea	edi,[edi+(256+ecx+1)*4]
	lea	edi,[edi+ecx]
    backward_compare_text:
	inc	edx
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	compare_in_next_segment
	dec	edi
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	test	[search_flags],AEFIND_CASESENSITIVE
	jnz	backward_compare_character
	xlatb
    backward_compare_character:
	cmp	al,[edi]
	loope	backward_compare_text
	pop	edi
	je	backward_text_found
	neg	ecx
	add	ecx,[edi]
	dec	ecx
	sub	edx,ecx
	mov	ecx,[edi+(256+ecx)*4]
	jmp	shift_backward_search_position
    compare_in_next_segment:
	sub	edx,SEGMENT_DATA_LENGTH
	mov	esi,[esi]
	btr	esi,0
	jnc	compare_blank_space
	dec	edx
	jmp	backward_compare_text
    compare_blank_space:
	pop	edi
	cmp	ecx,[edi]
	jbe	backward_text_found
    backward_text_not_in_this_line:
	mov	esi,[caret_line]
	mov	esi,[esi+4]
	or	esi,esi
	jz	text_not_found
	mov	[caret_line],esi
	dec	[caret_line_number]
	mov	ecx,[peak_line_length]
	mov	[caret_position],ecx
	jmp	search_backward
    backward_text_found:
	test	[search_flags],AEFIND_WHOLEWORDS
	jz	backward_left_text_edge_ok
	cmp	[caret_position],0
	je	backward_left_text_edge_ok
	mov	edi,[search_data]
	mov	ecx,[edi]
	lea	edi,[edi+(256+ecx)*4]
	mov	al,[edi+ecx-1]
	call	recognize_character
	jc	backward_left_text_edge_ok
	dec	[caret_position]
	call	get_caret_segment
	inc	[caret_position]
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	backward_left_text_edge_ok
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	call	recognize_character
	jnc	backward_found_not_acceptable
    backward_left_text_edge_ok:
	mov	eax,[caret_position]
	mov	[selection_position],eax
	mov	edi,[search_data]
	mov	ecx,[edi]
	add	eax,ecx
	mov	[caret_position],eax
	test	[search_flags],AEFIND_WHOLEWORDS
	jz	backward_right_text_edge_ok
	mov	al,[edi+(256+ecx)*4]
	call	recognize_character
	jc	backward_right_text_edge_ok
	call	get_caret_segment
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	backward_right_text_edge_ok
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	call	recognize_character
	jc	backward_right_text_edge_ok
	mov	eax,[selection_position]
	mov	[caret_position],eax
    backward_found_not_acceptable:
	mov	edi,[search_data]
	mov	ecx,[edi]
	mov	eax,[edi+(256+ecx-1)*4]
	sub	[caret_position],eax
	jbe	backward_text_not_in_this_line
	jmp	search_backward
    backward_right_text_edge_ok:
	mov	eax,[caret_position]
	xchg	eax,[selection_position]
	mov	[caret_position],eax
	mov	eax,[caret_line]
	mov	ecx,[caret_line_number]
	mov	[selection_line],eax
	mov	[selection_line_number],ecx
	add	esp,16
	clc
	retn
  release_search_data:
	mov	edi,[search_data]
	or	edi,edi
	jz	search_data_released
	mov	eax,[search_handle]
	call	release_memory
	mov	[search_data],0
    search_data_released:
	retn
