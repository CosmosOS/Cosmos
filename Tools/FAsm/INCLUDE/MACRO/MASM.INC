
; Simulate MASM's syntax

struc struct
 { struct .
   name@struct equ . }

struc ends
 { match =.,name@struct \{ ends \} }

struc proc [params]
 { common define@proc .,<params>
   name@proc equ . }

struc endp
 { match =.,name@proc \{ endp \} }

macro option setting
 { match =prologue:macro, setting \{ prologue@proc equ macro \}
   match =epilogue:macro, setting \{ epilogue@proc equ macro \} }

macro none procname,flag,parmbytes,localbytes,reglist { }

macro assume params
 {
   match reg:struct, params
   \{
      match assumed, reg\#@assumed \\{ irp name, assumed \\\{ restore name \\\} \\}
      macro label . \\{ \\}
      struc db [val] \\{ \common def@assumed reg,.,db val \\}
      struc dw [val] \\{ \common def@assumed reg,.,dw val \\}
      struc dp [val] \\{ \common def@assumed reg,.,dp val \\}
      struc dd [val] \\{ \common def@assumed reg,.,dd val \\}
      struc dt [val] \\{ \common def@assumed reg,.,dt val \\}
      struc dq [val] \\{ \common def@assumed reg,.,dq val \\}
      struc rb cnt \\{ def@assumed reg,.,rb cnt \\}
      struc rw cnt \\{ def@assumed reg,.,rw cnt \\}
      struc rp cnt \\{ def@assumed reg,.,rp cnt \\}
      struc rd cnt \\{ def@assumed reg,.,rd cnt \\}
      struc rt cnt \\{ def@assumed reg,.,rt cnt \\}
      struc rq cnt \\{ def@assumed reg,.,rq cnt \\}
      reg#@assumed equ
      virtual at reg
       reg struct
      end virtual
      purge label
      restruc db,dw,dp,dd,dt,dq
      restruc rb,rw,rp,rd,rt,rq \} }

macro def@assumed reg,name,def
 { match vars, reg#@assumed \{ reg#@assumed equ reg#@assumed, \}
   reg#@assumed equ reg#@assumed name
   local ..label
   name equ ..label
   ..label def }

struc none { label . }
