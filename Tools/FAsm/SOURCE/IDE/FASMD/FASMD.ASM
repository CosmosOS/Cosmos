
; flat assembler IDE for DOS/DPMI
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

	format	MZ
	heap	0
	stack	stack_segment:stack_top-stack_bottom
	entry	loader:init

segment loader use16

init:

	mov	ax,3301h
	xor	dl,dl
	int	21h

	mov	ax,1A00h
	xor	bx,bx
	int	10h
	cmp	al,1Ah
	jne	no_vga
	cmp	bl,8
	jne	no_vga

	mov	ax,1687h
	int	2Fh
	or	ax,ax			; DPMI installed?
	jnz	no_dpmi
	test	bl,1			; 32-bit programs supported?
	jz	no_dpmi
	mov	word [cs:mode_switch],di
	mov	word [cs:mode_switch+2],es
	mov	bx,si			; allocate memory for DPMI data
	mov	ah,48h
	int	21h
	jc	init_failed
	mov	es,ax
	mov	ds,[ds:2Ch]
	mov	ax,1
	call	far [cs:mode_switch]	; switch to protected mode
	jc	init_failed
	mov	cx,1
	xor	ax,ax
	int	31h			; allocate descriptor for code
	jc	init_failed
	mov	si,ax
	xor	ax,ax
	int	31h			; allocate descriptor for data
	jc	init_failed
	mov	di,ax
	mov	dx,cs
	lar	cx,dx
	shr	cx,8
	or	cx,0C000h
	mov	bx,si
	mov	ax,9
	int	31h			; set code descriptor access rights
	jc	init_failed
	mov	dx,ds
	lar	cx,dx
	shr	cx,8
	or	cx,0C000h
	mov	bx,di
	int	31h			; set data descriptor access rights
	jc	init_failed
	mov	ecx,main
	shl	ecx,4
	mov	dx,cx
	shr	ecx,16
	mov	ax,7
	int	31h			; set data descriptor base address
	jc	init_failed
	mov	bx,si
	int	31h			; set code descriptor base address
	jc	init_failed
	mov	cx,0FFFFh
	mov	dx,0FFFFh
	mov	ax,8			; set segment limit to 4 GB
	int	31h
	jc	init_failed
	mov	bx,di
	int	31h
	jc	init_failed
	mov	ax,ds
	mov	ds,di
	mov	[main_selector],di
	mov	[psp_selector],es
	mov	[environment_selector],ax
	cli
	mov	ss,di
	mov	esp,stack_top
	sti
	mov	es,di
	mov	cx,1
	xor	ax,ax
	int	31h			; allocate descriptor for BIOS data segment
	jc	init_failed
	mov	bx,ax
	lar	cx,[environment_selector]
	shr	cx,8
	mov	ax,9
	int	31h			; set descriptor access rights
	jc	init_failed
	xor	cx,cx
	mov	dx,400h
	mov	ax,7
	int	31h			; set base address of BIOS data segment
	jc	init_failed
	xor	cx,cx
	mov	dx,0FFh
	mov	ax,8
	int	31h			; set limit of BIOS data segment
	jc	init_failed
	mov	fs,bx
	mov	[bios_selector],bx
	mov	cx,1
	xor	ax,ax
	int	31h			; allocate descriptor for video segment
	jc	init_failed
	mov	bx,ax
	lar	cx,[environment_selector]
	shr	cx,8
	mov	ax,9
	int	31h			; set descriptor access rights
	jc	init_failed
	mov	cx,0Bh
	mov	dx,8000h
	mov	ax,7
	int	31h			; set base address of video segment
	jc	init_failed
	xor	cx,cx
	mov	dx,4000-1
	mov	ax,8
	int	31h			; set limit of video segment
	jc	init_failed
	mov	gs,bx
	mov	[video_selector],bx
	push	si
	push	start
	retf

  no_vga:
	call	init_error
	db	'Color VGA adapter is required.',0Dh,0Ah,24h
  no_dpmi:
	call	init_error
	db	'32-bit DPMI services are not available.',0Dh,0Ah,24h
  init_failed:
	call	init_error
	db	'DPMI initialization failed.',0Dh,0Ah,24h
  init_error:
	mov	ah,9
	pop	dx
	push	cs
	pop	ds
	int	21h
	mov	ax,4CFFh
	int	21h

  mode_switch dd ?

segment main use32

  start:
	call	init_video
	jc	startup_failed

	call	init_editor_memory
	jc	startup_failed

	xor	eax,eax
	mov	[next_instance],eax
	mov	[previous_instance],eax
	mov	[file_path],eax

	mov	ecx,1000h
	mov	[line_buffer_size],ecx
	call	get_memory
	or	eax,eax
	jz	startup_failed
	mov	[line_buffer],eax
	mov	[line_buffer_handle],ecx

	mov	edi,case_table
	xor	al,al
	mov	ecx,80h
      prepare_case_table:
	stosb
	inc	al
	loop	prepare_case_table
      prepare_extended_case_table:
	push	eax
	mov	dl,al
	mov	ax,6520h
	int	21h
	mov	al,[esp]
	jc	extended_case_character_ok
	mov	al,dl
      extended_case_character_ok:
	stosb
	pop	eax
	inc	al
	jnz	prepare_extended_case_table
	mov	esi,case_table+'a'
	mov	edi,case_table+'A'
	mov	ecx,26
	rep	movsb
	mov	edi,characters
	xor	al,al
      prepare_characters_table:
	stosb
	inc	al
	jnz	prepare_characters_table
	mov	esi,characters+'a'
	mov	edi,characters+'A'
	mov	ecx,26
	rep	movsb
	mov	edi,characters
	mov	esi,symbol_characters+1
	movzx	ecx,byte [esi-1]
	xor	eax,eax
      convert_table:
	lodsb
	mov	byte [edi+eax],0
	loop	convert_table
	mov	[selected_character],'p'

	call	get_low_memory

	xor	eax,eax
	mov	[main_project_file],eax
	mov	[clipboard],eax
	mov	[last_operation],al
	mov	[find_flags],eax

	call	update_positions
	call	switch_to_ide_screen

	mov	esi,81h
      process_arguments:
	push	ds
	mov	ds,[psp_selector]
	mov	edi,filename_buffer
      find_argument:
	lodsb
	cmp	al,20h
	je	find_argument
	cmp	al,9
	je	find_argument
	cmp	al,22h
	je	quoted_argument
	dec	esi
      copy_argument:
	lodsb
	cmp	al,20h
	je	argument_end
	cmp	al,9
	je	argument_end
	cmp	al,0Dh
	je	argument_end
	stosb
	jmp	copy_argument
      quoted_argument:
	lodsb
	cmp	al,0Dh
	je	argument_end
	stosb
	cmp	al,22h
	jne	quoted_argument
	lodsb
	cmp	al,22h
	je	quoted_argument
	dec	edi
      argument_end:
	dec	esi
	pop	ds
	cmp	edi,filename_buffer
	je	main_loop
	xor	al,al
	stosb
	push	esi
	mov	edx,filename_buffer
	call	load_file
	pop	esi
	jmp	process_arguments

  main_loop:

	call	update_cursor
	call	update_screen

	xor	al,al
	xchg	[current_operation],al
	mov	[last_operation],al
	mov	[was_selection],1
	mov	eax,[selection_line]
	or	eax,eax
	jz	no_selection
	cmp	eax,[caret_line]
	jne	get_command
	mov	eax,[selection_position]
	cmp	eax,[caret_position]
	jne	get_command
    no_selection:
	mov	[was_selection],0
	mov	eax,[caret_line]
	mov	[selection_line],eax
	mov	eax,[caret_position]
	mov	[selection_position],eax
	mov	eax,[caret_line_number]
	mov	[selection_line_number],eax
    get_command:
	call	wait_for_input
	cmp	ah,1
	je	close_editor
	jb	character
	cmp	al,0Eh
	je	new_editor
	cmp	ah,94h
	je	switch_editor
	cmp	ah,0A5h
	je	switch_editor
	cmp	ah,3Ch
	je	save_current
	cmp	ah,55h
	je	save_as
	cmp	ah,3Eh
	je	open_file
	cmp	ah,3Fh
	je	goto
	cmp	ah,41h
	je	search
	cmp	ah,5Ah
	je	search_next
	cmp	ah,64h
	je	replace
	cmp	ah,43h
	je	compile_and_run
	cmp	ah,66h
	je	compile
	cmp	ah,5Ch
	je	assign_to_compiler
	cmp	ah,6Ch
	je	show_user_screen
	cmp	ah,44h
	je	options
	cmp	ah,3Dh
	je	search_next
	test	byte [fs:17h],1000b
	jz	no_alt
	cmp	ah,2Dh
	je	close_all
	cmp	ah,0Eh
	je	undo
	cmp	ah,98h
	je	scroll_up
	cmp	ah,0A0h
	je	scroll_down
	cmp	ah,9Bh
	je	scroll_left
	cmp	ah,9Dh
	je	scroll_right
      no_alt:
	or	al,al
	jz	no_ascii
	cmp	al,0E0h
	jne	ascii
      no_ascii:
	cmp	ah,4Bh
	je	left_key
	cmp	ah,4Dh
	je	right_key
	cmp	ah,48h
	je	up_key
	cmp	ah,50h
	je	down_key
	cmp	ah,47h
	je	home_key
	cmp	ah,4Fh
	je	end_key
	cmp	ah,77h
	je	screen_home
	cmp	ah,75h
	je	screen_end
	cmp	ah,73h
	je	word_left
	cmp	ah,74h
	je	word_right
	cmp	ah,8Dh
	je	word_left
	cmp	ah,91h
	je	word_right
	cmp	ah,49h
	je	pgup_key
	cmp	ah,51h
	je	pgdn_key
	cmp	ah,84h
	je	text_home
	cmp	ah,76h
	je	text_end
	cmp	ah,52h
	je	ins_key
	cmp	ah,0A2h
	je	switch_blocks
	cmp	ah,40h
	je	f6_key
	cmp	ah,93h
	je	block_delete
	cmp	ah,92h
	je	block_copy
	cmp	ah,53h
	je	del_key
	cmp	ah,78h
	jb	get_command
	cmp	ah,80h
	ja	get_command
	sub	ah,77h
	movzx	ecx,ah
	jmp	select_editor
      ascii:
	cmp	al,7Fh
	je	word_back
	cmp	al,20h
	jae	character
	cmp	al,8
	je	backspace_key
	cmp	al,9
	je	tab_key
	cmp	al,0Dh
	je	enter_key
	cmp	al,19h
	je	ctrl_y_key
	cmp	al,1Ah
	je	undo
	cmp	al,18h
	je	block_cut
	cmp	al,3
	je	block_copy
	cmp	al,16h
	je	block_paste
	cmp	al,0Fh
	je	open_file
	cmp	al,13h
	je	save_current
	cmp	al,6
	je	search
	cmp	al,7
	je	goto
	cmp	al,1
	je	select_all
	cmp	al,2
	je	ascii_table
	jmp	get_command
  character:
	cmp	[was_selection],0
	je	no_selection_to_replace
	call	store_status_for_undo
	test	[editor_style],AES_SECURESEL
	jnz	character_undo_ok
	push	eax
	call	delete_block
	pop	eax
	call	put_character
	mov	esi,[caret_line]
	call	check_line_length
	mov	[selection_line],0
	jmp	text_modified
    no_selection_to_replace:
	mov	[current_operation],20h
	cmp	[last_operation],20h
	je	character_undo_ok
	call	store_status_for_undo
    character_undo_ok:
	call	put_character
	mov	esi,[caret_line]
	call	check_line_length
	mov	[selection_line],0
	jmp	text_modified
  tab_key:
	call	store_status_for_undo
	cmp	[was_selection],0
	je	tab_securesel
	test	[editor_style],AES_SECURESEL
	jnz	tab_securesel
	call	delete_block
    tab_securesel:
	call	tabulate
	mov	[selection_line],0
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_modified
  enter_key:
	call	store_status_for_undo
	cmp	[was_selection],0
	je	enter_secureselection_ok
	test	[editor_style],AES_SECURESEL
	jnz	enter_secureselection_ok
	call	delete_block
    enter_secureselection_ok:
	call	carriage_return
	mov	[selection_line],0
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	text_modified
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_modified
  backspace_key:
	test	byte [fs:17h],100b
	jnz	replace
	cmp	[was_selection],0
	je	no_selection_to_clear
	test	[editor_style],AES_SECURESEL
	jz	block_delete
    no_selection_to_clear:
	cmp	[caret_position],0
	je	line_back
	mov	[current_operation],8
	cmp	[last_operation],8
	je	undo_back_ok
	call	store_status_for_undo
    undo_back_ok:
	dec	[caret_position]
	call	delete_character
	mov	esi,[caret_line]
	call	check_line_length
	mov	[selection_line],0
	jmp	text_modified
    line_back:
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	get_command
	mov	esi,[caret_line]
	mov	esi,[esi+4]
	or	esi,esi
	jz	get_command
	call	store_status_for_undo
	mov	[caret_line],esi
	dec	[caret_line_number]
	call	check_line_length
	mov	[caret_position],ecx
	call	cut_line_break
	mov	esi,[caret_line]
	call	check_line_length
	mov	[selection_line],0
	jmp	text_modified
    word_back:
	call	store_status_for_undo
	push	[caret_position]
	mov	esi,[caret_line]
	xor	eax,eax
	xchg	eax,[esi+4]
	push	eax
	call	move_to_previous_word
	pop	eax
	mov	esi,[caret_line]
	mov	[esi+4],eax
	pop	ecx
	sub	ecx,[caret_position]
	call	delete_from_line
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_modified
  del_key:
	test	byte [fs:17h],11b
	jnz	block_cut
	cmp	[was_selection],0
	je	no_selection_on_del
	test	[editor_style],AES_SECURESEL
	jz	block_delete
    no_selection_on_del:
	mov	esi,[caret_line]
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	delete_char
	call	check_line_length
	cmp	ecx,[caret_position]
	ja	delete_char
	cmp	dword [esi],0
	je	get_command
	call	store_status_for_undo
	call	cut_line_break
	mov	esi,[caret_line]
	call	check_line_length
	mov	[selection_line],0
	jmp	text_modified
    delete_char:
	mov	[current_operation],0E0h
	cmp	[last_operation],0E0h
	je	undo_delete_ok
	call	store_status_for_undo
    undo_delete_ok:
	call	delete_character
	mov	esi,[caret_line]
	call	check_line_length
	mov	[selection_line],0
	jmp	text_modified
  ctrl_y_key:
	call	remove_line
	jmp	text_modified
  f6_key:
	call	duplicate_line
	jmp	text_modified
  left_key:
	cmp	[caret_position],0
	jle	text_modified
	dec	[caret_position]
	jmp	moved_caret
  right_key:
	mov	eax,[caret_position]
	cmp	eax,[maximum_position]
	jae	text_modified
	inc	[caret_position]
	jmp	moved_caret
  up_key:
	call	move_line_up
	jmp	moved_caret
  down_key:
	call	move_line_down
	jmp	moved_caret
  home_key:
	mov	[caret_position],0
	jmp	moved_caret
  end_key:
	call	move_to_line_end
	jmp	moved_caret
  screen_home:
	mov	eax,[window_line]
	mov	[caret_line],eax
	mov	eax,[window_line_number]
	mov	[caret_line_number],eax
	jmp	moved_caret
  screen_end:
	mov	eax,[window_line_number]
	add	eax,[window_height]
	dec	eax
	call	find_line
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
	jmp	moved_caret
  pgup_key:
	call	move_page_up
	jmp	moved_caret
  pgdn_key:
	call	move_page_down
	jmp	moved_caret
  text_home:
	mov	eax,[first_line]
	mov	[caret_line],eax
	mov	[caret_line_number],1
	jmp	moved_caret
  text_end:
	or	eax,-1
	call	find_line
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
	jmp	moved_caret
  word_left:
	call	move_to_previous_word
	jmp	moved_caret
  word_right:
	call	get_caret_segment
	call	move_to_next_word
	jmp	moved_caret
  scroll_left:
	cmp	[window_position],0
	je	main_loop
	dec	[window_position]
	jmp	moved_window
  scroll_right:
	inc	[window_position]
	jmp	moved_window
  scroll_up:
	mov	esi,[window_line]
	mov	esi,[esi+4]
	or	esi,esi
	jz	main_loop
	mov	[window_line],esi
	dec	[window_line_number]
	jmp	moved_window
  scroll_down:
	mov	esi,[window_line]
      find_next_window_line:
	mov	esi,[esi]
	btr	esi,0
	jc	find_next_window_line
	or	esi,esi
	jz	main_loop
	mov	[window_line],esi
	inc	[window_line_number]
	jmp	moved_window
  ins_key:
	and	byte [fs:18h],not 80h
	test	byte [fs:17h],11b
	jnz	block_paste
  switch_mode:
	xor	[editor_mode],AEMODE_OVERWRITE
	jmp	main_loop
  switch_blocks:
	xor	[editor_mode],AEMODE_VERTICALSEL
	jmp	main_loop
  block_delete:
	cmp	[was_selection],0
	je	get_command
	call	store_status_for_undo
	call	delete_block
	mov	[selection_line],0
	jmp	operation_done
  block_copy:
	cmp	[was_selection],0
	je	get_command
	call	copy_to_clipboard
	jmp	get_command
    copy_to_clipboard:
	cmp	[clipboard],0
	je	allocate_clipboard
	mov	ebx,[clipboard_handle]
	call	release_memory
    allocate_clipboard:
	call	get_block_length
	inc	ecx
	call	get_memory
	mov	[clipboard],eax
	mov	[clipboard_handle],ecx
	or	eax,eax
	jz	not_enough_memory
	mov	edi,[clipboard]
	call	copy_block
	retn
  block_cut:
	cmp	[was_selection],0
	je	get_command
	call	copy_to_clipboard
	jmp	block_delete
  block_paste:
	call	store_status_for_undo
	cmp	[was_selection],0
	je	paste_secureselection_ok
	test	[editor_style],AES_SECURESEL
	jnz	paste_secureselection_ok
	call	delete_block
    paste_secureselection_ok:
	mov	esi,[clipboard]
	or	esi,esi
	jz	operation_done
	call	insert_block
	jc	paste_failed
	test	[editor_style],AES_SECURESEL
	jz	no_selection_after_paste
	mov	eax,[caret_line]
	mov	ecx,[caret_line_number]
	mov	edx,[caret_position]
	xchg	eax,[selection_line]
	xchg	ecx,[selection_line_number]
	xchg	edx,[selection_position]
	mov	[caret_line],eax
	mov	[caret_line_number],ecx
	mov	[caret_position],edx
	jmp	operation_done
    no_selection_after_paste:
	mov	[selection_line],0
	jmp	operation_done
    paste_failed:
	call	undo_changes
	jmp	operation_done
  undo:
	cmp	[undo_data],0
	je	get_command
	call	undo_changes
	mov	[last_operation],0
	jmp	operation_done
  select_all:
	or	eax,-1
	call	find_line
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
	call	check_line_length
	mov	[caret_position],ecx
	mov	eax,1
	call	find_line
	mov	[selection_line],esi
	mov	[selection_line_number],ecx
	mov	[selection_position],0
	jmp	operation_done
  ascii_table:
	call	ascii_table_window
	jc	main_loop
	jmp	character

  moved_caret:
	test	byte [fs:17h],11b
	jnz	operation_done
	mov	[selection_line],0
	jmp	operation_done
  text_modified:
	cmp	[was_selection],0
	jne	operation_done
	mov	[selection_line],0
  operation_done:
	call	update_positions
	call	let_caret_appear
	call	update_window
	jmp	main_loop
  moved_window:
	call	update_positions
	cmp	[was_selection],0
	jne	main_loop
	mov	[selection_line],0
	jmp	main_loop

  new_editor:
	call	create_editor_instance
	jmp	main_loop
  close_editor:
	mov	[command_flags],1
	jmp	closing_loop
  close_all:
	mov	[command_flags],0
    closing_loop:
	cmp	[undo_data],0
	je	do_close
	mov	esi,[file_path]
	call	get_file_title
	mov	ebx,esi
	mov	esi,_saving_question
	mov	eax,2 shl 24
	mov	ax,[message_box_colors]
	mov	[first_button],_yes
	mov	[second_button],_no
	call	message_box
	cmp	eax,1
	jb	main_loop
	ja	do_close
	cmp	[file_path],0
	jne	save_before_closing
	call	get_saving_path
	jc	main_loop
    save_before_closing:
	call	save_file
	jc	main_loop
    do_close:
	call	remove_editor_instance
	jc	exit
	test	[command_flags],1
	jnz	main_loop
	call	update_cursor
	call	update_screen
	jmp	closing_loop
  switch_editor:
	test	byte [fs:17h],11b
	jnz	previous_editor
	mov	eax,[next_instance]
	or	eax,eax
	jnz	do_switch
	mov	eax,[previous_instance]
	or	eax,eax
	jz	get_command
    find_first_editor:
	mov	ebx,[eax+16+previous_instance-editor_data]
	or	ebx,ebx
	jz	do_switch
	mov	eax,ebx
	jmp	find_first_editor
    do_switch:
	call	switch_editor_instance
	jmp	main_loop
    previous_editor:
	mov	eax,[previous_instance]
	or	eax,eax
	jnz	do_switch
	mov	eax,[next_instance]
	or	eax,eax
	jz	get_command
    find_last_editor:
	mov	ebx,[eax+16+next_instance-editor_data]
	or	ebx,ebx
	jz	do_switch
	mov	eax,ebx
	jmp	find_last_editor
  select_editor:
	mov	eax,[editor_memory]
	mov	edx,[previous_instance]
    prepare_for_editor_counting:
	or	edx,edx
	jz	find_selected_editor
	mov	eax,edx
	mov	edx,[edx+16+previous_instance-editor_data]
	jmp	prepare_for_editor_counting
    find_selected_editor:
	dec	ecx
	jz	selected_editor_found
	mov	eax,[eax+16+next_instance-editor_data]
	or	eax,eax
	jz	get_command
	jmp	find_selected_editor
    selected_editor_found:
	cmp	eax,[editor_memory]
	je	get_command
	jmp	do_switch
  show_user_screen:
	call	switch_to_user_screen
	mov	ah,10h
	int	16h
	call	switch_to_ide_screen
	jmp	main_loop

  save_current:
	cmp	[file_path],0
	jne	do_save
  save_as:
	call	get_saving_path
	jc	main_loop
      do_save:
	call	save_file
	jmp	main_loop
  open_file:
	mov	esi,_open
	call	file_open_dialog
	jc	main_loop
	call	load_file
	jmp	main_loop
  goto:
	call	goto_dialog
	jc	main_loop
	or	edx,edx
	jz	goto_position_ok
	dec	edx
	mov	[caret_position],edx
	mov	[selection_position],edx
      goto_position_ok:
	or	ecx,ecx
	jz	goto_line_ok
	mov	eax,ecx
	call	find_line
	mov	[caret_line],esi
	mov	[selection_line],esi
	mov	[caret_line_number],ecx
	mov	[selection_line_number],ecx
      goto_line_ok:
	call	update_positions
	call	let_caret_appear
	call	update_window
	jmp	main_loop
  search:
	call	find_dialog
	jc	main_loop
	call	find_first
	jc	not_found
      show_found_text:
	mov	eax,[caret_position]
	xchg	eax,[selection_position]
	mov	[caret_position],eax
	call	update_positions
	call	let_caret_appear
	call	update_window
	mov	eax,[caret_position]
	xchg	eax,[selection_position]
	mov	[caret_position],eax
	jmp	main_loop
  replace:
	call	replace_dialog
	jc	main_loop
	push	edi
	call	find_first
	jc	not_found
	call	store_status_for_undo
      replace_loop:
	call	delete_block
	mov	esi,[esp]
	call	insert_block
	call	find_next
	jnc	replace_loop
	call	update_positions
	call	let_caret_appear
	call	update_window
	jmp	main_loop
  search_next:
	cmp	[search_data],0
	je	main_loop
	call	find_next
	jnc	show_found_text
      not_found:
	call	update_screen
	mov	esi,_not_found
	mov	ebx,_find
	movzx	eax,[message_box_colors]
	call	message_box
	jmp	main_loop
  compile:
	mov	[command_flags],0
	jmp	do_compile
  compile_and_run:
	mov	[command_flags],1
      do_compile:
	push	[editor_memory]
	mov	eax,[main_project_file]
	or	eax,eax
	jz	got_main_file
	call	switch_editor_instance
      got_main_file:
	cmp	[file_path],0
	jne	main_file_path_ok
	call	update_screen
	call	get_saving_path
	jc	main_loop
	call	update_screen
      main_file_path_ok:
	mov	eax,[editor_memory]
	push	eax
      find_first_to_save:
	mov	edx,[eax+16+previous_instance-editor_data]
	or	edx,edx
	jz	save_all_files
	mov	eax,edx
	jmp	find_first_to_save
      save_all_files:
	call	switch_editor_instance
	cmp	[file_path],0
	je	save_next
	cmp	[undo_data],0
	je	save_next
	call	save_file
	jc	main_loop
      save_next:
	mov	eax,[next_instance]
	or	eax,eax
	jnz	save_all_files
	pop	eax
	call	switch_editor_instance
	mov	esi,[file_path]
	mov	edi,buffer
	mov	ebx,edi
      copy_directory_path:
	lodsb
	or	al,al
	jz	directory_path_ok
	stosb
	cmp	al,'\'
	jne	copy_directory_path
	mov	ebx,edi
	jmp	copy_directory_path
      directory_path_ok:
	mov	byte [ebx],0
	mov	ah,0Eh
	mov	dl,[buffer]
	sub	dl,'A'
	cmp	dl,'Z'-'A'
	jbe	directory_drive_ok
	sub	dl,'a'-'A'
      directory_drive_ok:
	int	21h
	xor	dx,dx
	mov	ax,713Bh
	call	dos_int
	cmp	ax,7100h
	jne	compilation_directory_ok
	mov	ah,3Bh
	call	dos_int
      compilation_directory_ok:
	mov	eax,[file_path]
	mov	[input_file],eax
	pop	eax
	call	switch_editor_instance
	mov	cx,1000h
	mov	ah,1
	int	10h
	mov	esi,_compile
	mov	cx,0316h
	mov	ah,[window_colors]
	call	draw_centered_window
	add	edi,2
	mov	[progress_offset],edi
	mov	ax,500h
	mov	edi,buffer
	int	31h
	mov	eax,[edi]
      allocate_memory:
	mov	ecx,eax
	mov	edx,eax
	shr	edx,2
	sub	eax,edx
	mov	[memory_end],eax
	mov	[additional_memory_end],edx
	call	get_memory
	or	eax,eax
	jnz	memory_allocated
	mov	eax,[additional_memory_end]
	shl	eax,1
	cmp	eax,4000h
	jb	not_enough_memory
	jmp	allocate_memory
      memory_allocated:
	mov	[allocated_memory],ebx
	mov	[memory_start],eax
	add	eax,[memory_end]
	mov	[memory_end],eax
	mov	[additional_memory],eax
	add	[additional_memory_end],eax
	xor	eax,eax
	mov	[output_file],eax
	mov	[display_length],eax
	mov	[stack_limit],stack_bottom
	mov	[passes_limit],100
	mov	ax,204h
	mov	bl,9
	int	31h
	mov	dword [keyboard_handler],edx
	mov	word [keyboard_handler+4],cx
	mov	ax,205h
	mov	bl,9
	mov	cx,cs
	mov	edx,aborting_handler
	int	31h
	mov	eax,[fs:6Ch]
	mov	[start_time],eax
	call	preprocessor
	call	draw_progress_segment
	call	parser
	call	draw_progress_segment
	call	assembler
	call	draw_progress_segment
	call	formatter
	call	draw_progress_segment
	mov	ax,205h
	mov	bl,9
	mov	edx,dword [keyboard_handler]
	mov	cx,word [keyboard_handler+4]
	int	31h
	test	[command_flags],1
	jnz	execute
	call	flush_display_buffer
	call	update_screen
	mov	edi,buffer
	movzx	eax,[current_pass]
	inc	eax
	call	number_as_text
	mov	eax,' pas'
	stosd
	mov	eax,'ses,'
	stosd
	mov	al,20h
	stosb
	mov	eax,[fs:6Ch]
	sub	eax,[start_time]
	mov	ebx,100
	mul	ebx
	mov	ebx,182
	div	ebx
	or	eax,eax
	jz	show_bytes_count
	xor	edx,edx
	mov	ebx,10
	div	ebx
	push	edx
	call	number_as_text
	mov	al,'.'
	stosb
	pop	eax
	call	number_as_text
	mov	eax,' sec'
	stosd
	mov	eax,'onds'
	stosd
	mov	ax,', '
	stosw
      show_bytes_count:
	mov	eax,[written_size]
	call	number_as_text
	mov	eax,' byt'
	stosd
	mov	eax,'es.'
	stosd
	mov	ebx,[allocated_memory]
	call	release_memory
	mov	esi,buffer
	mov	ebx,_compile
	movzx	eax,[message_box_colors]
	mov	[first_button],_ok
	mov	[second_button],_get_display
	cmp	[display_length],0
	je	show_compilation_summary
	or	eax,2 shl 24
      show_compilation_summary:
	call	message_box
	cmp	eax,2
	jb	main_loop
	cmp	[clipboard],0
	je	get_display_to_clipboard
	mov	ebx,[clipboard_handle]
	call	release_memory
      get_display_to_clipboard:
	mov	ecx,[display_length]
	inc	ecx
	call	get_memory
	mov	[clipboard_handle],ebx
	mov	[clipboard],eax
	or	eax,eax
	jz	not_enough_memory
	xor	esi,esi
	mov	edi,eax
	mov	ecx,[display_length]
	push	ds
	mov	ds,[low_memory_selector]
	rep	movsb
	pop	ds
	xor	al,al
	stosb
	jmp	main_loop
    execute:
	mov	edx,[output_file]
	call	adapt_path
	mov	ebx,[allocated_memory]
	call	release_memory
	cmp	[output_format],3
	ja	cannot_execute
	call	release_low_memory
	call	switch_to_user_screen
	call	close_video
	mov	edi,buffer+200h
	lea	edx,[edi-buffer]
	mov	ax,0D00h
	stosw
	lea	ebx,[edi-buffer]
	xor	eax,eax
	stosw
	mov	ax,buffer_segment
	shl	eax,16
	mov	ax,dx
	stosd
	xor	eax,eax
	stosd
	stosd
	mov	ax,4B00h
	xor	dx,dx
	call	dos_int
	call	init_video
	jc	startup_failed
	call	switch_to_ide_screen
	call	get_low_memory
	jmp	main_loop
      cannot_execute:
	mov	esi,_not_executable
	mov	ebx,_error
	movzx	eax,[error_box_colors]
	call	message_box
	jmp	main_loop
    draw_progress_segment:
	mov	eax,[progress_offset]
	mov	ecx,4
      draw_progress_filler:
	mov	byte [gs:eax],254
	add	eax,2
	loop	draw_progress_filler
	mov	[progress_offset],eax
	ret
    aborting_handler:
	push	eax
	in	al,60h
	cmp	al,1
	jne	no_abort
	mov	dword [esp+4],abort_compiling
	mov	word [esp+4+4],cs
      no_abort:
	pop	eax
	jmp	pword [cs:keyboard_handler]
      abort_compiling:
	cli
	mov	ax,[cs:main_selector]
	mov	esp,stack_top
	mov	ss,ax
	mov	ds,ax
	mov	es,ax
	mov	fs,[bios_selector]
	mov	gs,[video_selector]
	mov	ax,205h
	mov	bl,9
	mov	edx,dword [keyboard_handler]
	mov	cx,word [keyboard_handler+4]
	int	31h
	sti
      discard_keyboard_buffer:
	mov	ah,11h
	int	16h
	jz	keyboard_buffer_ok
	mov	ah,10h
	int	16h
	jmp	discard_keyboard_buffer
      keyboard_buffer_ok:
	mov	ebx,[allocated_memory]
	call	release_memory
	jmp	main_loop
  assign_to_compiler:
	mov	eax,[editor_memory]
	xchg	eax,[main_project_file]
	cmp	eax,[editor_memory]
	jne	main_loop
	mov	[main_project_file],0
	jmp	main_loop
  options:
	push	[editor_style]
	call	options_dialog
	pop	eax
	jnc	main_loop
	mov	[editor_style],eax
	jmp	main_loop

  startup_failed:
	mov	esi,_startup_failed
      error_message_loop:
	lodsb
	or	al,al
	jz	error_message_ok
	mov	dl,al
	mov	ah,2
	int	21h
	jmp	error_message_loop
      error_message_ok:
	mov	ax,4C0Fh
	int	21h

  exit:
	call	switch_to_user_screen
	mov	ax,4C00h
	int	21h

  init_video:
	call	check_video_mode
	mov	bx,gs
	movzx	edx,byte [fs:4Ah]
	mov	[screen_width],edx
	shl	edx,1
	mov	[video_pitch],edx
	movzx	eax,byte [fs:84h]
	inc	eax
	mov	[screen_height],eax
	imul	edx,eax
	push	edx
	dec	edx
	shld	ecx,edx,16
	mov	ax,8
	int	31h
	jc	video_init_failed
	pop	ecx
	call	get_memory
	jc	video_init_failed
	mov	[video_storage],eax
	mov	[video_storage_handle],ebx
	clc
	ret
    video_init_failed:
	stc
	ret
    check_video_mode:
	test	byte [fs:65h],110b
	jnz	set_video_mode
	cmp	byte [fs:4Ah],80
	jb	set_video_mode
	ret
    set_video_mode:
	mov	ax,3
	int	10h
	ret
  close_video:
	mov	ebx,[video_storage_handle]
	call	release_memory
	ret
  switch_to_ide_screen:
	call	check_video_mode
	xor	esi,esi
	mov	edi,[video_storage]
	mov	ecx,[video_pitch]
	imul	ecx,[screen_height]
	rep	movs byte [es:edi],[gs:esi]
	mov	ah,3
	xor	bh,bh
	int	10h
	mov	[stored_cursor],cx
	mov	[stored_cursor_position],dx
	mov	ah,0Fh
	int	10h
	mov	[stored_page],bh
	mov	ax,0500h
	int	10h
	mov	al,[fs:65h]
	mov	[stored_mode],al
	mov	ax,1003h
	xor	bx,bx
	int	10h
	mov	eax,[screen_width]
	mov	[window_width],eax
	mov	eax,[screen_height]
	sub	eax,2
	mov	[window_height],eax
	call	update_window
	ret
  switch_to_user_screen:
	push	es gs
	pop	es
	mov	esi,[video_storage]
	xor	edi,edi
	mov	ecx,[video_pitch]
	imul	ecx,[screen_height]
	rep	movs byte [es:edi],[ds:esi]
	mov	ah,1
	mov	cx,[stored_cursor]
	int	10h
	mov	ah,2
	xor	bh,bh
	mov	dx,[stored_cursor_position]
	int	10h
	mov	ah,5
	mov	al,[stored_page]
	int	10h
	mov	ax,1003h
	xor	bh,bh
	mov	bl,[stored_mode]
	shr	bl,5
	and	bl,1
	int	10h
	pop	es
	ret

  create_editor_instance:
	call	flush_editor_data
	mov	esi,[editor_memory]
    find_last_instance:
	mov	eax,[esi+16+next_instance-editor_data]
	or	eax,eax
	jz	attach_new_instance
	mov	esi,eax
	jmp	find_last_instance
    attach_new_instance:
	push	esi
	call	init_editor_memory
	pop	esi
	jc	not_enough_memory
	mov	eax,[editor_memory]
	xchg	eax,[esi+16+next_instance-editor_data]
	mov	[next_instance],eax
	mov	[previous_instance],esi
	xor	eax,eax
	mov	[file_path],eax
     flush_editor_data:
	mov	edi,[editor_memory]
	add	edi,16
	mov	esi,editor_data
	mov	ecx,editor_data_size
	rep	movsb
	ret
  switch_editor_instance:
	call	flush_editor_data
	cmp	eax,[editor_memory]
	je	editor_switch_ok
	mov	[editor_memory],eax
	lea	esi,[eax+16]
	mov	edi,editor_data
	mov	ecx,editor_data_size
	rep	movsb
    editor_switch_ok:
	call	update_positions
	ret
  remove_editor_instance:
	mov	eax,[editor_memory]
	xor	eax,[main_project_file]
	jnz	main_project_file_ok
	mov	[main_project_file],eax
    main_project_file_ok:
	mov	esi,[previous_instance]
	mov	edi,[next_instance]
	mov	eax,edi
	or	edi,edi
	jz	next_instance_links_ok
	mov	[edi+16+previous_instance-editor_data],esi
    next_instance_links_ok:
	or	esi,esi
	jz	previous_instance_links_ok
	mov	[esi+16+next_instance-editor_data],edi
	mov	eax,esi
    previous_instance_links_ok:
	push	eax
	call	release_editor_memory
	mov	eax,[file_path]
	or	eax,eax
	jz	file_path_released
	mov	ebx,[file_path_handle]
	call	release_memory
    file_path_released:
	pop	eax
	or	eax,eax
	jz	no_instance_left
	call	switch_editor_instance
	clc
	ret
    no_instance_left:
	stc
	ret

  load_file:
	push	edx [editor_memory]
	call	get_full_pathname
	push	esi
	call	flush_editor_data
	mov	esi,[esp]
	mov	edx,[editor_memory]
      prepare_to_scan_editors:
	mov	eax,[edx+16+previous_instance-editor_data]
	or	eax,eax
	jz	scan_editors
	mov	edx,eax
	jmp	prepare_to_scan_editors
      scan_editors:
	mov	edi,[edx+16+file_path-editor_data]
	or	edi,edi
	jz	scan_next_editor
	xor	ecx,ecx
	mov	ebx,case_table
      compare_pathnames:
	mov	al,[esi+ecx]
	xlatb
	mov	ah,al
	mov	al,[edi+ecx]
	xlatb
	cmp	al,ah
	jne	scan_next_editor
	or	al,ah
	jz	file_found
	inc	ecx
	jmp	compare_pathnames
      scan_next_editor:
	mov	edx,[edx+16+next_instance-editor_data]
	or	edx,edx
	jnz	scan_editors
	mov	eax,[undo_data]
	or	eax,[file_path]
	jz	open_in_current_instance
	call	create_editor_instance
      open_in_current_instance:
	pop	esi
	call	use_pathname
	mov	edx,[file_path]
	call	open
	jc	load_failed
	xor	edx,edx
	mov	al,2
	call	lseek
	push	eax ebx
	lea	ecx,[eax+1]
	call	get_memory
	or	eax,eax
	jz	not_enough_memory
	mov	esi,eax
	mov	edi,ecx
	pop	ebx
	xor	edx,edx
	xor	al,al
	call	lseek
	pop	ecx
	mov	edx,esi
	mov	byte [edx+ecx],0
	call	read
	jc	load_failed
	call	close
	push	edi
	call	set_text
	pop	ebx
	call	release_memory
	add	esp,8
	ret
      file_found:
	mov	eax,edx
	call	switch_editor_instance
	pop	esi
	add	esp,8
	ret
    load_failed:
	mov	eax,[previous_instance]
	or	eax,[next_instance]
	jnz	cancel_instance
      cancel_path_only:
	xor	ebx,ebx
	xchg	ebx,[file_path]
	call	release_memory
	jmp	editor_cancelled
      cancel_instance:
	mov	eax,[editor_memory]
	cmp	eax,[esp]
	je	cancel_path_only
	call	remove_editor_instance
      editor_cancelled:
	pop	eax
	call	switch_editor_instance
	call	update_screen
	mov	edi,buffer
	mov	esi,_loading_error
	mov	ebx,esp
	call	sprintf
	pop	eax
	mov	esi,buffer
	mov	ebx,_error
	movzx	eax,[error_box_colors]
	call	message_box
	ret
  get_full_pathname:
	call	adapt_path
	sub	edi,buffer
	mov	ax,7160h
	xor	cx,cx
	xor	si,si
	stc
	call	dos_int
	jnc	got_full_pathname
	mov	ah,60h
	call	dos_int
	jc	full_pathname_exit
      got_full_pathname:
	lea	esi,[buffer + edi]
	cmp	word [esi],'\\'
	jne	full_pathname_ok
	cmp	dword [esi+3],'.\A.'
	jne	full_pathname_ok
	mov	al,[esi+2]
	mov	ah,':'
	add	esi,5
	mov	[esi],ax
      full_pathname_ok:
	clc
      full_pathname_exit:
	ret
  use_pathname:
	mov	edi,[file_path]
	or	edi,edi
	jnz	copy_pathname
	mov	ecx,1000h
	push	esi
	call	get_memory
	pop	esi
	or	eax,eax
	jz	not_enough_memory
	mov	edi,eax
	mov	[file_path],edi
	mov	[file_path_handle],ebx
      copy_pathname:
	lodsb
	stosb
	or	al,al
	jnz	copy_pathname
	ret
  save_file:
	mov	edx,[file_path]
	call	create
	jc	file_creation_error
	mov	[file_handle],ebx
	mov	esi,[first_line]
	mov	edi,[line_buffer]
    copy_text:
	mov	ecx,[esi+8]
	lea	eax,[edi+ecx]
	mov	edx,[line_buffer_size]
	shr	edx,1
	sub	eax,edx
	cmp	eax,[line_buffer]
	ja	flush_to_file
	mov	ebp,edi
	xor	edx,edx
	push	ecx
	call	copy_from_line
	pop	ecx
	test	[editor_style],AES_OPTIMALFILL
	jz	line_copied
	cmp	ecx,8
	jb	line_copied
	push	esi edi ecx
	mov	esi,ebp
	mov	edi,[line_buffer]
	mov	eax,[line_buffer_size]
	shr	eax,1
	add	edi,eax
	push	edi
	mov	ecx,[esp+4]
	xor	al,al
	rep	stosb
	mov	ecx,[esp+4]
	mov	edi,[esp]
	call	syntax_proc
	pop	ebx ecx edi
	mov	esi,ebp
	mov	edi,ebp
	sub	ebx,esi
	xor	edx,edx
    optimal_fill:
	lodsb
	cmp	al,20h
	jne	store_character
	cmp	byte [esi-1+ebx],0
	jne	store_character
	mov	eax,esi
	sub	eax,ebp
	test	eax,111b
	jz	store_tab
	inc	edx
	mov	al,20h
	stosb
	loop	optimal_fill
	jmp	optimal_fill_done
    store_tab:
	mov	al,20h
	or	edx,edx
	jz	store_character
	sub	edi,edx
	mov	al,9
    store_character:
	stosb
	xor	edx,edx
	loop	optimal_fill
    optimal_fill_done:
	pop	esi
	jmp	line_copied
    line_copied:
	or	esi,esi
	jz	flush_to_file
	mov	ax,0A0Dh
	stosw
	jmp	copy_text
    flush_to_file:
	push	esi
	mov	edx,[line_buffer]
	mov	ecx,edi
	sub	ecx,edx
	mov	ebx,[file_handle]
	call	write
	jc	file_writing_error
	pop	esi
	mov	edi,[line_buffer]
	or	esi,esi
	jnz	copy_text
	mov	ebx,[file_handle]
	call	close
	call	clear_undo_data
	clc
	ret
     file_writing_error:
	add	esp,4
     file_creation_error:
	call	update_screen
	mov	esi,_saving_error
	mov	ebx,_error
	movzx	eax,[error_box_colors]
	call	message_box
	stc
	ret
  get_saving_path:
	mov	esi,_save_as
	call	file_open_dialog
	jc	saving_aborted
	push	edx
	call	open
	pop	edx
	jc	saving_allowed
	push	edx
	call	close
	call	update_screen
	mov	edi,buffer
	mov	esi,_overwrite_question
	mov	ebx,esp
	call	sprintf
	mov	esi,buffer
	mov	ebx,_save_as
	mov	eax,2 shl 24
	mov	ax,[message_box_colors]
	mov	[first_button],_yes
	mov	[second_button],_no
	call	message_box
	pop	edx
	cmp	eax,1
	jb	saving_aborted
	je	saving_allowed
	call	update_screen
	jmp	get_saving_path
     saving_allowed:
	call	get_full_pathname
	jc	invalid_saving_path
	call	use_pathname
	clc
	ret
     invalid_saving_path:
	call	update_screen
	mov	esi,_invalid_path
	mov	ebx,_error
	movzx	eax,[error_box_colors]
	call	message_box
     saving_aborted:
	stc
	ret

  update_positions:
	mov	eax,[screen_width]
	mov	[window_width],eax
	mov	eax,[screen_height]
	sub	eax,2
	mov	[window_height],eax
	call	update_window
	ret
  update_cursor:
	xor	bh,bh
	mov	edx,[caret_position]
	sub	edx,[window_position]
	jc	cursor_out_of_sight
	cmp	edx,[window_width]
	jae	cursor_out_of_sight
	mov	eax,[caret_line_number]
	sub	eax,[window_line_number]
	jc	cursor_out_of_sight
	cmp	eax,[window_height]
	jae	cursor_out_of_sight
	inc	al
	mov	dh,al
	mov	ah,2
	int	10h
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	block_cursor
	mov	ah,1
	mov	cx,0D0Eh
	int	10h
	ret
    block_cursor:
	mov	ah,1
	mov	cx,000Fh
	int	10h
	ret
    cursor_out_of_sight:
	mov	ah,1
	mov	cx,1000h
	int	10h
	ret

; Text drawing

  update_screen:
	call	update_title_bar
	mov	eax,[peak_line_length]
	xor	edx,edx
	mov	ebx,SEGMENT_DATA_LENGTH
	div	ebx
	inc	eax
	mul	ebx
	shl	eax,1
	mov	ecx,eax
	cmp	[line_buffer],0
	je	line_buffer_reallocate
	cmp	ecx,[line_buffer_size]
	jbe	line_buffer_ok
	mov	[line_buffer],0
	push	ecx
	mov	ebx,[line_buffer_handle]
	call	release_memory
	pop	ecx
    line_buffer_reallocate:
	call	get_memory
	or	eax,eax
	jz	memory_shortage
	mov	[line_buffer],eax
	mov	[line_buffer_handle],ebx
    line_buffer_ok:
	mov	esi,[window_line]
	mov	edx,[window_line_number]
	mov	eax,[video_pitch]
	mov	[screen_offset],eax
	mov	edi,[line_buffer]
    prepare_line:
	add	esi,SEGMENT_HEADER_LENGTH
	mov	ecx,SEGMENT_DATA_LENGTH
	rep	movsb
	mov	esi,[esi-SEGMENT_LENGTH]
	btr	esi,0
	jc	prepare_line
	push	esi edx
	mov	ecx,edi
	mov	esi,[line_buffer]
	sub	ecx,esi
	push	ecx
	mov	al,[text_colors]
	and	al,0Fh
	rep	stosb
	mov	esi,[line_buffer]
	mov	ecx,[esp]
	lea	edi,[esi+ecx]
	call	syntax_proc
    line_prepared:
	mov	edi,screen_row_buffer
	mov	ecx,[window_width]
	mov	al,20h
	mov	ah,[text_colors]
	rep	stosw
	pop	ecx
	mov	esi,[line_buffer]
	lea	ebx,[esi+ecx]
	add	esi,[window_position]
	add	ebx,[window_position]
	sub	ecx,[window_position]
	jbe	text_drawing_ok
	mov	edi,screen_row_buffer
	cmp	ecx,[window_width]
	jbe	draw_text
	mov	ecx,[window_width]
    draw_text:
	movsb
	mov	al,[text_colors]
	and	al,0F0h
	or	al,[ebx]
	stosb
	inc	ebx
	loop	draw_text
    text_drawing_ok:
	mov	edi,screen_row_buffer
	cmp	[selection_line],0
	je	selection_marked
	mov	eax,[selection_line_number]
	cmp	eax,[caret_line_number]
	jne	mark_multiline_selection
	cmp	eax,[esp]
	jne	selection_marked
    mark_simple_selection:
	mov	eax,[selection_position]
	mov	ecx,[caret_position]
	cmp	eax,ecx
	jbe	simple_selection_boundaries_ok
	xchg	eax,ecx
    simple_selection_boundaries_ok:
	sub	ecx,[window_position]
	jbe	selection_marked
	sub	eax,[window_position]
	jae	simple_selection_start_ok
	xor	eax,eax
    simple_selection_start_ok:
	cmp	ecx,[window_width]
	jbe	simple_selection_length_ok
	mov	ecx,[window_width]
    simple_selection_length_ok:
	sub	ecx,eax
	jbe	selection_marked
	lea	edi,[screen_row_buffer+eax*2]
    draw_selection:
	inc	edi
	mov	al,[selection_colors]
	stosb
	loop	draw_selection
	jmp	selection_marked
    mark_multiline_selection:
	test	[editor_mode],AEMODE_VERTICALSEL
	jnz	mark_vertical_selection
	mov	eax,[selection_line_number]
	mov	ebx,[selection_position]
	mov	edx,[caret_line_number]
	mov	ebp,[caret_position]
	cmp	eax,edx
	jbe	multiline_selection_boundaries_ok
	xchg	eax,edx
	xchg	ebx,ebp
    multiline_selection_boundaries_ok:
	mov	edi,screen_row_buffer
	mov	ecx,[window_width]
	cmp	eax,[esp]
	ja	selection_marked
	je	mark_selection_start
	cmp	edx,[esp]
	ja	draw_selection
	jb	selection_marked
    mark_selection_end:
	cmp	ebp,[window_position]
	jbe	selection_marked
	sub	ebp,[window_position]
	cmp	ecx,ebp
	jbe	draw_selection
	mov	ecx,ebp
	jmp	draw_selection
    mark_selection_start:
	sub	ebx,[window_position]
	jbe	draw_selection
	sub	ecx,ebx
	jbe	selection_marked
	lea	edi,[edi+ebx*2]
	jmp	draw_selection
    mark_vertical_selection:
	mov	eax,[selection_line_number]
	mov	edx,[caret_line_number]
	sub	eax,[esp]
	jz	mark_simple_selection
	sub	edx,[esp]
	jz	mark_simple_selection
	xor	eax,edx
	js	mark_simple_selection
    selection_marked:
	push	es gs
	pop	es
	mov	esi,screen_row_buffer
	mov	edi,[screen_offset]
	mov	ecx,[window_width]
	shr	ecx,1
	rep	movsd
	setc	cl
	rep	movsw
	mov	eax,[window_width]
	shl	eax,1
	sub	edi,eax
	add	edi,[video_pitch]
	mov	[screen_offset],edi
	pop	es
	pop	edx esi
	inc	edx
	mov	eax,edx
	sub	eax,[window_line_number]
	cmp	eax,[window_height]
	jae	screen_ok
	mov	edi,[line_buffer]
	or	esi,esi
	jnz	prepare_line
	push	esi edx
	push	[window_position]
	jmp	line_prepared
    screen_ok:
	call	update_status_bar
	ret

  syntax_proc:
	mov	ebx,characters
	xor	edx,edx
    scan_syntax:
	lodsb
    check_character:
	cmp	al,20h
	je	syntax_space
	cmp	al,3Bh
	je	syntax_comment
	mov	ah,al
	xlatb
	or	al,al
	jz	syntax_symbol
	or	edx,edx
	jnz	syntax_neutral
	cmp	ah,27h
	je	syntax_string
	cmp	ah,22h
	je	syntax_string
	cmp	ah,24h
	je	syntax_pascal_hex
	cmp	ah,39h
	ja	syntax_neutral
	cmp	ah,30h
	jae	syntax_number
    syntax_neutral:
	or	edx,-1
	inc	edi
	loop	scan_syntax
	jmp	syntax_done
    syntax_space:
	xor	edx,edx
	inc	edi
	loop	scan_syntax
	jmp	syntax_done
    syntax_symbol:
	mov	al,[symbol_color]
	stosb
	xor	edx,edx
	loop	scan_syntax
	jmp	syntax_done
    syntax_pascal_hex:
	cmp	ecx,1
	je	syntax_neutral
	mov	al,[esi]
	mov	ah,al
	xlatb
	or	al,al
	jz	syntax_neutral
	cmp	ah,24h
	jne	syntax_number
	cmp	ecx,2
	je	syntax_neutral
	mov	al,[esi+1]
	xlatb
	or	al,al
	jz	syntax_neutral
    syntax_number:
	mov	al,[number_color]
	stosb
	loop	number_character
	jmp	syntax_done
    number_character:
	lodsb
	mov	ah,al
	xlatb
	xchg	al,ah
	or	ah,ah
	jz	check_character
	cmp	al,20h
	je	check_character
	cmp	al,3Bh
	je	check_character
	mov	al,[number_color]
	stosb
	loop	number_character
	jmp	syntax_done
    syntax_string:
	mov	al,[string_color]
	stosb
	dec	ecx
	jz	syntax_done
	lodsb
	cmp	al,ah
	jne	syntax_string
	mov	al,[string_color]
	stosb
	dec	ecx
	jz	syntax_done
	lodsb
	cmp	al,ah
	je	syntax_string
	xor	edx,edx
	jmp	check_character
    syntax_comment:
	mov	al,[comment_color]
	stosb
	loop	syntax_comment
    syntax_done:
	ret

; Status drawing

  update_status_bar:
	mov	edi,screen_row_buffer
	mov	al,20h
	mov	ecx,[screen_width]
	rep	stosb
	mov	edi,screen_row_buffer+256
	mov	eax,'Row '
	stosd
	mov	eax,[caret_line_number]
	call	number_as_text
	mov	al,'/'
	stosb
	mov	eax,[lines_count]
	call	number_as_text
	mov	eax,', Co'
	stosd
	mov	eax,'lumn'
	stosd
	mov	al,' '
	stosb
	mov	eax,[caret_position]
	inc	eax
	call	number_as_text
	mov	al,'/'
	stosb
	mov	eax,[maximum_position]
	inc	eax
	call	number_as_text
	mov	ecx,edi
	mov	esi,screen_row_buffer+256
	sub	ecx,esi
	mov	eax,[screen_width]
	lea	edi,[screen_row_buffer+eax-1]
	sub	edi,ecx
	mov	byte [edi-2],0B3h
	lea	ebx,[edi-11]
	rep	movsb
	mov	edi,ebx
	sub	ebx,2
	mov	byte [ebx],0B3h
	push	ebx
	cmp	[undo_data],0
	je	modified_status_ok
	mov	eax,'Modi'
	stosd
	mov	eax,'fied'
	stosd
     modified_status_ok:
	mov	edi,screen_row_buffer+1
	xor	eax,eax
	mov	edx,[previous_instance]
     count_preceding_instances:
	inc	eax
	or	edx,edx
	jz	preceding_instances_count_ok
	mov	edx,[edx+16+previous_instance-editor_data]
	jmp	count_preceding_instances
     preceding_instances_count_ok:
	push	eax
	call	number_as_text
	mov	al,'/'
	stosb
	pop	eax
	mov	edx,[next_instance]
     count_following_instances:
	or	edx,edx
	jz	following_instances_count_ok
	inc	eax
	mov	edx,[edx+16+next_instance-editor_data]
	jmp	count_following_instances
     following_instances_count_ok:
	call	number_as_text
	inc	edi
	mov	al,'-'
	stosb
	inc	edi
	pop	ecx
	sub	ecx,edi
	mov	esi,[file_path]
	call	get_file_title
      print_file_title:
	lodsb
	or	al,al
	jz	editor_title_ok
	stosb
	loop	print_file_title
      editor_title_ok:
	push	es gs
	pop	es
	mov	edi,[video_pitch]
	mov	eax,[screen_height]
	dec	eax
	imul	edi,eax
	mov	esi,screen_row_buffer
	mov	ecx,[screen_width]
	mov	ah,[status_colors]
     draw_status_bar:
	lodsb
	stosw
	loop	draw_status_bar
	pop	es
	ret
     get_file_title:
	or	esi,esi
	jz	untitled
	mov	ebx,esi
      find_file_name:
	lodsb
	or	al,al
	jz	file_title_ok
	cmp	al,'\'
	jne	find_file_name
	mov	ebx,esi
	jmp	find_file_name
      file_title_ok:
	mov	esi,ebx
	ret
      untitled:
	mov	esi,_untitled
	ret
  update_title_bar:
	mov	edi,screen_row_buffer
	mov	al,20h
	mov	ecx,[screen_width]
	sub	ecx,10+1
	rep	stosb
	mov	al,0B3h
	stosb
	mov	esi,_caption
	mov	edi,screen_row_buffer+1
     draw_caption:
	lodsb
	or	al,al
	jz	caption_ok
	stosb
	jmp	draw_caption
     caption_ok:
	mov	edx,[main_project_file]
	or	edx,edx
	jz	main_file_title_ok
	mov	al,20h
	stosb
	mov	al,'-'
	stosb
	mov	al,20h
	stosb
	mov	esi,[file_path]
	cmp	edx,[editor_memory]
	je	get_main_file_title
	mov	esi,[edx+16+file_path-editor_data]
     get_main_file_title:
	call	get_file_title
	mov	ecx,[screen_width]
	add	ecx,screen_row_buffer-14
	sub	ecx,edi
     print_main_file_title:
	lodsb
	or	al,al
	jz	main_file_title_ok
	stosb
	loop	print_main_file_title
     main_file_title_ok:
	push	es gs
	pop	es
	xor	edi,edi
	mov	esi,screen_row_buffer
	mov	ecx,[screen_width]
	sub	ecx,10
	mov	ah,[status_colors]
     draw_title_bar:
	lodsb
	stosw
	loop	draw_title_bar
	pop	es
	call	update_clock
	ret
  update_clock:
	mov	edi,[screen_width]
	mov	ecx,10
	sub	edi,ecx
	shl	edi,1
	mov	al,[status_colors]
     prepare_clock_colors:
	mov	[gs:edi+(ecx-1)*2+1],al
	loop	prepare_clock_colors
	mov	ah,2Ch
	int	21h
	mov	[gs:edi],byte 20h
	mov	al,ch
	aam
	add	ax,'00'
	mov	[gs:edi+1*2],ah
	mov	[gs:edi+2*2],al
	mov	[gs:edi+3*2],byte ':'
	mov	al,cl
	aam
	add	ax,'00'
	mov	[gs:edi+4*2],ah
	mov	[gs:edi+5*2],al
	mov	[gs:edi+6*2],byte ':'
	mov	al,dh
	aam
	add	ax,'00'
	mov	[gs:edi+7*2],ah
	mov	[gs:edi+8*2],al
	mov	[gs:edi+9*2],byte 20h
	ret
  wait_for_input:
	call	update_clock
	mov	ah,11h
	int	16h
	jz	wait_for_input
	mov	ah,10h
	int	16h
	ret

  number_as_text:
	push	ebx
	mov	ecx,1000000000
	xor	edx,edx
	xor	bl,bl
      number_loop:
	div	ecx
	push	edx
	cmp	ecx,1
	je	store_digit
	or	bl,bl
	jnz	store_digit
	or	al,al
	jz	digit_ok
	not	bl
      store_digit:
	add	al,'0'
	stosb
      digit_ok:
	mov	eax,ecx
	xor	edx,edx
	mov	ecx,10
	div	ecx
	mov	ecx,eax
	pop	eax
	or	ecx,ecx
	jnz	number_loop
	pop	ebx
	ret
  sprintf:
	lodsb
	cmp	al,'%'
	je	format_parameter
	stosb
	or	al,al
	jnz	sprintf
	ret
    format_parameter:
	lodsb
	mov	edx,[ebx]
	add	ebx,4
	cmp	al,'s'
	je	insert_string
	cmp	al,'d'
	je	insert_number
	or	al,al
	jnz	sprintf
	dec	esi
	jmp	sprintf
    insert_number:
	push	esi
	mov	eax,edx
	call	number_as_text
	pop	esi
	jmp	sprintf
    insert_string:
	push	esi
	mov	esi,edx
    string_insertion_loop:
	lodsb
	or	al,al
	jz	string_insertion_ok
	stosb
	jmp	string_insertion_loop
    string_insertion_ok:
	pop	esi
	jmp	sprintf

; Windows

  message_box:
	push	eax ebx
	xor	ebp,ebp
	mov	edi,buffer+200h
	mov	dl,[esp+4+3]
	or	dl,dl
	jnz	calculate_buttons_width
	mov	dl,1
	mov	[first_button],_ok
    calculate_buttons_width:
	mov	[buttons_width],0
	mov	eax,[first_button]
	call	add_button_width
	dec	dl
	jz	buttons_width_ok
	add	[buttons_width],2
	mov	eax,[second_button]
	call	add_button_width
	jmp	buttons_width_ok
    add_button_width:
	push	edi
	mov	edi,eax
	xor	al,al
	or	ecx,-1
	repne	scasb
	neg	cl
	add	cl,6-2
	add	[buttons_width],cl
	pop	edi
	ret
    buttons_width_ok:
	mov	al,[buttons_width]
	mov	[message_width],al
    start_message_line:
	mov	ebx,edi
	mov	edx,edi
	mov	ecx,[screen_width]
	sub	ecx,14
	mov	eax,[screen_height]
	sub	eax,7
	inc	ebp
	cmp	ebp,eax
	jae	message_prepared
    copy_message:
	lodsb
	stosb
	cmp	al,20h
	jne	message_character_ok
	mov	edx,edi
    message_character_ok:
	cmp	byte [esi],0
	je	message_prepared
	loop	copy_message
	cmp	edx,ebx
	je	split_word
	lea	eax,[edx-1]
	mov	ecx,[screen_height]
	sub	ecx,7
	inc	ebp
	cmp	ebp,ecx
	je	cut_message
	mov	byte [eax],0Ah
	sub	eax,ebx
	mov	ebx,edx
	mov	ecx,[screen_width]
	lea	ecx,[ecx-14+ebx]
	sub	ecx,edi
	cmp	al,[message_width]
	jbe	copy_message
	mov	[message_width],al
	jmp	copy_message
    cut_message:
	mov	edi,eax
	jmp	message_prepared
    split_word:
	mov	eax,[screen_width]
	sub	eax,14
	mov	[message_width],al
	mov	al,0Ah
	stosb
	jmp	start_message_line
    message_prepared:
	mov	eax,edi
	sub	eax,ebx
	cmp	al,[message_width]
	jbe	message_width_ok
	mov	[message_width],al
    message_width_ok:
	xor	al,al
	stosb
	mov	ecx,ebp
	mov	ch,cl
	add	ch,6
	mov	cl,[message_width]
	add	cl,8
	pop	esi
	mov	ah,[esp+1]
	call	draw_centered_window
	mov	esi,buffer+200h
	add	edi,2*2
    draw_message:
	xor	ecx,ecx
	add	edi,[video_pitch]
    draw_message_row:
	lodsb
	or	al,al
	jz	message_drawn
	cmp	al,0Ah
	je	draw_message
	mov	byte [gs:edi+ecx*2],al
	inc	ecx
	jmp	draw_message_row
    message_drawn:
	add	edi,[video_pitch]
	add	edi,[video_pitch]
	mov	cx,1000h
	mov	ah,1
	int	10h
	movzx	eax,[message_width]
	sub	al,[buttons_width]
	and	al,not 1
	add	edi,eax
	cmp	byte [esp+3],2
	jae	two_button_message
	pop	eax
	mov	ah,al
	mov	esi,[first_button]
	call	draw_button
    wait_for_ok:
	call	wait_for_input
	cmp	ah,1
	je	message_aborted
	cmp	al,20h
	je	message_ok
	cmp	al,0Dh
	jne	wait_for_ok
    message_ok:
	mov	eax,1
	ret
    message_aborted:
	xor	eax,eax
	ret
    two_button_message:
	pop	edx
	xor	cl,cl
	test	edx,10000h
	jnz	two_button_loop
	mov	cl,-1
    two_button_loop:
	push	edi
	mov	al,dl
	and	al,cl
	mov	ah,cl
	not	ah
	and	ah,dh
	or	ah,al
	mov	esi,[first_button]
	call	draw_button
	add	edi,2*2
	mov	al,dh
	and	al,cl
	mov	ah,cl
	not	ah
	and	ah,dl
	or	ah,al
	mov	esi,[second_button]
	call	draw_button
	push	ecx edx
	call	wait_for_input
	pop	edx ecx edi
	cmp	ah,1
	je	message_aborted
	cmp	al,9
	je	two_button_switch
	cmp	ah,0Fh
	je	two_button_switch
	cmp	ah,4Bh
	je	two_button_switch
	cmp	ah,4Dh
	je	two_button_switch
	cmp	ah,48h
	je	two_button_switch
	cmp	ah,50h
	je	two_button_switch
	cmp	al,0Dh
	je	button_selected
	cmp	al,20h
	jne	two_button_loop
    button_selected:
	or	cl,cl
	jnz	message_ok
	mov	eax,2
	ret
    two_button_switch:
	not	cl
	jmp	two_button_loop
    draw_button:
	push	es gs
	pop	es
	mov	al,'['
	stosw
	mov	al,20h
	stosw
	stosw
    draw_button_label:
	lodsb
	or	al,al
	jz	button_label_ok
	stosw
	jmp	draw_button_label
    button_label_ok:
	mov	al,20h
	stosw
	stosw
	mov	al,']'
	stosw
	pop	es
	ret
  draw_centered_window:
	mov	dl,byte [screen_width]
	sub	dl,cl
	shr	dl,1
	mov	dh,byte [screen_height]
	sub	dh,ch
	shr	dh,1
  draw_window:
	push	es gs
	pop	es
	movzx	edi,dh
	imul	edi,[video_pitch]
	movzx	ebx,dl
	shl	ebx,1
	add	edi,ebx
	movzx	edx,ch
	movzx	ecx,cl
	sub	ecx,4
	sub	edx,2
	push	ecx edi
	mov	al,' '
	stos	word [edi]
	mov	al,'É'
	stos	word [edi]
	mov	al,' '
	stos	word [edi]
	dec	ecx
      draw_title:
	lods	byte [esi]
	or	al,al
	jz	title_ok
	stos	word [edi]
	loop	draw_title
	jmp	finish_upper_border
      title_ok:
	mov	al,' '
	stos	word [edi]
	dec	ecx
	jz	finish_upper_border
	mov	al,'Í'
	rep	stos word [edi]
      finish_upper_border:
	mov	al,'»'
	stos	word [edi]
	mov	al,' '
	stos	word [edi]
	pop	edi ecx
	add	edi,[video_pitch]
	lea	ebp,[edi+2*2]
      draw_window_lines:
	push	ecx edi
	mov	al,' '
	stos	word [edi]
	mov	al,'º'
	stos	word [edi]
	mov	al,' '
	rep	stos word [edi]
	mov	al,'º'
	stos	word [edi]
	mov	al,' '
	stos	word [edi]
	mov	byte [es:edi+1],8
	mov	byte [es:edi+3],8
	pop	edi ecx
	add	edi,[video_pitch]
	dec	edx
	jnz	draw_window_lines
	push	ecx edi
	mov	al,' '
	stos	word [edi]
	mov	al,'È'
	stos	word [edi]
	mov	al,'Í'
	rep	stos word [edi]
	mov	al,'¼'
	stos	word [edi]
	mov	al,' '
	stos	word [edi]
	mov	byte [es:edi+1],8
	mov	byte [es:edi+3],8
	pop	edi ecx
	mov	eax,[video_pitch]
	lea	edi,[edi+eax+2*2+1]
	mov	al,8
	add	ecx,4
      finish_bottom_shadow:
	stos	byte [edi]
	inc	edi
	loop	finish_bottom_shadow
	mov	edi,ebp
	mov	eax,edi
	cdq
	idiv	[video_pitch]
	sar	edx,1
	mov	dh,al
	pop	es
	ret
  create_edit_box:
	mov	[ebx],dx
	xchg	ax,cx
	and	ah,0Fh
	mov	[ebx+2],ax
	mov	[ebx+4],ecx
	xor	eax,eax
	mov	[ebx+8],eax
	mov	[ebx+12],eax
	lea	edi,[ebx+16]
	xor	ecx,ecx
	or	esi,esi
	jz	edit_box_text_ok
      init_edit_box_text:
	lodsb
	or	al,al
	jz	edit_box_text_ok
	stosb
	inc	ecx
	cmp	cx,[ebx+6]
	jb	init_edit_box_text
      edit_box_text_ok:
	mov	[ebx+8],cx
	mov	[ebx+12],cx
	xor	al,al
	stosb
	test	byte [ebx+3],1
	jz	draw_edit_box
    set_edit_box_focus:
	push	ebx
	mov	ah,1
	mov	cx,0D0Eh
	int	10h
	pop	ebx
	jmp	draw_edit_box
    kill_edit_box_focus:
	mov	word [ebx+10],0
    draw_edit_box:
	test	byte [ebx+3],1
	jz	edit_box_position_ok
	mov	ax,[ebx+10]
	sub	ax,[ebx+12]
	jae	edit_box_position_correction
	movzx	ax,byte [ebx+2]
	add	ax,[ebx+10]
	sub	ax,2
	sub	ax,[ebx+12]
	jae	edit_box_position_ok
    edit_box_position_correction:
	sub	[ebx+10],ax
    edit_box_position_ok:
	push	es gs
	pop	es
	movzx	edi,byte [ebx+1]
	imul	edi,[video_pitch]
	movzx	eax,byte [ebx]
	lea	edi,[edi+eax*2]
	mov	al,20h
	cmp	word [ebx+10],0
	je	edit_box_left_edge_ok
	mov	al,1Bh
      edit_box_left_edge_ok:
	mov	ah,[ebx+4]
	stosw
	movzx	eax,word [ebx+10]
	lea	esi,[ebx+16+eax]
	movzx	ebp,byte [ebx+2]
	sub	ebp,2
	mov	edx,[ebx+12]
	cmp	dx,[ebx+14]
	jbe	draw_edit_box_before_selection
	ror	edx,16
      draw_edit_box_before_selection:
	movzx	ecx,dx
	sub	cx,[ebx+10]
	jbe	draw_edit_box_selection
	mov	ah,[ebx+4]
	call	draw_edit_box_part
      draw_edit_box_selection:
	mov	ax,dx
	shr	edx,16
	mov	cx,dx
	sub	cx,ax
	jbe	draw_edit_box_after_selection
	mov	ah,[ebx+5]
	test	byte [ebx+3],1
	jnz	edit_box_selection_visible
	mov	ah,[ebx+4]
      edit_box_selection_visible:
	call	draw_edit_box_part
      draw_edit_box_after_selection:
	mov	ah,[ebx+4]
	mov	cx,[ebx+8]
	sub	cx,dx
	jbe	draw_edit_box_ending
	call	draw_edit_box_part
      draw_edit_box_ending:
	mov	ecx,ebp
	mov	al,20h
	rep	stosw
	movzx	edx,word [ebx+8]
	lea	edx,[ebx+16+edx]
	cmp	esi,edx
	jae	edit_box_right_edge_ok
	mov	al,1Ah
      edit_box_right_edge_ok:
	stosw
	test	byte [ebx+3],1
	jz	edit_box_cursor_ok
	mov	dx,[ebx]
	inc	dl
	mov	ax,[ebx+12]
	sub	ax,[ebx+10]
	add	dl,al
	mov	ah,2
	push	ebx
	mov	bh,0
	int	10h
	pop	ebx
      edit_box_cursor_ok:
	pop	es
	ret
      draw_edit_box_part:
	or	bp,bp
	jz	edit_box_part_ok
	cmp	cx,bp
	jbe	edit_box_part_length_ok
	mov	cx,bp
      edit_box_part_length_ok:
	sub	bp,cx
      draw_edit_box_character:
	lodsb
	stosw
	loopw	draw_edit_box_character
      edit_box_part_ok:
	ret
  set_box_focus:
	or	byte [ebx+3],1
	mov	cl,[ebx+3]
	shr	cl,4
	jz	set_edit_box_focus
	cmp	cl,1
	je	set_check_box_focus
	ret
  kill_box_focus:
	and	byte [ebx+3],not 1
	mov	cl,[ebx+3]
	shr	cl,4
	jz	kill_edit_box_focus
	ret
  process_box_command:
	mov	cl,[ebx+3]
	shr	cl,4
	jz	edit_box_command
	cmp	cl,1
	je	check_box_command
	stc
	ret
    edit_box_command:
	cmp	al,0E0h
	jne	edit_box_ascii_code
	cmp	ah,4Bh
	je	edit_box_left_key
	cmp	ah,4Dh
	je	edit_box_right_key
	cmp	ah,47h
	je	edit_box_home_key
	cmp	ah,4Fh
	je	edit_box_end_key
	cmp	ah,53h
	je	edit_box_delete
	cmp	ah,52h
	je	edit_box_insert
	cmp	ah,93h
	je	edit_box_ctrl_delete
	cmp	ah,92h
	je	edit_box_ctrl_insert
    edit_box_unknown_command:
	stc
	ret
      edit_box_ascii_code:
	cmp	al,8
	je	edit_box_backspace
	cmp	al,18h
	je	edit_box_cut_block
	cmp	al,3
	je	edit_box_ctrl_insert
	cmp	al,16h
	je	edit_box_paste_block
	cmp	al,20h
	jb	edit_box_unknown_command
	test	byte [ebx+3],100b
	jz	edit_box_ascii_code_ok
	cmp	al,30h
	jb	edit_box_command_done
	cmp	al,39h
	ja	edit_box_command_done
      edit_box_ascii_code_ok:
	mov	dx,word [ebx+12]
	cmp	dx,word [ebx+14]
	je	edit_box_character
	push	eax
	call	edit_box_delete_block
	pop	eax
    edit_box_character:
	call	edit_box_insert_character
	jc	edit_box_command_done
	jmp	edit_box_no_selection
    edit_box_delete:
	test	byte [fs:17h],11b
	jnz	edit_box_cut_block
	mov	dx,word [ebx+12]
	cmp	dx,word [ebx+14]
	jne	edit_box_ctrl_delete
	cmp	dx,[ebx+8]
	je	edit_box_command_done
	inc	dx
	mov	[ebx+14],dx
	jmp	edit_box_ctrl_delete
    edit_box_backspace:
	mov	dx,[ebx+12]
	cmp	dx,word [ebx+14]
	jne	edit_box_ctrl_delete
	or	dx,dx
	jz	edit_box_command_done
	dec	word [ebx+12]
	mov	[ebx+14],dx
    edit_box_ctrl_delete:
	call	edit_box_delete_block
	jmp	edit_box_no_selection
    edit_box_ctrl_insert:
	call	edit_box_copy_block
	jmp	edit_box_command_done
    edit_box_cut_block:
	call	edit_box_copy_block
	call	edit_box_delete_block
	jmp	edit_box_no_selection
    edit_box_insert_character:
	test	byte [ebx+3],100b
	jz	edit_box_character_allowed
	cmp	al,30h
	jb	edit_box_disallowed_character
	cmp	al,39h
	ja	edit_box_disallowed_character
      edit_box_character_allowed:
	movzx	ecx,word [ebx+8]
	cmp	cx,[ebx+6]
	je	edit_box_full
	lea	edi,[ebx+16+ecx]
	lea	esi,[edi-1]
	sub	cx,[ebx+12]
	std
	rep	movsb
	cld
	stosb
	inc	word [ebx+8]
	inc	word [ebx+12]
      edit_box_disallowed_character:
	clc
	ret
      edit_box_full:
	stc
	ret
    edit_box_delete_block:
	movzx	eax,word [ebx+12]
	lea	esi,[ebx+16+eax]
	movzx	eax,word [ebx+14]
	lea	edi,[ebx+16+eax]
	movzx	eax,word [ebx+8]
	lea	ecx,[ebx+16+eax]
	cmp	esi,edi
	jae	edit_box_shift_rest_of_line
	xchg	esi,edi
      edit_box_shift_rest_of_line:
	sub	ecx,esi
	mov	eax,edi
	rep	movsb
	mov	ecx,esi
	sub	ecx,edi
	sub	[ebx+8],cx
	sub	eax,ebx
	sub	eax,16
	mov	[ebx+12],ax
	mov	[ebx+14],ax
	ret
    edit_box_copy_block:
	movzx	ecx,word [ebx+12]
	movzx	edx,word [ebx+14]
	cmp	ecx,edx
	je	edit_box_block_copied
	ja	edit_box_block_start_ok
	xchg	ecx,edx
    edit_box_block_start_ok:
	sub	ecx,edx
	lea	esi,[ebx+16+edx]
	push	ebx esi ecx
	cmp	[clipboard],0
	je	edit_box_allocate_clipboard
	mov	ebx,[clipboard_handle]
	call	release_memory
    edit_box_allocate_clipboard:
	mov	ecx,[esp]
	inc	ecx
	call	get_memory
	mov	[clipboard],eax
	mov	[clipboard_handle],ecx
	or	eax,eax
	jz	not_enough_memory
	mov	edi,eax
	pop	ecx esi ebx
	rep	movsb
	xor	al,al
	stosb
    edit_box_block_copied:
	ret
    edit_box_insert:
	test	byte [fs:17h],11b
	jz	edit_box_command_done
    edit_box_paste_block:
	call	edit_box_delete_block
	mov	esi,[clipboard]
	or	esi,esi
	jz	edit_box_command_done
      edit_box_insert_block:
	lodsb
	or	al,al
	jz	edit_box_no_selection
	cmp	al,0Dh
	je	edit_box_no_selection
	push	esi
	call	edit_box_insert_character
	pop	esi
	jnc	edit_box_insert_block
	jmp	edit_box_no_selection
    edit_box_left_key:
	cmp	word [ebx+12],0
	je	edit_box_command_done
	dec	word [ebx+12]
	jmp	edit_box_moved_cursor
    edit_box_right_key:
	mov	ax,[ebx+8]
	cmp	[ebx+12],ax
	je	edit_box_command_done
	inc	word [ebx+12]
	jmp	edit_box_moved_cursor
    edit_box_home_key:
	mov	word [ebx+12],0
	jmp	edit_box_moved_cursor
    edit_box_end_key:
	mov	ax,[ebx+8]
	mov	[ebx+12],ax
	jmp	edit_box_moved_cursor
    edit_box_moved_cursor:
	test	byte [fs:17h],11b
	jnz	edit_box_redraw
    edit_box_no_selection:
	mov	ax,[ebx+12]
	mov	[ebx+14],ax
    edit_box_redraw:
	call	draw_edit_box
    edit_box_command_done:
	movzx	eax,word [ebx+8]
	mov	byte [ebx+16+eax],0
	clc
	ret
  create_check_box:
	mov	[ebx],dx
	and	ch,0Fh
	or	ch,1 shl 4
	mov	[ebx+2],cx
	mov	[ebx+4],eax
	mov	[ebx+8],edi
	movzx	edx,byte [ebx+1]
	imul	edx,[video_pitch]
	movzx	ecx,byte [ebx]
	lea	edx,[edx+ecx*2]
	mov	byte [gs:edx],'['
	add	edx,2
	test	[edi],eax
	mov	al,20h
	jz	check_box_initial_state_ok
	mov	al,'+'
      check_box_initial_state_ok:
	mov	byte [gs:edx],al
	add	edx,2
	mov	byte [gs:edx],']'
	add	edx,2*2
	movzx	ecx,byte [ebx+2]
      draw_check_box_text:
	lodsb
	or	al,al
	jz	check_box_text_ok
	mov	[gs:edx],al
	add	edx,2
	loop	draw_check_box_text
      check_box_text_ok:
	test	byte [ebx+3],1
	jnz	set_check_box_focus
	ret
    set_check_box_focus:
	push	ebx
	mov	ah,2
	mov	dx,[ebx]
	inc	dl
	xor	bh,bh
	int	10h
	mov	ah,1
	mov	cx,0D0Eh
	int	10h
	pop	ebx
	ret
    check_box_command:
	cmp	al,20h
	je	switch_check_box_state
	stc
	ret
    switch_check_box_state:
	mov	edi,[ebx+8]
	mov	eax,[ebx+4]
	xor	[edi],eax
	movzx	edx,byte [ebx+1]
	imul	edx,[video_pitch]
	movzx	ecx,byte [ebx]
	lea	edx,[edx+(ecx+1)*2]
	test	[edi],eax
	mov	al,20h
	jz	check_box_state_ok
	mov	al,'+'
      check_box_state_ok:
	mov	byte [gs:edx],al
	clc
	ret
  init_common_dialog:
	xor	eax,eax
	mov	[boxes_count],eax
	mov	[current_box],eax
	ret
  register_box_in_dialog:
	mov	eax,[boxes_count]
	inc	[boxes_count]
	mov	[boxes+eax*4],ebx
	test	byte [ebx+3],1
	jz	box_registered
	mov	[current_box],eax
      box_registered:
	ret
  common_dialog_loop:
	call	wait_for_input
	cmp	ah,1
	je	common_dialog_aborted
	cmp	al,0Dh
	je	common_dialog_ok
	cmp	al,9
	je	common_dialog_cycle_boxes
	cmp	ah,0Fh
	je	common_dialog_reverse_cycle_boxes
	mov	edx,[current_box]
	mov	ebx,[boxes+edx*4]
	call	process_box_command
	jnc	common_dialog_loop
	cmp	ah,48h
	je	common_dialog_previous_box
	cmp	ah,50h
	je	common_dialog_next_box
	cmp	ah,4Bh
	je	common_dialog_previous_box
	cmp	ah,4Dh
	je	common_dialog_next_box
	jmp	common_dialog_loop
      common_dialog_next_box:
	mov	edx,[current_box]
	lea	eax,[edx+1]
	cmp	eax,[boxes_count]
	je	common_dialog_loop
	mov	[current_box],eax
	jmp	common_dialog_switch_box
      common_dialog_previous_box:
	mov	edx,[current_box]
	or	edx,edx
	jz	common_dialog_loop
	dec	[current_box]
      common_dialog_switch_box:
	mov	ebx,[boxes+edx*4]
	call	kill_box_focus
	mov	edx,[current_box]
	mov	ebx,[boxes+edx*4]
	call	set_box_focus
	jmp	common_dialog_loop
      common_dialog_cycle_boxes:
	mov	edx,[current_box]
	inc	[current_box]
	mov	eax,[current_box]
	cmp	eax,[boxes_count]
	jb	common_dialog_switch_box
	mov	[current_box],0
	jmp	common_dialog_switch_box
      common_dialog_reverse_cycle_boxes:
	mov	edx,[current_box]
	sub	[current_box],1
	jnc	common_dialog_switch_box
	mov	eax,[boxes_count]
	add	[current_box],eax
	jmp	common_dialog_switch_box
      common_dialog_ok:
	clc
	ret
      common_dialog_aborted:
	stc
	ret
  get_entered_number:
	xor	edx,edx
      get_digit:
	lodsb
	or	al,al
	jz	entered_number_ok
	sub	al,30h
	movzx	eax,al
	imul	edx,10
	add	edx,eax
	jmp	get_digit
      entered_number_ok:
	ret
  draw_static:
	lodsb
	or	al,al
	jz	static_ok
	mov	[gs:edi],al
	add	edi,2
	jmp	draw_static
      static_ok:
	ret
  file_open_dialog:
	mov	dx,0408h
	mov	cx,0520h
	mov	ah,[window_colors]
	call	draw_window
	mov	ebx,filename_buffer
	xor	esi,esi
	mov	ecx,24 + 256 shl 16 + 1 shl 8
	add	dx,0102h
	mov	ax,[edit_box_colors]
	call	create_edit_box
      file_open_loop:
	call	wait_for_input
	cmp	ah,1
	je	filename_aborted
	cmp	al,0Dh
	je	filename_ok
	mov	ebx,filename_buffer
	call	process_box_command
	jmp	file_open_loop
      filename_ok:
	mov	edx,filename_buffer+16
	clc
	ret
      filename_aborted:
	stc
	ret
  goto_dialog:
	mov	esi,_position
	mov	dx,080Bh
	mov	cx,0719h
	mov	ah,[window_colors]
	call	draw_window
	push	edx
	push	edi
	mov	eax,[video_pitch]
	lea	edi,[edi+eax+5*2]
	mov	esi,_row
	call	draw_static
	pop	edi
	mov	eax,[video_pitch]
	lea	eax,[eax*3]
	lea	edi,[edi+eax+2*2]
	mov	esi,_column
	call	draw_static
	call	init_common_dialog
	mov	edi,buffer+200h
	mov	esi,edi
	mov	eax,[caret_line_number]
	call	number_as_text
	xor	al,al
	stosb
	mov	dx,[esp]
	mov	ebx,buffer
	mov	ecx,9 + 7 shl 16 + 101b shl 8
	add	dx,010Ah
	mov	ax,[edit_box_colors]
	call	create_edit_box
	call	register_box_in_dialog
	mov	edi,buffer+200h
	mov	esi,edi
	mov	eax,[caret_position]
	inc	eax
	call	number_as_text
	xor	al,al
	stosb
	pop	edx
	mov	ebx,buffer+100h
	mov	ecx,9 + 7 shl 16 + 100b shl 8
	add	dx,030Ah
	mov	ax,[edit_box_colors]
	call	create_edit_box
	call	register_box_in_dialog
	call	common_dialog_loop
	jc	goto_dialog_done
	mov	esi,buffer+16
	call	get_entered_number
	mov	ecx,edx
	mov	esi,buffer+100h+16
	call	get_entered_number
	clc
      goto_dialog_done:
	ret
  find_dialog:
	push	[find_flags]
	mov	esi,_find
	mov	dx,060Dh
	mov	cx,0936h
	mov	ah,[window_colors]
	call	draw_window
	push	edx
	mov	eax,[video_pitch]
	lea	edi,[edi+eax+2*2]
	mov	esi,_text_to_find
	call	draw_static
	call	init_common_dialog
	call	get_word_at_caret
	mov	edi,[line_buffer]
	mov	esi,[caret_line]
	call	copy_from_line
	xor	al,al
	stosb
	mov	dx,[esp]
	mov	ebx,buffer
	mov	esi,[line_buffer]
	mov	ecx,32 + 1000 shl 16 + 1 shl 8
	add	dx,0110h
	mov	ax,[edit_box_colors]
	call	create_edit_box
	call	register_box_in_dialog
	mov	dx,[esp]
	mov	ebx,buffer+800h
	mov	esi,_case_sensitive
	mov	edi,find_flags
	mov	eax,AEFIND_CASESENSITIVE
	mov	cx,10h
	add	dx,0310h
	call	create_check_box
	call	register_box_in_dialog
	mov	dx,[esp]
	mov	ebx,buffer+810h
	mov	esi,_whole_words
	mov	edi,find_flags
	mov	eax,AEFIND_WHOLEWORDS
	mov	cx,10h
	add	dx,0410h
	call	create_check_box
	call	register_box_in_dialog
	pop	edx
	mov	ebx,buffer+820h
	mov	esi,_backward
	mov	edi,find_flags
	mov	eax,AEFIND_BACKWARD
	mov	cx,10h
	add	dx,0510h
	call	create_check_box
	call	register_box_in_dialog
	call	common_dialog_loop
	pop	eax
	jc	find_dialog_aborted
	mov	esi,buffer+16
	mov	eax,[find_flags]
	clc
	ret
      find_dialog_aborted:
	mov	[find_flags],eax
	ret
  replace_dialog:
	push	[find_flags]
	mov	esi,_replace
	mov	dx,060Dh
	mov	cx,0B36h
	mov	ah,[window_colors]
	call	draw_window
	push	edx
	push	edi
	mov	eax,[video_pitch]
	lea	edi,[edi+eax+2*2]
	mov	esi,_text_to_find
	call	draw_static
	pop	edi
	mov	eax,[video_pitch]
	imul	eax,3
	lea	edi,[edi+eax+6*2]
	mov	esi,_new_text
	call	draw_static
	call	init_common_dialog
	call	get_word_at_caret
	mov	edi,[line_buffer]
	mov	esi,[caret_line]
	call	copy_from_line
	xor	al,al
	stosb
	mov	dx,[esp]
	mov	ebx,buffer
	mov	esi,[line_buffer]
	mov	ecx,32 + 1000 shl 16 + 1 shl 8
	add	dx,0110h
	mov	ax,[edit_box_colors]
	call	create_edit_box
	call	register_box_in_dialog
	mov	dx,[esp]
	mov	ebx,buffer+400h
	xor	esi,esi
	mov	ecx,32 + 1000 shl 16
	add	dx,0310h
	mov	ax,[edit_box_colors]
	call	create_edit_box
	call	register_box_in_dialog
	mov	dx,[esp]
	mov	ebx,buffer+800h
	mov	esi,_case_sensitive
	mov	edi,find_flags
	mov	eax,AEFIND_CASESENSITIVE
	mov	cx,10h
	add	dx,0510h
	call	create_check_box
	call	register_box_in_dialog
	mov	dx,[esp]
	mov	ebx,buffer+810h
	mov	esi,_whole_words
	mov	edi,find_flags
	mov	eax,AEFIND_WHOLEWORDS
	mov	cx,10h
	add	dx,0610h
	call	create_check_box
	call	register_box_in_dialog
	pop	edx
	mov	ebx,buffer+820h
	mov	esi,_backward
	mov	edi,find_flags
	mov	eax,AEFIND_BACKWARD
	mov	cx,10h
	add	dx,0710h
	call	create_check_box
	call	register_box_in_dialog
	call	common_dialog_loop
	pop	eax
	jc	replace_dialog_aborted
	mov	esi,buffer+16
	mov	edi,buffer+400h+16
	mov	eax,[find_flags]
	clc
	ret
      replace_dialog_aborted:
	mov	[find_flags],eax
	ret
  options_dialog:
	mov	esi,_options
	mov	dx,0616h
	mov	cx,0922h
	mov	ah,[window_colors]
	call	draw_window
	push	edx
	call	init_common_dialog
	mov	ebx,buffer
	mov	esi,_secure_selection
	mov	edi,editor_style
	mov	eax,AES_SECURESEL
	mov	cx,16h + 1 shl 8
	add	dx,0102h
	call	create_check_box
	call	register_box_in_dialog
	mov	dx,[esp]
	mov	ebx,buffer+10h
	mov	esi,_auto_brackets
	mov	edi,editor_style
	mov	eax,AES_AUTOBRACKETS
	mov	cx,16h
	add	dx,0202h
	call	create_check_box
	call	register_box_in_dialog
	mov	dx,[esp]
	mov	ebx,buffer+20h
	mov	esi,_auto_indent
	mov	edi,editor_style
	mov	eax,AES_AUTOINDENT
	mov	cx,16h
	add	dx,0302h
	call	create_check_box
	call	register_box_in_dialog
	mov	dx,[esp]
	mov	ebx,buffer+30h
	mov	esi,_smart_tabs
	mov	edi,editor_style
	mov	eax,AES_SMARTTABS
	mov	cx,16h
	add	dx,0402h
	call	create_check_box
	call	register_box_in_dialog
	pop	edx
	mov	ebx,buffer+40h
	mov	esi,_optimal_fill
	mov	edi,editor_style
	mov	eax,AES_OPTIMALFILL
	mov	cx,16h
	add	dx,0502h
	call	create_check_box
	call	register_box_in_dialog
	jmp	common_dialog_loop
  ascii_table_window:
	mov	esi,_ascii_table
	mov	dx,0616h
	mov	cx,0C24h
	mov	ah,[window_colors]
	call	draw_window
	push	edx
	mov	ah,[window_colors]
	xor	al,al
	mov	edx,8
      draw_ascii_table:
	mov	ecx,32
      draw_ascii_row:
	mov	[gs:edi],ax
	add	edi,2
	inc	al
	loop	draw_ascii_row
	add	edi,[video_pitch]
	sub	edi,32*2
	dec	edx
	jnz	draw_ascii_table
	mov	ecx,32
      draw_ascii_table_border:
	mov	byte [gs:edi+(ecx-1)*2],'Ä'
	loop	draw_ascii_table_border
	add	edi,[video_pitch]
	push	edi
	mov	ah,1
	mov	cx,000Fh
	int	10h
      ascii_table_update:
	mov	dl,[selected_character]
	mov	dh,dl
	and	dl,11111b
	shr	dh,5
	add	dx,word [esp+4]
	mov	ah,2
	xor	bh,bh
	int	10h
	mov	edi,screen_row_buffer+10h
	mov	eax,'Dec:'
	stosd
	mov	al,20h
	stosb
	movzx	eax,[selected_character]
	call	number_as_text
	mov	al,20h
	stosb
	mov	eax,'Hex:'
	stosd
	mov	al,20h
	stosb
	mov	al,[selected_character]
	shr	al,4
	cmp	al,10
	sbb	al,69h
	das
	stosb
	mov	al,[selected_character]
	and	al,0Fh
	cmp	al,10
	sbb	al,69h
	das
	stosb
	xor	al,al
	stosb
	sub	edi,30+1
	mov	esi,edi
	mov	eax,'Char'
	stosd
	mov	eax,'acte'
	stosd
	mov	al,[selected_character]
	shl	eax,24
	or	eax,'r: '
	stosd
	mov	ecx,screen_row_buffer+10h
	sub	ecx,edi
	mov	al,20h
	rep	stosb
	mov	edi,[esp]
	add	edi,1*2
	call	draw_static
      ascii_table_loop:
	call	wait_for_input
	or	ah,ah
	jz	pure_ascii_char
	cmp	ah,48h
	je	ascii_table_up
	cmp	ah,50h
	je	ascii_table_down
	cmp	ah,4Bh
	je	ascii_table_left
	cmp	ah,4Dh
	je	ascii_table_right
	cmp	ah,1
	je	ascii_table_exit
	cmp	al,0E0h
	je	ascii_table_loop
	cmp	al,13
	je	ascii_table_done
	cmp	al,2
	je	ascii_table_done
	cmp	al,20h
	jb	ascii_table_loop
      pure_ascii_char:
	mov	[selected_character],al
	jmp	ascii_table_update
      ascii_table_exit:
	pop	eax eax
	stc
	ret
      ascii_table_done:
	pop	eax eax
	mov	al,[selected_character]
	clc
	ret
      ascii_table_up:
	cmp	[selected_character],20h
	jb	ascii_table_loop
	sub	[selected_character],20h
	jmp	ascii_table_update
      ascii_table_down:
	cmp	[selected_character],0E0h
	jae	ascii_table_loop
	add	[selected_character],20h
	jmp	ascii_table_update
      ascii_table_left:
	test	[selected_character],11111b
	jz	ascii_table_loop
	dec	[selected_character]
	jmp	ascii_table_update
      ascii_table_right:
	mov	al,[selected_character]
	inc	al
	test	al,11111b
	jz	ascii_table_loop
	mov	[selected_character],al
	jmp	ascii_table_update

; Memory allocation

  get_memory:
	push	esi edi
	mov	ebx,ecx
	shr	ebx,16
	mov	ax,501h
	int	31h
	jc	dpmi_allocation_failed
	mov	ax,bx
	shl	eax,16
	mov	ax,cx
	mov	edx,main
	shl	edx,4
	sub	eax,edx
	mov	bx,si
	shl	ebx,16
	mov	bx,di
	pop	edi esi
	ret
    dpmi_allocation_failed:
	xor	eax,eax
	pop	edi esi
	ret
  release_memory:
	push	esi edi
	mov	esi,ebx
	shr	esi,16
	mov	di,bx
	mov	ax,502h
	int	31h
	pop	edi esi
	ret
  get_low_memory:
	mov	ax,100h
	mov	bx,-1
	int	31h
	movzx	eax,bx
	shl	eax,4
	mov	[low_memory_size],eax
	mov	ax,100h
	int	31h
	mov	[low_memory_selector],dx
	jnc	low_memory_ok
	xor	edx,edx
	mov	[low_memory_size],edx
      low_memory_ok:
	ret
  release_low_memory:
	cmp	[low_memory_size],0
	je	low_memory_ok
	mov	ax,101h
	mov	dx,[low_memory_selector]
	int	31h
	ret

; File operations

  dos_int:
	push	0 0 0
	pushw	buffer_segment buffer_segment
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
  open:
	push	esi edi
	call	adapt_path
	mov	ax,716Ch
	mov	bx,100000b
	mov	dx,1
	xor	cx,cx
	xor	si,si
	call	dos_int
	jnc	open_done
	cmp	ax,7100h
	je	old_open
	stc
	jmp	open_done
      old_open:
	mov	ax,3D00h
	xor	dx,dx
	call	dos_int
      open_done:
	mov	bx,ax
	pop	edi esi
	ret
    adapt_path:
	mov	esi,edx
	mov	edi,buffer
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
	call	dos_int
	jnc	create_done
	cmp	ax,7100h
	je	old_create
	stc
	jmp	create_done
    old_create:
	mov	ah,3Ch
	xor	cx,cx
	xor	dx,dx
	call	dos_int
    create_done:
	mov	bx,ax
	pop	edi esi
	ret
  write:
	push	edx esi edi ebp
	mov	ebp,ecx
	mov	esi,edx
      write_loop:
	mov	ecx,1000h
	sub	ebp,1000h
	jnc	do_write
	add	ebp,1000h
	mov	ecx,ebp
	xor	ebp,ebp
      do_write:
	push	ecx
	mov	edi,buffer
	shr	ecx,2
	rep	movsd
	mov	ecx,[esp]
	and	ecx,11b
	rep	movsb
	pop	ecx
	mov	ah,40h
	xor	dx,dx
	call	dos_int
	or	ebp,ebp
	jnz	write_loop
	pop	ebp edi esi edx
	ret
  read:
	push	edx esi edi ebp
	mov	ebp,ecx
	mov	edi,edx
      read_loop:
	mov	ecx,1000h
	sub	ebp,1000h
	jnc	do_read
	add	ebp,1000h
	mov	ecx,ebp
	xor	ebp,ebp
      do_read:
	push	ecx
	mov	ah,3Fh
	xor	dx,dx
	call	dos_int
	cmp	ax,cx
	jne	eof
	mov	esi,buffer
	mov	ecx,[esp]
	shr	ecx,2
	rep	movsd
	pop	ecx
	and	ecx,11b
	rep	movsb
	or	ebp,ebp
	jnz	read_loop
      read_done:
	pop	ebp edi esi edx
	ret
      eof:
	pop	ecx
	stc
	jmp	read_done
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

; Other functions needed by assembler core

  get_environment_variable:
	mov	ebx,esi
	push	ds
	mov	ds,[environment_selector]
	xor	esi,esi
      compare_variable_names:
	mov	edx,ebx
      compare_name_char:
	lodsb
	mov	ah,[es:edx]
	inc	edx
	cmp	al,'='
	je	end_of_variable_name
	or	ah,ah
	jz	next_variable
	sub	ah,al
	jz	compare_name_char
	cmp	ah,20h
	jne	next_variable
	cmp	al,41h
	jb	next_variable
	cmp	al,5Ah
	jna	compare_name_char
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
  display_block:
	push	es
	mov	es,[low_memory_selector]
	mov	edi,[display_length]
	mov	eax,[display_length]
	add	eax,ecx
	cmp	eax,[low_memory_size]
	ja	not_enough_memory
	mov	[display_length],eax
	rep	movsb
	pop	es
	ret

; Error handling

  not_enough_memory:
	call	update_screen
	mov	esi,_memory_error
	mov	ebx,_error
	movzx	eax,[error_box_colors]
	call	message_box
	mov	esp,stack_top
	jmp	main_loop

  fatal_error:
	pop	esi
	mov	esp,stack_top
	push	esi
	mov	ax,205h
	mov	bl,9
	mov	edx,dword [keyboard_handler]
	mov	cx,word [keyboard_handler+4]
	int	31h
	call	flush_display_buffer
	mov	ebx,[allocated_memory]
	call	release_memory
	call	update_screen
      show_error_summary:
	mov	esi,_assembler_error
	mov	edi,buffer
	mov	ebx,esp
	call	sprintf
	mov	esi,buffer
	mov	ebx,_compile
	movzx	eax,[error_box_colors]
	mov	[first_button],_ok
	mov	[second_button],_get_display
	cmp	[display_length],0
	je	show_compilation_summary
	or	eax,2000000h
	jmp	show_compilation_summary
  assembler_error:
	pop	esi
	mov	esp,stack_top
	push	esi
	mov	ax,205h
	mov	bl,9
	mov	edx,dword [keyboard_handler]
	mov	cx,word [keyboard_handler+4]
	int	31h
	call	flush_display_buffer
	call	update_screen
	mov	ebx,[current_line]
      find_error_origin:
	test	dword [ebx+4],80000000h
	jz	error_origin_found
	mov	ebx,[ebx+8]
	jmp	find_error_origin
      error_origin_found:
	mov	esi,[ebx]
	mov	edi,filename_buffer
      copy_error_file:
	lodsb
	stosb
	or	al,al
	jnz	copy_error_file
	push	dword [ebx+4]
	mov	ebx,[allocated_memory]
	call	release_memory
	mov	edx,filename_buffer
	call	load_file
      show_error_line:
	pop	eax
	call	find_line
	xor	eax,eax
	mov	[selection_line],esi
	mov	[selection_line_number],ecx
	mov	[selection_position],eax
	mov	[caret_position],eax
	push	esi
	lea	edi,[esi+SEGMENT_HEADER_LENGTH]
	lea	ebp,[esi+SEGMENT_LENGTH]
	mov	ebx,characters
	xor	edx,edx
      check_for_more_lines:
	call	peek_character
	jc	no_more_lines
	cmp	al,3Bh
	je	no_more_lines
	mov	ah,al
	xlatb
	or	al,al
	jz	symbol
	or	edx,edx
	jnz	neutral
	cmp	ah,27h
	je	quoted
	cmp	ah,22h
	je	quoted
      neutral:
	or	edx,-1
	jmp	check_for_more_lines
      peek_character:
	cmp	edi,ebp
	je	peek_next_segment
	mov	al,[edi]
	inc	edi
	clc
	ret
      peek_next_segment:
	mov	esi,[esi]
	btr	esi,0
	lea	edi,[esi+SEGMENT_HEADER_LENGTH]
	lea	ebp,[esi+SEGMENT_LENGTH]
	jc	peek_character
	stc
	ret
      symbol:
	cmp	ah,'\'
	je	backslash
	xor	edx,edx
	jmp	check_for_more_lines
      quoted:
	call	peek_character
	jc	no_more_lines
	cmp	al,ah
	jne	quoted
	call	peek_character
	jc	no_more_lines
	cmp	al,ah
	je	quoted
	dec	esi
	xor	edx,edx
	jmp	check_for_more_lines
      backslash:
	call	peek_character
	jc	more_lines
	cmp	al,20h
	je	backslash
	cmp	al,3Bh
	jne	no_more_lines
      comment:
	mov	esi,[esi]
	btr	esi,0
	jc	comment
      more_lines:
	or	esi,esi
	jz	last_line
	inc	ecx
	mov	[esp],esi
	jmp	check_for_more_lines
      last_line:
	pop	[caret_line]
	mov	[caret_line_number],ecx
	mov	eax,[maximum_position]
	mov	[caret_position],eax
	jmp	error_line_highlighted
      no_more_lines:
	or	esi,esi
	jz	last_line
	pop	eax
	inc	ecx
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
      error_line_highlighted:
	call	let_caret_appear
	mov	eax,[caret_line]
	xchg	eax,[selection_line]
	mov	[caret_line],eax
	mov	eax,[caret_line_number]
	xchg	eax,[selection_line_number]
	mov	[caret_line_number],eax
	mov	eax,[caret_position]
	xchg	eax,[selection_position]
	mov	[caret_position],eax
	call	let_caret_appear
	call	update_window
	call	update_screen
	jmp	show_error_summary

; Assembler core

  include '..\..\errors.inc'
  include '..\..\expressi.inc'
  include '..\..\preproce.inc'
  include '..\..\parser.inc'
  include '..\..\assemble.inc'
  include '..\..\formats.inc'
  include '..\..\x86_64.inc'
  include '..\..\tables.inc'

; Assembler constants

  include '..\..\version.inc'

; String constants

  _caption db 'flat assembler ',VERSION_STRING,0
  _copyright db 'Copyright (c) 1999-2007, Tomasz Grysztar',0

  _untitled db 'Untitled',0
  _compile db 'Compile',0
  _error db 'Error',0
  _ok db 'OK',0
  _yes db 'Yes',0
  _no db 'No',0
  _get_display db 'Get display to clipboard',0
  _open db 'Open',0
  _save_as db 'Save as',0
  _position db 'Position',0
  _row db 'Row:',0
  _column db 'Column:',0
  _find db 'Find',0
  _replace db 'Replace',0
  _text_to_find db 'Text to find:',0
  _new_text db 'New text:',0
  _case_sensitive db 'Case sensitive',0
  _whole_words db 'Whole words',0
  _backward db 'Backward search',0
  _options db 'Editor options',0
  _secure_selection db 'Secure selection',0
  _auto_brackets db 'Automatic brackets',0
  _auto_indent db 'Automatic indents',0
  _smart_tabs db 'Smart tabulation',0
  _optimal_fill db 'Optimal fill on saving',0
  _ascii_table db 'ASCII table',0

  _startup_failed db 'Failed to allocate required memory.',0
  _memory_error db 'Not enough memory to complete this operation.',0
  _loading_error db 'Could not load file %s.',0
  _saving_error db 'Could not write file to disk.',0
  _not_executable db 'Cannot execute this kind of file.',0
  _saving_question db 'File was modified. Save it now?',0
  _overwrite_question db 'File %s already exists. Do you want to replace it?',0
  _invalid_path db 'Invalid path.',0
  _not_found db 'Text not found.',0
  _assembler_error db 'Error: %s.',0

; Configuration

  editor_style dd AES_AUTOINDENT+AES_SMARTTABS+AES_SECURESEL+AES_OPTIMALFILL

  text_colors db 87h
  selection_colors db 7Fh
  symbol_color db 0Fh
  number_color db 0Ah
  string_color db 0Ch
  comment_color db 3
  status_colors db 2Fh
  window_colors db 3Fh
  message_box_colors dw 3F1Fh
  error_box_colors dw 4E6Eh
  edit_box_colors dw 9F1Fh

; Assembly Editor core

  include '..\memory.inc'
  include '..\navigate.inc'
  include '..\edit.inc'
  include '..\blocks.inc'
  include '..\search.inc'
  include '..\undo.inc'

; Assembly Editor constants

  SEGMENT_LENGTH	= 100h
  BLOCK_LENGTH		= 100h * SEGMENT_LENGTH
  SEGMENT_HEADER_LENGTH = 16
  SEGMENT_DATA_LENGTH	= SEGMENT_LENGTH - SEGMENT_HEADER_LENGTH

  AEMODE_OVERWRITE   = 1
  AEMODE_VERTICALSEL = 2

  AEFIND_CASESENSITIVE = 1
  AEFIND_WHOLEWORDS    = 2
  AEFIND_BACKWARD      = 4

  AES_AUTOINDENT   = 0001h
  AES_AUTOBRACKETS = 0002h
  AES_SMARTTABS    = 0004h
  AES_SECURESEL    = 0008h
  AES_OPTIMALFILL  = 0010h

; Assembly Editor data

  editor_data:

  next_instance dd ?
  previous_instance dd ?
  file_path dd ?
  file_path_handle dd ?

  include '..\variable.inc'

  editor_data_size = $ - editor_data

  case_table db 100h dup ?

; Assembler core data

  include '..\..\variable.inc'

; Interface specific data

  psp_selector dw ?
  environment_selector dw ?
  main_selector dw ?
  bios_selector dw ?
  video_selector dw ?

  screen_width dd ?
  screen_height dd ?
  video_pitch dd ?

  video_storage dd ?
  video_storage_handle dd ?
  stored_cursor dw ?
  stored_cursor_position dw ?
  stored_page db ?
  stored_mode db ?

  low_memory_size dd ?
  low_memory_selector dw ?

  last_operation db ?
  current_operation db ?
  was_selection db ?

  line_buffer dd ?
  line_buffer_size dd ?
  line_buffer_handle dd ?
  screen_offset dd ?
  screen_row_buffer db 512 dup ?

  clipboard dd ?
  clipboard_handle dd ?

  main_project_file dd ?
  allocated_memory dd ?
  start_time dd ?
  progress_offset dd ?
  keyboard_handler dp ?
  display_length dd ?

  find_flags dd ?
  selected_character db ?
  command_flags db ?
  message_width db ?
  buttons_width db ?
  first_button dd ?
  second_button dd ?

  file_handle dd ?
  filename_buffer db 17+256 dup ?

  current_box dd ?
  boxes_count dd ?
  boxes dd 16 dup ?

segment buffer_segment

  buffer = (buffer_segment-main) shl 4

  db 1000h dup ?

segment stack_segment

  stack_bottom = (stack_segment-main) shl 4

  db 4000h dup ?

  stack_top = stack_bottom + $
