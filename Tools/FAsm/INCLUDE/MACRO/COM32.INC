
; Macroinstructions for interfacing the COM (Component Object Model) classes

macro cominvk object,proc,[arg]
 { common
    if ~ arg eq
   reverse
     pushd arg
   common
    end if
    mov eax,[object]
    push eax
    mov eax,[eax]
    call [eax+object#.#proc] }

macro comcall handle,object,proc,[arg]
 { common
    if ~ arg eq
   reverse
     pushd arg
   common
    end if
    if handle eqtype eax | handle eqtype 0
     push handle
     local ..handle
     label ..handle at handle
     mov eax,[..handle]
    else
     mov eax,handle
     push eax
     mov eax,[eax]
    end if
    call [eax+object#.#proc] }

macro interface name,[proc]
 { common
    struc name \{
    match any, fields@struct \\{ fields@struct equ fields@struct,.,name, \\}
    match , fields@struct \\{ . dd ?
    virtual at 0
   forward
    .#proc dd ?
   common
    end virtual \\} \}
    virtual at 0
   forward
     name#.#proc dd ?
   common
    end virtual }

