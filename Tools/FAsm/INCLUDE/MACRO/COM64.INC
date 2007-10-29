
; Macroinstructions for interfacing the COM (Component Object Model) classes

macro cominvk object,proc,[arg]
 { common
    macro call dummy
    \{ mov rax,[object]
       mov rax,[rax]
       call [rax+object#.#proc] \}
    fastcall proc,[object],arg
    purge call }

macro comcall handle,object,proc,[arg]
 { common
    macro call dummy
    \{ if handle eqtype rax | handle eqtype 0
	push handle
	local ..handle
	label ..handle at handle
	mov rax,[..handle]
       else
	mov rax,handle
	mov rax,[rax]
       end if
       call [rax+object#.#proc] \}
    fastcall proc,handle,arg
    purge call }

macro interface name,[proc]
 { common
    struc name \{
    match any, fields@struct \\{ fields@struct equ fields@struct,.,name, \\}
    match , fields@struct \\{ . dq ?
    virtual at 0
   forward
    .#proc dq ?
   common
    end virtual \\} \}
    virtual at 0
   forward
     name#.#proc dq ?
   common
    end virtual }

