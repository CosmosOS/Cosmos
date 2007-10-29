
; flat assembler IDE for Win32
; Copyright (c) 1999-2007, Tomasz Grysztar.
; All rights reserved.

format PE GUI 4.0
entry start
stack 20000h

include 'win32a.inc'
include 'asmedit.ash'

include '..\..\version.inc'

IDE_VERSION_STRING equ "0.93.13"

struct EDITITEM
  header      TC_ITEMHEADER
  hwnd	      dd ?
  pszpath     dd ?
ends

FM_NEW	       = WM_USER + 0
FM_OPEN        = WM_USER + 1
FM_SAVE        = WM_USER + 2
FM_COMPILE     = WM_USER + 3
FM_SELECT      = WM_USER + 4
FM_ASSIGN      = WM_USER + 5
FM_GETSELECTED = WM_USER + 6
FM_GETASSIGNED = WM_USER + 7
FM_GETHANDLE   = WM_USER + 8

CF_REPLACEPROMPT = 1

struct HH_AKLINK
  cbStruct     dd ?
  fReserved    dd ?
  pszKeywords  dd ?
  pszUrl       dd ?
  pszMsgText   dd ?
  pszMsgTitle  dd ?
  pszWindow    dd ?
  fIndexOnFail dd ?
ends

HH_DISPLAY_TOPIC  = 0
HH_KEYWORD_LOOKUP = 0Dh

section '.data' data readable writeable

  _caption db 'flat assembler ',VERSION_STRING,0
  _class db 'FASMW_IDE2',0
  _asmedit_class db 'ASMEDIT',0
  _listbox_class db 'LISTBOX',0
  _tabctrl_class db 'SysTabControl32',0
  _htmlhelp_library db 'HHCTRL.OCX',0
  _htmlhelp_api db 'HtmlHelpA',0

  _memory_error db 'Not enough memory to complete this operation.',0
  _loading_error db 'Could not load file %s.',0
  _run_object_error db 'Cannot execute object file.',0
  _saving_question db 'File was modified. Save it now?',0
  _not_found db 'Text not found.',0
  _replace_prompt db 'Replace this occurence?',0
  _untitled db 'Untitled',0
  _font_face db 'Courier New',0
  _row_column db 9,'%d,'
  _value db '%d'
  _null db 0
  _summary db '%d passes, %d.%d seconds, %d bytes.',0
  _summary_short db '%d passes, %d bytes.',0
  _assembler_error db 'Error: %s.',0
  _line_number db '%s [%d]',0
  _color db '%d,%d,%d',0
  _modified_status db 9,'Modified',0
  _pick_help db 'Pick help file',0
  _find db 'Find',0
  _replace db 'Replace',0

  _asm_extension db 'ASM',0

  _section_environment db 'Environment',0
  _section_compiler db 'Compiler',0
  _key_compiler_memory db 'Memory',0
  _key_compiler_priority db 'Priority',0
  _section_options db 'Options',0
  _key_options_securesel db 'SecureSelection',0
  _key_options_autobrackets db 'AutoBrackets',0
  _key_options_autoindent db 'AutoIndent',0
  _key_options_smarttabs db 'SmartTabs',0
  _key_options_optimalfill db 'OptimalFill',0
  _key_options_revivedeadkeys db 'ReviveDeadKeys',0
  _key_options_consolecaret db 'ConsoleCaret',0
  _key_options_oneinstanceonly db 'OneInstanceOnly',0
  _section_colors db 'Colors',0
  _key_color_text db 'Text',0
  _key_color_background db 'Background',0
  _key_color_seltext db 'SelectionText',0
  _key_color_selbackground db 'SelectionBackground',0
  _key_color_symbols db 'Symbols',0
  _key_color_numbers db 'Numbers',0
  _key_color_strings db 'Strings',0
  _key_color_comments db 'Comments',0
  _section_font db 'Font',0
  _key_font_face db 'Face',0
  _key_font_height db 'Height',0
  _key_font_width db 'Width',0
  _key_font_weight db 'Weight',0
  _key_font_italic db 'Italic',0
  _key_font_charset db 'CharSet',0
  _section_window db 'Window',0
  _key_window_top db 'Top',0
  _key_window_left db 'Left',0
  _key_window_right db 'Right',0
  _key_window_bottom db 'Bottom',0
  _key_window_maximized db 'Maximized',0
  _section_help db 'Help',0
  _key_help_path db 'Path',0

  _appearance_settings db 'Font',0
		       db 'Text color',0
		       db 'Background color',0
		       db 'Selection text color',0
		       db 'Selection background color',0
		       db 'Symbols color',0
		       db 'Numbers color',0
		       db 'Strings color',0
		       db 'Comments color',0
		       db 0

  _memory_settings db '1024',0
		   db '2048',0
		   db '4096',0
		   db '8192',0
		   db '16384',0
		   db '32768',0
		   db '65536',0
		   db 0

  _priority_settings db 'Idle',0
		     db 'Low',0
		     db 'Normal',0
		     db 'High',0
		     db 'Realtime',0
		     db 0

  asmedit_style dd AES_AUTOINDENT+AES_SMARTTABS+AES_OPTIMALFILL+AES_CONSOLECARET

  editor_colors rd 4
  asm_syntax_colors dd 0xF03030,0x009000,0x0000B0,0x808080

  preview_text db 0Dh,0Ah
	       db ' org 100h',0Dh,0Ah
	       db 0Dh,0Ah
	       db ' mov ah,09h ',' ; write',0Dh,0Ah
	       db ' mov dx,text',0Dh,0Ah
	       db ' int 21h',0Dh,0Ah
	       db ' int 20h',0Dh,0Ah
	       db 0Dh,0Ah
	       db ' text db "Hello!",24h',0Dh,0Ah
	       db 0
  preview_selection dd 1,5,1,6

  asm_filter db 'Assembler files',0,'*.ASM;*.INC;*.ASH',0
	     db 'All files',0,'*.*',0
	     db 0

  help_filter db 'Help files',0,'*.HLP;*.CHM',0
	      db 0

  whell_scroll_lines dd 3

  HtmlHelp dd 0

section '.udata' readable writeable

  hinstance dd ?
  hkey_main dd ?
  hmenu_main dd ?
  hmenu_edit dd ?
  hmenu_tab dd ?
  hacc dd ?
  hfont dd ?
  hwnd_main dd ?
  hwnd_status dd ?
  hwnd_tabctrl dd ?
  hwnd_history dd ?
  hwnd_asmedit dd ?
  hwnd_compiler dd ?
  hwnd_progress dd ?
  himl dd ?
  hthread dd ?
  hmem_display dd ?
  hmem_error_data dd ?
  hfile dd ?

  command_flags dd ?
  search_settings dd ?
  compiler_memory dd ?
  compiler_priority dd ?
  assigned_file dd ?
  program_arguments dd ?

  param_buffer rd 10h
  user_colors rd 40h
  name_buffer rb 100h
  search_string rb 1000h
  replace_string rb 1000h
  string_buffer rb 1000h
  help_path rb 1000h
  ini_path rb 1000h
  path_buffer rb 4000h

  msg MSG
  wc WNDCLASS
  rc RECT
  pt POINT
  ei EDITITEM
  font LOGFONT
  bm BITMAP
  tcht TC_HITTESTINFO
  wp WINDOWPLACEMENT
  aepos AEPOS
  ofn OPENFILENAME
  cf CHOOSEFONT
  cc CHOOSECOLOR
  systime SYSTEMTIME
  sinfo STARTUPINFO
  pinfo PROCESS_INFORMATION
  cp COPYDATASTRUCT

  bytes_count dd ?
  asmedit_font dd ?

  tmp_colors rd 8
  tmp_font LOGFONT
  backup_font LOGFONT

  hhkey HH_AKLINK

  case_table rb 100h

  status_update_disabled db ?

section '.code' code readable executable

include 'asmedit.inc'

  start:

	invoke	GetModuleHandle,0
	mov	[hinstance],eax

	invoke	GetCommandLine
	mov	esi,eax
	mov	edi,ini_path
      find_program_path:
	lodsb
	cmp	al,20h
	je	find_program_path
	cmp	al,22h
	je	quoted_program_path
	cmp	al,0Dh
	je	program_path_ok
	or	al,al
	jnz	get_program_path
	dec	esi
	jmp	program_path_ok
      get_program_path:
	stosb
	lodsb
	cmp	al,20h
	je	program_path_ok
	cmp	al,0Dh
	je	program_path_ok
	or	al,al
	jnz	get_program_path
	dec	esi
	jmp	program_path_ok
      quoted_program_path:
	lodsb
	cmp	al,22h
	je	program_path_ok
	cmp	al,0Dh
	je	program_path_ok
	stosb
	or	al,al
	jnz	quoted_program_path
	dec	esi
      program_path_ok:
	mov	[program_arguments],esi
	mov	ebx,edi
      find_program_extension:
	cmp	ebx,ini_path
	je	make_ini_extension
	dec	ebx
	mov	al,[ebx]
	cmp	al,'\'
	je	make_ini_extension
	cmp	al,'/'
	je	make_ini_extension
	cmp	al,'.'
	jne	find_program_extension
	mov	edi,ebx
	jmp	find_program_extension
      make_ini_extension:
	mov	eax,'.INI'
	stosd
	xor	al,al
	stosb
	invoke	GetFullPathName,ini_path,1000h,ini_path,param_buffer

	mov	[param_buffer],0
	stdcall GetIniBit,ini_path,_section_options,_key_options_oneinstanceonly,param_buffer,1
	cmp	[param_buffer],0
	je	create_new_window
	invoke	FindWindow,_class,NULL
	or	eax,eax
	jnz	window_already_exists

      create_new_window:
	invoke	LoadCursor,0,IDC_IBEAM
	mov	[wc.hCursor],eax
	mov	[wc.style],CS_GLOBALCLASS+CS_DBLCLKS
	mov	[wc.lpfnWndProc],AsmEdit
	mov	eax,[hinstance]
	mov	[wc.hInstance],eax
	mov	[wc.cbWndExtra],4
	xor	eax,eax
	mov	[wc.hbrBackground],eax
	mov	[wc.cbClsExtra],eax
	mov	[wc.lpszMenuName],eax
	mov	[wc.lpszClassName],_asmedit_class
	invoke	RegisterClass,wc
	or	eax,eax
	jz	end_loop
	invoke	CreateFont,0,0,0,0,0,FALSE,FALSE,FALSE,ANSI_CHARSET,OUT_RASTER_PRECIS,CLIP_DEFAULT_PRECIS,DEFAULT_QUALITY,FIXED_PITCH+FF_DONTCARE,NULL
	or	eax,eax
	jz	end_loop
	mov	[asmedit_font],eax

	mov	edi,case_table
	xor	ebx,ebx
	mov	esi,100h
      make_case_table:
	invoke	CharUpper,ebx
	stosb
	inc	bl
	dec	esi
	jnz	make_case_table
	mov	edi,characters
	mov	ecx,100h
	xor	al,al
      prepare_characters_table:
	stosb
	inc	al
	loop	prepare_characters_table
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

	invoke	LoadIcon,[hinstance],IDI_MAIN
	mov	[wc.hIcon],eax
	invoke	LoadCursor,0,IDC_ARROW
	mov	[wc.hCursor],eax
	mov	[wc.style],0
	mov	[wc.lpfnWndProc],MainWindow
	mov	[wc.cbClsExtra],0
	mov	[wc.cbWndExtra],0
	mov	eax,[hinstance]
	mov	[wc.hInstance],eax
	mov	[wc.hbrBackground],COLOR_BTNFACE+1
	mov	[wc.lpszMenuName],0
	mov	[wc.lpszClassName],_class
	invoke	RegisterClass,wc

	invoke	LoadMenu,[hinstance],IDM_MAIN
	mov	[hmenu_main],eax
	invoke	GetSubMenu,eax,1
	mov	[hmenu_edit],eax
	invoke	LoadMenu,[hinstance],IDM_TAB
	invoke	GetSubMenu,eax,0
	mov	[hmenu_tab],eax
	invoke	LoadAccelerators,[hinstance],IDA_MAIN
	mov	[hacc],eax
	invoke	CreateWindowEx,0,_class,_caption,WS_OVERLAPPEDWINDOW+WS_CLIPCHILDREN+WS_CLIPSIBLINGS,96,64,384,324,NULL,[hmenu_main],[hinstance],NULL
	or	eax,eax
	jz	end_loop
	mov	[hwnd_main],eax
	mov	eax,SW_SHOW
	test	[wp.flags],WPF_RESTORETOMAXIMIZED
	jz	show_main_window
	mov	eax,SW_SHOWMAXIMIZED
      show_main_window:
	invoke	ShowWindow,[hwnd_main],eax
	invoke	UpdateWindow,[hwnd_main]
  msg_loop:
	invoke	GetMessage,msg,NULL,0,0
	or	eax,eax
	jz	end_loop
	invoke	TranslateAccelerator,[hwnd_main],[hacc],msg
	or	eax,eax
	jnz	msg_loop
	cmp	[msg.message],WM_KEYDOWN
	je	msg_dispatch
	invoke	TranslateMessage,msg
      msg_dispatch:
	invoke	DispatchMessage,msg
	jmp	msg_loop

  window_already_exists:
	mov	ebx,eax
	invoke	GetWindowPlacement,ebx,wp
	mov	eax,SW_SHOWNORMAL
	cmp	[wp.showCmd],SW_SHOWMAXIMIZED
	jne	show_existing_window
	mov	eax,SW_SHOWMAXIMIZED
      show_existing_window:
	invoke	ShowWindow,ebx,eax
	invoke	SetForegroundWindow,ebx
	mov	edi,[program_arguments]
	mov	[cp.lpData],edi
	or	ecx,-1
	xor	al,al
	repne	scasb
	neg	ecx
	mov	[cp.cbData],ecx
	invoke	SendMessage,ebx,WM_COPYDATA,NULL,cp

  end_loop:
	invoke	ExitProcess,[msg.wParam]

proc MainWindow hwnd,wmsg,wparam,lparam
	push	ebx esi edi
	cmp	[wmsg],WM_CREATE
	je	wmcreate
	cmp	[wmsg],WM_COPYDATA
	je	wmcopydata
	cmp	[wmsg],WM_GETMINMAXINFO
	je	wmgetminmaxinfo
	cmp	[wmsg],WM_SIZE
	je	wmsize
	cmp	[wmsg],WM_SETFOCUS
	je	wmsetfocus
	cmp	[wmsg],FM_NEW
	je	fmnew
	cmp	[wmsg],FM_OPEN
	je	fmopen
	cmp	[wmsg],FM_SAVE
	je	fmsave
	cmp	[wmsg],FM_COMPILE
	je	fmcompile
	cmp	[wmsg],FM_SELECT
	je	fmselect
	cmp	[wmsg],FM_ASSIGN
	je	fmassign
	cmp	[wmsg],FM_GETSELECTED
	je	fmgetselected
	cmp	[wmsg],FM_GETASSIGNED
	je	fmgetassigned
	cmp	[wmsg],FM_GETHANDLE
	je	fmgethandle
	cmp	[wmsg],WM_INITMENU
	je	wminitmenu
	cmp	[wmsg],WM_COMMAND
	je	wmcommand
	cmp	[wmsg],WM_NOTIFY
	je	wmnotify
	cmp	[wmsg],WM_DROPFILES
	je	wmdropfiles
	cmp	[wmsg],WM_CLOSE
	je	wmclose
	cmp	[wmsg],WM_DESTROY
	je	wmdestroy
	invoke	DefWindowProc,[hwnd],[wmsg],[wparam],[lparam]
	jmp	finish
  wmcreate:
	xor	eax,eax
	mov	[search_settings],eax
	mov	[search_string],al
	mov	[replace_string],al
	mov	[status_update_disabled],al
	mov	[compiler_memory],16384
	mov	[compiler_priority],THREAD_PRIORITY_NORMAL
	mov	[assigned_file],-1
	mov	[help_path],0
	mov	[ofn.lStructSize],sizeof.OPENFILENAME
	mov	eax,[hwnd]
	mov	[ofn.hwndOwner],eax
	mov	eax,[hinstance]
	mov	[ofn.hInstance],eax
	mov	[ofn.lpstrCustomFilter],NULL
	mov	[ofn.nFilterIndex],1
	mov	[ofn.nMaxFile],1000h
	mov	[ofn.lpstrFileTitle],name_buffer
	mov	[ofn.nMaxFileTitle],100h
	mov	[ofn.lpstrInitialDir],NULL
	mov	[ofn.lpstrDefExt],_asm_extension
	mov	[font.lfHeight],16
	mov	[font.lfWidth],0
	mov	[font.lfEscapement],0
	mov	[font.lfOrientation],0
	mov	[font.lfWeight],0
	mov	[font.lfItalic],FALSE
	mov	[font.lfUnderline],FALSE
	mov	[font.lfStrikeOut],FALSE
	mov	[font.lfCharSet],DEFAULT_CHARSET
	mov	[font.lfOutPrecision],OUT_RASTER_PRECIS
	mov	[font.lfClipPrecision],CLIP_DEFAULT_PRECIS
	mov	[font.lfQuality],DEFAULT_QUALITY
	mov	[font.lfPitchAndFamily],FIXED_PITCH+FF_DONTCARE
	mov	edi,font.lfFaceName
	mov	esi,_font_face
      copy_font_face:
	lodsb
	stosb
	or	al,al
	jnz	copy_font_face
	invoke	GetSysColor,COLOR_WINDOWTEXT
	mov	[editor_colors],eax
	invoke	GetSysColor,COLOR_WINDOW
	mov	[editor_colors+4],eax
	invoke	GetSysColor,COLOR_HIGHLIGHTTEXT
	mov	[editor_colors+8],eax
	invoke	GetSysColor,COLOR_HIGHLIGHT
	mov	[editor_colors+12],eax
	mov	esi,editor_colors
	mov	edi,user_colors
	mov	ecx,8
	rep	movsd
	mov	[wp.length],sizeof.WINDOWPLACEMENT
	invoke	GetWindowPlacement,[hwnd],wp
	stdcall GetIniInteger,ini_path,_section_compiler,_key_compiler_memory,compiler_memory
	stdcall GetIniInteger,ini_path,_section_compiler,_key_compiler_priority,compiler_priority
	stdcall GetIniBit,ini_path,_section_options,_key_options_securesel,asmedit_style,AES_SECURESEL
	stdcall GetIniBit,ini_path,_section_options,_key_options_autobrackets,asmedit_style,AES_AUTOBRACKETS
	stdcall GetIniBit,ini_path,_section_options,_key_options_autoindent,asmedit_style,AES_AUTOINDENT
	stdcall GetIniBit,ini_path,_section_options,_key_options_smarttabs,asmedit_style,AES_SMARTTABS
	stdcall GetIniBit,ini_path,_section_options,_key_options_optimalfill,asmedit_style,AES_OPTIMALFILL
	stdcall GetIniBit,ini_path,_section_options,_key_options_revivedeadkeys,asmedit_style,AES_REVIVEDEADKEYS
	stdcall GetIniBit,ini_path,_section_options,_key_options_consolecaret,asmedit_style,AES_CONSOLECARET
	stdcall GetIniColor,ini_path,_section_colors,_key_color_text,editor_colors
	stdcall GetIniColor,ini_path,_section_colors,_key_color_background,editor_colors+4
	stdcall GetIniColor,ini_path,_section_colors,_key_color_seltext,editor_colors+8
	stdcall GetIniColor,ini_path,_section_colors,_key_color_selbackground,editor_colors+12
	stdcall GetIniColor,ini_path,_section_colors,_key_color_symbols,asm_syntax_colors
	stdcall GetIniColor,ini_path,_section_colors,_key_color_numbers,asm_syntax_colors+4
	stdcall GetIniColor,ini_path,_section_colors,_key_color_strings,asm_syntax_colors+8
	stdcall GetIniColor,ini_path,_section_colors,_key_color_comments,asm_syntax_colors+12
	invoke	GetPrivateProfileString,_section_font,_key_font_face,font.lfFaceName,font.lfFaceName,32,ini_path
	stdcall GetIniInteger,ini_path,_section_font,_key_font_height,font.lfHeight
	stdcall GetIniInteger,ini_path,_section_font,_key_font_width,font.lfWidth
	stdcall GetIniInteger,ini_path,_section_font,_key_font_weight,font.lfWeight
	stdcall GetIniBit,ini_path,_section_font,_key_font_italic,font.lfItalic,1
	stdcall GetIniByte,ini_path,_section_font,_key_font_charset,font.lfCharSet
	stdcall GetIniInteger,ini_path,_section_window,_key_window_top,wp.rcNormalPosition.top
	stdcall GetIniInteger,ini_path,_section_window,_key_window_left,wp.rcNormalPosition.left
	stdcall GetIniInteger,ini_path,_section_window,_key_window_right,wp.rcNormalPosition.right
	stdcall GetIniInteger,ini_path,_section_window,_key_window_bottom,wp.rcNormalPosition.bottom
	stdcall GetIniBit,ini_path,_section_window,_key_window_maximized,wp.flags,WPF_RESTORETOMAXIMIZED
	invoke	GetPrivateProfileString,_section_help,_key_help_path,help_path,help_path,1000h,ini_path
	mov	[wp.showCmd],SW_HIDE
	invoke	SetWindowPlacement,[hwnd],wp
	invoke	CreateFontIndirect,font
	mov	[hfont],eax
	invoke	CreateStatusWindow,WS_CHILD+WS_VISIBLE+SBS_SIZEGRIP,NULL,[hwnd],0
	or	eax,eax
	jz	failed
	mov	[hwnd_status],eax
	mov	[param_buffer],48h
	mov	[param_buffer+4],90h
	mov	[param_buffer+8],-1
	invoke	SendMessage,eax,SB_SETPARTS,3,param_buffer
	invoke	CreateWindowEx,0,_listbox_class,NULL,WS_CHILD+LBS_HASSTRINGS,0,0,0,0,[hwnd],NULL,[hinstance],NULL
	or	eax,eax
	jz	failed
	mov	[hwnd_history],eax
	invoke	CreateWindowEx,0,_tabctrl_class,NULL,WS_VISIBLE+WS_CHILD+TCS_FOCUSNEVER+TCS_BOTTOM,0,0,0,0,[hwnd],NULL,[hinstance],NULL
	or	eax,eax
	jz	failed
	mov	[hwnd_tabctrl],eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETITEMEXTRA,8,0
	invoke	SendMessage,[hwnd_status],WM_GETFONT,0,0
	invoke	SendMessage,[hwnd_tabctrl],WM_SETFONT,eax,FALSE
	invoke	LoadBitmap,[hinstance],IDB_ASSIGN
	mov	ebx,eax
	invoke	GetObject,ebx,sizeof.BITMAP,bm
	invoke	ImageList_Create,[bm.bmWidth],[bm.bmHeight],ILC_COLOR4,1,0
	or	eax,eax
	jz	failed
	mov	[himl],eax
	invoke	ImageList_Add,[himl],ebx,NULL
	invoke	DeleteObject,ebx
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETIMAGELIST,0,[himl]
	invoke	SendMessage,[hwnd],FM_NEW,_untitled,0
	cmp	eax,-1
	je	failed
	invoke	DragAcceptFiles,[hwnd],TRUE
	mov	esi,[program_arguments]
      find_argument:
	lodsb
	cmp	al,20h
	je	find_argument
	xor	ecx,ecx
	cmp	al,22h
	je	quoted_argument
	cmp	al,0Dh
	je	command_line_ok
	or	al,al
	jz	command_line_ok
	lea	edx,[esi-1]
      find_argument_end:
	inc	ecx
	lodsb
	cmp	al,20h
	je	argument_end
	cmp	al,0Dh
	je	argument_end
	or	al,al
	jz	argument_end
	jmp	find_argument_end
      quoted_argument:
	mov	edx,esi
      find_quoted_argument_end:
	lodsb
	cmp	al,22h
	je	quoted_argument_end
	cmp	al,0Dh
	je	quoted_argument_end
	or	al,al
	jz	quoted_argument_end
	inc	ecx
	jmp	find_quoted_argument_end
      argument_end:
	dec	esi
      quoted_argument_end:
	push	eax edx esi
	mov	esi,edx
	mov	edi,path_buffer
	rep	movsb
	xor	al,al
	stosb
	invoke	GetFileTitle,path_buffer,name_buffer,100h
	invoke	SendMessage,[hwnd],FM_OPEN,name_buffer,path_buffer
	cmp	eax,-1
	jne	load_ok
	cinvoke wsprintf,string_buffer,_loading_error,path_buffer
	invoke	MessageBox,[hwnd],string_buffer,_caption,MB_ICONERROR+MB_OK
      load_ok:
	pop	esi edx eax
	jmp	find_argument
      command_line_ok:
	xor	eax,eax
	jmp	finish
  wmcopydata:
	mov	ebx,[lparam]
	virtual at ebx
	cmd	COPYDATASTRUCT
	end	virtual
	mov	esi,[cmd.lpData]
	jmp	find_argument
  wmgetminmaxinfo:
	mov	ebx,[lparam]
	virtual at ebx
	mmi	MINMAXINFO
	end	virtual
	mov	[mmi.ptMinTrackSize.x],240
	mov	[mmi.ptMinTrackSize.y],160
	jmp	finish
  wmsize:
	invoke	SendMessage,[hwnd_status],WM_SIZE,0,0
	xor	eax,eax
	mov	[rc.left],eax
	mov	[rc.top],eax
	mov	[rc.right],eax
	mov	[rc.bottom],eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_ADJUSTRECT,TRUE,rc
	mov	esi,[rc.bottom]
	sub	esi,[rc.top]
	invoke	GetWindowRect,[hwnd_status],rc
	mov	ebx,[rc.bottom]
	sub	ebx,[rc.top]
	invoke	GetClientRect,[hwnd],rc
	sub	[rc.bottom],ebx
	sub	[rc.bottom],esi
	invoke	SetWindowPos,[hwnd_tabctrl],[hwnd_asmedit],0,[rc.bottom],[rc.right],esi,0
	invoke	GetSystemMetrics,SM_CYFIXEDFRAME
	shl	eax,1
	add	[rc.bottom],eax
	invoke	MoveWindow,[hwnd_asmedit],0,0,[rc.right],[rc.bottom],TRUE
	jmp	finish
  wmsetfocus:
	invoke	SetFocus,[hwnd_asmedit]
	jmp	finish
  fmnew:
	invoke	CreateWindowEx,WS_EX_STATICEDGE,_asmedit_class,NULL,WS_CHILD+WS_HSCROLL+WS_VSCROLL+ES_NOHIDESEL,0,0,0,0,[hwnd],NULL,[hinstance],NULL
	or	eax,eax
	jz	failed
	mov	[ei.header.mask],TCIF_TEXT+TCIF_PARAM
	mov	[ei.hwnd],eax
	mov	eax,[wparam]
	mov	[ei.header.pszText],eax
	mov	[ei.pszpath],0
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEMCOUNT,0,0
	invoke	SendMessage,[hwnd_tabctrl],TCM_INSERTITEM,eax,ei
	mov	ebx,eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETCURSEL,eax,0
	invoke	SendMessage,[hwnd],FM_SELECT,ebx,0
	invoke	SetFocus,[hwnd]
	mov	eax,ebx
	jmp	finish
  fmopen:
	invoke	VirtualAlloc,0,1000h,MEM_COMMIT,PAGE_READWRITE
	or	eax,eax
	jz	failed
	mov	edi,eax
	mov	esi,[lparam]
	mov	[lparam],edi
      copy_path_for_asmedit:
	lodsb
	stosb
	or	al,al
	jnz	copy_path_for_asmedit
	xor	ebx,ebx
      check_if_already_loaded:
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,ebx,ei
	or	eax,eax
	jz	load_file
	invoke	lstrcmpi,[ei.pszpath],[lparam]
	or	eax,eax
	jz	show_already_loaded
	inc	ebx
	jmp	check_if_already_loaded
      show_already_loaded:
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETCURSEL,ebx,0
	invoke	SendMessage,[hwnd],FM_SELECT,ebx,0
	xor	eax,eax
	jmp	finish
      load_file:
	invoke	CreateFile,[lparam],GENERIC_READ,FILE_SHARE_READ,0,OPEN_EXISTING,0,0
	cmp	eax,-1
	je	open_failed
	mov	ebx,eax
	invoke	GetFileSize,ebx,NULL
	inc	eax
	push	eax
	invoke	VirtualAlloc,0,eax,MEM_COMMIT,PAGE_READWRITE
	or	eax,eax
	jz	load_out_of_memory
	pop	ecx
	dec	ecx
	push	MEM_RELEASE 0 eax
	mov	byte [eax+ecx],0
	invoke	ReadFile,ebx,eax,ecx,param_buffer,0
	invoke	CloseHandle,ebx
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEMCOUNT,0,0
	cmp	eax,1
	jne	new_asmedit
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,0,ei
	cmp	[ei.pszpath],0
	jne	new_asmedit
	invoke	SendMessage,[ei.hwnd],EM_CANUNDO,0,0
	or	eax,eax
	jnz	new_asmedit
	mov	[ei.header.mask],TCIF_TEXT+TCIF_PARAM
	mov	eax,[wparam]
	mov	[ei.header.pszText],eax
	mov	eax,[lparam]
	mov	[ei.pszpath],eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETITEM,0,ei
	xor	ebx,ebx
	jmp	set_asmedit_text
      new_asmedit:
	invoke	SendMessage,[hwnd],FM_NEW,[wparam],0
	cmp	eax,-1
	jne	set_path
	add	esp,12
	jmp	open_failed
      set_path:
	mov	ebx,eax
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,ebx,ei
	mov	eax,[lparam]
	mov	[ei.pszpath],eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETITEM,ebx,ei
      set_asmedit_text:
	invoke	SendMessage,[hwnd_asmedit],WM_SETTEXT,0,dword [esp]
	call	[VirtualFree]
	mov	eax,ebx
	jmp	finish
      load_out_of_memory:
	invoke	CloseHandle,ebx
      open_failed:
	invoke	VirtualFree,[lparam],0,MEM_RELEASE
	jmp	failed
  fmsave:
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,[wparam],ei
	or	eax,eax
	jz	failed
	mov	eax,[ei.pszpath]
	or	eax,eax
	jz	failed
	invoke	CreateFile,eax,GENERIC_WRITE,0,0,CREATE_ALWAYS,0,0
	cmp	eax,-1
	je	failed
	mov	ebx,eax
	invoke	SendMessage,[ei.hwnd],WM_GETTEXTLENGTH,0,0
	inc	eax
	mov	[wparam],eax
	invoke	VirtualAlloc,0,eax,MEM_COMMIT,PAGE_READWRITE
	or	eax,eax
	jz	save_out_of_memory
	mov	[lparam],eax
	invoke	SendMessage,[ei.hwnd],WM_GETTEXT,[wparam],eax
	invoke	WriteFile,ebx,[lparam],eax,param_buffer,0
	invoke	CloseHandle,ebx
	invoke	VirtualFree,[lparam],0,MEM_RELEASE
	invoke	SendMessage,[ei.hwnd],EM_EMPTYUNDOBUFFER,0,0
	invoke	SendMessage,[hwnd_status],SB_SETTEXT,1,_null
	xor	eax,eax
	jmp	finish
      save_out_of_memory:
	invoke	CloseHandle,ebx
	jmp	failed
  fmcompile:
	mov	eax,[assigned_file]
	push	eax
	cmp	eax,-1
	jne	assigned_ok
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	mov	[assigned_file],eax
      assigned_ok:
	invoke	SendMessage,[hwnd_main],FM_SAVE,eax,0
	xor	ebx,ebx
	or	eax,eax
	jz	save_all
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETCURSEL,[assigned_file],0
	invoke	SendMessage,[hwnd_main],FM_SELECT,[assigned_file],0
	invoke	SendMessage,[hwnd_main],WM_COMMAND,IDM_SAVEAS,0
	or	eax,eax
	jz	save_all
	or	eax,-1
	jmp	compile_done
      save_all:
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,ebx,ei
	or	eax,eax
	jz	do_compile
	invoke	SendMessage,[ei.hwnd],EM_CANUNDO,0,0
	or	eax,eax
	jz	save_next
	invoke	SendMessage,[hwnd_main],FM_SAVE,ebx,0
      save_next:
	inc	ebx
	jmp	save_all
      do_compile:
	mov	eax,[wparam]
	mov	[output_file],eax
	invoke	DialogBoxParam,[hinstance],IDD_COMPILE,[hwnd],CompileDialog,[assigned_file]
	cmp	eax,-1
	je	compile_done
	push	eax
	cmp	eax,2
	je	error_details
	or	eax,eax
	jnz	make_summary
	cmp	[lparam],FALSE
	jne	make_summary
	invoke	GlobalFree,[hmem_display]
	jmp	summary_done
      make_summary:
	invoke	DialogBoxParam,[hinstance],IDD_SUMMARY,[hwnd],SummaryDialog,eax
	jmp	summary_done
      error_details:
	invoke	DialogBoxParam,[hinstance],IDD_ERRORSUMMARY,[hwnd],SummaryDialog,eax
	invoke	GlobalFree,[hmem_error_data]
      summary_done:
	pop	eax
      compile_done:
	pop	edx
	cmp	edx,-1
	jne	finish
	or	[assigned_file],-1
	jmp	finish
  fmselect:
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,[wparam],ei
	invoke	GetWindowLong,[hwnd_asmedit],GWL_STYLE
	and	eax,not WS_VISIBLE
	invoke	SetWindowLong,[hwnd_asmedit],GWL_STYLE,eax
	mov	ebx,[ei.hwnd]
	mov	[hwnd_asmedit],ebx
	mov	eax,WS_CHILD+WS_HSCROLL+WS_VSCROLL+ES_NOHIDESEL
	or	eax,[asmedit_style]
	invoke	SetWindowLong,ebx,GWL_STYLE,eax
	invoke	SendMessage,ebx,WM_SETFONT,[hfont],0
	invoke	SendMessage,ebx,AEM_SETTEXTCOLOR,[editor_colors],[editor_colors+4]
	invoke	SendMessage,ebx,AEM_SETSELCOLOR,[editor_colors+8],[editor_colors+12]
	invoke	SendMessage,ebx,AEM_SETSYNTAXHIGHLIGHT,asm_syntax_colors,fasm_syntax
	invoke	SendMessage,ebx,AEM_SETRIGHTCLICKMENU,[hmenu_edit],[hwnd]
	invoke	SendMessage,[hwnd],WM_SIZE,0,0
	invoke	ShowWindow,ebx,SW_SHOW
	invoke	UpdateWindow,ebx
	invoke	SetFocus,[hwnd]
	jmp	finish
  fmassign:
	mov	eax,[wparam]
	cmp	[assigned_file],-1
	je	new_assign
	push	eax
	mov	[ei.header.mask],TCIF_IMAGE
	mov	[ei.header.iImage],-1
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETITEM,[assigned_file],ei
	pop	eax
      new_assign:
	mov	[assigned_file],eax
	mov	[ei.header.mask],TCIF_IMAGE
	mov	[ei.header.iImage],0
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETITEM,eax,ei
	or	eax,eax
	jnz	fmgetassigned
	or	eax,-1
	mov	[assigned_file],eax
	jmp	finish
  fmgetassigned:
	mov	eax,[assigned_file]
	jmp	finish
  fmgetselected:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	jmp	finish
  fmgethandle:
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,[wparam],ei
	or	eax,eax
	jz	finish
	mov	eax,[ei.hwnd]
	jmp	finish
  wminitmenu:
	mov	esi,[hwnd_asmedit]
	invoke	SendMessage,esi,EM_CANUNDO,0,0
	or	eax,eax
	setz	bl
	neg	bl
	and	ebx,MF_GRAYED
	or	ebx,MF_BYCOMMAND
	invoke	EnableMenuItem,[wparam],IDM_UNDO,ebx
	invoke	SendMessage,esi,AEM_GETPOS,aepos,0
	mov	eax,[aepos.selectionLine]
	cmp	eax,[aepos.caretLine]
	sete	bh
	mov	eax,[aepos.selectionPosition]
	cmp	eax,[aepos.caretPosition]
	sete	bl
	and	bl,bh
	neg	bl
	and	ebx,MF_GRAYED
	or	ebx,MF_BYCOMMAND
	invoke	EnableMenuItem,[wparam],IDM_CUT,ebx
	invoke	EnableMenuItem,[wparam],IDM_COPY,ebx
	invoke	EnableMenuItem,[wparam],IDM_DELETE,ebx
	invoke	IsClipboardFormatAvailable,CF_TEXT
	neg	al
	not	al
	and	eax,MF_GRAYED
	or	eax,MF_BYCOMMAND
	invoke	EnableMenuItem,[wparam],IDM_PASTE,eax
	invoke	SendMessage,esi,AEM_GETMODE,0,0
	mov	ebx,eax
	test	eax,AEMODE_VERTICALSEL
	setnz	al
	neg	al
	and	eax,MF_CHECKED
	or	eax,MF_BYCOMMAND
	invoke	CheckMenuItem,[wparam],IDM_VERTICAL,eax
	invoke	SendMessage,esi,AEM_CANFINDNEXT,0,0
	or	eax,eax
	setz	al
	neg	al
	and	eax,MF_GRAYED
	or	eax,MF_BYCOMMAND
	invoke	EnableMenuItem,[wparam],IDM_FINDNEXT,eax
	test	[asmedit_style],AES_SECURESEL
	setnz	al
	neg	al
	and	eax,MF_CHECKED
	or	eax,MF_BYCOMMAND
	invoke	CheckMenuItem,[wparam],IDM_SECURESEL,eax
	test	[asmedit_style],AES_AUTOBRACKETS
	setnz	al
	neg	al
	and	eax,MF_CHECKED
	or	eax,MF_BYCOMMAND
	invoke	CheckMenuItem,[wparam],IDM_AUTOBRACKETS,eax
	test	[asmedit_style],AES_AUTOINDENT
	setnz	al
	neg	al
	and	eax,MF_CHECKED
	or	eax,MF_BYCOMMAND
	invoke	CheckMenuItem,[wparam],IDM_AUTOINDENT,eax
	test	[asmedit_style],AES_SMARTTABS
	setnz	al
	neg	al
	and	eax,MF_CHECKED
	or	eax,MF_BYCOMMAND
	invoke	CheckMenuItem,[wparam],IDM_SMARTTABS,eax
	test	[asmedit_style],AES_OPTIMALFILL
	setnz	al
	neg	al
	and	eax,MF_CHECKED
	or	eax,MF_BYCOMMAND
	invoke	CheckMenuItem,[wparam],IDM_OPTIMALFILL,eax
	test	[asmedit_style],AES_REVIVEDEADKEYS
	setnz	al
	neg	al
	and	eax,MF_CHECKED
	or	eax,MF_BYCOMMAND
	invoke	CheckMenuItem,[wparam],IDM_REVIVEDEADKEYS,eax
	cmp	[help_path],0
	sete	bl
	neg	bl
	and	ebx,MF_GRAYED
	or	ebx,MF_BYCOMMAND
	invoke	EnableMenuItem,[wparam],IDM_CONTENTS,ebx
	invoke	EnableMenuItem,[wparam],IDM_KEYWORD,ebx
	jmp	finish
  wmcommand:
	mov	eax,[wparam]
	mov	ebx,[lparam]
	or	ebx,ebx
	jz	menu_command
	cmp	ebx,[hwnd_asmedit]
	jne	finish
	shr	eax,16
	cmp	eax,AEN_SETFOCUS
	je	update_status_bar
	cmp	eax,AEN_TEXTCHANGE
	je	update_status_bar
	cmp	eax,AEN_POSCHANGE
	je	update_status_bar
	cmp	eax,AEN_OUTOFMEMORY
	je	not_enough_mem
	jmp	finish
      update_status_bar:
	cmp	[status_update_disabled],0
	jne	finish
	invoke	SendMessage,[hwnd_asmedit],AEM_GETPOS,aepos,0
	cinvoke wsprintf,string_buffer,_row_column,[aepos.caretLine],[aepos.caretPosition]
	invoke	SendMessage,[hwnd_status],SB_SETTEXT,0,string_buffer
	mov	esi,_null
	invoke	SendMessage,[hwnd_asmedit],EM_CANUNDO,0,0
	or	eax,eax
	jz	modified_status_ok
	mov	esi,_modified_status
      modified_status_ok:
	invoke	SendMessage,[hwnd_status],SB_SETTEXT,1,esi
	jmp	finish
      not_enough_mem:
	invoke	SendMessage,[hwnd_asmedit],AEM_FINDFIRST,0,0
	invoke	MessageBox,[hwnd],_memory_error,_caption,MB_ICONERROR+MB_OK
	jmp	finish
  menu_command:
	and	eax,0FFFFh
	mov	ebx,[hwnd_asmedit]
	cmp	eax,IDM_NEW
	je	new_file
	cmp	eax,IDM_OPEN
	je	open_file
	cmp	eax,IDM_SAVE
	je	save_file
	cmp	eax,IDM_SAVEAS
	je	save_file_as
	cmp	eax,IDM_NEXT
	je	next_file
	cmp	eax,IDM_PREVIOUS
	je	previous_file
	cmp	eax,IDM_OPENFOLDER
	je	open_folder
	cmp	eax,IDM_CLOSE
	je	close_file
	cmp	eax,IDM_EXIT
	je	exit
	cmp	eax,IDM_UNDO
	je	undo
	cmp	eax,IDM_CUT
	je	cut
	cmp	eax,IDM_COPY
	je	copy
	cmp	eax,IDM_PASTE
	je	paste
	cmp	eax,IDM_DELETE
	je	delete
	cmp	eax,IDM_SELECTALL
	je	select_all
	cmp	eax,IDM_VERTICAL
	je	vertical
	cmp	eax,IDM_POSITION
	je	position
	cmp	eax,IDM_FIND
	je	find
	cmp	eax,IDM_FINDNEXT
	je	findnext
	cmp	eax,IDM_REPLACE
	je	replace
	cmp	eax,IDM_RUN
	je	run
	cmp	eax,IDM_COMPILE
	je	compile
	cmp	eax,IDM_ASSIGN
	je	assign
	cmp	eax,IDM_APPEARANCE
	je	appearance
	cmp	eax,IDM_COMPILERSETUP
	je	compiler_setup
	cmp	eax,IDM_SECURESEL
	je	option_securesel
	cmp	eax,IDM_AUTOBRACKETS
	je	option_autobrackets
	cmp	eax,IDM_AUTOINDENT
	je	option_autoindent
	cmp	eax,IDM_SMARTTABS
	je	option_smarttabs
	cmp	eax,IDM_OPTIMALFILL
	je	option_optimalfill
	cmp	eax,IDM_REVIVEDEADKEYS
	je	option_revivedeadkeys
	cmp	eax,IDM_CONTENTS
	je	contents
	cmp	eax,IDM_KEYWORD
	je	keyword
	cmp	eax,IDM_PICKHELP
	je	pick_help
	cmp	eax,IDM_ABOUT
	je	about
	sub	eax,IDM_SELECTFILE
	jc	finish
	cmp	eax,9
	ja	finish
	mov	ebx,eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEMCOUNT,0,0
	cmp	ebx,eax
	ja	finish
	dec	ebx
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETCURSEL,ebx,0
	invoke	SendMessage,[hwnd],FM_SELECT,ebx,0
	jmp	finish
  new_file:
	invoke	SendMessage,[hwnd],FM_NEW,_untitled,0
	jmp	finish
  open_file:
	mov	[ofn.lpstrFile],path_buffer
	mov	[path_buffer],0
	mov	[ofn.lpstrFilter],asm_filter
	mov	[ofn.Flags],OFN_EXPLORER+OFN_ALLOWMULTISELECT+OFN_FILEMUSTEXIST+OFN_HIDEREADONLY
	mov	[ofn.lpstrFileTitle],name_buffer
	mov	[ofn.lpstrTitle],NULL
	invoke	GetOpenFileName,ofn
	or	eax,eax
	jz	finish
	test	[ofn.Flags],OFN_ALLOWMULTISELECT
	jz	open_single_file
	mov	esi,path_buffer
	movzx	eax,[ofn.nFileOffset]
	add	esi,eax
	mov	byte [esi-1],'\'
	mov	edi,esi
      open_chosen_files:
	cmp	byte [esi],0
	je	finish
	push	edi
      move_file_name:
	lodsb
	stosb
	or	al,al
	jnz	move_file_name
	pop	edi
	invoke	GetFileTitle,path_buffer,name_buffer,100h
	invoke	SendMessage,[hwnd],FM_OPEN,name_buffer,path_buffer
	cmp	eax,-1
	jne	open_chosen_files
	invoke	wvsprintf,string_buffer,_loading_error,ofn.lpstrFile
	invoke	MessageBox,[hwnd],string_buffer,_caption,MB_ICONERROR+MB_OK
	jmp	open_chosen_files
      open_single_file:
	invoke	SendMessage,[hwnd],FM_OPEN,name_buffer,path_buffer
	cmp	eax,-1
	jne	finish
	invoke	wvsprintf,string_buffer,_loading_error,ofn.lpstrFile
	invoke	MessageBox,[hwnd],string_buffer,_caption,MB_ICONERROR+MB_OK
	jmp	finish
  save_file:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	invoke	SendMessage,[hwnd],FM_SAVE,eax,0
	or	eax,eax
	jnz	save_file_as
	jmp	finish
  save_file_as:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	mov	ebx,eax
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,ebx,ei
	mov	eax,[ei.pszpath]
	or	eax,eax
	jnz	alloc_ok
	invoke	VirtualAlloc,0,1000h,MEM_COMMIT,PAGE_READWRITE
	mov	[ei.pszpath],eax
      alloc_ok:
	mov	[lparam],eax
	mov	[ofn.lpstrFile],eax
	mov	[ofn.lpstrFilter],asm_filter
	mov	[ofn.Flags],OFN_EXPLORER+OFN_HIDEREADONLY+OFN_OVERWRITEPROMPT
	invoke	GetSaveFileName,ofn
	or	eax,eax
	jz	save_cancelled
	mov	[ei.header.pszText],name_buffer
	mov	[ei.header.mask],TCIF_TEXT+TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETITEM,ebx,ei
	invoke	SendMessage,[hwnd],FM_SAVE,ebx,0
	xor	esi,esi
      check_if_overwritten:
	cmp	esi,ebx
	je	not_overwritten
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,esi,ei
	or	eax,eax
	jz	save_ok
	invoke	lstrcmpi,[ei.pszpath],[lparam]
	or	eax,eax
	jz	remove_overwritten
      not_overwritten:
	inc	esi
	jmp	check_if_overwritten
      remove_overwritten:
	invoke	VirtualFree,[ei.pszpath],0,MEM_RELEASE
	invoke	SendMessage,[hwnd_tabctrl],TCM_DELETEITEM,esi,0
	cmp	[assigned_file],-1
	je	save_ok
	cmp	esi,[assigned_file]
	ja	save_ok
	je	assigned_overwritten
	dec	[assigned_file]
	jmp	save_ok
      assigned_overwritten:
	mov	[assigned_file],-1
      save_ok:
	xor	eax,eax
	jmp	finish
      save_cancelled:
	mov	eax,[lparam]
	xchg	eax,[ei.pszpath]
	cmp	eax,[ei.pszpath]
	je	finish
	invoke	VirtualFree,eax,0,MEM_RELEASE
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETITEM,eax,ei
	jmp	finish
  next_file:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEMCOUNT,0,0
	mov	ebx,eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	inc	eax
	cmp	eax,ebx
	jb	select_file
	xor	eax,eax
      select_file:
	push	eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETCURSEL,eax,0
	pop	eax
	invoke	SendMessage,[hwnd],FM_SELECT,eax,0
	jmp	finish
  previous_file:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	sub	eax,1
	jnc	select_file
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEMCOUNT,0,0
	dec	eax
	call	select_file
	jmp	select_file
  open_folder:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,eax,ei
	or	eax,eax
	jz	finish
	mov	eax,[ei.pszpath]
	or	eax,eax
	jz	finish
	invoke	GetFullPathName,eax,1000h,path_buffer,param_buffer
	mov	edi,[param_buffer]
	xor	al,al
	stosb
	invoke	ShellExecute,HWND_DESKTOP,NULL,path_buffer,NULL,NULL,SW_SHOWNORMAL
	jmp	finish
  close_file:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEMCOUNT,0,0
	cmp	eax,1
	jbe	close_window
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	mov	ebx,eax
	mov	[ei.header.mask],TCIF_PARAM+TCIF_TEXT
	mov	[ei.header.pszText],name_buffer
	mov	[ei.header.cchTextMax],100h
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,ebx,ei
	mov	eax,[ei.hwnd]
	mov	[wparam],eax
	invoke	SendMessage,eax,EM_CANUNDO,0,0
	or	eax,eax
	jnz	close_modified
	cmp	[ei.pszpath],0
	jne	do_close
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEMCOUNT,0,0
	cmp	eax,1
	jne	do_close
	jmp	failed
      close_modified:
	mov	eax,MB_ICONQUESTION+MB_YESNOCANCEL
	or	eax,[lparam]
	invoke	MessageBox,[hwnd],_saving_question,[ei.header.pszText],eax
	cmp	eax,IDCANCEL
	je	failed
	cmp	eax,IDNO
	je	do_close
	invoke	SendMessage,[hwnd],WM_COMMAND,IDM_SAVE,0
	or	eax,eax
	jnz	failed
      do_close:
	cmp	[ei.pszpath],0
	je	delete_tab
	invoke	VirtualFree,[ei.pszpath],0,MEM_RELEASE
      delete_tab:
	invoke	SendMessage,[hwnd_tabctrl],TCM_DELETEITEM,ebx,0
	cmp	ebx,[assigned_file]
	jg	tab_deleted
	je	assigned_deleted
	dec	[assigned_file]
	jmp	tab_deleted
      assigned_deleted:
	mov	[assigned_file],-1
      tab_deleted:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEMCOUNT,0,0
	dec	eax
	cmp	eax,ebx
	jge	select_next
	sub	ebx,1
	jnc	select_next
	invoke	SendMessage,[hwnd],FM_NEW,_untitled,0
	jmp	destroy_asmedit
      select_next:
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETCURSEL,ebx,0
	invoke	SendMessage,[hwnd],FM_SELECT,ebx,0
      destroy_asmedit:
	invoke	DestroyWindow,[wparam]
	xor	eax,eax
	jmp	finish
  exit:
	mov	[lparam],0
	jmp	close_window
  undo:
	invoke	SendMessage,ebx,WM_UNDO,0,0
	jmp	finish
  cut:
	invoke	SendMessage,ebx,WM_CUT,0,0
	jmp	finish
  copy:
	invoke	SendMessage,ebx,WM_COPY,0,0
	jmp	finish
  paste:
	invoke	SendMessage,ebx,WM_PASTE,0,0
	jmp	finish
  delete:
	invoke	SendMessage,ebx,WM_CLEAR,0,0
	jmp	finish
  select_all:
	mov	[aepos.selectionLine],1
	mov	[aepos.selectionPosition],1
	mov	[aepos.caretLine],-1
	mov	[aepos.caretPosition],1
	invoke	SendMessage,ebx,AEM_SETPOS,aepos,0
	invoke	SendMessage,ebx,AEM_GETPOS,aepos,0
	invoke	SendMessage,ebx,AEM_GETLINELENGTH,[aepos.caretLine],0
	inc	eax
	mov	[aepos.caretPosition],eax
	invoke	SendMessage,ebx,AEM_SETPOS,aepos,0
	jmp	finish
  vertical:
	invoke	SendMessage,ebx,AEM_GETMODE,0,0
	xor	eax,AEMODE_VERTICALSEL
	invoke	SendMessage,ebx,AEM_SETMODE,eax,0
	jmp	finish
  position:
	invoke	DialogBoxParam,[hinstance],IDD_POSITION,[hwnd],PositionDialog,0
	jmp	finish
  find:
	invoke	DialogBoxParam,[hinstance],IDD_FIND,[hwnd],FindDialog,0
	or	eax,eax
	jz	finish
	invoke	SendMessage,ebx,AEM_FINDFIRST,[search_settings],search_string
	or	eax,eax
	jnz	finish
      not_found:
	invoke	MessageBox,[hwnd],_not_found,_find,MB_ICONINFORMATION+MB_OK
	jmp	finish
  findnext:
	invoke	SendMessage,ebx,AEM_FINDNEXT,0,0
	or	eax,eax
	jz	not_found
	jmp	finish
  replace:
	invoke	DialogBoxParam,[hinstance],IDD_REPLACE,[hwnd],ReplaceDialog,0
	or	eax,eax
	jz	finish
	test	[command_flags],CF_REPLACEPROMPT
	jnz	.start_replacing
	or	[status_update_disabled],-1
      .start_replacing:
	invoke	SendMessage,ebx,AEM_FINDFIRST,[search_settings],search_string
	or	eax,eax
	jz	not_found
	invoke	SendMessage,ebx,AEM_GETMODE,0,0
	push	eax
	and	eax,not (AEMODE_VERTICALSEL + AEMODE_OVERWRITE)
	invoke	SendMessage,ebx,AEM_SETMODE,eax,0
      .confirm_replace:
	test	[command_flags],CF_REPLACEPROMPT
	jz	.replace
	invoke	UpdateWindow,edi
	invoke	MessageBox,[hwnd],_replace_prompt,_replace,MB_ICONQUESTION+MB_YESNOCANCEL
	cmp	eax,IDCANCEL
	je	.replace_finish
	cmp	eax,IDNO
	je	.replace_next
      .replace:
	push	ebx edi
	invoke	SendMessage,ebx,EM_REPLACESEL,FALSE,replace_string
	pop	edi ebx
      .replace_next:
	invoke	SendMessage,ebx,AEM_FINDNEXT,0,0
	or	eax,eax
	jnz	.confirm_replace
      .replace_finish:
	pop	eax
	invoke	SendMessage,ebx,AEM_SETMODE,eax,0
	mov	[status_update_disabled],0
	jmp	finish
  run:
	invoke	SendMessage,[hwnd],FM_COMPILE,0,FALSE
	or	eax,eax
	jnz	finish
	mov	[sinfo.cb],sizeof.STARTUPINFO
	mov	[sinfo.dwFlags],0
	cmp	[output_format],3
	ja	run_object
	invoke	CreateProcess,path_buffer,NULL,NULL,NULL,FALSE,NORMAL_PRIORITY_CLASS,NULL,NULL,sinfo,pinfo
	invoke	CloseHandle,[pinfo.hThread]
	invoke	CloseHandle,[pinfo.hProcess]
	jmp	finish
      run_object:
	invoke	MessageBox,[hwnd],_run_object_error,_caption,MB_ICONERROR+MB_OK
	jmp	finish
  compile:
	invoke	SendMessage,[hwnd],FM_COMPILE,0,TRUE
	jmp	finish
  assign:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	cmp	eax,[assigned_file]
	jne	do_assign
	or	eax,-1
      do_assign:
	invoke	SendMessage,[hwnd],FM_ASSIGN,eax,0
	jmp	finish
  appearance:
	invoke	DialogBoxParam,[hinstance],IDD_APPEARANCE,[hwnd],AppearanceSetup,0
	or	eax,eax
	jnz	update
	jmp	finish
  compiler_setup:
	invoke	DialogBoxParam,[hinstance],IDD_COMPILERSETUP,[hwnd],CompilerSetup,0
	jmp	finish
  option_securesel:
	xor	[asmedit_style],AES_SECURESEL
	jmp	update
  option_autobrackets:
	xor	[asmedit_style],AES_AUTOBRACKETS
	jmp	update
  option_autoindent:
	xor	[asmedit_style],AES_AUTOINDENT
	jmp	update
  option_smarttabs:
	xor	[asmedit_style],AES_SMARTTABS
	jmp	update
  option_optimalfill:
	xor	[asmedit_style],AES_OPTIMALFILL
	jmp	update
  option_revivedeadkeys:
	xor	[asmedit_style],AES_REVIVEDEADKEYS
	jmp	update
  contents:
	call	get_help_file_extension
	cmp	eax,'.hlp'
	je	winhelp_contents
	call	check_htmlhelp
	jc	winhelp_contents
	invoke	HtmlHelp,[hwnd],help_path,HH_DISPLAY_TOPIC,0
	jmp	finish
      winhelp_contents:
	invoke	WinHelp,[hwnd],help_path,HELP_FINDER,0
	jmp	finish
      get_help_file_extension:
	mov	esi,help_path
      skip_help_path:
	lodsb
	or	al,al
	jnz	skip_help_path
	mov	ebx,characters
	dec	esi
	mov	ecx,4
      convert_extension:
	dec	esi
	shl	eax,8
	mov	al,[esi]
	xlatb
	loop	convert_extension
	retn
      check_htmlhelp:
	cmp	[HtmlHelp],0
	jne	htmlhelp_ok
	invoke	LoadLibrary,_htmlhelp_library
	or	eax,eax
	jz	htmlhelp_unavailable
	invoke	GetProcAddress,eax,_htmlhelp_api
	or	eax,eax
	jz	htmlhelp_unavailable
	mov	[HtmlHelp],eax
      htmlhelp_ok:
	clc
	retn
      htmlhelp_unavailable:
	stc
	retn
  keyword:
	invoke	SendMessage,[hwnd_asmedit],AEM_GETWORDATCARET,1000h,string_buffer
	call	get_help_file_extension
	cmp	eax,'.hlp'
	je	winhelp_keyword
	call	check_htmlhelp
	jc	winhelp_keyword
	mov	[hhkey.cbStruct],sizeof.HH_AKLINK
	mov	[hhkey.pszKeywords],string_buffer
	mov	[hhkey.pszUrl],0
	mov	[hhkey.fIndexOnFail],TRUE
	invoke	HtmlHelp,[hwnd],help_path,HH_DISPLAY_TOPIC,0
	invoke	HtmlHelp,[hwnd],help_path,HH_KEYWORD_LOOKUP,hhkey
	jmp	finish
      winhelp_keyword:
	invoke	WinHelp,[hwnd],help_path,HELP_KEY,string_buffer
	jmp	finish
  pick_help:
	mov	[ofn.lpstrFile],help_path
	mov	[ofn.lpstrFilter],help_filter
	mov	[ofn.Flags],OFN_EXPLORER+OFN_FILEMUSTEXIST+OFN_HIDEREADONLY
	mov	[ofn.lpstrTitle],_pick_help
	invoke	GetOpenFileName,ofn
	jmp	finish
  about:
	invoke	DialogBoxParam,[hinstance],IDD_ABOUT,[hwnd],AboutDialog,0
	jmp	finish
  failed:
	or	eax,-1
	jmp	finish
  wmnotify:
	mov	ebx,[lparam]
	virtual at ebx
	nmh	NMHDR
	end	virtual
	cmp	[nmh.code],NM_RCLICK
	je	rclick
	cmp	[nmh.code],TCN_SELCHANGING
	je	selchanging
	cmp	[nmh.code],TCN_SELCHANGE
	jne	finish
      update:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	invoke	SendMessage,[hwnd],FM_SELECT,eax,0
	jmp	finish
      selchanging:
	xor	eax,eax
	jmp	finish
      rclick:
	invoke	GetCursorPos,pt
	invoke	GetWindowRect,[hwnd_tabctrl],rc
	mov	eax,[pt.x]
	sub	eax,[rc.left]
	mov	[tcht.pt.x],eax
	mov	eax,[pt.y]
	sub	eax,[rc.top]
	mov	[tcht.pt.y],eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_HITTEST,0,tcht
	cmp	eax,-1
	je	finish
	mov	ebx,eax
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETCURSEL,ebx,0
	invoke	SendMessage,[hwnd],FM_SELECT,ebx,0
	cmp	ebx,[assigned_file]
	sete	bl
	neg	bl
	and	ebx,MF_CHECKED
	or	ebx,MF_BYCOMMAND
	invoke	CheckMenuItem,[hmenu_tab],IDM_ASSIGN,ebx
	invoke	TrackPopupMenu,[hmenu_tab],TPM_RIGHTBUTTON,[pt.x],[pt.y],0,[hwnd],0
	jmp	finish
  wmdropfiles:
	invoke	DragQueryFile,[wparam],-1,NULL,0
	xor	ebx,ebx
      drop_files:
	cmp	ebx,eax
	je	drag_finish
	push	eax
	invoke	DragQueryFile,[wparam],ebx,path_buffer,1000h
	push	ebx
	invoke	GetFileTitle,path_buffer,name_buffer,100h
	invoke	SendMessage,[hwnd],FM_OPEN,name_buffer,path_buffer
	cmp	eax,-1
	jne	drop_ok
	cinvoke wsprintf,string_buffer,_loading_error,path_buffer
	invoke	MessageBox,[hwnd],string_buffer,_caption,MB_ICONERROR+MB_OK
      drop_ok:
	pop	ebx eax
	inc	ebx
	jmp	drop_files
      drag_finish:
	invoke	DragFinish,[wparam]
	xor	eax,eax
	jmp	finish
  wmclose:
	mov	[lparam],MB_DEFBUTTON2
      close_window:
	mov	[wparam],0
      check_and_exit:
	mov	[ei.header.mask],TCIF_PARAM+TCIF_TEXT
	mov	[ei.header.pszText],name_buffer
	mov	[ei.header.cchTextMax],100h
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,[wparam],ei
	or	eax,eax
	jz	quit
	mov	esi,[ei.pszpath]
	invoke	SendMessage,[ei.hwnd],EM_CANUNDO,0,0
	or	eax,eax
	jz	release_path
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETCURSEL,[wparam],0
	invoke	SendMessage,[hwnd],FM_SELECT,[wparam],0
	mov	eax,MB_ICONQUESTION+MB_YESNOCANCEL
	or	eax,[lparam]
	invoke	MessageBox,[hwnd],_saving_question,[ei.header.pszText],eax
	cmp	eax,IDCANCEL
	je	finish
	cmp	eax,IDNO
	je	release_path
	invoke	SendMessage,[hwnd],WM_COMMAND,IDM_SAVE,0
	or	eax,eax
	jnz	finish
      release_path:
	or	esi,esi
	jz	check_next
	invoke	VirtualFree,esi,0,MEM_RELEASE
      check_next:
	inc	[wparam]
	jmp	check_and_exit
      quit:
	stdcall WriteIniInteger,ini_path,_section_compiler,_key_compiler_memory,[compiler_memory]
	stdcall WriteIniInteger,ini_path,_section_compiler,_key_compiler_priority,[compiler_priority]
	stdcall WriteIniBit,ini_path,_section_options,_key_options_securesel,[asmedit_style],AES_SECURESEL
	stdcall WriteIniBit,ini_path,_section_options,_key_options_autobrackets,[asmedit_style],AES_AUTOBRACKETS
	stdcall WriteIniBit,ini_path,_section_options,_key_options_autoindent,[asmedit_style],AES_AUTOINDENT
	stdcall WriteIniBit,ini_path,_section_options,_key_options_smarttabs,[asmedit_style],AES_SMARTTABS
	stdcall WriteIniBit,ini_path,_section_options,_key_options_optimalfill,[asmedit_style],AES_OPTIMALFILL
	stdcall WriteIniBit,ini_path,_section_options,_key_options_revivedeadkeys,[asmedit_style],AES_REVIVEDEADKEYS
	stdcall WriteIniBit,ini_path,_section_options,_key_options_consolecaret,[asmedit_style],AES_CONSOLECARET
	stdcall WriteIniColor,ini_path,_section_colors,_key_color_text,[editor_colors]
	stdcall WriteIniColor,ini_path,_section_colors,_key_color_background,[editor_colors+4]
	stdcall WriteIniColor,ini_path,_section_colors,_key_color_seltext,[editor_colors+8]
	stdcall WriteIniColor,ini_path,_section_colors,_key_color_selbackground,[editor_colors+12]
	stdcall WriteIniColor,ini_path,_section_colors,_key_color_symbols,[asm_syntax_colors]
	stdcall WriteIniColor,ini_path,_section_colors,_key_color_numbers,[asm_syntax_colors+4]
	stdcall WriteIniColor,ini_path,_section_colors,_key_color_strings,[asm_syntax_colors+8]
	stdcall WriteIniColor,ini_path,_section_colors,_key_color_comments,[asm_syntax_colors+12]
	invoke	WritePrivateProfileString,_section_font,_key_font_face,font.lfFaceName,ini_path
	stdcall WriteIniInteger,ini_path,_section_font,_key_font_height,[font.lfHeight]
	stdcall WriteIniInteger,ini_path,_section_font,_key_font_width,[font.lfWidth]
	stdcall WriteIniInteger,ini_path,_section_font,_key_font_weight,[font.lfWeight]
	stdcall WriteIniBit,ini_path,_section_font,_key_font_italic,dword [font.lfItalic],1
	movzx	eax,[font.lfCharSet]
	stdcall WriteIniInteger,ini_path,_section_font,_key_font_charset,eax
	invoke	GetWindowPlacement,[hwnd],wp
	stdcall WriteIniInteger,ini_path,_section_window,_key_window_top,[wp.rcNormalPosition.top]
	stdcall WriteIniInteger,ini_path,_section_window,_key_window_left,[wp.rcNormalPosition.left]
	stdcall WriteIniInteger,ini_path,_section_window,_key_window_right,[wp.rcNormalPosition.right]
	stdcall WriteIniInteger,ini_path,_section_window,_key_window_bottom,[wp.rcNormalPosition.bottom]
	cmp	[wp.showCmd],SW_SHOWMAXIMIZED
	sete	al
	stdcall WriteIniBit,ini_path,_section_window,_key_window_maximized,eax,1
	invoke	WritePrivateProfileString,_section_help,_key_help_path,help_path,ini_path
	invoke	DestroyWindow,[hwnd]
	jmp	finish
  wmdestroy:
	invoke	WinHelp,[hwnd],0,HELP_QUIT,0
	invoke	ImageList_Destroy,[himl]
	invoke	PostQuitMessage,0
	xor	eax,eax
  finish:
	pop	edi esi ebx
	ret
endp

proc WriteIniInteger ini,sec,key,val
	cinvoke wsprintf,string_buffer,_value,[val]
	invoke	WritePrivateProfileString,[sec],[key],string_buffer,[ini]
	ret
endp

proc WriteIniColor ini,sec,key,color
	movzx	eax,byte [color]
	movzx	ebx,byte [color+1]
	movzx	ecx,byte [color+2]
	cinvoke wsprintf,string_buffer,_color,eax,ebx,ecx
	invoke	WritePrivateProfileString,[sec],[key],string_buffer,[ini]
	ret
endp

proc WriteIniBit ini,sec,key,val,mask
	mov	eax,[val]
	test	eax,[mask]
	setnz	al
	movzx	eax,al
	cinvoke wsprintf,string_buffer,_value,eax
	invoke	WritePrivateProfileString,[sec],[key],string_buffer,[ini]
	ret
endp

proc GetIniInteger ini,sec,key,lpval
	mov	[string_buffer],0
	invoke	GetPrivateProfileString,[sec],[key],string_buffer,string_buffer,1000h,[ini]
	mov	esi,string_buffer
	cmp	byte [esi],0
	je	.done
	call	atoi
	jc	.done
	mov	ebx,[lpval]
	mov	[ebx],eax
      .done:
	ret
endp

proc atoi
	lodsb
	cmp	al,20h
	je	atoi
	cmp	al,9
	je	atoi
	mov	bl,al
	xor	eax,eax
	xor	edx,edx
	cmp	bl,'-'
	je	atoi_digit
	cmp	bl,'+'
	je	atoi_digit
	dec	esi
      atoi_digit:
	mov	dl,[esi]
	sub	dl,30h
	jc	atoi_done
	cmp	dl,9
	ja	atoi_done
	mov	ecx,eax
	shl	ecx,1
	jc	atoi_overflow
	shl	ecx,1
	jc	atoi_overflow
	add	eax,ecx
	shl	eax,1
	jc	atoi_overflow
	js	atoi_overflow
	add	eax,edx
	jc	atoi_overflow
	inc	esi
	jmp	atoi_digit
      atoi_overflow:
	stc
	ret
      atoi_done:
	cmp	bl,'-'
	jne	atoi_sign_ok
	neg	eax
      atoi_sign_ok:
	clc
	ret
endp

proc GetIniColor ini,sec,key,lpcolor
	mov	[string_buffer],0
	invoke	GetPrivateProfileString,[sec],[key],string_buffer,string_buffer,1000h,[ini]
	mov	esi,string_buffer
	cmp	byte [esi],0
	je	.done
	call	atoi
	jc	.done
	cmp	eax,0FFh
	ja	.done
	mov	edi,eax
	call	.find
	jne	.done
	call	atoi
	jc	.done
	cmp	eax,0FFh
	ja	.done
	shl	eax,8
	or	edi,eax
	call	.find
	jne	.done
	call	atoi
	jc	.done
	cmp	eax,0FFh
	ja	.done
	shl	eax,16
	or	edi,eax
	mov	ebx,[lpcolor]
	mov	[ebx],edi
      .done:
	ret
      .find:
	lodsb
	cmp	al,20h
	je	.find
	cmp	al,9
	je	.find
	cmp	al,','
	retn
endp

proc GetIniByte ini,sec,key,lpval
	mov	[string_buffer],0
	invoke	GetPrivateProfileString,[sec],[key],string_buffer,string_buffer,1000h,[ini]
	mov	esi,string_buffer
	cmp	byte [esi],0
	je	.done
	call	atoi
	jc	.done
	cmp	eax,100h
	jae	.done
	mov	ebx,[lpval]
	mov	[ebx],al
      .done:
	ret
endp

proc GetIniBit ini,sec,key,lpval,mask
	mov	[string_buffer],0
	invoke	GetPrivateProfileString,[sec],[key],string_buffer,string_buffer,1000h,[ini]
	mov	esi,string_buffer
	xor	eax,eax
      .find:
	lodsb
	cmp	al,20h
	je	.find
	cmp	al,9
	je	.find
	sub	al,30h
	jc	.done
	cmp	al,1
	ja	.done
	neg	eax
	mov	ebx,[lpval]
	mov	edx,[mask]
	not	edx
	and	[ebx],edx
	and	eax,[mask]
	or	[ebx],eax
      .done:
	ret
endp

proc fasm_syntax lpLine,uChars,lpColors
	push	ebx esi edi
	mov	esi,[lpLine]
	mov	edi,[lpColors]
	mov	ebx,characters
	mov	ecx,[uChars]
	xor	edx,edx
  .scan_syntax:
	lodsb
  .check_character:
	cmp	al,20h
	je	.syntax_space
	cmp	al,3Bh
	je	.syntax_comment
	mov	ah,al
	xlatb
	or	al,al
	jz	.syntax_symbol
	or	edx,edx
	jnz	.syntax_neutral
	cmp	ah,27h
	je	.syntax_string
	cmp	ah,22h
	je	.syntax_string
	cmp	ah,24h
	je	.syntax_pascal_hex
	cmp	ah,39h
	ja	.syntax_neutral
	cmp	ah,30h
	jae	.syntax_number
  .syntax_neutral:
	or	edx,-1
	inc	edi
	loop	.scan_syntax
	jmp	.done
  .syntax_space:
	xor	edx,edx
	inc	edi
	loop	.scan_syntax
	jmp	.done
  .syntax_symbol:
	mov	al,1
	stosb
	xor	edx,edx
	loop	.scan_syntax
	jmp	.done
  .syntax_pascal_hex:
	cmp	ecx,1
	je	.syntax_neutral
	mov	al,[esi]
	mov	ah,al
	xlatb
	or	al,al
	jz	.syntax_neutral
	cmp	ah,24h
	jne	.syntax_number
	cmp	ecx,2
	je	.syntax_neutral
	mov	al,[esi+1]
	xlatb
	or	al,al
	jz	.syntax_neutral
  .syntax_number:
	mov	al,2
	stosb
	loop	.number_character
	jmp	.done
  .number_character:
	lodsb
	mov	ah,al
	xlatb
	xchg	al,ah
	or	ah,ah
	jz	.check_character
	cmp	al,20h
	je	.check_character
	cmp	al,3Bh
	je	.check_character
	mov	al,2
	stosb
	loop	.number_character
	jmp	.done
  .syntax_string:
	mov	al,3
	stosb
	dec	ecx
	jz	.done
	lodsb
	cmp	al,ah
	jne	.syntax_string
	mov	al,3
	stosb
	dec	ecx
	jz	.done
	lodsb
	cmp	al,ah
	je	.syntax_string
	xor	edx,edx
	jmp	.check_character
  .syntax_comment:
	mov	al,4
	stosb
	loop	.syntax_comment
  .done:
	pop	edi esi ebx
	ret
endp

proc PositionDialog hwnd_dlg,msg,wparam,lparam
	push	ebx esi edi
	cmp	[msg],WM_INITDIALOG
	je	.initdialog
	cmp	[msg],WM_COMMAND
	je	.command
	cmp	[msg],WM_CLOSE
	je	.close
	xor	eax,eax
	jmp	.finish
  .initdialog:
	invoke	SendMessage,[hwnd_asmedit],AEM_GETPOS,aepos,0
	cinvoke wsprintf,string_buffer,_value,[aepos.caretLine]
	invoke	SetDlgItemText,[hwnd_dlg],ID_ROW,string_buffer
	cinvoke wsprintf,string_buffer,_value,[aepos.caretPosition]
	invoke	SetDlgItemText,[hwnd_dlg],ID_COLUMN,string_buffer
	jmp	.processed
  .command:
	cmp	[wparam],IDCANCEL
	je	.close
	cmp	[wparam],IDOK
	jne	.processed
	invoke	GetDlgItemInt,[hwnd_dlg],ID_ROW,param_buffer,FALSE
	mov	[aepos.caretLine],eax
	mov	[aepos.selectionLine],eax
	invoke	GetDlgItemInt,[hwnd_dlg],ID_COLUMN,param_buffer,FALSE
	mov	[aepos.caretPosition],eax
	mov	[aepos.selectionPosition],eax
	invoke	IsDlgButtonChecked,[hwnd_dlg],ID_SELECT
	or	eax,eax
	jz	.position
	mov	[aepos.selectionLine],0
	mov	[aepos.selectionPosition],0
  .position:
	invoke	SendMessage,[hwnd_asmedit],AEM_SETPOS,aepos,0
	invoke	EndDialog,[hwnd_dlg],TRUE
	jmp	.processed
  .close:
	invoke	EndDialog,[hwnd_dlg],FALSE
  .processed:
	mov	eax,1
  .finish:
	pop	edi esi ebx
	ret
endp

proc GetStringsFromHistory hwnd_combobox
	push	ebx esi
	invoke	SendMessage,[hwnd_history],LB_GETCOUNT,0,0
	mov	esi,eax
	xor	ebx,ebx
  .get_string:
	cmp	ebx,esi
	je	.finish
	invoke	SendMessage,[hwnd_history],LB_GETTEXT,ebx,string_buffer
	invoke	SendMessage,[hwnd_combobox],CB_ADDSTRING,0,string_buffer
	inc	ebx
	jmp	.get_string
  .finish:
	pop	esi ebx
	ret
endp

proc AddStringToHistory lpstr
	mov	eax,[lpstr]
	cmp	byte [eax],0
	je	.finish
	invoke	SendMessage,[hwnd_history],LB_FINDSTRINGEXACT,-1,[lpstr]
	cmp	eax,LB_ERR
	je	.insert
	invoke	SendMessage,[hwnd_history],LB_DELETESTRING,eax,0
  .insert:
	invoke	SendMessage,[hwnd_history],LB_INSERTSTRING,0,[lpstr]
	cmp	eax,LB_ERRSPACE
	jne	.finish
	invoke	SendMessage,[hwnd_history],LB_GETCOUNT,0,0
	sub	eax,1
	jc	.finish
	invoke	SendMessage,[hwnd_history],LB_DELETESTRING,eax,0
	jmp	.insert
  .finish:
	ret
endp

proc FindDialog hwnd_dlg,msg,wparam,lparam
	push	ebx esi edi
	cmp	[msg],WM_INITDIALOG
	je	.initdialog
	cmp	[msg],WM_COMMAND
	je	.command
	cmp	[msg],WM_CLOSE
	je	.close
	xor	eax,eax
	jmp	.finish
  .initdialog:
	invoke	SendMessage,[hwnd_asmedit],AEM_GETWORDATCARET,1000h,search_string
	invoke	SetDlgItemText,[hwnd_dlg],ID_TEXT,search_string
	invoke	GetDlgItem,[hwnd_dlg],ID_TEXT
	stdcall GetStringsFromHistory,eax
	xor	eax,eax
	test	[search_settings],AEFIND_CASESENSITIVE
	setnz	al
	invoke	CheckDlgButton,[hwnd_dlg],ID_CASESENSITIVE,eax
	xor	eax,eax
	test	[search_settings],AEFIND_WHOLEWORDS
	setnz	al
	invoke	CheckDlgButton,[hwnd_dlg],ID_WHOLEWORDS,eax
	xor	eax,eax
	test	[search_settings],AEFIND_BACKWARD
	setnz	al
	invoke	CheckDlgButton,[hwnd_dlg],ID_BACKWARD,eax
	jmp	.update
  .command:
	cmp	[wparam],ID_TEXT + CBN_EDITCHANGE shl 16
	je	.update
	cmp	[wparam],ID_TEXT + CBN_SELCHANGE shl 16
	je	.selchange
	cmp	[wparam],IDCANCEL
	je	.close
	cmp	[wparam],IDOK
	jne	.processed
	xor	ebx,ebx
	invoke	IsDlgButtonChecked,[hwnd_dlg],ID_CASESENSITIVE
	or	eax,eax
	jz	.casesensitive_ok
	or	ebx,AEFIND_CASESENSITIVE
  .casesensitive_ok:
	invoke	IsDlgButtonChecked,[hwnd_dlg],ID_WHOLEWORDS
	or	eax,eax
	jz	 .wholewords_ok
	or	ebx,AEFIND_WHOLEWORDS
  .wholewords_ok:
	invoke	IsDlgButtonChecked,[hwnd_dlg],ID_BACKWARD
	or	eax,eax
	jz	.backward_ok
	or	ebx,AEFIND_BACKWARD
  .backward_ok:
	mov	[search_settings],ebx
	stdcall AddStringToHistory,search_string
	invoke	EndDialog,[hwnd_dlg],TRUE
	jmp	.processed
  .selchange:
	invoke	PostMessage,[hwnd_dlg],WM_COMMAND,ID_TEXT + CBN_EDITCHANGE shl 16,0
	jmp	.processed
  .update:
	invoke	GetDlgItemText,[hwnd_dlg],ID_TEXT,search_string,1000h
	xor	ebx,ebx
	cmp	[search_string],0
	setnz	bl
	invoke	GetDlgItem,[hwnd_dlg],IDOK
	invoke	EnableWindow,eax,ebx
	jmp	.processed
  .close:
	invoke	EndDialog,[hwnd_dlg],FALSE
  .processed:
	mov	eax,1
  .finish:
	pop	edi esi ebx
	ret
endp

proc ReplaceDialog hwnd_dlg,msg,wparam,lparam
	push	ebx esi edi
	cmp	[msg],WM_INITDIALOG
	je	.initdialog
	cmp	[msg],WM_COMMAND
	je	.command
	jmp	.finish
  .initdialog:
	invoke	SetDlgItemText,[hwnd_dlg],ID_NEWTEXT,replace_string
	invoke	GetDlgItem,[hwnd_dlg],ID_NEWTEXT
	stdcall GetStringsFromHistory,eax
	xor	eax,eax
	test	[command_flags],CF_REPLACEPROMPT
	setnz	al
	invoke	CheckDlgButton,[hwnd_dlg],ID_PROMPT,eax
	jmp	.finish
  .command:
	cmp	[wparam],IDOK
	jne	.finish
	invoke	GetDlgItemText,[hwnd_dlg],ID_NEWTEXT,replace_string,1000h
	xor	ebx,ebx
	invoke	IsDlgButtonChecked,[hwnd_dlg],ID_PROMPT
	or	eax,eax
	jz	.prompt_ok
	or	ebx,CF_REPLACEPROMPT
  .prompt_ok:
	mov	[command_flags],ebx
	stdcall AddStringToHistory,replace_string
  .finish:
	stdcall FindDialog,[hwnd_dlg],[msg],[wparam],[lparam]
	pop	edi esi ebx
	ret
endp

proc CompileDialog hwnd_dlg,msg,wparam,lparam
	push	ebx esi edi
	cmp	[msg],WM_INITDIALOG
	je	.initdialog
	cmp	[msg],WM_COMMAND
	je	.command
	cmp	[msg],WM_CLOSE
	je	.close
	xor	eax,eax
	jmp	.finish
  .initdialog:
	mov	eax,[hwnd_dlg]
	mov	[hwnd_compiler],eax
	invoke	GetDlgItem,[hwnd_dlg],ID_PROGRESS
	mov	[hwnd_progress],eax
	invoke	SendMessage,eax,PBM_SETRANGE,0,40000h
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,[lparam],ei
	invoke	CreateThread,NULL,10000h,flat_assembler,[ei.pszpath],0,param_buffer
	mov	[hthread],eax
	jmp	.processed
  .command:
	cmp	[wparam],IDCANCEL
	je	.close
	cmp	[wparam],IDOK
	jne	.finish
  .get_exit_code:
	invoke	WaitForSingleObject,[hthread],-1
	invoke	GetExitCodeThread,[hthread],param_buffer
	invoke	CloseHandle,[hthread]
	invoke	EndDialog,[hwnd_dlg],[param_buffer]
	jmp	.processed
  .close:
	invoke	TerminateThread,[hthread],0FFh
	invoke	CloseHandle,[hthread]
	invoke	GlobalFree,[hmem_display]
	mov	eax,[memory_start]
	or	eax,eax
	jz	.cancel
	invoke	VirtualFree,eax,0,MEM_RELEASE
	mov	[memory_start],0
	cmp	[error_data_size],0
	je	.cancel
	invoke	GlobalFree,[hmem_error_data]
  .cancel:
	invoke	EndDialog,[hwnd_dlg],-1
  .processed:
	mov	eax,1
  .finish:
	pop	edi esi ebx
	ret
endp

proc ShowLine pszPath,nLine
	mov	[ei.header.mask],TCIF_PARAM
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETCURSEL,0,0
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,eax,ei
	invoke	lstrcmpi,[ei.pszpath],[pszPath]
	or	eax,eax
	jz	current_file_ok
	xor	ebx,ebx
      find_file_window:
	invoke	SendMessage,[hwnd_tabctrl],TCM_GETITEM,ebx,ei
	or	eax,eax
	jz	load_for_show
	invoke	lstrcmpi,[ei.pszpath],[pszPath]
	or	eax,eax
	jz	show_file
	inc	ebx
	jmp	find_file_window
      load_for_show:
	mov	esi,[pszPath]
	mov	edi,path_buffer
      copy_path_for_show:
	lodsb
	stosb
	or	al,al
	jnz	copy_path_for_show
	invoke	GetFileTitle,path_buffer,name_buffer,100h
	invoke	SendMessage,[hwnd_main],FM_OPEN,name_buffer,path_buffer
	cmp	eax,-1
	je	show_failed
	jmp	current_file_ok
      show_file:
	invoke	SendMessage,[hwnd_tabctrl],TCM_SETCURSEL,ebx,0
	invoke	SendMessage,[hwnd_main],FM_SELECT,ebx,0
      current_file_ok:
	mov	eax,[nLine]
	mov	[aepos.selectionLine],eax
	mov	[aepos.caretLine],eax
      get_lines_to_show:
	invoke	SendMessage,[hwnd_asmedit],AEM_GETLINELENGTH,[nLine],0
	mov	esi,string_buffer
	cmp	eax,1000h
	jb	get_line_data
	mov	edi,eax
	invoke	VirtualAlloc,0,edi,MEM_COMMIT,PAGE_READWRITE
	or	eax,eax
	jz	show_lines
	mov	esi,eax
      get_line_data:
	invoke	SendMessage,[hwnd_asmedit],AEM_GETLINE,[nLine],esi
	push	esi
	mov	ecx,eax
	or	ecx,ecx
	jz	no_more_lines
	mov	eax,[nLine]
	mov	[aepos.caretLine],eax
	mov	ebx,characters
	xor	edx,edx
      check_for_more_lines:
	lodsb
	cmp	al,3Bh
	je	no_more_lines
	mov	ah,al
	xlatb
	or	al,al
	jz	.symbol
	or	edx,edx
	jnz	.neutral
	cmp	ah,27h
	je	.quoted
	cmp	ah,22h
	je	.quoted
      .neutral:
	or	edx,-1
	loop	check_for_more_lines
	jmp	no_more_lines
      .symbol:
	cmp	ah,'\'
	je	.backslash
	xor	edx,edx
	loop	check_for_more_lines
	jmp	no_more_lines
      .quoted:
	dec	ecx
	jz	no_more_lines
	lodsb
	cmp	al,ah
	jne	.quoted
	dec	ecx
	jz	no_more_lines
	lodsb
	cmp	al,ah
	je	.quoted
	dec	esi
	xor	edx,edx
	jmp	check_for_more_lines
      .backslash:
	dec	ecx
	jz	more_lines
	lodsb
	cmp	al,20h
	je	.backslash
	cmp	al,3Bh
	jne	no_more_lines
      more_lines:
	inc	[nLine]
	pop	esi
	cmp	esi,string_buffer
	je	get_lines_to_show
	invoke	VirtualFree,esi,edi,MEM_DECOMMIT
	jmp	get_lines_to_show
      no_more_lines:
	pop	esi
	cmp	esi,string_buffer
	je	show_lines
	invoke	VirtualFree,esi,edi,MEM_DECOMMIT
      show_lines:
	mov	[aepos.selectionPosition],1
	inc	[aepos.caretLine]
	mov	[aepos.caretPosition],1
	invoke	SendMessage,[hwnd_asmedit],AEM_GETLINELENGTH,[aepos.caretLine],0
	cmp	eax,-1
	jne	show_ok
	dec	[aepos.caretLine]
	invoke	SendMessage,[hwnd_asmedit],AEM_GETLINELENGTH,[aepos.caretLine],0
	inc	eax
	mov	[aepos.caretPosition],eax
      show_ok:
	invoke	SendMessage,[hwnd_asmedit],AEM_SETPOS,aepos,0
	invoke	SendMessage,[hwnd_asmedit],AEM_GETMODE,0,0
	and	eax,not AEMODE_VERTICALSEL
	invoke	SendMessage,[hwnd_asmedit],AEM_SETMODE,eax,0
	mov	eax,[aepos.selectionLine]
	xchg	eax,[aepos.caretLine]
	mov	[aepos.selectionLine],eax
	mov	eax,[aepos.selectionPosition]
	xchg	eax,[aepos.caretPosition]
	mov	[aepos.selectionPosition],eax
	invoke	SendMessage,[hwnd_asmedit],AEM_SETPOS,aepos,0
	xor	eax,eax
	ret
      show_failed:
	or	eax,-1
	ret
endp

proc SummaryDialog hwnd_dlg,msg,wparam,lparam
	push	ebx esi edi
	cmp	[msg],WM_INITDIALOG
	je	.initdialog
	cmp	[msg],WM_COMMAND
	je	.command
	cmp	[msg],WM_CLOSE
	je	.close
	xor	eax,eax
	jmp	.finish
  .initdialog:
	invoke	GlobalLock,[hmem_display]
	invoke	SetDlgItemText,[hwnd_dlg],ID_DISPLAY,eax
	invoke	GlobalUnlock,[hmem_display]
	invoke	GlobalFree,[hmem_display]
	cmp	[lparam],2
	je	.error_details
	cmp	[lparam],0
	jne	.error_message
	movzx	eax,[current_pass]
	inc	eax
	mov	[param_buffer],eax
	mov	eax,[written_size]
	mov	[param_buffer+4],eax
	mov	[param_buffer+12],eax
	mov	eax,[total_time]
	xor	edx,edx
	mov	ebx,100
	div	ebx
	mov	ebx,_summary_short
	or	eax,eax
	jz	.summary_ok
	xor	edx,edx
	mov	ebx,10
	div	ebx
	mov	[param_buffer+4],eax
	mov	[param_buffer+8],edx
	mov	ebx,_summary
  .summary_ok:
	invoke	wvsprintf,string_buffer,ebx,param_buffer
	invoke	SetDlgItemText,[hwnd_dlg],ID_MESSAGE,string_buffer
	cmp	[lparam],2
	jne	.processed
  .show_line:
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_LINES,LB_GETCURSEL,0,0
	lea	ebx,[eax+1]
	invoke	GlobalLock,[hmem_error_data]
	mov	esi,[eax+ebx*8]
	add	esi,eax
	mov	eax,[eax+ebx*8+4]
	stdcall ShowLine,esi,eax
	invoke	GlobalUnlock,[hmem_error_data]
	jmp	.processed
  .error_details:
	invoke	GlobalLock,[hmem_error_data]
	mov	edi,eax
	xor	ebx,ebx
  .get_error_lines:
	inc	ebx
	mov	esi,[edi+ebx*8]
	add	esi,edi
	mov	eax,[edi+ebx*8+4]
	mov	[param_buffer+4],eax
	invoke	GetFullPathName,esi,1000h,path_buffer,param_buffer
	invoke	wvsprintf,string_buffer,_line_number,param_buffer
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_LINES,LB_ADDSTRING,0,string_buffer
	cmp	ebx,[edi]
	jb	.get_error_lines
	mov	eax,[edi+4]
	add	eax,edi
	invoke	SetDlgItemText,[hwnd_dlg],ID_INSTRUCTION,eax
	invoke	GlobalUnlock,[hmem_error_data]
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_LINES,LB_SETCURSEL,0,0
  .error_message:
	invoke	LoadIcon,0,IDI_HAND
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_ICON,STM_SETICON,eax,0
	mov	eax,[error_message]
	mov	[param_buffer],eax
	mov	ebx,_assembler_error
	jmp	.summary_ok
  .command:
	cmp	[wparam],ID_LINES + LBN_SELCHANGE shl 16
	je	.show_line
	cmp	[wparam],IDCANCEL
	je	.close
	cmp	[wparam],IDOK
	jne	.finish
	invoke	EndDialog,[hwnd_dlg],TRUE
	jmp	.processed
  .close:
	invoke	EndDialog,[hwnd_dlg],FALSE
  .processed:
	mov	eax,1
  .finish:
	pop	edi esi ebx
	ret
endp

proc AddStrings hwnd_combobox,lpstrings
	push	ebx esi
	mov	esi,[lpstrings]
  .add_string:
	cmp	byte [esi],0
	je	.finish
	invoke	SendMessage,[hwnd_combobox],CB_ADDSTRING,0,esi
  .next_string:
	lodsb
	or	al,al
	jnz	.next_string
	jmp	 .add_string
  .finish:
	pop	esi ebx
	ret
endp

proc AppearanceSetup hwnd_dlg,msg,wparam,lparam
	push	ebx esi edi
	cmp	[msg],WM_INITDIALOG
	je	.initdialog
	cmp	[msg],WM_DESTROY
	je	.destroy
	cmp	[msg],WM_COMMAND
	je	.command
	cmp	[msg],WM_CLOSE
	je	.close
  .notprocessed:
	xor	eax,eax
	jmp	.finish
  .initdialog:
	xor	eax,eax
	test	[asmedit_style],AES_CONSOLECARET
	setnz	al
	invoke	CheckDlgButton,[hwnd_dlg],ID_CONSOLECARET,eax
	mov	[cf.lStructSize],sizeof.CHOOSEFONT
	mov	eax,[hwnd_dlg]
	mov	[cf.hwndOwner],eax
	mov	[cf.Flags],CF_FIXEDPITCHONLY+CF_SCREENFONTS+CF_FORCEFONTEXIST+CF_INITTOLOGFONTSTRUCT
	mov	[cf.lpLogFont],tmp_font
	mov	[cc.lStructSize],sizeof.CHOOSECOLOR
	mov	eax,[hinstance]
	mov	[cc.hInstance],eax
	mov	eax,[hwnd_dlg]
	mov	[cc.hwndOwner],eax
	mov	[cc.lpCustColors],user_colors
	mov	[cc.Flags],CC_RGBINIT
	mov	esi,font
	mov	edi,tmp_font
	mov	ecx,sizeof.LOGFONT shr 2
	rep	movsd
	mov	esi,editor_colors
	mov	edi,tmp_colors
	mov	ecx,8
	rep	movsd
	mov	esi,editor_colors
	mov	edi,user_colors+20h
	mov	ecx,8
	rep	movsd
	invoke	GetDlgItem,[hwnd_dlg],ID_SETTING
	stdcall AddStrings,eax,_appearance_settings
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_SETTING,CB_SETCURSEL,0,0
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PREVIEW,WM_SETTEXT,0,preview_text
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PREVIEW,AEM_SETPOS,preview_selection,0
	invoke	CreateFontIndirect,[cf.lpLogFont]
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PREVIEW,WM_SETFONT,eax,0
  .update_colors:
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PREVIEW,AEM_SETTEXTCOLOR,[tmp_colors],[tmp_colors+4]
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PREVIEW,AEM_SETSELCOLOR,[tmp_colors+8],[tmp_colors+12]
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PREVIEW,AEM_SETSYNTAXHIGHLIGHT,tmp_colors+16,fasm_syntax
	jmp	.processed
  .destroy:
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PREVIEW,WM_GETFONT,0,0
	invoke	DeleteObject,eax
	jmp	.finish
  .command:
	cmp	[wparam],IDCANCEL
	je	.close
	cmp	[wparam],IDOK
	je	.ok
	cmp	[wparam],ID_CHANGE
	jne	.processed
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_SETTING,CB_GETCURSEL,0,0
	or	eax,eax
	jz	.change_font
	cmp	eax,8
	ja	.processed
	lea	ebx,[tmp_colors+(eax-1)*4]
	mov	eax,[ebx]
	mov	[cc.rgbResult],eax
	invoke	ChooseColor,cc
	or	eax,eax
	jz	.processed
	mov	eax,[cc.rgbResult]
	mov	[ebx],eax
	jmp	.update_colors
  .change_font:
	mov	esi,tmp_font
	mov	edi,backup_font
	mov	ecx,sizeof.LOGFONT shr 2
	rep	movsd
	invoke	ChooseFont,cf
	or	eax,eax
	jz	.change_font_cancelled
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PREVIEW,WM_GETFONT,0,0
	mov	ebx,eax
	invoke	CreateFontIndirect,[cf.lpLogFont]
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PREVIEW,WM_SETFONT,eax,0
	invoke	DeleteObject,ebx
	jmp	.processed
  .change_font_cancelled:
	mov	esi,backup_font
	mov	edi,tmp_font
	mov	ecx,sizeof.LOGFONT shr 2
	rep	movsd
	jmp	.processed
  .ok:
	mov	esi,tmp_colors
	mov	edi,editor_colors
	mov	ecx,8
	rep	movsd
	mov	esi,tmp_font
	mov	edi,font
	mov	ecx,sizeof.LOGFONT shr 2
	rep	movsd
	invoke	CreateFontIndirect,font
	xchg	eax,[hfont]
	invoke	DeleteObject,eax
	invoke	IsDlgButtonChecked,[hwnd_dlg],ID_CONSOLECARET
	or	eax,eax
	setnz	al
	neg	eax
	and	eax,AES_CONSOLECARET
	and	[asmedit_style],not AES_CONSOLECARET
	or	[asmedit_style],eax
	invoke	EndDialog,[hwnd_dlg],TRUE
	jmp	finish
  .close:
	invoke	EndDialog,[hwnd_dlg],FALSE
  .processed:
	mov	eax,1
  .finish:
	pop	edi esi ebx
	ret
endp

proc CompilerSetup hwnd_dlg,msg,wparam,lparam
	push	ebx esi edi
	cmp	[msg],WM_INITDIALOG
	je	.initdialog
	cmp	[msg],WM_COMMAND
	je	.command
	cmp	[msg],WM_CLOSE
	je	.close
  .notprocessed:
	xor	eax,eax
	jmp	.finish
  .initdialog:
	invoke	GetDlgItem,[hwnd_dlg],ID_MEMORY
	stdcall AddStrings,eax,_memory_settings
	invoke	GetDlgItem,[hwnd_dlg],ID_PRIORITY
	stdcall AddStrings,eax,_priority_settings
	cinvoke wsprintf,string_buffer,_value,[compiler_memory]
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_MEMORY,CB_FINDSTRINGEXACT,-1,string_buffer
	cmp	eax,CB_ERR
	je	.set_memory
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_MEMORY,CB_SETCURSEL,eax,0
	jmp	.memory_ok
  .set_memory:
	invoke	SetDlgItemText,[hwnd_dlg],ID_MEMORY,string_buffer
  .memory_ok:
	mov	eax,[compiler_priority]
	cmp	eax,2
	jg	.realtime
	cmp	eax,-2
	jl	.idle
	jmp	.priority_ok
  .idle:
	mov	eax,-4
	jmp	.priority_ok
  .realtime:
	mov	eax,4
  .priority_ok:
	sar	eax,1
	add	eax,2
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PRIORITY,CB_SETCURSEL,eax,0
	jmp	.processed
  .command:
	cmp	[wparam],IDCANCEL
	je	.close
	cmp	[wparam],IDOK
	jne	.finish
	invoke	GetDlgItemInt,[hwnd_dlg],ID_MEMORY,param_buffer,FALSE
	mov	[compiler_memory],eax
	invoke	SendDlgItemMessage,[hwnd_dlg],ID_PRIORITY,CB_GETCURSEL,0,0
	sub	eax,2
	sal	eax,1
	cmp	eax,4
	je	.set_realtime
	cmp	eax,-4
	je	.set_idle
	jmp	.set_priority
  .set_idle:
	mov	eax,-15
	jmp	.set_priority
  .set_realtime:
	mov	eax,15
  .set_priority:
	mov	[compiler_priority],eax
	invoke	EndDialog,[hwnd_dlg],TRUE
	jmp	finish
  .close:
	invoke	EndDialog,[hwnd_dlg],FALSE
  .processed:
	mov	eax,1
  .finish:
	pop	edi esi ebx
	ret
endp

proc AboutDialog hwnd,msg,wparam,lparam
	push	ebx esi edi
	cmp	[msg],WM_COMMAND
	je	.close
	cmp	[msg],WM_CLOSE
	je	.close
	xor	eax,eax
	jmp	.finish
  .close:
	invoke	EndDialog,[hwnd],0
  .processed:
	mov	eax,1
  .finish:
	pop	edi esi ebx
	ret
endp

section '.flat' code data readable executable writeable

include 'fasm.inc'

section '.idata' import data readable writeable

  library kernel,'KERNEL32.DLL',\
	  user,'USER32.DLL',\
	  gdi,'GDI32.DLL',\
	  comctl,'COMCTL32.DLL',\
	  comdlg,'COMDLG32.DLL',\
	  shell,'SHELL32.DLL'

  import kernel,\
	 GetModuleHandle,'GetModuleHandleA',\
	 LoadLibrary,'LoadLibraryA',\
	 GetProcAddress,'GetProcAddress',\
	 GetCommandLine,'GetCommandLineA',\
	 GetFullPathName,'GetFullPathNameA',\
	 SetCurrentDirectory,'SetCurrentDirectoryA',\
	 CreateFile,'CreateFileA',\
	 GetFileSize,'GetFileSize',\
	 ReadFile,'ReadFile',\
	 WriteFile,'WriteFile',\
	 SetFilePointer,'SetFilePointer',\
	 CloseHandle,'CloseHandle',\
	 lstrcmpi,'lstrcmpiA',\
	 GlobalAlloc,'GlobalAlloc',\
	 GlobalReAlloc,'GlobalReAlloc',\
	 GlobalLock,'GlobalLock',\
	 GlobalUnlock,'GlobalUnlock',\
	 GlobalFree,'GlobalFree',\
	 VirtualAlloc,'VirtualAlloc',\
	 VirtualFree,'VirtualFree',\
	 CreateThread,'CreateThread',\
	 SetThreadPriority,'SetThreadPriority',\
	 TerminateThread,'TerminateThread',\
	 ExitThread,'ExitThread',\
	 GetExitCodeThread,'GetExitCodeThread',\
	 WaitForSingleObject,'WaitForSingleObject',\
	 CreateProcess,'CreateProcessA',\
	 AllocConsole,'AllocConsole',\
	 SetConsoleTitle,'SetConsoleTitleA',\
	 FreeConsole,'FreeConsole',\
	 GetEnvironmentVariable,'GetEnvironmentVariableA',\
	 GetSystemTime,'GetSystemTime',\
	 GetTickCount,'GetTickCount',\
	 GetPrivateProfileString,'GetPrivateProfileStringA',\
	 WritePrivateProfileString,'WritePrivateProfileStringA',\
	 ExitProcess,'ExitProcess'

  import user,\
	 RegisterClass,'RegisterClassA',\
	 CreateCaret,'CreateCaret',\
	 ShowCaret,'ShowCaret',\
	 HideCaret,'HideCaret',\
	 SetCaretPos,'SetCaretPos',\
	 DestroyCaret,'DestroyCaret',\
	 BeginPaint,'BeginPaint',\
	 EndPaint,'EndPaint',\
	 GetDC,'GetDC',\
	 GetUpdateRect,'GetUpdateRect',\
	 ReleaseDC,'ReleaseDC',\
	 DrawText,'DrawTextA',\
	 FillRect,'FillRect',\
	 InvalidateRect,'InvalidateRect',\
	 GetKeyboardState,'GetKeyboardState',\
	 ToAscii,'ToAscii',\
	 GetScrollInfo,'GetScrollInfo',\
	 SetScrollInfo,'SetScrollInfo',\
	 SetCapture,'SetCapture',\
	 ReleaseCapture,'ReleaseCapture',\
	 GetCursorPos,'GetCursorPos',\
	 ClientToScreen,'ClientToScreen',\
	 OpenClipboard,'OpenClipboard',\
	 CloseClipboard,'CloseClipboard',\
	 EmptyClipboard,'EmptyClipboard',\
	 GetClipboardData,'GetClipboardData',\
	 SetClipboardData,'SetClipboardData',\
	 LoadCursor,'LoadCursorA',\
	 LoadIcon,'LoadIconA',\
	 LoadBitmap,'LoadBitmapA',\
	 LoadMenu,'LoadMenuA',\
	 EnableMenuItem,'EnableMenuItem',\
	 CheckMenuItem,'CheckMenuItem',\
	 GetSubMenu,'GetSubMenu',\
	 TrackPopupMenu,'TrackPopupMenu',\
	 LoadAccelerators,'LoadAcceleratorsA',\
	 IsClipboardFormatAvailable,'IsClipboardFormatAvailable',\
	 CharLower,'CharLowerA',\
	 CharUpper,'CharUpperA',\
	 wsprintf,'wsprintfA',\
	 wvsprintf,'wvsprintfA',\
	 MessageBox,'MessageBoxA',\
	 WinHelp,'WinHelpA',\
	 DialogBoxParam,'DialogBoxParamA',\
	 GetDlgItem,'GetDlgItem',\
	 GetDlgItemInt,'GetDlgItemInt',\
	 SetDlgItemText,'SetDlgItemTextA',\
	 GetDlgItemText,'GetDlgItemTextA',\
	 CheckDlgButton,'CheckDlgButton',\
	 IsDlgButtonChecked,'IsDlgButtonChecked',\
	 SendDlgItemMessage,'SendDlgItemMessageA',\
	 EndDialog,'EndDialog',\
	 FindWindow,'FindWindowA',\
	 SetForegroundWindow,'SetForegroundWindow',\
	 CreateWindowEx,'CreateWindowExA',\
	 DestroyWindow,'DestroyWindow',\
	 GetWindowLong,'GetWindowLongA',\
	 SetWindowLong,'SetWindowLongA',\
	 DefWindowProc,'DefWindowProcA',\
	 GetClientRect,'GetClientRect',\
	 GetWindowRect,'GetWindowRect',\
	 MoveWindow,'MoveWindow',\
	 SetWindowPos,'SetWindowPos',\
	 GetWindowPlacement,'GetWindowPlacement',\
	 SetWindowPlacement,'SetWindowPlacement',\
	 ShowWindow,'ShowWindow',\
	 EnableWindow,'EnableWindow',\
	 UpdateWindow,'UpdateWindow',\
	 IsZoomed,'IsZoomed',\
	 SetFocus,'SetFocus',\
	 GetSystemMetrics,'GetSystemMetrics',\
	 GetSysColor,'GetSysColor',\
	 SendMessage,'SendMessageA',\
	 GetMessage,'GetMessageA',\
	 TranslateAccelerator,'TranslateAccelerator',\
	 TranslateMessage,'TranslateMessage',\
	 DispatchMessage,'DispatchMessageA',\
	 PostMessage,'PostMessageA',\
	 PostQuitMessage,'PostQuitMessage'

  import gdi,\
	 SetBkColor,'SetBkColor',\
	 SetTextColor,'SetTextColor',\
	 CreateSolidBrush,'CreateSolidBrush',\
	 CreateFont,'CreateFontA',\
	 CreateFontIndirect,'CreateFontIndirectA',\
	 GetTextMetrics,'GetTextMetricsA',\
	 GetTextExtentPoint32,'GetTextExtentPoint32A',\
	 CreateCompatibleDC,'CreateCompatibleDC',\
	 DeleteDC,'DeleteDC',\
	 CreateBitmap,'CreateBitmap',\
	 SelectObject,'SelectObject',\
	 GetObject,'GetObjectA',\
	 DeleteObject,'DeleteObject'

  import comctl,\
	 CreateStatusWindow,'CreateStatusWindowA',\
	 ImageList_Create,'ImageList_Create',\
	 ImageList_Add,'ImageList_Add',\
	 ImageList_Destroy,'ImageList_Destroy'

  import comdlg,\
	 GetOpenFileName,'GetOpenFileNameA',\
	 GetSaveFileName,'GetSaveFileNameA',\
	 GetFileTitle,'GetFileTitleA',\
	 ChooseFont,'ChooseFontA',\
	 ChooseColor,'ChooseColorA'

  import shell,\
	 DragAcceptFiles,'DragAcceptFiles',\
	 DragQueryFile,'DragQueryFile',\
	 DragFinish,'DragFinish',\
	 ShellExecute,'ShellExecuteA'

section '.rsrc' resource data readable

  directory RT_MENU,menus,\
	    RT_ACCELERATOR,accelerators,\
	    RT_DIALOG,dialogs,\
	    RT_GROUP_ICON,group_icons,\
	    RT_ICON,icons,\
	    RT_BITMAP,bitmaps,\
	    RT_VERSION,versions

  resource menus,\
	   IDM_MAIN,LANG_ENGLISH+SUBLANG_DEFAULT,main_menu,\
	   IDM_TAB,LANG_ENGLISH+SUBLANG_DEFAULT,popup_menu

  resource accelerators,\
	   IDA_MAIN,LANG_ENGLISH+SUBLANG_DEFAULT,main_keys

  resource dialogs,\
	   IDD_POSITION,LANG_ENGLISH+SUBLANG_DEFAULT,position_dialog,\
	   IDD_FIND,LANG_ENGLISH+SUBLANG_DEFAULT,find_dialog,\
	   IDD_REPLACE,LANG_ENGLISH+SUBLANG_DEFAULT,replace_dialog,\
	   IDD_COMPILE,LANG_ENGLISH+SUBLANG_DEFAULT,compile_dialog,\
	   IDD_SUMMARY,LANG_ENGLISH+SUBLANG_DEFAULT,summary_dialog,\
	   IDD_ERRORSUMMARY,LANG_ENGLISH+SUBLANG_DEFAULT,error_summary_dialog,\
	   IDD_APPEARANCE,LANG_ENGLISH+SUBLANG_DEFAULT,appearance_dialog,\
	   IDD_COMPILERSETUP,LANG_ENGLISH+SUBLANG_DEFAULT,compiler_setup_dialog,\
	   IDD_ABOUT,LANG_ENGLISH+SUBLANG_DEFAULT,about_dialog

  resource group_icons,\
	   IDI_MAIN,LANG_NEUTRAL,main_icon

  resource icons,\
	   1,LANG_NEUTRAL,main_icon_data

  resource bitmaps,\
	   IDB_ASSIGN,LANG_NEUTRAL,assign_bitmap

  resource versions,\
	   1,LANG_NEUTRAL,version

  IDM_MAIN	    = 101
  IDM_TAB	    = 102
  IDA_MAIN	    = 201
  IDD_POSITION	    = 301
  IDD_FIND	    = 302
  IDD_REPLACE	    = 303
  IDD_COMPILE	    = 304
  IDD_SUMMARY	    = 305
  IDD_ERRORSUMMARY  = 306
  IDD_APPEARANCE    = 307
  IDD_COMPILERSETUP = 308
  IDD_ABOUT	    = 309
  IDI_MAIN	    = 401
  IDB_ASSIGN	    = 501

  IDM_NEW	     = 1101
  IDM_OPEN	     = 1102
  IDM_SAVE	     = 1103
  IDM_SAVEAS	     = 1104
  IDM_NEXT	     = 1105
  IDM_PREVIOUS	     = 1106
  IDM_OPENFOLDER     = 1107
  IDM_CLOSE	     = 1108
  IDM_EXIT	     = 1109
  IDM_SELECTFILE     = 1110
  IDM_UNDO	     = 1201
  IDM_CUT	     = 1202
  IDM_COPY	     = 1203
  IDM_PASTE	     = 1204
  IDM_DELETE	     = 1205
  IDM_SELECTALL      = 1206
  IDM_VERTICAL	     = 1207
  IDM_POSITION	     = 1301
  IDM_FIND	     = 1302
  IDM_FINDNEXT	     = 1303
  IDM_REPLACE	     = 1304
  IDM_RUN	     = 1401
  IDM_COMPILE	     = 1402
  IDM_ASSIGN	     = 1409
  IDM_APPEARANCE     = 1501
  IDM_COMPILERSETUP  = 1502
  IDM_SECURESEL      = 1505
  IDM_AUTOBRACKETS   = 1506
  IDM_AUTOINDENT     = 1507
  IDM_SMARTTABS      = 1508
  IDM_OPTIMALFILL    = 1509
  IDM_REVIVEDEADKEYS = 1510
  IDM_CONTENTS	     = 1901
  IDM_KEYWORD	     = 1902
  IDM_PICKHELP	     = 1903
  IDM_ABOUT	     = 1909

  ID_CHANGE	   = 2001
  ID_SELECT	   = 2101
  ID_CASESENSITIVE = 2102
  ID_WHOLEWORDS    = 2103
  ID_BACKWARD	   = 2104
  ID_PROMPT	   = 2105
  ID_CONSOLECARET  = 2106
  ID_ROW	   = 2201
  ID_COLUMN	   = 2202
  ID_DISPLAY	   = 2203
  ID_INSTRUCTION   = 2204
  ID_TEXT	   = 2301
  ID_NEWTEXT	   = 2302
  ID_SETTING	   = 2303
  ID_MEMORY	   = 2304
  ID_PRIORITY	   = 2305
  ID_LINES	   = 2306
  ID_ICON	   = 2401
  ID_MESSAGE	   = 2402
  ID_PROGRESS	   = 2801
  ID_PREVIEW	   = 2901

  _ equ ,09h,

  menu main_menu
       menuitem '&File',0,MFR_POPUP
		menuitem '&New' _ 'Ctrl+N',IDM_NEW
		menuitem '&Open...' _ 'Ctrl+O',IDM_OPEN
		menuitem '&Save' _ 'Ctrl+S',IDM_SAVE
		menuitem 'Save &as...',IDM_SAVEAS
		menuseparator
		menuitem 'E&xit' _ 'Alt+X',IDM_EXIT,MFR_END
       menuitem '&Edit',0,MFR_POPUP
		menuitem '&Undo' _ 'Ctrl+Z',IDM_UNDO
		menuseparator
		menuitem 'Cu&t' _ 'Ctrl+X',IDM_CUT
		menuitem '&Copy' _ 'Ctrl+C',IDM_COPY
		menuitem '&Paste' _ 'Ctrl+V',IDM_PASTE
		menuitem '&Delete',IDM_DELETE
		menuseparator
		menuitem 'Select &all' _ 'Ctrl+A',IDM_SELECTALL
		menuseparator
		menuitem '&Vertical selection' _ 'Alt+Ins',IDM_VERTICAL,MFR_END
       menuitem '&Search',0,MFR_POPUP
		menuitem '&Position...' _ 'Ctrl+G',IDM_POSITION
		menuseparator
		menuitem '&Find...' _ 'Ctrl+F',IDM_FIND
		menuitem 'Find &next' _ 'F3',IDM_FINDNEXT
		menuitem '&Replace...' _ 'Ctrl+H',IDM_REPLACE,MFR_END
       menuitem '&Run',0,MFR_POPUP
		menuitem '&Run' _ 'F9',IDM_RUN
		menuitem '&Compile' _ 'Ctrl+F9',IDM_COMPILE,MFR_END
       menuitem '&Options',0,MFR_POPUP
		menuitem '&Appearance...',IDM_APPEARANCE
		menuitem '&Compiler setup...',IDM_COMPILERSETUP
		menuseparator
		menuitem '&Secure selection',IDM_SECURESEL
		menuitem 'Automatic &brackets',IDM_AUTOBRACKETS
		menuitem 'Automatic &indents',IDM_AUTOINDENT
		menuitem 'Smart &tabulation',IDM_SMARTTABS
		menuitem '&Optimal fill on saving',IDM_OPTIMALFILL
		menuitem '&Revive dead keys',IDM_REVIVEDEADKEYS,MFR_END
       menuitem '&Help',0,MFR_POPUP + MFR_END
		menuitem '&Contents' _ 'Alt+F1',IDM_CONTENTS
		menuitem '&Keyword search' _ 'F1',IDM_KEYWORD
		menuseparator
		menuitem '&Pick help file...',IDM_PICKHELP
		menuseparator
		menuitem '&About...',IDM_ABOUT,MFR_END

  menu popup_menu
       menuitem '',0,MFR_POPUP+MFR_END
		menuitem '&Assign to compiler',IDM_ASSIGN,0
		menuitem 'Open &folder',IDM_OPENFOLDER,0
		menuseparator
		menuitem '&Close',IDM_CLOSE,MFR_END

  accelerator main_keys,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'N',IDM_NEW,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'O',IDM_OPEN,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'S',IDM_SAVE,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'Z',IDM_UNDO,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'X',IDM_CUT,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'C',IDM_COPY,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'V',IDM_PASTE,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'A',IDM_SELECTALL,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'G',IDM_POSITION,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'F',IDM_FIND,\
	      FVIRTKEY+FNOINVERT+FCONTROL,'H',IDM_REPLACE,\
	      FVIRTKEY+FNOINVERT+FCONTROL,VK_TAB,IDM_NEXT,\
	      FVIRTKEY+FNOINVERT+FCONTROL+FSHIFT,VK_TAB,IDM_PREVIOUS,\
	      FVIRTKEY+FNOINVERT,VK_F1,IDM_KEYWORD,\
	      FVIRTKEY+FNOINVERT+FALT,VK_F1,IDM_CONTENTS,\
	      FVIRTKEY+FNOINVERT,VK_F2,IDM_SAVE,\
	      FVIRTKEY+FNOINVERT+FSHIFT,VK_F2,IDM_SAVEAS,\
	      FVIRTKEY+FNOINVERT,VK_F4,IDM_OPEN,\
	      FVIRTKEY+FNOINVERT,VK_F3,IDM_FINDNEXT,\
	      FVIRTKEY+FNOINVERT,VK_F5,IDM_POSITION,\
	      FVIRTKEY+FNOINVERT,VK_F7,IDM_FIND,\
	      FVIRTKEY+FNOINVERT+FSHIFT,VK_F7,IDM_FINDNEXT,\
	      FVIRTKEY+FNOINVERT+FCONTROL,VK_F7,IDM_REPLACE,\
	      FVIRTKEY+FNOINVERT,VK_F9,IDM_RUN,\
	      FVIRTKEY+FNOINVERT+FCONTROL,VK_F9,IDM_COMPILE,\
	      FVIRTKEY+FNOINVERT+FSHIFT,VK_F9,IDM_ASSIGN,\
	      FVIRTKEY+FNOINVERT,VK_ESCAPE,IDM_CLOSE,\
	      FVIRTKEY+FNOINVERT+FALT,'X',IDM_EXIT,\
	      FVIRTKEY+FNOINVERT+FALT,'1',IDM_SELECTFILE+1,\
	      FVIRTKEY+FNOINVERT+FALT,'2',IDM_SELECTFILE+2,\
	      FVIRTKEY+FNOINVERT+FALT,'3',IDM_SELECTFILE+3,\
	      FVIRTKEY+FNOINVERT+FALT,'4',IDM_SELECTFILE+4,\
	      FVIRTKEY+FNOINVERT+FALT,'5',IDM_SELECTFILE+5,\
	      FVIRTKEY+FNOINVERT+FALT,'6',IDM_SELECTFILE+6,\
	      FVIRTKEY+FNOINVERT+FALT,'7',IDM_SELECTFILE+7,\
	      FVIRTKEY+FNOINVERT+FALT,'8',IDM_SELECTFILE+8,\
	      FVIRTKEY+FNOINVERT+FALT,'9',IDM_SELECTFILE+9

  dialog position_dialog,'Position',40,40,126,54,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'STATIC','&Row:',-1,4,8,28,8,WS_VISIBLE+SS_RIGHT
    dialogitem 'EDIT','',ID_ROW,36,6,34,12,WS_VISIBLE+WS_BORDER+WS_TABSTOP+ES_NUMBER
    dialogitem 'STATIC','&Column:',-1,4,26,28,8,WS_VISIBLE+SS_RIGHT
    dialogitem 'EDIT','',ID_COLUMN,36,24,34,12,WS_VISIBLE+WS_BORDER+WS_TABSTOP+ES_NUMBER
    dialogitem 'BUTTON','&Select',ID_SELECT,36,42,48,8,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','OK',IDOK,78,6,42,14,WS_VISIBLE+WS_TABSTOP+BS_DEFPUSHBUTTON
    dialogitem 'BUTTON','C&ancel',IDCANCEL,78,22,42,14,WS_VISIBLE+WS_TABSTOP+BS_PUSHBUTTON
  enddialog

  dialog find_dialog,'Find',60,60,254,54,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'STATIC','&Text to find:',-1,4,8,40,8,WS_VISIBLE+SS_RIGHT
    dialogitem 'COMBOBOX','',ID_TEXT,48,6,150,64,WS_VISIBLE+WS_BORDER+WS_TABSTOP+CBS_DROPDOWN+CBS_AUTOHSCROLL+WS_VSCROLL
    dialogitem 'BUTTON','&Case sensitive',ID_CASESENSITIVE,48,24,70,8,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','&Whole words',ID_WHOLEWORDS,48,38,70,8,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','&Backward',ID_BACKWARD,124,24,70,8,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','&Find first',IDOK,206,6,42,14,WS_VISIBLE+WS_TABSTOP+BS_DEFPUSHBUTTON
    dialogitem 'BUTTON','C&ancel',IDCANCEL,206,22,42,14,WS_VISIBLE+WS_TABSTOP+BS_PUSHBUTTON
  enddialog

  dialog replace_dialog,'Replace',60,60,254,72,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'STATIC','&Text to find:',-1,4,8,40,8,WS_VISIBLE+SS_RIGHT
    dialogitem 'COMBOBOX','',ID_TEXT,48,6,150,64,WS_VISIBLE+WS_BORDER+WS_TABSTOP+CBS_DROPDOWN+CBS_AUTOHSCROLL+WS_VSCROLL
    dialogitem 'STATIC','&New text:',-1,4,26,40,8,WS_VISIBLE+SS_RIGHT
    dialogitem 'COMBOBOX','',ID_NEWTEXT,48,24,150,64,WS_VISIBLE+WS_BORDER+WS_TABSTOP+CBS_DROPDOWN+CBS_AUTOHSCROLL+WS_VSCROLL
    dialogitem 'BUTTON','&Case sensitive',ID_CASESENSITIVE,48,42,70,8,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','&Whole words',ID_WHOLEWORDS,48,56,70,8,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','&Backward',ID_BACKWARD,124,42,70,8,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','&Prompt on replace',ID_PROMPT,124,56,70,8,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','&Replace',IDOK,206,6,42,14,WS_VISIBLE+WS_TABSTOP+BS_DEFPUSHBUTTON
    dialogitem 'BUTTON','C&ancel',IDCANCEL,206,22,42,14,WS_VISIBLE+WS_TABSTOP+BS_PUSHBUTTON
  enddialog

  dialog compile_dialog,'Compile',64,64,192,42,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'msctls_progress32','',ID_PROGRESS,8,6,176,12,WS_VISIBLE
    dialogitem 'BUTTON','C&ancel',IDCANCEL,75,24,42,14,WS_VISIBLE+WS_TABSTOP+BS_PUSHBUTTON
  enddialog

  dialog summary_dialog,'Compile',48,24,192,130,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'BUTTON','OK',IDCANCEL,142,108,42,14,WS_VISIBLE+WS_TABSTOP+BS_DEFPUSHBUTTON
    dialogitem 'STATIC',IDI_ASTERISK,ID_ICON,8,4,0,0,WS_VISIBLE+SS_ICON
    dialogitem 'STATIC','',ID_MESSAGE,36,10,148,8,WS_VISIBLE
    dialogitem 'STATIC','&Display:',-1,8,28,176,8,WS_VISIBLE
    dialogitem 'EDIT','',ID_DISPLAY,8,40,176,64,WS_VISIBLE+WS_BORDER+WS_TABSTOP+ES_MULTILINE+ES_READONLY+ES_AUTOHSCROLL+ES_AUTOVSCROLL+WS_VSCROLL
  enddialog

  dialog error_summary_dialog,'Compile',48,24,192,144,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'STATIC','&Source:',-1,8,96,176,8,WS_VISIBLE
    dialogitem 'LISTBOX','',ID_LINES,8,108,126,32,WS_VISIBLE+WS_BORDER+WS_TABSTOP+WS_VSCROLL+LBS_NOTIFY
    dialogitem 'BUTTON','OK',IDCANCEL,142,108,42,14,WS_VISIBLE+WS_TABSTOP+BS_DEFPUSHBUTTON
    dialogitem 'STATIC',IDI_HAND,ID_ICON,8,4,0,0,WS_VISIBLE+SS_ICON
    dialogitem 'STATIC','',ID_MESSAGE,36,10,148,8,WS_VISIBLE
    dialogitem 'STATIC','&Display:',-1,8,28,176,8,WS_VISIBLE
    dialogitem 'EDIT','',ID_DISPLAY,8,40,176,24,WS_VISIBLE+WS_BORDER+WS_TABSTOP+ES_MULTILINE+ES_READONLY+ES_AUTOHSCROLL+ES_AUTOVSCROLL+WS_VSCROLL
    dialogitem 'STATIC','&Instruction:',-1,8,68,176,8,WS_VISIBLE
    dialogitem 'EDIT','',ID_INSTRUCTION,8,80,176,12,WS_VISIBLE+WS_BORDER+WS_TABSTOP+ES_READONLY+ES_AUTOHSCROLL
  enddialog

  dialog appearance_dialog,'Appearance',50,20,186,166,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'COMBOBOX','',ID_SETTING,8,6,120,140,WS_VISIBLE+WS_TABSTOP+CBS_DROPDOWNLIST+WS_VSCROLL
    dialogitem 'BUTTON','C&hange...',ID_CHANGE,134,6,42,13,WS_VISIBLE+WS_TABSTOP+BS_PUSHBUTTON
    dialogitem 'ASMEDIT','',ID_PREVIEW,8,24,168,120,WS_VISIBLE+WS_BORDER+WS_DISABLED+ES_NOHIDESEL
    dialogitem 'BUTTON','&Console caret',ID_CONSOLECARET,8,151,80,8,WS_VISIBLE+WS_TABSTOP+BS_AUTOCHECKBOX
    dialogitem 'BUTTON','OK',IDOK,88,148,42,14,WS_VISIBLE+WS_TABSTOP+BS_DEFPUSHBUTTON
    dialogitem 'BUTTON','C&ancel',IDCANCEL,134,148,42,14,WS_VISIBLE+WS_TABSTOP+BS_PUSHBUTTON
  enddialog

  dialog compiler_setup_dialog,'Compiler setup',54,28,148,44,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'STATIC','&Memory:',-1,4,8,30,8,WS_VISIBLE+SS_RIGHT
    dialogitem 'COMBOBOX','',ID_MEMORY,38,6,54,96,WS_VISIBLE+WS_BORDER+WS_TABSTOP+ES_NUMBER+CBS_DROPDOWN+WS_VSCROLL
    dialogitem 'STATIC','&Priority:',-1,4,26,30,8,WS_VISIBLE+SS_RIGHT
    dialogitem 'COMBOBOX','',ID_PRIORITY,38,24,54,96,WS_VISIBLE+WS_TABSTOP+CBS_DROPDOWNLIST+WS_VSCROLL
    dialogitem 'BUTTON','OK',IDOK,100,6,42,14,WS_VISIBLE+WS_TABSTOP+BS_DEFPUSHBUTTON
    dialogitem 'BUTTON','C&ancel',IDCANCEL,100,22,42,14,WS_VISIBLE+WS_TABSTOP+BS_PUSHBUTTON
  enddialog

  dialog about_dialog,'About',40,40,172,60,WS_CAPTION+WS_POPUP+WS_SYSMENU+DS_MODALFRAME
    dialogitem 'STATIC',<'flat assembler ',2014h,' version ',VERSION_STRING,0Dh,0Ah,'Copyright ',0A9h,' 1999-2007 Tomasz Grysztar.'>,-1,27,10,144,40,WS_VISIBLE+SS_CENTER
    dialogitem 'STATIC',IDI_MAIN,-1,8,8,32,32,WS_VISIBLE+SS_ICON
    dialogitem 'STATIC','',-1,4,34,164,11,WS_VISIBLE+SS_ETCHEDHORZ
    dialogitem 'STATIC',<'Interface version ',IDE_VERSION_STRING>,-1,4,38,100,20,WS_VISIBLE+SS_LEFT
    dialogitem 'BUTTON','OK',IDOK,124,40,42,14,WS_VISIBLE+WS_TABSTOP+BS_DEFPUSHBUTTON
  enddialog

  icon main_icon,main_icon_data,'resource\fasmw.ico'

  bitmap assign_bitmap,'resource\assign.bmp'

  versioninfo version,VOS__WINDOWS32,VFT_APP,VFT2_UNKNOWN,LANG_ENGLISH+SUBLANG_DEFAULT,0,\
	      'FileDescription','flat assembler',\
	      'LegalCopyright',<'Copyright ',0A9h,' 1999-2007 Tomasz Grysztar.'>,\
	      'FileVersion',IDE_VERSION_STRING,\
	      'ProductVersion',VERSION_STRING,\
	      'OriginalFilename','FASMW.EXE'
