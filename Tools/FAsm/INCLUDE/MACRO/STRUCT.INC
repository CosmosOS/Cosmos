
; Macroinstructions for defining data structures

macro struct name
 { fields@struct equ name
   match child parent, name \{ fields@struct equ child,fields@\#parent \}
   sub@struct equ
   struc db [val] \{ \common define field@struct .,db,<val>
			     fields@struct equ fields@struct,field@struct \}
   struc dw [val] \{ \common define field@struct .,dw,<val>
			     fields@struct equ fields@struct,field@struct \}
   struc du [val] \{ \common define field@struct .,du,<val>
			     fields@struct equ fields@struct,field@struct \}
   struc dd [val] \{ \common define field@struct .,dd,<val>
			     fields@struct equ fields@struct,field@struct \}
   struc dp [val] \{ \common define field@struct .,dp,<val>
			     fields@struct equ fields@struct,field@struct \}
   struc dq [val] \{ \common define field@struct .,dq,<val>
			     fields@struct equ fields@struct,field@struct \}
   struc dt [val] \{ \common define field@struct .,dt,<val>
			     fields@struct equ fields@struct,field@struct \}
   struc rb count \{ define field@struct .,db,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   struc rw count \{ define field@struct .,dw,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   struc rd count \{ define field@struct .,dd,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   struc rp count \{ define field@struct .,dp,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   struc rq count \{ define field@struct .,dq,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   struc rt count \{ define field@struct .,dt,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   macro db [val] \{ \common \local anonymous
		     define field@struct anonymous,db,<val>
		     fields@struct equ fields@struct,field@struct \}
   macro dw [val] \{ \common \local anonymous
		     define field@struct anonymous,dw,<val>
		     fields@struct equ fields@struct,field@struct \}
   macro du [val] \{ \common \local anonymous
		     define field@struct anonymous,du,<val>
		     fields@struct equ fields@struct,field@struct \}
   macro dd [val] \{ \common \local anonymous
		     define field@struct anonymous,dd,<val>
		     fields@struct equ fields@struct,field@struct \}
   macro dp [val] \{ \common \local anonymous
		     define field@struct anonymous,dp,<val>
		     fields@struct equ fields@struct,field@struct \}
   macro dq [val] \{ \common \local anonymous
		     define field@struct anonymous,dq,<val>
		     fields@struct equ fields@struct,field@struct \}
   macro dt [val] \{ \common \local anonymous
		     define field@struct anonymous,dt,<val>
		     fields@struct equ fields@struct,field@struct \}
   macro rb count \{ \local anonymous
		     define field@struct anonymous,db,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   macro rw count \{ \local anonymous
		     define field@struct anonymous,dw,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   macro rd count \{ \local anonymous
		     define field@struct anonymous,dd,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   macro rp count \{ \local anonymous
		     define field@struct anonymous,dp,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   macro rq count \{ \local anonymous
		     define field@struct anonymous,dq,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   macro rt count \{ \local anonymous
		     define field@struct anonymous,dt,count dup (?)
		     fields@struct equ fields@struct,field@struct \}
   macro union \{ fields@struct equ fields@struct,,union,<
		  sub@struct equ union \}
   macro struct \{ fields@struct equ fields@struct,,substruct,<
		  sub@struct equ substruct \}
   virtual at 0 }

macro ends
 { match , sub@struct \{ restruc db,dw,du,dd,dp,dq,dt
			 restruc rb,rw,rd,rp,rq,rt
			 purge db,dw,du,dd,dp,dq,dt
			 purge rb,rw,rd,rp,rq,rt
			 purge union,struct
			 match name=,fields,fields@struct \\{ fields@struct equ
							      make@struct name,fields
							      define fields@\\#name fields \\}
			 end virtual \}
   match any, sub@struct \{ fields@struct equ fields@struct> \}
   restore sub@struct }

macro make@struct name,[field,type,def]
 { common
    if $
     display 'Error: definition of ',`name,' contains illegal instructions.',0Dh,0Ah
     err
    end if
    local define
    define equ name
   forward
    local sub
    match , field \{ make@substruct type,name,sub def
		     define equ define,.,sub, \}
    match any, field \{ define equ define,.#field,type,<def> \}
   common
    match fields, define \{ define@struct fields \} }

macro define@struct name,[field,type,def]
 { common
    local list
    list equ
   forward
    if ~ field eq .
     name#field type def
     sizeof.#name#field = $ - name#field
    else
     label name#.#type
     rb sizeof.#type
    end if
    local value
    match any, list \{ list equ list, \}
    list equ list <value>
   common
    sizeof.#name = $
    restruc name
    match values, list \{
    struc name value \\{
    match any, fields@struct \\\{ fields@struct equ fields@struct,.,name,<values> \\\}
    match , fields@struct \\\{ label .
   forward
     match , value \\\\{ field type def \\\\}
     match any, value \\\\{ field type value
			    if ~ field eq .
			     rb sizeof.#name#field - ($-field)
			    end if \\\\}
   common \\\} \\}
    macro name value \\{
   forward
     match , value \\\{ type def \\\}
     match any, value \\\{ \\\local ..field
			   ..field = $
			   type value
			   if ~ field eq .
			    rb sizeof.#name#field - ($-..field)
			   end if \\\}
   common \\} \} }
macro enable@substruct
 { macro make@substruct substruct,parent,name,[field,type,def]
    \{ \common
	\local define
	define equ parent,name
       \forward
	\local sub
	match , field \\{ match any, type \\\{ enable@substruct
					       make@substruct type,name,sub def
					       purge make@substruct
					       define equ define,.,sub, \\\} \\}
	match any, field \\{ define equ define,.\#field,type,<def> \\}
       \common
	match fields, define \\{ define@\#substruct fields \\} \} }

enable@substruct

macro define@union parent,name,[field,type,def]
 { common
    virtual at parent#.#name
   forward
    if ~ field eq .
     virtual at parent#.#name
      parent#field type def
      sizeof.#parent#field = $ - parent#field
     end virtual
     if sizeof.#parent#field > $ - parent#.#name
      rb sizeof.#parent#field - ($ - parent#.#name)
     end if
    else
     label name#.#type at parent#.#name
     if sizeof.#type > $ - parent#.#name
      rb sizeof.#type - ($ - parent#.#name)
     end if
    end if
   common
    sizeof.#name = $ - parent#.#name
    end virtual
    struc name [value] \{ \common
    label .\#name
    last@union equ
   forward
    match any, last@union \\{ virtual at .\#name
			       field type def
			      end virtual \\}
    match , last@union \\{ match , value \\\{ field type def \\\}
			   match any, value \\\{ field type value \\\} \\}
    last@union equ field
   common rb sizeof.#name - ($ - .\#name) \} }

macro define@substruct parent,name,[field,type,def]
 { common
    virtual at parent#.#name
   forward
    if ~ field eq .
     parent#field type def
     sizeof.#parent#field = $ - parent#field
    else
     label name#.#type
     rb sizeof.#type
    end if
   common
    sizeof.#name = $ - parent#.#name
    end virtual
    struc name value \{
    label .\#name
   forward
     match , value \\{ field type def \\}
     match any, value \\{ field type value
			  if ~ field eq .
			   rb sizeof.#parent#field - ($-field)
			  end if \\}
   common \} }
