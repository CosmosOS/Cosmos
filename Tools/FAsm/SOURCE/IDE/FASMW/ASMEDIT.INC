
; Assembly Editor interface for Win32 IDE
; Copyright (c) 2001-2007, Tomasz Grysztar.
; All rights reserved.

SEGMENT_LENGTH	      = 100h
BLOCK_LENGTH	      = 100h * SEGMENT_LENGTH
SEGMENT_HEADER_LENGTH = 16
SEGMENT_DATA_LENGTH   = SEGMENT_LENGTH - SEGMENT_HEADER_LENGTH

proc AsmEdit hwnd,wmsg,wparam,lparam

 locals

  editor_style dd ?

 label editor_data

  include '../variable.inc'

  editor_font dd ?
  font_width dd ?
  font_height dd ?
  screen_base dd ?
  screen_offset dd ?
  screen_width dd ?
  screen_height dd ?
  screen_size dd ?
  line_buffer dd ?
  line_buffer_size dd ?
  background_color dd ?
  text_color dd ?
  selection_background dd ?
  selection_text dd ?
  syntax_proc dd ?
  syntax_colors dd ?
  caret_x dd ?
  caret_y dd ?
  menu_handle dd ?
  menu_window dd ?
  current_operation db ?
  last_operation db ?
  was_selection db ?
  mouse_select db ?
  focus db ?

  editor_data_size = $ - editor_data

  return_value dd ?
  background_brush dd ?
  selection_brush dd ?
  ps PAINTSTRUCT
  tm TEXTMETRIC
  sc SCROLLINFO
  point POINT
  rect RECT
  size SIZE
  char dd ?
  kbstate rb 100h
  line_selection db ?
  clipboard_opened db ?
  redraw_now db ?
  notification db ?

 endl

	push	ebx esi edi
	invoke	GetWindowLong,[hwnd],GWL_STYLE
	mov	[editor_style],eax
	cmp	[wmsg],WM_CREATE
	je	wm_create
	cmp	[wmsg],WM_GETDLGCODE
	je	wm_getdlgcode
	invoke	GetWindowLong,[hwnd],0
	or	eax,eax
	jz	defwndproc
	mov	[editor_memory],eax
	lea	esi,[eax+16]
	lea	edi,[editor_data]
	mov	ecx,editor_data_size
	rep	movsb
	mov	[return_value],0
	mov	[notification],0
	mov	[redraw_now],0
	mov	[clipboard_opened],0
	cmp	[wmsg],WM_DESTROY
	je	wm_destroy
	cmp	[wmsg],WM_PAINT
	je	wm_paint
	invoke	HideCaret,[hwnd]
	cmp	[wmsg],WM_HSCROLL
	je	wm_hscroll
	cmp	[wmsg],WM_VSCROLL
	je	wm_vscroll
	cmp	[wmsg],WM_SIZE
	je	wm_size
	mov	eax,[selection_line]
	or	eax,eax
	jz	no_selection
	cmp	eax,[caret_line]
	jne	selection_present
	mov	eax,[selection_position]
	cmp	eax,[caret_position]
	je	no_selection
    selection_present:
	mov	[was_selection],1
	jmp	selection_status_ok
    no_selection:
	mov	[was_selection],0
    selection_status_ok:
	xor	al,al
	xchg	[current_operation],al
	mov	[last_operation],al
	mov	eax,[wmsg]
	cmp	eax,WM_SETFOCUS
	je	wm_setfocus
	cmp	eax,WM_KILLFOCUS
	je	wm_killfocus
	cmp	eax,WM_KEYDOWN
	je	wm_keydown
	cmp	eax,WM_SYSKEYDOWN
	je	wm_syskeydown
	cmp	eax,WM_CHAR
	je	wm_char
	cmp	eax,WM_LBUTTONDOWN
	je	wm_lbuttondown
	cmp	eax,WM_LBUTTONUP
	je	wm_lbuttonup
	cmp	eax,WM_MOUSEMOVE
	je	wm_mousemove
	cmp	eax,WM_LBUTTONDBLCLK
	je	wm_lbuttondblclk
	cmp	eax,WM_MOUSEWHEEL
	je	wm_mousewheel
	cmp	eax,WM_RBUTTONDOWN
	je	wm_rbuttondown
	cmp	eax,WM_COPY
	je	wm_copy
	cmp	eax,WM_CUT
	je	wm_cut
	cmp	eax,WM_PASTE
	je	wm_paste
	cmp	eax,WM_CLEAR
	je	wm_clear
	cmp	eax,WM_SETTEXT
	je	wm_settext
	cmp	eax,WM_GETTEXTLENGTH
	je	wm_gettextlength
	cmp	eax,WM_GETTEXT
	je	wm_gettext
	cmp	eax,WM_SETFONT
	je	wm_setfont
	cmp	eax,WM_GETFONT
	je	wm_getfont
	cmp	eax,WM_UNDO
	je	wm_undo
	cmp	eax,EM_UNDO
	je	wm_undo
	cmp	eax,EM_CANUNDO
	je	em_canundo
	cmp	eax,EM_EMPTYUNDOBUFFER
	je	em_emptyundobuffer
	cmp	eax,EM_REPLACESEL
	je	em_replacesel
	cmp	eax,AEM_SETMODE
	je	aem_setmode
	cmp	eax,AEM_GETMODE
	je	aem_getmode
	cmp	eax,AEM_SETSYNTAXHIGHLIGHT
	je	aem_setsyntaxhighlight
	cmp	eax,AEM_SETRIGHTCLICKMENU
	je	aem_setrightclickmenu
	cmp	eax,AEM_GETLINELENGTH
	je	aem_getlinelength
	cmp	eax,AEM_GETLINE
	je	aem_getline
	cmp	eax,AEM_SETPOS
	je	aem_setpos
	cmp	eax,AEM_GETPOS
	je	aem_getpos
	cmp	eax,AEM_FINDFIRST
	je	aem_findfirst
	cmp	eax,AEM_FINDNEXT
	je	aem_findnext
	cmp	eax,AEM_CANFINDNEXT
	je	aem_canfindnext
	cmp	eax,AEM_GETWORDATCARET
	je	aem_getwordatcaret
	cmp	eax,AEM_SETTEXTCOLOR
	je	aem_settextcolor
	cmp	eax,AEM_SETSELCOLOR
	je	aem_setselcolor
	invoke	ShowCaret,[hwnd]
defwndproc:
	invoke	DefWindowProc,[hwnd],[wmsg],[wparam],[lparam]
	jmp	finish_wndproc
wm_create:
	call	init_editor_memory
	jc	create_failed
	invoke	SetWindowLong,[hwnd],0,[editor_memory]
	mov	[syntax_proc],SyntaxProc
	mov	[syntax_colors],0
	mov	[current_operation],0
	mov	[focus],0
	mov	[mouse_select],0
	mov	[menu_handle],0
	mov	eax,SEGMENT_DATA_LENGTH*2
	mov	[line_buffer_size],eax
	invoke	VirtualAlloc,0,eax,MEM_COMMIT,PAGE_READWRITE
	or	eax,eax
	jz	create_failed
	mov	[line_buffer],eax
	mov	[screen_base],0
	invoke	GetSysColor,COLOR_WINDOW
	mov	[background_color],eax
	invoke	GetSysColor,COLOR_WINDOWTEXT
	mov	[text_color],eax
	invoke	GetSysColor,COLOR_HIGHLIGHT
	mov	[selection_background],eax
	invoke	GetSysColor,COLOR_HIGHLIGHTTEXT
	mov	[selection_text],eax
	mov	eax,[asmedit_font]
	mov	[editor_font],eax
	invoke	GetDC,[hwnd]
	mov	ebx,eax
	invoke	SelectObject,ebx,[editor_font]
	lea	edi,[char]
	mov	byte [edi],20h
	lea	eax,[size]
	invoke	GetTextExtentPoint32,ebx,edi,1,eax
	mov	eax,[size.cy]
	mov	[font_height],eax
	mov	eax,[size.cx]
	mov	[font_width],eax
	invoke	ReleaseDC,[hwnd],ebx
	mov	[return_value],0
	jmp	done
    create_failed:
	or	eax,-1
	jmp	finish_wndproc
wm_destroy:
	invoke	VirtualFree,[line_buffer],0,MEM_RELEASE
	invoke	VirtualFree,[screen_base],0,MEM_RELEASE
	call	release_search_data
	call	release_editor_memory
	invoke	SetWindowLong,[hwnd],0,0
	xor	eax,eax
	jmp	finish_wndproc
wm_getdlgcode:
	mov	eax,DLGC_WANTCHARS+DLGC_WANTARROWS
	jmp	finish_wndproc
wm_paint:
	lea	eax,[rect]
	invoke	GetUpdateRect,[hwnd],eax,FALSE
	or	eax,eax
	jz	finish_wndproc
	cmp	[screen_base],0
	je	finish_wndproc
	lea	eax,[ps]
	invoke	BeginPaint,[hwnd],eax
	mov	ebx,eax
	invoke	CreateSolidBrush,[background_color]
	mov	[background_brush],eax
	invoke	CreateSolidBrush,[selection_background]
	mov	[selection_brush],eax
	invoke	SelectObject,ebx,[editor_font]
	mov	esi,[screen_base]
	add	esi,[screen_offset]
	mov	eax,[screen_width]
	mul	[screen_height]
	lea	edi,[esi+eax]
	mov	[rect.top],0
	mov	eax,[font_height]
	mov	[rect.bottom],eax
	mov	ecx,[screen_height]
    paint_screen:
	push	ecx
	mov	[rect.left],0
	mov	ecx,[screen_width]
    paint_line:
	cmp	byte [esi],0
	je	paint_empty_block
	mov	edx,1
	mov	al,[edi]
    get_characters_block:
	cmp	edx,ecx
	je	get_color
	cmp	al,[edi+edx]
	jne	get_color
	cmp	byte [esi+edx],0
	je	get_color
	inc	edx
	jmp	get_characters_block
    paint_empty_block:
	mov	edx,1
	test	byte [edi],80h
	jnz	get_empty_selection
    get_empty_block:
	cmp	edx,ecx
	je	fill_empty_block
	cmp	byte [esi+edx],0
	jne	fill_empty_block
	test	byte [edi+edx],80h
	jnz	fill_empty_block
	inc	edx
	jmp	get_empty_block
    fill_empty_block:
	push	ecx edx
	mov	eax,[font_width]
	mul	edx
	add	eax,[rect.left]
	mov	[rect.right],eax
	lea	eax,[rect]
	invoke	FillRect,ebx,eax,[background_brush]
	jmp	paint_next_block
    get_empty_selection:
	cmp	edx,ecx
	je	fill_empty_selection
	cmp	byte [esi+edx],0
	jne	fill_empty_selection
	test	byte [edi+edx],80h
	jz	fill_empty_selection
	inc	edx
	jmp	get_empty_selection
    fill_empty_selection:
	push	ecx edx
	mov	eax,[font_width]
	mul	edx
	add	eax,[rect.left]
	mov	[rect.right],eax
	lea	eax,[rect]
	invoke	FillRect,ebx,eax,[selection_brush]
	jmp	paint_next_block
    get_color:
	push	ecx edx
	test	byte [edi],80h
	jnz	highlight_color
	invoke	SetBkColor,ebx,[background_color]
	mov	al,[edi]
	or	al,al
	jnz	syntax_color
    default_color:
	invoke	SetTextColor,ebx,[text_color]
	jmp	color_ok
    syntax_color:
	movzx	eax,al
	mov	edx,[syntax_colors]
	or	edx,edx
	jz	default_color
	mov	eax,[edx+(eax-1)*4]
	invoke	SetTextColor,ebx,eax
	jmp	color_ok
    highlight_color:
	invoke	SetBkColor,ebx,[selection_background]
	invoke	SetTextColor,ebx,[selection_text]
    color_ok:
	mov	ecx,[esp]
	mov	eax,[font_width]
	mul	ecx
	add	eax,[rect.left]
	mov	[rect.right],eax
	lea	eax,[rect]
	invoke	DrawText,ebx,esi,ecx,eax,DT_LEFT+DT_NOPREFIX+DT_SINGLELINE
    paint_next_block:
	pop	edx ecx
	sub	ecx,edx
	add	esi,edx
	add	edi,edx
	mov	eax,[rect.right]
	mov	[rect.left],eax
	or	ecx,ecx
	jnz	paint_line
	mov	eax,[font_height]
	add	[rect.top],eax
	add	[rect.bottom],eax
	pop	ecx
	dec	ecx
	jnz	paint_screen
	invoke	DeleteObject,[background_brush]
	invoke	DeleteObject,[selection_brush]
	lea	eax,[ps]
	invoke	EndPaint,[hwnd],eax
	xor	eax,eax
	jmp	finish_wndproc
wm_setfocus:
	or	[focus],-1
	call	create_caret
	mov	[notification],AEN_SETFOCUS
	cmp	[was_selection],0
	je	done
	jmp	moved_window
wm_killfocus:
	mov	[focus],0
	invoke	DestroyCaret
	mov	[notification],AEN_KILLFOCUS
	cmp	[was_selection],0
	je	done
	jmp	moved_window
wm_hscroll:
	mov	[sc.cbSize],sizeof.SCROLLINFO
	mov	[sc.fMask],SIF_ALL
	lea	eax,[sc]
	invoke	GetScrollInfo,[hwnd],SB_HORZ,eax
	movzx	eax,word [wparam]
	cmp	eax,SB_LINEUP
	je	hscroll_left
	cmp	eax,SB_LINEDOWN
	je	hscroll_right
	cmp	eax,SB_THUMBTRACK
	je	hscroll_pos
	cmp	eax,SB_PAGEUP
	je	hscroll_wleft
	cmp	eax,SB_PAGEDOWN
	je	hscroll_wright
    hscroll_ignore:
	jmp	done
    hscroll_left:
	cmp	[window_position],0
	je	hscroll_ignore
	dec	[window_position]
	jmp	moved_window
    hscroll_right:
	mov	eax,[maximum_position]
	sub	eax,[sc.nPage]
	cmp	[window_position],eax
	jge	hscroll_ignore
	inc	[window_position]
	jmp	moved_window
    hscroll_pos:
	mov	eax,[sc.nTrackPos]
	mov	[window_position],eax
	jmp	moved_window
    hscroll_wleft:
	mov	eax,[sc.nPage]
	sub	[window_position],eax
	jnc	moved_window
	mov	[window_position],0
	jmp	moved_window
    hscroll_wright:
	mov	eax,[sc.nPage]
	mov	ecx,[maximum_position]
	sub	ecx,eax
	add	[window_position],eax
	cmp	[window_position],ecx
	jbe	moved_window
	mov	[window_position],ecx
	jmp	moved_window
wm_vscroll:
	mov	[sc.cbSize],sizeof.SCROLLINFO
	mov	[sc.fMask],SIF_ALL
	lea	eax,[sc]
	invoke	GetScrollInfo,[hwnd],SB_VERT,eax
	movzx	eax,word [wparam]
	cmp	eax,SB_LINEUP
	je	vscroll_up
	cmp	eax,SB_LINEDOWN
	je	vscroll_down
	cmp	eax,SB_THUMBTRACK
	je	vscroll_pos
	cmp	eax,SB_PAGEUP
	je	vscroll_pageup
	cmp	eax,SB_PAGEDOWN
	je	vscroll_pagedown
    vscroll_ignore:
	jmp	done
    vscroll_up:
	mov	esi,[window_line]
	mov	esi,[esi+4]
	or	esi,esi
	jz	vscroll_ignore
	dec	[window_line_number]
	mov	[window_line],esi
	jmp	moved_window
    vscroll_down:
	mov	eax,[sc.nPos]
	add	eax,[sc.nPage]
	cmp	eax,[sc.nMax]
	ja	vscroll_ignore
	mov	esi,[window_line]
    vscroll_skip_line:
	mov	esi,[esi]
	btr	esi,0
	jc	vscroll_skip_line
	or	esi,esi
	jz	vscroll_ignore
	inc	[window_line_number]
	mov	[window_line],esi
	jmp	moved_window
    vscroll_pos:
	mov	eax,[sc.nTrackPos]
	call	find_line
	or	esi,esi
	jz	vscroll_ignore
	mov	[window_line],esi
	mov	[window_line_number],ecx
	jmp	moved_window
    vscroll_pageup:
	mov	esi,[window_line]
	mov	ecx,[sc.nPage]
    scrolling_up:
	mov	eax,[esi+4]
	or	eax,eax
	jz	scroll_ok
	dec	[window_line_number]
	mov	esi,eax
	loop	scrolling_up
	jmp	scroll_ok
    vscroll_pagedown:
	mov	esi,[window_line]
	mov	eax,[sc.nPos]
	add	eax,[sc.nPage]
	mov	ecx,[sc.nMax]
	sub	ecx,eax
	inc	ecx
	cmp	ecx,[sc.nPage]
	jbe	scroll_down
	mov	ecx,[sc.nPage]
    scrolling_down:
	mov	eax,[esi]
	btr	eax,0
	jnc	scroll_next_line
	mov	esi,eax
	jmp	scrolling_down
    scroll_next_line:
	or	eax,eax
	jz	scroll_ok
	inc	[window_line_number]
	mov	esi,eax
	loop	scrolling_down
    scroll_ok:
	mov	[window_line],esi
	jmp	moved_window
wm_mousewheel:
	mov	esi,[window_line]
	mov	eax,[wparam]
	sar	eax,16
	cdq
	mov	ecx,120
	idiv	ecx
	imul	eax,[whell_scroll_lines]
	mov	ecx,eax
	cmp	ecx,0
	jg	scroll_up
	neg	ecx
	jnz	scroll_down
	jmp	done
wm_size:
	call	update_positions
	mov	eax,[screen_width]
	mul	[screen_height]
	mov	[screen_size],eax
	mov	[screen_offset],0
	shl	eax,2
	or	eax,eax
	jz	screen_allocated
	mov	ebx,eax
	mov	eax,[screen_base]
	or	eax,eax
	jz	screen_released
	invoke	VirtualFree,eax,0,MEM_RELEASE
    screen_released:
	invoke	VirtualAlloc,0,ebx,MEM_COMMIT,PAGE_READWRITE
    screen_allocated:
	mov	[screen_base],eax
	call	update_screen
	lea	ebx,[rect]
	invoke	GetClientRect,[hwnd],ebx
	invoke	InvalidateRect,[hwnd],ebx,FALSE
	jmp	done
wm_keydown:
	lea	ebx,[kbstate]
	invoke	GetKeyboardState,ebx
	cmp	[was_selection],0
	jne	process_key
	mov	eax,[caret_line]
	mov	[selection_line],eax
	mov	eax,[caret_position]
	mov	[selection_position],eax
	mov	eax,[caret_line_number]
	mov	[selection_line_number],eax
    process_key:
	mov	eax,[wparam]
	cmp	eax,VK_LEFT
	je	left_key
	cmp	eax,VK_RIGHT
	je	right_key
	cmp	eax,VK_UP
	je	up_key
	cmp	eax,VK_DOWN
	je	down_key
	cmp	eax,VK_BACK
	je	backspace_key
	cmp	eax,VK_HOME
	je	home_key
	cmp	eax,VK_END
	je	end_key
	cmp	eax,VK_PGUP
	je	pgup_key
	cmp	eax,VK_PGDN
	je	pgdn_key
	cmp	eax,VK_DELETE
	je	del_key
	cmp	eax,VK_INSERT
	je	ins_key
	cmp	eax,VK_F6
	je	f6_key
	cmp	eax,VK_APPS
	je	menu_key
	test	[kbstate+VK_CONTROL],80h
	jz	convert_to_ascii
	cmp	eax,'Y'
	je	ctrl_y_key
    convert_to_ascii:
	mov	ax,word [lparam+2]
	and	eax,7Fh
	lea	ebx,[kbstate]
	lea	edx,[char]
	invoke	ToAscii,[wparam],eax,ebx,edx,FALSE
	cmp	eax,2
	je	two_characters
	or	eax,eax
	jz	ignore
	jg	process_character
	test	[editor_style],AES_REVIVEDEADKEYS
	je	ignore
	lea	edx,[char]
	invoke	ToAscii,VK_SPACE,0,ebx,edx,FALSE
	jmp	process_character
      two_characters:
	movzx	eax,byte [char+1]
	invoke	PostMessage,[hwnd],WM_CHAR,eax,1 shl 31
      process_character:
	mov	al,byte [char]
	cmp	al,20h
	jae	character
	cmp	al,0Dh
	je	character
	cmp	al,9
	je	character
	jmp	ignore
  left_key:
	test	[kbstate+VK_MENU],80h
	jnz	scroll_left
	test	[kbstate+VK_CONTROL],80h
	jnz	word_left
	cmp	[caret_position],0
	jle	moved_caret
	dec	[caret_position]
	jmp	moved_caret
  right_key:
	test	[kbstate+VK_MENU],80h
	jnz	scroll_right
	test	[kbstate+VK_CONTROL],80h
	jnz	word_right
	mov	eax,[caret_position]
	cmp	eax,[maximum_position]
	jae	moved_caret
	inc	[caret_position]
	jmp	moved_caret
  up_key:
	test	[kbstate+VK_MENU],80h
	jnz	scroll_up
	call	move_line_up
	jmp	moved_caret
  down_key:
	test	[kbstate+VK_MENU],80h
	jnz	scroll_down
	call	move_line_down
	jmp	moved_caret
  home_key:
	test	[kbstate+VK_CONTROL],80h
	jnz	screen_home
	mov	[caret_position],0
	jmp	moved_caret
  end_key:
	test	[kbstate+VK_CONTROL],80h
	jnz	screen_end
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
	test	[kbstate+VK_CONTROL],80h
	jnz	text_home
	call	move_page_up
	jmp	moved_caret
  pgdn_key:
	test	[kbstate+VK_CONTROL],80h
	jnz	text_end
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
	je	scroll_done
	dec	[window_position]
	jmp	scroll_done
  scroll_right:
	inc	[window_position]
	jmp	scroll_done
  scroll_up:
	mov	esi,[window_line]
	mov	esi,[esi+4]
	or	esi,esi
	jz	scroll_done
	mov	[window_line],esi
	dec	[window_line_number]
	jmp	scroll_done
  scroll_down:
	mov	esi,[window_line]
      find_next_window_line:
	mov	esi,[esi]
	btr	esi,0
	jc	find_next_window_line
	or	esi,esi
	jz	scroll_done
	mov	[window_line],esi
	inc	[window_line_number]
      scroll_done:
	cmp	[was_selection],0
	jne	moved_window
	mov	[selection_line],0
	jmp	moved_window
  ins_key:
	test	[kbstate+VK_MENU],80h
	jnz	switch_blocks
	test	[kbstate+VK_CONTROL],80h
	jnz	wm_copy
	test	[kbstate+VK_SHIFT],80h
	jnz	wm_paste
	xor	[editor_mode],AEMODE_OVERWRITE
	mov	[notification],AEN_MODECHANGE
	call	create_caret
	cmp	[was_selection],1
	je	done
	mov	[selection_line],0
	jmp	done
  del_key:
	test	[kbstate+VK_CONTROL],80h
	jnz	wm_clear
	test	[kbstate+VK_SHIFT],80h
	jnz	wm_cut
	cmp	[was_selection],0
	je	no_selection_on_del
	test	[editor_style],AES_SECURESEL
	jz	wm_clear
    no_selection_on_del:
	mov	esi,[caret_line]
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	delete_char
	call	check_line_length
	cmp	ecx,[caret_position]
	ja	delete_char
	cmp	dword [esi],0
	je	ignore
	call	store_status_for_undo
	call	cut_line_break
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_altered
    delete_char:
	mov	[current_operation],VK_DELETE
	cmp	[last_operation],VK_DELETE
	je	undo_delete_ok
	call	store_status_for_undo
    undo_delete_ok:
	call	delete_character
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_altered
  backspace_key:
	test	[kbstate+VK_MENU],80h
	jnz	wm_undo
	cmp	[was_selection],0
	je	no_selection_to_clear
	test	[editor_style],AES_SECURESEL
	jz	wm_clear
    no_selection_to_clear:
	cmp	[caret_position],0
	je	line_back
	test	[kbstate+VK_CONTROL],80h
	jnz	word_back
	mov	[current_operation],VK_BACK
	cmp	[last_operation],VK_BACK
	je	undo_back_ok
	call	store_status_for_undo
    undo_back_ok:
	dec	[caret_position]
	call	delete_character
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_altered
    line_back:
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	ignore
	mov	esi,[caret_line]
	mov	esi,[esi+4]
	or	esi,esi
	jz	ignore
	call	store_status_for_undo
	mov	[caret_line],esi
	dec	[caret_line_number]
	call	check_line_length
	mov	[caret_position],ecx
	call	cut_line_break
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_altered
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
	jmp	text_altered
  character:
	cmp	al,0Dh
	je	enter_key
	cmp	al,9
	je	tab_key
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
	jmp	text_altered
    no_selection_to_replace:
	mov	[current_operation],VK_SPACE
	cmp	[last_operation],VK_SPACE
	je	character_undo_ok
	call	store_status_for_undo
    character_undo_ok:
	call	put_character
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_altered
  tab_key:
	call	store_status_for_undo
	cmp	[was_selection],0
	je	tab_securesel
	test	[editor_style],AES_SECURESEL
	jnz	tab_securesel
	call	delete_block
    tab_securesel:
	call	tabulate
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_altered
  enter_key:
	call	store_status_for_undo
	cmp	[was_selection],0
	je	enter_secureselection_ok
	test	[editor_style],AES_SECURESEL
	jnz	enter_secureselection_ok
	call	delete_block
    enter_secureselection_ok:
	call	carriage_return
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	text_altered
	mov	esi,[caret_line]
	call	check_line_length
	jmp	text_altered
  ctrl_y_key:
	call	remove_line
	jmp	text_altered
  f6_key:
	call	duplicate_line
	jmp	text_altered
  menu_key:
	cmp	[menu_handle],0
	je	ignore
	lea	ebx,[point]
	mov	eax,[caret_x]
	mov	[ebx+POINT.x],eax
	mov	eax,[caret_y]
	add	eax,[font_height]
	mov	[ebx+POINT.y],eax
	invoke	ClientToScreen,[hwnd],ebx
	invoke	TrackPopupMenu,[menu_handle],TPM_RIGHTBUTTON,[ebx+POINT.x],[ebx+POINT.y],0,[menu_window],0
	jmp	ignore
wm_syskeydown:
	mov	eax,[wparam]
	cmp	eax,VK_INSERT
	je	switch_blocks
	cmp	eax,VK_BACK
	je	wm_undo
	cmp	eax,VK_LEFT
	je	scroll_left
	cmp	eax,VK_RIGHT
	je	scroll_right
	cmp	eax,VK_UP
	je	scroll_up
	cmp	eax,VK_DOWN
	je	scroll_down
	mov	al,[last_operation]
	mov	[current_operation],al
	invoke	ShowCaret,[hwnd]
	jmp	defwndproc
  switch_blocks:
	xor	[editor_mode],AEMODE_VERTICALSEL
	mov	[notification],AEN_MODECHANGE
	cmp	[was_selection],0
	je	ignore
	jmp	moved_window
wm_char:
	test	[lparam],1 shl 31
	jz	ignore
	mov	eax,[wparam]
	jmp	character
wm_lbuttondown:
	cmp	[focus],0
	jne	focus_ok
	invoke	SetFocus,[hwnd]
	mov	esi,[editor_memory]
	add	esi,16
	lea	edi,[editor_data]
	mov	ecx,editor_data_size
	rep	movsb
    focus_ok:
	lea	ebx,[kbstate]
	invoke	GetKeyboardState,ebx
	cmp	[was_selection],0
	jne	selection_ok
	mov	eax,[caret_line]
	mov	[selection_line],eax
	mov	eax,[caret_position]
	mov	[selection_position],eax
	mov	eax,[caret_line_number]
	mov	[selection_line_number],eax
    selection_ok:
	call	get_mouse_position
	invoke	SetCapture,[hwnd]
	or	[mouse_select],-1
	jmp	moved_caret
    get_mouse_position:
	mov	ax,word [lparam]
	cwde
	cdq
	test	[editor_style],AES_CONSOLECARET
	jnz	get_mouse_column
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	get_mouse_column
	mov	ebx,[font_width]
	shr	ebx,1
	adc	eax,ebx
      get_mouse_column:
	idiv	[font_width]
	add	eax,[window_position]
	cmp	eax,[maximum_position]
	jg	mouse_out_of_line
	cmp	eax,0
	jge	click_position_ok
	xor	eax,eax
	jmp	click_position_ok
    mouse_out_of_line:
	mov	eax,[maximum_position]
    click_position_ok:
	mov	[caret_position],eax
	mov	ax,word [lparam+2]
	cwde
	cdq
	idiv	[font_height]
	add	eax,[window_line_number]
	cmp	eax,0
	jg	click_line_ok
	mov	eax,1
    click_line_ok:
	call	find_line
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
	retn
wm_lbuttonup:
	invoke	ReleaseCapture
	mov	[mouse_select],0
	jmp	done
wm_mousemove:
	cmp	[mouse_select],0
	je	ignore
	cmp	[was_selection],0
	jne	select
	mov	eax,[caret_line]
	mov	ebx,[caret_line_number]
	mov	[selection_line],eax
	mov	[selection_line_number],ebx
	mov	eax,[caret_position]
	mov	[selection_position],eax
    select:
	call	get_mouse_position
	jmp	moved_selection
wm_lbuttondblclk:
	mov	[mouse_select],0
	call	get_mouse_position
	call	get_word_at_caret
	mov	[selection_position],edx
	add	edx,ecx
	mov	[caret_position],edx
	mov	eax,[caret_line]
	mov	[selection_line],eax
	mov	eax,[caret_line_number]
	mov	[selection_line_number],eax
	jmp	moved_selection
wm_rbuttondown:
	cmp	[menu_handle],0
	je	ignore
	lea	eax,[point]
	invoke	GetCursorPos,eax
	invoke	TrackPopupMenu,[menu_handle],TPM_RIGHTBUTTON,[point.x],[point.y],0,[menu_window],0
	jmp	ignore
wm_copy:
	cmp	[was_selection],0
	je	ignore
	call	copy_to_clipboard
	jmp	ignore
    copy_to_clipboard:
	call	get_block_length
	inc	ecx
	invoke	GlobalAlloc,GMEM_MOVEABLE+GMEM_DDESHARE,ecx
	mov	ebx,eax
	invoke	GlobalLock,ebx
	mov	edi,eax
	push	ebx
	call	copy_block
	pop	ebx
	invoke	GlobalUnlock,ebx
	invoke	OpenClipboard,[hwnd]
	invoke	EmptyClipboard
	invoke	SetClipboardData,CF_TEXT,ebx
	or	eax,eax
	jz	copy_failed
	invoke	CloseClipboard
	retn
    copy_failed:
	invoke	GlobalFree,ebx
	retn
wm_cut:
	cmp	[was_selection],0
	je	ignore
	call	copy_to_clipboard
wm_clear:
	cmp	[was_selection],0
	je	ignore
	call	store_status_for_undo
	call	delete_block
	mov	[selection_line],0
	jmp	text_changed
wm_paste:
	call	store_status_for_undo
	cmp	[was_selection],0
	je	paste_secureselection_ok
	test	[editor_style],AES_SECURESEL
	jnz	paste_secureselection_ok
	call	delete_block
    paste_secureselection_ok:
	invoke	OpenClipboard,NULL
	invoke	GetClipboardData,CF_TEXT
	or	eax,eax
	jnz	do_paste
	invoke	CloseClipboard
	jmp	ignore
    do_paste:
	or	[clipboard_opened],-1
	push	eax
	invoke	GlobalLock,[esp]
	mov	esi,eax
	call	insert_block
	jc	paste_failed
	pop	ebx
	invoke	GlobalUnlock,ebx
	invoke	CloseClipboard
	mov	[clipboard_opened],0
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
	jmp	text_changed
    no_selection_after_paste:
	mov	[selection_line],0
	jmp	text_changed
    paste_failed:
	call	undo_changes
	pop	ebx
	invoke	GlobalUnlock,ebx
	invoke	CloseClipboard
	mov	[clipboard_opened],0
	jmp	text_changed
wm_settext:
	mov	esi,[lparam]
	call	set_text
	mov	[return_value],TRUE
	jmp	text_changed
wm_gettextlength:
	mov	esi,[first_line]
    count_text_length:
	mov	eax,[esi+8]
	add	[return_value],eax
    skip_counted_line_segments:
	mov	esi,[esi]
	or	esi,esi
	jz	ignore
	btr	esi,0
	jc	skip_counted_line_segments
	add	[return_value],2
	jmp	count_text_length
wm_gettext:
	mov	esi,[first_line]
	mov	edi,[lparam]
	sub	[wparam],1
	jc	ignore
    copy_text:
	mov	ecx,[esi+8]
	test	[editor_style],AES_OPTIMALFILL
	jz	direct_copy
	cmp	ecx,8
	jb	direct_copy
	push	edi ecx
	mov	edi,[line_buffer]
	xor	edx,edx
	call	copy_from_line
	push	esi
	mov	esi,[line_buffer]
	mov	edi,[line_buffer]
	add	edi,[peak_line_length]
	mov	ebx,edi
	mov	ecx,[esp+4]
	xor	al,al
	rep	stosb
	mov	ecx,[esp+4]
	invoke	syntax_proc,esi,ecx,ebx
	pop	esi ecx edi
	mov	eax,[return_value]
	add	eax,ecx
	cmp	eax,[wparam]
	jbe	optimal_fill_size_ok
	mov	ecx,[wparam]
	sub	eax,[return_value]
    optimal_fill_size_ok:
	push	esi edi
	mov	esi,[line_buffer]
	mov	ebx,[peak_line_length]
	jecxz	optimal_fill_done
	xor	edx,edx
    optimal_fill:
	lodsb
	cmp	al,20h
	jne	store_character
	cmp	byte [esi-1+ebx],0
	jne	store_character
	mov	eax,esi
	sub	eax,[line_buffer]
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
	pop	eax
	neg	eax
	add	eax,edi
	add	[return_value],eax
	pop	esi
	jmp	line_copied
    direct_copy:
	mov	eax,[return_value]
	add	eax,ecx
	cmp	eax,[wparam]
	jbe	direct_copy_size_ok
	mov	ecx,[wparam]
	sub	eax,[return_value]
    direct_copy_size_ok:
	add	[return_value],ecx
	xor	edx,edx
	call	copy_from_line
    line_copied:
	or	esi,esi
	jz	text_copied
	mov	ecx,[return_value]
	add	ecx,2
	cmp	ecx,[wparam]
	ja	text_copied
	mov	ax,0A0Dh
	stosw
	mov	[return_value],ecx
	cmp	ecx,[wparam]
	jne	copy_text
    text_copied:
	xor	al,al
	stosb
	jmp	ignore
aem_getlinelength:
	mov	esi,[caret_line]
	mov	eax,[wparam]
	or	eax,eax
	jz	return_length
	call	find_line
	cmp	ecx,[wparam]
	je	return_length
	or	[return_value],-1
	jmp	ignore
    return_length:
	mov	ecx,[esi+8]
	mov	[return_value],ecx
	jmp	ignore
aem_getline:
	mov	esi,[caret_line]
	mov	eax,[wparam]
	or	eax,eax
	jz	get_line
	call	find_line
	cmp	ecx,[wparam]
	jne	ignore
    get_line:
	mov	ecx,[esi+8]
	mov	[return_value],ecx
	mov	edi,[lparam]
	xor	edx,edx
	call	copy_from_line
	xor	al,al
	stosb
	jmp	ignore
wm_setfont:
	mov	esi,[wparam]
	or	esi,esi
	jnz	get_metrics
	mov	esi,[asmedit_font]
    get_metrics:
	invoke	GetDC,[hwnd]
	mov	ebx,eax
	invoke	SelectObject,ebx,esi
	lea	edi,[char]
	mov	byte [edi],20h
	lea	eax,[size]
	invoke	GetTextExtentPoint32,ebx,edi,1,eax
	lea	eax,[tm]
	invoke	GetTextMetrics,ebx,eax
	invoke	ReleaseDC,[hwnd],ebx
	test	[tm.tmPitchAndFamily],TMPF_FIXED_PITCH
	jnz	ignore
	mov	[return_value],esi
	mov	[editor_font],esi
	mov	eax,[size.cy]
	mov	[font_height],eax
	mov	eax,[size.cx]
	mov	[font_width],eax
	call	create_caret
	mov	eax,[lparam]
	mov	[redraw_now],al
	jmp	wm_size
wm_getfont:
	mov	eax,[editor_font]
	cmp	eax,[asmedit_font]
	je	ignore
	mov	[return_value],eax
	jmp	ignore
wm_undo:
	cmp	[undo_data],0
	je	ignore
	call	undo_changes
	call	create_caret
	mov	[last_operation],0
	jmp	text_changed
em_canundo:
	mov	eax,[undo_data]
	or	eax,eax
	jz	ignore
	mov	[return_value],TRUE
	jmp	ignore
em_emptyundobuffer:
	call	clear_undo_data
	jmp	ignore
em_replacesel:
	test	[editor_mode],AEMODE_VERTICALSEL + AEMODE_OVERWRITE
	jnz	ignore
	cmp	[undo_data],0
	je	replacesel_undopoint
	cmp	[wparam],0
	je	replace_undo_ok
    replacesel_undopoint:
	call	store_status_for_undo
    replace_undo_ok:
	cmp	[was_selection],0
	je	simple_replace
	push	[caret_line_number]
	push	[caret_position]
	call	delete_block
	pop	edx ecx
	cmp	ecx,[caret_line_number]
	jne	simple_replace
	cmp	edx,[caret_position]
	jne	simple_replace
	mov	esi,[lparam]
	call	insert_block
	mov	esi,[caret_line]
	mov	ecx,[caret_line_number]
	mov	edx,[caret_position]
	xchg	esi,[selection_line]
	xchg	ecx,[selection_line_number]
	xchg	edx,[selection_position]
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
	mov	[caret_position],edx
	mov	[notification],AEN_TEXTCHANGE
	jmp	text_changed
    simple_replace:
	mov	esi,[lparam]
	call	insert_block
	mov	[notification],AEN_TEXTCHANGE
	jmp	text_changed
aem_setmode:
	mov	eax,[wparam]
	xchg	[editor_mode],eax
	cmp	eax,[editor_mode]
	je	ignore
	mov	[notification],AEN_MODECHANGE
	call	create_caret
	cmp	[was_selection],0
	jne	moved_window
	jmp	done
aem_getmode:
	mov	eax,[editor_mode]
	mov	[return_value],eax
	jmp	ignore
aem_setsyntaxhighlight:
	mov	eax,[wparam]
	mov	ebx,[lparam]
	mov	[syntax_colors],eax
	mov	[syntax_proc],ebx
	or	eax,eax
	jnz	wm_size
	mov	[syntax_proc],SyntaxProc
	jmp	wm_size
aem_setrightclickmenu:
	mov	eax,[wparam]
	mov	ebx,[lparam]
	mov	[menu_handle],eax
	mov	[menu_window],ebx
	jmp	ignore
aem_settextcolor:
	mov	eax,[wparam]
	mov	ebx,[lparam]
	mov	[text_color],eax
	mov	[background_color],ebx
	jmp	wm_size
aem_setselcolor:
	mov	eax,[wparam]
	mov	ebx,[lparam]
	mov	[selection_text],eax
	mov	[selection_background],ebx
	jmp	wm_size
aem_setpos:
	mov	edi,[wparam]
	virtual at edi
	pos	AEPOS
	end	virtual
	cmp	[selection_line],0
	jne	selection_current_ok
	mov	eax,[caret_line]
	mov	[selection_line],eax
	mov	eax,[caret_line_number]
	mov	[selection_line_number],eax
	mov	eax,[caret_position]
	mov	[selection_position],eax
    selection_current_ok:
	mov	eax,[pos.selectionLine]
	or	eax,eax
	jz	selection_line_ok
	call	find_line
	mov	[selection_line],esi
	mov	[selection_line_number],ecx
    selection_line_ok:
	mov	eax,[pos.selectionPosition]
	sub	eax,1
	jc	selection_position_set
	mov	[selection_position],eax
    selection_position_set:
	mov	eax,[pos.caretLine]
	or	eax,eax
	jz	caret_line_ok
	call	find_line
	mov	[caret_line],esi
	mov	[caret_line_number],ecx
    caret_line_ok:
	mov	eax,[pos.caretPosition]
	sub	eax,1
	jc	moved_selection
	mov	[caret_position],eax
	jmp	moved_selection
aem_getpos:
	mov	edi,[wparam]
	mov	eax,[caret_line_number]
	mov	[pos.selectionLine],eax
	mov	[pos.caretLine],eax
	mov	eax,[caret_position]
	inc	eax
	mov	[pos.selectionPosition],eax
	mov	[pos.caretPosition],eax
	cmp	[selection_line],0
	je	ignore
	mov	eax,[selection_line_number]
	mov	[pos.selectionLine],eax
	mov	eax,[selection_position]
	inc	eax
	mov	[pos.selectionPosition],eax
	jmp	ignore
aem_findfirst:
	mov	esi,[lparam]
	mov	eax,[wparam]
	call	find_first
	jnc	show_found_text
	jmp	ignore
aem_findnext:
	call	find_next
	jc	ignore
    show_found_text:
	mov	eax,[caret_position]
	xchg	eax,[selection_position]
	mov	[caret_position],eax
	call	update_window
	call	let_caret_appear
	call	update_positions
	mov	eax,[caret_position]
	xchg	eax,[selection_position]
	mov	[caret_position],eax
	mov	[return_value],TRUE
	jmp	moved_selection
aem_canfindnext:
	cmp	[search_data],0
	je	ignore
	mov	[return_value],TRUE
	jmp	ignore
aem_getwordatcaret:
	cmp	[wparam],0
	je	ignore
	call	get_word_at_caret
	mov	edi,[lparam]
	mov	eax,[wparam]
	dec	eax
	jz	word_copied
	cmp	ecx,eax
	jbe	copy_word
	mov	ecx,eax
    copy_word:
	mov	[return_value],ecx
	mov	esi,[caret_line]
	call	copy_from_line
    word_copied:
	xor	al,al
	stosb
	jmp	ignore
moved_caret:
	test	[kbstate+VK_SHIFT],80h
	jnz	moved_selection
	mov	[selection_line],0
moved_selection:
	mov	[notification],AEN_POSCHANGE
	jmp	update_all
moved_window:
	call	update_positions
	jmp	refresh
text_altered:
	mov	[selection_line],0
text_changed:
	mov	[notification],AEN_TEXTCHANGE
	mov	eax,[peak_line_length]
	xor	edx,edx
	mov	ebx,SEGMENT_DATA_LENGTH
	div	ebx
	inc	eax
	mul	ebx
	shl	eax,1
	cmp	eax,[line_buffer_size]
	je	update_all
	mov	[line_buffer_size],eax
	invoke	VirtualAlloc,0,[line_buffer_size],MEM_COMMIT,PAGE_READWRITE
	or	eax,eax
	jz	memory_shortage
	xchg	[line_buffer],eax
	invoke	VirtualFree,eax,0,MEM_RELEASE
    update_all:
	call	update_window
	call	let_caret_appear
	call	update_positions
    refresh:
	cmp	[screen_base],0
	je	wm_size
	mov	eax,[screen_size]
	shl	eax,1
	xor	[screen_offset],eax
	call	update_screen
	mov	esi,[screen_base]
	mov	eax,[screen_size]
	lea	edi,[esi+eax*2]
	mov	[rect.top],0
	mov	edx,[font_height]
	mov	[rect.bottom],edx
	mov	ecx,[screen_height]
    refresh_screen:
	push	ecx
	mov	ebx,[screen_size]
	mov	edx,[font_width]
	xor	eax,eax
	mov	[rect.left],eax
	mov	[rect.right],eax
	mov	ecx,[screen_width]
    refresh_line:
	mov	al,[esi]
	mov	ah,[esi+ebx]
	cmp	al,[edi]
	jne	refresh_changed
	cmp	ah,[edi+ebx]
	jne	refresh_changed
	inc	esi
	inc	edi
	add	[rect.left],edx
	add	[rect.right],edx
	loop	refresh_line
	jmp	refresh_next_line
    refresh_changed:
	mov	al,[esi]
	mov	ah,[esi+ebx]
	inc	esi
	add	[rect.right],edx
	cmp	al,[edi]
	jne	changed_more
	cmp	ah,[edi+ebx]
	jne	changed_more
	inc	edi
	jmp	invalidate
    changed_more:
	inc	edi
	loop	refresh_changed
    invalidate:
	push	ecx edx
	lea	eax,[rect]
	invoke	InvalidateRect,[hwnd],eax,FALSE
	pop	edx ecx
	mov	eax,[rect.right]
	mov	[rect.left],eax
	jecxz	refresh_next_line
	loop	refresh_line
    refresh_next_line:
	mov	eax,[font_height]
	add	[rect.top],eax
	add	[rect.bottom],eax
	pop	ecx
	dec	ecx
	jnz	refresh_screen
	jmp	done
ignore:
	mov	dl,[last_operation]
	mov	[current_operation],dl
	cmp	[was_selection],0
	jne	done
	mov	[selection_line],0
done:
	cmp	[focus],0
	je	caret_ok
	call	update_caret_position
	invoke	ShowCaret,[hwnd]
    caret_ok:
	lea	esi,[editor_data]
	mov	edi,[editor_memory]
	add	edi,16
	mov	ecx,editor_data_size
	rep	movsb
	cmp	[notification],0
	je	notification_ok
	invoke	GetWindowLong,[hwnd],GWL_HWNDPARENT
	mov	edi,eax
	invoke	GetWindowLong,[hwnd],GWL_ID
	movzx	ebx,[notification]
	shl	ebx,16
	or	eax,ebx
	invoke	SendMessage,edi,WM_COMMAND,eax,[hwnd]
    notification_ok:
	cmp	[redraw_now],0
	je	redraw_ok
	invoke	UpdateWindow,[hwnd]
    redraw_ok:
	mov	eax,[return_value]
finish_wndproc:
	pop	edi esi ebx
	ret
not_enough_memory:
	lea	esp,[editor_memory-10h]
	mov	[notification],AEN_OUTOFMEMORY
	or	[return_value],-1
	cmp	[clipboard_opened],0
	je	ignore
	invoke	CloseClipboard
	mov	[clipboard_opened],0
	jmp	ignore
get_memory:
	invoke	VirtualAlloc,0,ecx,MEM_COMMIT,PAGE_READWRITE
	mov	ebx,eax
	retn
release_memory:
	invoke	VirtualFree,ebx,0,MEM_RELEASE
	retn
update_positions:
	lea	eax,[rect]
	invoke	GetClientRect,[hwnd],eax
	mov	eax,[rect.right]
	sub	eax,[rect.left]
	cdq
	div	[font_width]
	mov	[window_width],eax
	add	edx,-1
	adc	eax,0
	mov	[screen_width],eax
	mov	eax,[rect.bottom]
	sub	eax,[rect.top]
	cdq
	div	[font_height]
	mov	[window_height],eax
	add	edx,-1
	adc	eax,0
	mov	[screen_height],eax
	call	update_window
    setup_vscroll:
	mov	ecx,[lines_count]
	mov	[sc.cbSize],sizeof.SCROLLINFO
	mov	[sc.fMask],SIF_DISABLENOSCROLL+SIF_RANGE+SIF_PAGE+SIF_POS
	mov	[sc.nMin],1
	mov	[sc.nMax],ecx
	mov	eax,[window_height]
	mov	[sc.nPage],eax
	mov	edx,[window_line_number]
	mov	[sc.nPos],edx
	test	[editor_style],WS_VSCROLL
	jz	setup_hscroll
	lea	eax,[sc]
	invoke	SetScrollInfo,[hwnd],SB_VERT,eax,TRUE
    setup_hscroll:
	mov	ecx,[maximum_position]
	mov	[sc.nMin],0
	mov	[sc.nMax],ecx
	mov	eax,[window_width]
	mov	[sc.nPage],eax
	mov	edx,[window_position]
	mov	[sc.nPos],edx
	test	[editor_style],WS_HSCROLL
	jz	setup_caret
	lea	eax,[sc]
	invoke	SetScrollInfo,[hwnd],SB_HORZ,eax,TRUE
    setup_caret:
	mov	eax,[font_width]
	mov	edx,[caret_position]
	sub	edx,[window_position]
	imul	eax,edx
	mov	[caret_x],eax
	mov	eax,[font_height]
	mov	edx,[caret_line_number]
	sub	edx,[window_line_number]
	imul	eax,edx
	mov	[caret_y],eax
	retn
create_caret:
	xor	eax,eax
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	block_caret
	test	[editor_style],AES_CONSOLECARET
	jnz	console_caret
	invoke	CreateCaret,[hwnd],NULL,0,[font_height]
	jmp	update_caret_position
    block_caret:
	invoke	CreateCaret,[hwnd],NULL,[font_width],[font_height]
	jmp	update_caret_position
    console_caret:
	invoke	CreateCaret,[hwnd],NULL,[font_width],2
update_caret_position:
	mov	eax,[caret_y]
	test	[editor_mode],AEMODE_OVERWRITE
	jnz	set_position
	test	[editor_style],AES_CONSOLECARET
	jz	set_position
	add	eax,[font_height]
	sub	eax,2
    set_position:
	invoke	SetCaretPos,[caret_x],eax
	retn
update_screen:
	mov	edi,[screen_base]
	or	edi,edi
	jz	screen_prepared
	add	edi,[screen_offset]
	mov	ebx,[screen_size]
	add	ebx,edi
	push	ebx
	mov	ecx,[screen_height]
	mov	esi,[window_line]
    prepare_screen:
	push	ecx
	test	byte [esi],1
	jnz	prepare_long_line
	push	esi
	add	esi,SEGMENT_HEADER_LENGTH
	mov	ecx,SEGMENT_DATA_LENGTH
	call	prepare_line
	pop	esi
	mov	esi,[esi]
	jmp	prepare_next_line
    prepare_long_line:
	push	edi
	mov	edi,[line_buffer]
	xor	eax,eax
    combine_long_line_segments:
	add	esi,SEGMENT_HEADER_LENGTH
	mov	ecx,SEGMENT_DATA_LENGTH
	rep	movsb
	add	eax,SEGMENT_DATA_LENGTH
	mov	esi,[esi-SEGMENT_LENGTH]
	btr	esi,0
	jc	combine_long_line_segments
	mov	ecx,eax
	mov	eax,esi
	mov	esi,edi
	sub	esi,ecx
	pop	edi
	push	eax
	call	prepare_line
	pop	esi
    prepare_next_line:
	pop	ecx
	dec	ecx
	jz	prepare_selection
	or	esi,esi
	jnz	prepare_screen
    prepare_empty_lines:
	imul	ecx,[screen_width]
	xor	al,al
	mov	edx,ecx
	rep	stosb
	xchg	edi,ebx
	mov	ecx,edx
	rep	stosb
    prepare_selection:
	pop	ebx
	test	[editor_style],ES_NOHIDESEL
	jnz	hideselection_ok
	cmp	[focus],0
	je	screen_prepared
    hideselection_ok:
	cmp	[selection_line],0
	je	screen_prepared
	mov	eax,[window_line_number]
	mov	esi,[selection_line_number]
	mov	edi,[caret_line_number]
	sub	esi,eax
	sub	edi,eax
	mov	ecx,[window_position]
	mov	eax,[selection_position]
	mov	edx,[caret_position]
	sub	eax,ecx
	sub	edx,ecx
	cmp	esi,edi
	jle	selection_boundaries_ok
	xchg	esi,edi
	xchg	eax,edx
    selection_boundaries_ok:
	mov	ecx,[screen_height]
	cmp	edi,0
	jl	screen_prepared
	cmp	esi,ecx
	jge	screen_prepared
	cmp	esi,edi
	je	prepare_vsel
	test	[editor_mode],AEMODE_VERTICALSEL
	jz	prepare_hsel
    prepare_vsel:
	cmp	eax,edx
	jle	vselection_boundaries_ok
	xchg	eax,edx
    vselection_boundaries_ok:
	cmp	esi,0
	jge	vselection_start_ok
	xor	esi,esi
    vselection_start_ok:
	inc	edi
	cmp	edi,ecx
	jle	vselection_end_ok
	mov	edi,ecx
    vselection_end_ok:
	mov	ecx,[screen_width]
	cmp	edx,0
	jl	screen_prepared
	cmp	eax,ecx
	jge	screen_prepared
	cmp	eax,0
	jge	vselection_line_start_ok
	xor	eax,eax
    vselection_line_start_ok:
	cmp	edx,ecx
	jle	vselection_line_end_ok
	mov	edx,ecx
    vselection_line_end_ok:
	mov	ecx,edi
	sub	ecx,esi
	imul	esi,[screen_width]
	add	ebx,esi
    prepare_vselection_line:
	push	eax ecx
	mov	edi,ebx
	mov	ecx,edx
	sub	ecx,eax
	lea	edi,[ebx+eax]
	mov	al,80h
	rep	stosb
	add	ebx,[screen_width]
	pop	ecx eax
	loop	prepare_vselection_line
	jmp	screen_prepared
    prepare_hsel:
	cmp	esi,0
	jge	hselection_start_ok
	xor	esi,esi
	xor	eax,eax
    hselection_start_ok:
	cmp	edi,ecx
	jl	hselection_end_ok
	mov	edi,ecx
	xor	edx,edx
    hselection_end_ok:
	inc	esi
	mov	ecx,edi
	sub	ecx,esi
	imul	ecx,[screen_width]
	imul	esi,[screen_width]
	lea	edi,[ebx+esi]
	neg	eax
	add	eax,[screen_width]
	cmp	eax,0
	jle	hselection_start_line_ok
	sub	edi,eax
	add	ecx,eax
	sub	eax,[screen_width]
	jle	hselection_start_line_ok
	add	edi,eax
	sub	ecx,eax
    hselection_start_line_ok:
	cmp	edx,0
	jle	hselection_end_line_ok
	add	ecx,edx
	sub	edx,[screen_width]
	jle	hselection_end_line_ok
	sub	ecx,edx
    hselection_end_line_ok:
	mov	al,80h
	rep	stosb
    screen_prepared:
	retn
    prepare_line:
	push	edi ecx
	mov	ecx,[esp]
	mov	edi,[line_buffer]
	add	edi,ecx
	shr	ecx,2
	xor	eax,eax
	rep	stosd
	mov	ecx,[esp]
	sub	edi,ecx
	invoke	syntax_proc,esi,ecx,edi
	mov	edx,edi
	pop	eax edi
	push	esi edx
	mov	edx,[window_position]
	lea	esi,[esi+edx]
	sub	eax,edx
	jnc	line_position_ok
	xor	eax,eax
	jmp	line_length_ok
    line_position_ok:
	cmp	eax,[screen_width]
	jbe	line_length_ok
	mov	eax,[screen_width]
    line_length_ok:
	mov	ecx,eax
	rep	movsb
	pop	esi
	add	esi,edx
	mov	ecx,eax
	xchg	edi,ebx
	rep	movsb
	xchg	edi,ebx
	pop	esi
	cmp	eax,[screen_width]
	jb	after_end_of_line
	retn
    after_end_of_line:
	mov	ecx,[screen_width]
	sub	ecx,eax
	xor	al,al
	mov	edx,ecx
	rep	stosb
	xchg	edi,ebx
	mov	ecx,edx
	rep	stosb
	xchg	edi,ebx
	retn

include '..\memory.inc'
include '..\navigate.inc'
include '..\edit.inc'
include '..\blocks.inc'
include '..\search.inc'
include '..\undo.inc'

endp

proc SyntaxProc lpLine,uChars,lpColors
	ret
endp
