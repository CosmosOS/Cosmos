
; Extended Win32 programming headers with parameters count checking (ASCII)

include 'win32a.inc'

include 'macro/if.inc'

macro allow_nesting
{ macro pushd value
  \{ match ,value \\{
      pushx equ \\}
     match =pushx =invoke proc,pushx value \\{
      allow_nesting
      invoke proc
      purge pushd,invoke,stdcall,cinvoke,ccall
      push eax
      pushx equ \\}
     match =pushx =stdcall proc,pushx value \\{
      allow_nesting
      stdcall proc
      purge pushd,invoke,stdcall,cinvoke,ccall
      push eax
      pushx equ \\}
     match =pushx =cinvoke proc,pushx value \\{
      allow_nesting
      cinvoke proc
      purge pushd,invoke,stdcall,cinvoke,ccall
      push eax
      pushx equ \\}
     match =pushx =ccall proc,pushx value \\{
      allow_nesting
      ccall proc
      purge pushd,invoke,stdcall,cinvoke,ccall
      push eax
      pushx equ \\}
     match =pushx,pushx \\{
      pushd <value>
      pushx equ \\}
     restore pushx \}
  macro invoke proc,[arg]
  \{ \common count@stdcall = 0
	     if ~ arg eq
     \forward count@stdcall = count@stdcall+1
	      match =double value, arg \\{ count@stdcall = count@stdcall+1 \\}
     \common end if
	     if defined proc \# %
	      if count@stdcall <> proc \# %
	       display "Error: invalid count of parameters for ",\`proc,".",0Dh,0Ah
	       err
	      end if
	     end if
     \reverse pushd <arg>
     \common call [proc] \}
  macro stdcall proc,[arg]
  \{ \common count@stdcall = 0
	     if ~ arg eq
     \forward count@stdcall = count@stdcall+1
	      match =double value, arg \\{ count@stdcall = count@stdcall+1 \\}
     \common end if
	     if defined proc \# %
	      if count@stdcall <> proc \# %
	       display "Error: invalid count of parameters for ",\`proc,".",0Dh,0Ah
	       err
	      end if
	     end if
     \reverse pushd <arg>
     \common call proc \}
  macro cinvoke proc,[arg]
  \{ \common \local size
	     size = 0
	     if ~ arg eq
     \reverse pushd <arg>
	      size = size+4
	      match =double any,arg \\{ size = size+4 \\}
     \common end if
	     call [proc]
	     if size
	     add esp,size
	     end if \}
  macro ccall proc,[arg]
  \{ \common \local size
	     size = 0
	     if ~ arg eq
     \reverse pushd <arg>
	      size = size+4
	      match =double any,arg \\{ size = size+4 \\}
     \common end if
	     call proc
	     if size
	     add esp,size
	     end if \} }

macro pushd value
{ match first=,more, value \{ \local ..continue
   call ..continue
   db value,0
   ..continue:
   pushd equ \}
  match pushd =addr var,pushd value \{ \local ..opcode,..address
   virtual at 0
    label ..address at var
    mov eax,dword [..address]
    load ..opcode from 0
   end virtual
   if ..opcode = 0A1h
    push var
   else
    lea edx,[..address]
    push edx
   end if
   pushd equ \}
  match pushd =double [var],pushd value \{
   push dword [var+4]
   push dword [var]
   pushd equ \}
  match pushd =double =ptr var,pushd value \{
   push dword [var+4]
   push dword [var]
   pushd equ \}
  match pushd =double num,pushd value \{ \local ..high,..low
   virtual at 0
    dq num
    load ..low dword from 0
    load ..high dword from 4
   end virtual
   push ..high
   push ..low
   pushd equ \}
  match pushd,pushd \{ \local ..continue
   if value eqtype ''
    call ..continue
    db value,0
    ..continue:
   else
    push value
   end if
   pushd equ \}
  restore pushd }

allow_nesting

include 'pcount/kernel32.inc'
include 'pcount/user32.inc'
include 'pcount/gdi32.inc'
include 'pcount/advapi32.inc'
include 'pcount/comctl32.inc'
include 'pcount/comdlg32.inc'
include 'pcount/shell32.inc'
include 'pcount/wsock32.inc'

macro import lib,[functions]
{ common macro import_#lib \{ import lib,functions \} }

macro api [functions]
{ common macro all_api \{ all_api
			  api functions \} }
macro all_api {}

include 'api/kernel32.inc'
include 'api/user32.inc'
include 'api/gdi32.inc'
include 'api/advapi32.inc'
include 'api/comctl32.inc'
include 'api/comdlg32.inc'
include 'api/shell32.inc'
include 'api/wsock32.inc'

purge import,api

macro .data { section '.data' data readable writeable }

macro .code { section '.text' code readable executable }

macro .end label
{
   entry label

   section '.idata' import data readable writeable

     library kernel32,'KERNEL32.DLL',\
	     user32,'USER32.DLL',\
	     gdi32,'GDI32.DLL',\
	     advapi32,'ADVAPI32.DLL',\
	     comctl32,'COMCTL32.DLL',\
	     comdlg32,'COMDLG32.DLL',\
	     shell32,'SHELL32.DLL',\
	     wsock32,'WSOCK32.DLL'

     import_kernel32
     import_user32
     import_gdi32
     import_advapi32
     import_comctl32
     import_comdlg32
     import_shell32
     import_wsock32

     all_api
}

virtual at 0
 xchg eax,eax
 detected_16bit = $-1
end virtual

if detected_16bit
 format PE GUI 4.0
end if
