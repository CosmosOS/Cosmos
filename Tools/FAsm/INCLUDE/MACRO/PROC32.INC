
; Macroinstructions for defining and calling procedures

macro stdcall proc,[arg]		; directly call STDCALL procedure
 { common
    if ~ arg eq
   reverse
    pushd arg
   common
    end if
    call proc }

macro invoke proc,[arg] 		; indirectly call STDCALL procedure
 { common
    if ~ arg eq
   reverse
     pushd arg
   common
    end if
    call [proc] }

macro ccall proc,[arg]			; directly call CDECL procedure
 { common
    size@ccall = 0
    if ~ arg eq
   reverse
    pushd arg
    size@ccall = size@ccall+4
   common
    end if
    call proc
    if size@ccall
    add esp,size@ccall
    end if }

macro cinvoke proc,[arg]		; indirectly call CDECL procedure
 { common
    size@ccall = 0
    if ~ arg eq
   reverse
    pushd arg
    size@ccall = size@ccall+4
   common
    end if
    call [proc]
    if size@ccall
    add esp,size@ccall
    end if }

macro proc [args]			; define procedure
 { common
    match name params, args>
    \{ define@proc name,<params \} }

prologue@proc equ prologuedef

macro prologuedef procname,flag,parmbytes,localbytes,reglist
 { if parmbytes | localbytes
    push ebp
    mov ebp,esp
    if localbytes
     sub esp,localbytes
    end if
   end if
   irps reg, reglist \{ push reg \} }

epilogue@proc equ epiloguedef

macro epiloguedef procname,flag,parmbytes,localbytes,reglist
 { irps reg, reglist \{ reverse pop reg \}
   if parmbytes | localbytes
    leave
   end if
   if flag and 10000b
    retn
   else
    retn parmbytes
   end if }

macro define@proc name,statement
 { local params,flag,regs,parmbytes,localbytes,current
   if used name
   name:
   match =stdcall args, statement \{ params equ args
				     flag = 11b \}
   match =stdcall, statement \{ params equ
				flag = 11b \}
   match =c args, statement \{ params equ args
			       flag = 10001b \}
   match =c, statement \{ params equ
			  flag = 10001b \}
   match =params, params \{ params equ statement
			    flag = 0 \}
   virtual at ebp+8
   match =uses reglist=,args, params \{ regs equ reglist
					params equ args \}
   match =regs =uses reglist, regs params \{ regs equ reglist
					     params equ \}
   match =regs, regs \{ regs equ \}
   match =,args, params \{ defargs@proc args \}
   match =args@proc args, args@proc params \{ defargs@proc args \}
   parmbytes = $ - (ebp+8)
   end virtual
   name # % = parmbytes/4
   all@vars equ
   current = 0
   match prologue:reglist, prologue@proc:<regs> \{ prologue name,flag,parmbytes,localbytes,reglist \}
   macro locals
   \{ virtual at ebp-localbytes+current
      macro label def \\{ match . type,def> \\\{ deflocal@proc .,label,<type \\\} \\}
      struc db [val] \\{ \common deflocal@proc .,db,val \\}
      struc du [val] \\{ \common deflocal@proc .,du,val \\}
      struc dw [val] \\{ \common deflocal@proc .,dw,val \\}
      struc dp [val] \\{ \common deflocal@proc .,dp,val \\}
      struc dd [val] \\{ \common deflocal@proc .,dd,val \\}
      struc dt [val] \\{ \common deflocal@proc .,dt,val \\}
      struc dq [val] \\{ \common deflocal@proc .,dq,val \\}
      struc rb cnt \\{ deflocal@proc .,rb cnt, \\}
      struc rw cnt \\{ deflocal@proc .,rw cnt, \\}
      struc rp cnt \\{ deflocal@proc .,rp cnt, \\}
      struc rd cnt \\{ deflocal@proc .,rd cnt, \\}
      struc rt cnt \\{ deflocal@proc .,rt cnt, \\}
      struc rq cnt \\{ deflocal@proc .,rq cnt, \\} \}
   macro endl
   \{ purge label
      restruc db,du,dw,dp,dd,dt,dq
      restruc rb,rw,rp,rd,rt,rq
      current = $-(ebp-localbytes)
      end virtual \}
   macro ret operand
   \{ match any, operand \\{ retn operand \\}
      match , operand \\{ match epilogue:reglist, epilogue@proc:<regs>
			  \\\{ epilogue name,flag,parmbytes,localbytes,reglist \\\} \\} \}
   macro finish@proc \{ localbytes = (((current-1) shr 2)+1) shl 2
			end if \} }

macro defargs@proc [arg]
 { common
    if ~ arg eq
   forward
     local ..arg,current@arg
     match argname:type, arg
      \{ current@arg equ argname
	 label ..arg type
	 argname equ ..arg
	 if dqword eq type
	   dd ?,?,?,?
	 else if tbyte eq type
	   dd ?,?,?
	 else if qword eq type | pword eq type
	   dd ?,?
	 else
	   dd ?
	 end if \}
     match =current@arg,current@arg
      \{ current@arg equ arg
	 arg equ ..arg
	 ..arg dd ? \}
   common
     args@proc equ current@arg
   forward
     restore current@arg
   common
    end if }

macro deflocal@proc name,def,[val]
 { common
    match vars, all@vars \{ all@vars equ all@vars, \}
    all@vars equ all@vars name
   forward
    local ..var,..tmp
    match =label,def \{ ..tmp equ \}
    match tmp,..tmp \{ ..var def val \}
    match ,..tmp \{ label ..var val \}
    match =?, val \{ ..tmp equ \}
    match any =dup (=?), val \{ ..tmp equ \}
    match tmp : value, ..tmp : val
     \{ tmp: end virtual
	initlocal@proc ..var,def value
	virtual at tmp\}
   common
    match first rest, ..var, \{ name equ first \} }

macro initlocal@proc name,def
 { virtual at name
    def
    size@initlocal = $ - name
   end virtual
   position@initlocal = 0
   while size@initlocal > position@initlocal
    virtual at name
     def
     if size@initlocal - position@initlocal < 2
      current@initlocal = 1
      load byte@initlocal byte from name+position@initlocal
     else if size@initlocal - position@initlocal < 4
      current@initlocal = 2
      load word@initlocal word from name+position@initlocal
     else
      current@initlocal = 4
      load dword@initlocal dword from name+position@initlocal
     end if
    end virtual
    if current@initlocal = 1
     mov byte [name+position@initlocal],byte@initlocal
    else if current@initlocal = 2
     mov word [name+position@initlocal],word@initlocal
    else
     mov dword [name+position@initlocal],dword@initlocal
    end if
    position@initlocal = position@initlocal + current@initlocal
   end while }

macro endp
 { purge ret,locals,endl
   finish@proc
   purge finish@proc
   restore regs@proc
   match all,args@proc \{ restore all \}
   restore args@proc
   match all,all@vars \{ restore all \} }

macro local [var]
 { common
    locals
   forward done@local equ
    match varname[count]:vartype, var
    \{ match =BYTE, vartype \\{ varname rb count
				restore done@local \\}
       match =WORD, vartype \\{ varname rw count
				restore done@local \\}
       match =DWORD, vartype \\{ varname rd count
				 restore done@local \\}
       match =PWORD, vartype \\{ varname rp count
				 restore done@local \\}
       match =QWORD, vartype \\{ varname rq count
				 restore done@local \\}
       match =TBYTE, vartype \\{ varname rt count
				 restore done@local \\}
       match =DQWORD, vartype \\{ label varname dqword
				  rq count+count
				  restore done@local \\}
       match , done@local \\{ virtual
			       varname vartype
			      end virtual
			      rb count*sizeof.\#vartype
			      restore done@local \\} \}
    match :varname:vartype, done@local:var
    \{ match =BYTE, vartype \\{ varname db ?
				restore done@local \\}
       match =WORD, vartype \\{ varname dw ?
				restore done@local \\}
       match =DWORD, vartype \\{ varname dd ?
				 restore done@local \\}
       match =PWORD, vartype \\{ varname dp ?
				 restore done@local \\}
       match =QWORD, vartype \\{ varname dq ?
				 restore done@local \\}
       match =TBYTE, vartype \\{ varname dt ?
				 restore done@local \\}
       match =DQWORD, vartype \\{ label varname dqword
				  dq ?,?
				  restore done@local \\}
       match , done@local \\{ varname vartype
			      restore done@local \\} \}
    match ,done@local
    \{ var
       restore done@local \}
   common
    endl }
