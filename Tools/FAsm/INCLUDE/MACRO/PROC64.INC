
; Macroinstructions for defining and calling procedures (x64 version)

macro invoke proc,[arg]
 { common fastcall [proc],arg }

macro fastcall proc,[arg]
 { common local stackspace,argscount,counter
    if argscount and 1
     stackspace = (argscount+1)*8
    else
     stackspace = argscount*8
    end if
    counter = 0
    if stackspace
     sub rsp,stackspace
    end if
   forward
    if ~ arg eq
     counter = counter + 1
     if counter <= 4
       if arg eqtype 0
	size@arg = 8
       else if arg eqtype byte 0
	virtual at 0
	 mov [rbx],arg
	 local opcode
	 load opcode byte from 0
	 if opcode and 0F8h = 48h
	  size@arg = 8
	 else if opcode = 66h
	  size@arg = 2
	 else if opcode = 0C7h
	  size@arg = 4
	 else
	  size@arg = 1
	 end if
	end virtual
       else
	virtual at 0
	 inc arg
	 local opcode
	 load opcode byte from 0
	 if opcode = 67h
	  load opcode byte from 1
	 end if
	 if opcode and 0F8h = 48h
	  size@arg = 8
	 else if opcode = 66h
	  size@arg = 2
	 else if opcode = 0FFh
	  size@arg = 4
	 else
	  size@arg = 1
	 end if
	end virtual
       end if
       if counter = 1
	if size@arg = 8
	 if ~ arg eq rcx
	  mov rcx,arg
	 end if
	else if size@arg = 4
	 if ~ arg eq ecx
	  mov ecx,arg
	 end if
	else if size@arg = 2
	 if ~ arg eq cx
	  mov cx,arg
	 end if
	else if size@arg = 1
	 if ~ arg eq cl
	  mov cl,arg
	 end if
	end if
       else if counter = 2
	if size@arg = 8
	 if ~ arg eq rdx
	  mov rdx,arg
	 end if
	else if size@arg = 4
	 if ~ arg eq edx
	  mov edx,arg
	 end if
	else if size@arg = 2
	 if ~ arg eq dx
	  mov dx,arg
	 end if
	else if size@arg = 1
	 if ~ arg eq dl
	  mov dl,arg
	 end if
	end if
       else if counter = 3
	if size@arg = 8
	 if ~ arg eq r8
	  mov r8,arg
	 end if
	else if size@arg = 4
	 if ~ arg eq r8d
	  mov r8d,arg
	 end if
	else if size@arg = 2
	 if ~ arg eq r8w
	  mov r8w,arg
	 end if
	else if size@arg = 1
	 if ~ arg eq r8b
	  mov r8b,arg
	 end if
	end if
       else if counter = 4
	if size@arg = 8
	 if ~ arg eq r9
	  mov r9,arg
	 end if
	else if size@arg = 4
	 if ~ arg eq r9d
	  mov r9d,arg
	 end if
	else if size@arg = 2
	 if ~ arg eq r9w
	  mov r9w,arg
	 end if
	else if size@arg = 1
	 if ~ arg eq r9b
	  mov r9b,arg
	 end if
	end if
       end if
     else
       if arg eqtype [0] | arg eqtype byte [0]
	virtual at 0
	 inc arg
	 local opcode
	 load opcode byte from 0
	end virtual
	if opcode and 0F8h = 48h
	 mov rax,arg
	 mov [rsp+(counter-1)*8],rax
	else if opcode = 66h
	 mov ax,arg
	 mov [rsp+(counter-1)*8],ax
	else if opcode = 0FFh
	 mov eax,arg
	 mov [rsp+(counter-1)*8],eax
	else
	 mov al,arg
	 mov [rsp+(counter-1)*8],al
	end if
       else if arg eqtype 0
	mov qword [rsp+(counter-1)*8],arg
       else
	mov [rsp+(counter-1)*8],arg
       end if
     end if
    end if
   common
    argscount = counter
    call proc
    if stackspace
     add rsp,stackspace
    end if }

macro proc [args]
 { common
    match name params, args>
    \{ define@proc name,<params \} }

prologue@proc equ prologuedef

macro prologuedef procname,flag,parmbytes,localbytes,reglist
 { local alignment,counter
   if parmbytes | localbytes
    push rbp
    mov rbp,rsp
    if localbytes+alignment
     sub rsp,localbytes+alignment
    end if
   end if
   counter = 0
   irps reg, reglist \{ push reg
			counter = counter+1 \}
   alignment = 8*(counter and 1) }

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
			    flag = 10000b \}
   virtual at rbp+16
   match =uses reglist=,args, params \{ regs equ reglist
					params equ args \}
   match =regs =uses reglist, regs params \{ regs equ reglist
					     params equ \}
   match =regs, regs \{ regs equ \}
   match =,args, params \{ defargs@proc args \}
   match =args@proc args, args@proc params \{ defargs@proc args \}
   parmbytes = $ - (rbp+16)
   end virtual
   name # % = parmbytes/8
   all@vars equ
   current = 0
   match prologue:reglist, prologue@proc:<regs> \{ prologue name,flag,parmbytes,localbytes,reglist \}
   macro locals
   \{ virtual at rbp-localbytes+current
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
      current = $-(rbp-localbytes)
      end virtual \}
   macro ret operand
   \{ match any, operand \\{ retn operand \\}
      match , operand \\{ match epilogue:reglist, epilogue@proc:<regs>
			  \\\{ epilogue name,flag,parmbytes,localbytes,reglist \\\} \\} \}
   macro finish@proc \{ localbytes = (current+15) and (not 15)
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
	   dq ?,?
	 else if tbyte eq type
	   dq ?,?
	 else
	   dq ?
	 end if \}
     match =current@arg,current@arg
      \{ current@arg equ arg
	 arg equ ..arg
	 ..arg dq ? \}
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
 { virtual
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
     else if size@initlocal - position@initlocal < 8
      current@initlocal = 4
      load dword@initlocal dword from name+position@initlocal
     else
      load qword@initlocal dword from name+position@initlocal
      if ( qword@initlocal > 0 & qword@initlocal < 80000000h ) | ( qword@initlocal < 0 & qword@initlocal >= -80000000h )
       current@initlocal = 8
      else
       current@initlocal = 4
       dword@initlocal = qword@initlocal and 0FFFFFFFFh
      end if
     end if
    end virtual
    if current@initlocal = 1
     mov byte [name+position@initlocal],byte@initlocal
    else if current@initlocal = 2
     mov word [name+position@initlocal],word@initlocal
    else if current@initlocal = 4
     mov dword [name+position@initlocal],dword@initlocal
    else
     mov qword [name+position@initlocal],qword@initlocal
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
				 