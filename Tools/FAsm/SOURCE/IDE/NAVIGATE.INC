
; Assembly Editor core
; Copyright (c) 2001-2007, Tomasz Grysztar.
; All rights reserved.

find_line:
	mov	esi,[first_line]
	mov	ecx,1
	mov	edx,[window_line_number]
	cmp	eax,edx
	jae	forward_from_window
	sub	edx,eax
	cmp	edx,eax
	jb	backward_from_window
	jmp	find_forward
    forward_from_window:
	mov	esi,[window_line]
	mov	ecx,edx
    find_forward:
	cmp	ecx,eax
	je	line_found
	cmp	[esi],dword 0
	je	line_found
	mov	ebx,esi
    forward_skip_line:
	mov	ebx,[ebx]
	btr	ebx,0
	jc	forward_skip_line
	or	ebx,ebx
	jz	line_found
	mov	esi,ebx
	inc	ecx
	jmp	find_forward
    backward_from_window:
	mov	esi,[window_line]
	mov	ecx,[window_line_number]
    find_backward:
	cmp	ecx,eax
	je	line_found
	cmp	[esi+4],dword 0
	je	line_found
	mov	esi,[esi+4]
	dec	ecx
	jmp	find_backward
    line_found:
	retn

get_caret_segment:
	mov	edx,[caret_position]
	mov	esi,[caret_line]
    find_segment:
	cmp	edx,SEGMENT_DATA_LENGTH
	jb	segment_found
	test	byte [esi],1
	jz	segment_found
	mov	esi,[esi]
	dec	esi
	sub	edx,SEGMENT_DATA_LENGTH
	jmp	find_segment
    segment_found:
	retn

check_line_length:
	xor	edx,edx
	mov	ebx,esi
    count_line_segments:
	test	byte [esi],1
	jz	last_line_segment
	mov	esi,[esi]
	dec	esi
	add	edx,SEGMENT_DATA_LENGTH
	jmp	count_line_segments
    last_line_segment:
	lea	edi,[esi+SEGMENT_LENGTH-1]
	mov	al,20h
	mov	ecx,SEGMENT_DATA_LENGTH
	std
	repe	scasb
	cld
	setne	al
	movzx	eax,al
	add	ecx,eax
	jnz	line_segments_ok
	or	edx,edx
	jz	line_segments_ok
	call	store_segment_for_undo
	or	eax,-1
	xchg	eax,[esi]
	mov	esi,[esi+4]
	dec	esi
	mov	[esi],eax
	inc	[released_segments]
	sub	edx,SEGMENT_DATA_LENGTH
	jmp	last_line_segment
    line_segments_ok:
	add	ecx,edx
	mov	edx,ecx
	xchg	[ebx+8],edx
	cmp	edx,[ebx+8]
	je	line_length_checked
	push	edx
	mov	eax,[ebx+8]
	call	register_length
	pop	eax
	call	unregister_length
    line_length_checked:
	retn
  register_length:
	cmp	eax,[peak_line_length]
	jbe	peak_length_ok
	mov	[peak_line_length],eax
    peak_length_ok:
	or	eax,eax
	jz	ignore_zero_length
	push	ebx
	call	find_lengths_segment
	inc	dword [ebx+SEGMENT_HEADER_LENGTH+eax*4]
	pop	ebx
    ignore_zero_length:
	retn
    find_lengths_segment:
	mov	ebx,[lengths_table]
    try_lengths_segment:
	cmp	eax,SEGMENT_DATA_LENGTH/4
	jb	length_entry_ok
	sub	eax,SEGMENT_DATA_LENGTH/4
	cmp	dword [ebx],0
	je	add_lengths_segment
	mov	ebx,[ebx]
	jmp	try_lengths_segment
    add_lengths_segment:
	push	eax ecx edi
	call	allocate_segment
	jc	memory_shortage
	mov	[ebx],eax
	mov	edi,eax
	xor	eax,eax
	stosd
	mov	eax,ebx
	stosd
	mov	eax,[ebx+8]
	add	eax,SEGMENT_DATA_LENGTH/4
	stosd
	add	edi,SEGMENT_HEADER_LENGTH-12
	mov	ecx,SEGMENT_DATA_LENGTH shr 2
	xor	eax,eax
	rep	stosd
	lea	ebx,[edi-SEGMENT_LENGTH]
	pop	edi ecx eax
	jmp	try_lengths_segment
    length_entry_ok:
	retn
  unregister_length:
	or	eax,eax
	jz	ignore_zero_length
	push	ebx
	cmp	eax,[peak_line_length]
	je	unregister_peak_length
	call	find_lengths_segment
	dec	dword [ebx+SEGMENT_HEADER_LENGTH+eax*4]
    length_unregistered:
	pop	ebx
	retn
    unregister_peak_length:
	call	find_lengths_segment
	dec	dword [ebx+SEGMENT_HEADER_LENGTH+eax*4]
	jnz	length_unregistered
    find_reduced_peak:
	or	eax,eax
	jz	try_earlier_lengths_segment
	dec	eax
	cmp	dword [ebx+SEGMENT_HEADER_LENGTH+eax*4],0
	je	find_reduced_peak
	add	eax,[ebx+8]
	mov	[peak_line_length],eax
	jmp	length_unregistered
    try_earlier_lengths_segment:
	mov	ebx,[ebx+4]
	mov	eax,SEGMENT_DATA_LENGTH/4
	or	ebx,ebx
	jnz	find_reduced_peak
	mov	[peak_line_length],ebx
	jmp	length_unregistered

update_window:
	mov	edx,[window_line_number]
	cmp	edx,1
	je	window_vertical_position_ok
	add	edx,[window_height]
	dec	edx
	cmp	edx,[lines_count]
	jle	window_vertical_position_ok
	sub	edx,[lines_count]
    window_vertical_correction:
	mov	esi,[window_line]
	mov	esi,[esi+4]
	or	esi,esi
	jz	window_vertical_position_ok
	mov	[window_line],esi
	dec	[window_line_number]
	dec	edx
	jnz	window_vertical_correction
    window_vertical_position_ok:
	mov	ecx,[peak_line_length]
	cmp	ecx,[caret_position]
	jae	caret_position_ok
	mov	ecx,[caret_position]
    caret_position_ok:
	cmp	[selection_line],0
	je	selection_position_ok
	cmp	ecx,[selection_position]
	jae	selection_position_ok
	mov	ecx,[selection_position]
    selection_position_ok:
	mov	eax,[window_width]
	dec	eax
	cmp	ecx,eax
	jae	edit_space_width_ok
	mov	ecx,eax
    edit_space_width_ok:
	mov	[maximum_position],ecx
	mov	edx,[window_position]
	or	edx,edx
	jz	window_horizontal_position_ok
	add	edx,[window_width]
	inc	ecx
	cmp	edx,ecx
	jbe	window_horizontal_position_ok
	sub	edx,ecx
	sub	[window_position],edx
	jnc	window_horizontal_position_ok
	mov	[window_position],0
    window_horizontal_position_ok:
	retn

let_caret_appear:
	mov	eax,[caret_position]
	cmp	eax,[window_position]
	jl	horizontal_correction
	mov	ecx,[window_width]
	jecxz	last_position_ok
	dec	ecx
    last_position_ok:
	add	ecx,[window_position]
	mov	eax,[caret_position]
	sub	eax,ecx
	jbe	horizontal_ok
	add	eax,[window_position]
    horizontal_correction:
	mov	[window_position],eax
    horizontal_ok:
	mov	esi,[caret_line]
	mov	ecx,[caret_line_number]
	cmp	ecx,[window_line_number]
	jl	vertical_correction
	mov	eax,[window_height]
	or	eax,eax
	jz	vertical_check
	dec	eax
    vertical_check:
	neg	eax
	add	eax,[caret_line_number]
	cmp	[window_line_number],eax
	jge	vertical_ok
	mov	esi,[window_line]
	mov	ecx,[window_line_number]
    vertical_find:
	mov	esi,[esi]
	btr	esi,0
	jc	vertical_find
	inc	ecx
	cmp	ecx,eax
	jl	vertical_find
    vertical_correction:
	mov	[window_line],esi
	mov	[window_line_number],ecx
    vertical_ok:
	retn

move_line_up:
	mov	esi,[caret_line]
	mov	eax,[esi+4]
	or	eax,eax
	jz	cannot_move
	mov	[caret_line],eax
	dec	[caret_line_number]
	clc
	retn
    cannot_move:
	stc
	retn

move_line_down:
	mov	esi,[caret_line]
    find_next_line:
	mov	eax,[esi]
	or	eax,eax
	jz	cannot_move
	btr	eax,0
	jnc	down_ok
	mov	esi,eax
	jmp	find_next_line
    down_ok:
	mov	[caret_line],eax
	inc	[caret_line_number]
	clc
	retn

move_page_up:
	mov	eax,[caret_line_number]
	sub	eax,[window_height]
	ja	pgup_caret_ok
	mov	eax,1
    pgup_caret_ok:
	call	find_line
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
	mov	eax,[window_line_number]
	sub	eax,[window_height]
	ja	pgup_window_ok
	mov	eax,1
	cmp	[window_line_number],eax
	je	moved_ok
    pgup_window_ok:
	call	find_line
	mov	[window_line],esi
	mov	[window_line_number],ecx
	retn

move_page_down:
	mov	eax,[caret_line_number]
	add	eax,[window_height]
	call	find_line
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
	mov	eax,[window_line_number]
	mov	ecx,[window_height]
	add	eax,ecx
	mov	ebx,[lines_count]
	sub	ebx,ecx
	jbe	moved_ok
	inc	ebx
	cmp	eax,ebx
	jb	pgdn_window_ok
	mov	eax,ebx
	cmp	[window_line_number],eax
	je	moved_ok
    pgdn_window_ok:
	call	find_line
	mov	[window_line],esi
	mov	[window_line_number],ecx
    moved_ok:
	retn

move_to_previous_word:
	call	get_caret_segment
	mov	ecx,[caret_position]
	sub	ecx,edx
	cmp	edx,SEGMENT_DATA_LENGTH
	jbe	find_word_to_the_left
	mov	edx,SEGMENT_DATA_LENGTH
    find_word_to_the_left:
	sub	edx,1
	jc	word_in_previous_segment
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	call	recognize_character
	jnc	find_word_start
	jmp	find_word_to_the_left
    word_in_previous_segment:
	mov	eax,[esi+4]
	or	eax,eax
	jz	previous_word_ok
	mov	esi,eax
	btr	esi,0
	jnc	word_in_previous_line
	mov	edx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	jmp	find_word_to_the_left
    word_in_previous_line:
	mov	[caret_line],esi
	dec	[caret_line_number]
	mov	edx,SEGMENT_DATA_LENGTH
	xor	ecx,ecx
    find_last_segment:
	test	byte [esi],1
	jz	find_word_to_the_left
	mov	esi,[esi]
	dec	esi
	add	ecx,SEGMENT_DATA_LENGTH
	jmp	find_last_segment
    find_word_start:
	sub	edx,1
	jc	word_on_segment_edge
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	call	recognize_character
	jc	previous_word_ok
	jmp	find_word_start
    word_on_segment_edge:
	mov	esi,[esi+4]
	btr	esi,0
	jnc	previous_word_ok
	mov	edx,SEGMENT_DATA_LENGTH
	sub	ecx,edx
	jmp	find_word_start
    previous_word_ok:
	add	ecx,edx
	inc	ecx
	mov	[caret_position],ecx
	retn

move_to_next_word:
	mov	ecx,[caret_position]
	sub	ecx,edx
    find_word_end:
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	word_wraps_segment_edge
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	call	recognize_character
	jc	find_word_to_the_right
	add	edx,1
	jmp	find_word_end
    word_wraps_segment_edge:
	mov	esi,[esi]
	or	esi,esi
	jz	move_to_line_end
	btr	esi,0
	jnc	word_in_next_line
	add	ecx,SEGMENT_DATA_LENGTH
	sub	edx,SEGMENT_DATA_LENGTH
	jmp	find_word_end
    find_word_to_the_right:
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	word_in_next_segment
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	call	recognize_character
	jnc	next_word_ok
	add	edx,1
	jmp	find_word_to_the_right
    word_in_next_segment:
	mov	esi,[esi]
	or	esi,esi
	jz	move_to_line_end
	btr	esi,0
	jnc	word_in_next_line
	add	ecx,SEGMENT_DATA_LENGTH
	sub	edx,SEGMENT_DATA_LENGTH
	jmp	find_word_to_the_right
    word_in_next_line:
	mov	[caret_line],esi
	inc	[caret_line_number]
	xor	ecx,ecx
	xor	edx,edx
	jmp	find_word_to_the_right
    next_word_ok:
	add	ecx,edx
	mov	[caret_position],ecx
	retn
  move_to_line_end:
	mov	esi,[caret_line]
	mov	eax,[esi+8]
	mov	[caret_position],eax
	retn

get_word_at_caret:
	call	get_caret_segment
	mov	ebx,[caret_position]
	sub	ebx,edx
    find_left_edge:
	or	edx,edx
	jz	left_edge_in_previous_segment
	cmp	edx,SEGMENT_DATA_LENGTH
	ja	left_edge_ok
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx-1]
	call	recognize_character
	jc	left_edge_ok
	dec	edx
	jmp	find_left_edge
    left_edge_in_previous_segment:
	mov	esi,[esi+4]
	btr	esi,0
	jnc	left_edge_ok
	mov	edx,SEGMENT_DATA_LENGTH
	sub	ebx,edx
	jmp	find_left_edge
    left_edge_ok:
	add	ebx,edx
	call	get_caret_segment
	mov	ecx,[caret_position]
	sub	ecx,edx
    find_right_edge:
	cmp	edx,SEGMENT_DATA_LENGTH
	jae	right_edge_in_next_segment
	mov	al,[esi+SEGMENT_HEADER_LENGTH+edx]
	call	recognize_character
	jc	right_edge_ok
	inc	edx
	jmp	find_right_edge
    right_edge_in_next_segment:
	mov	esi,[esi]
	btr	esi,0
	jnc	right_edge_ok
	xor	edx,edx
	add	ecx,SEGMENT_DATA_LENGTH
	jmp	find_right_edge
    right_edge_ok:
	add	ecx,edx
	sub	ecx,ebx
	mov	edx,ebx
	retn
