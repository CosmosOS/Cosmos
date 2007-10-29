
; Macroinstructions for HLL-style conditional operations

macro .if [arg]
{
  common
  __IF equ
  local ..endif
  __ENDIF equ ..endif
  local ..else
  __ELSE equ ..else
  JNCOND __ELSE,arg
}

macro .else
{
  jmp __ENDIF
  __ELSE:
  restore __IF
  __IF equ ,
}

macro .elseif [arg]
{
  common
  jmp __ENDIF
  __ELSE:
  restore __ELSE
  local ..else
  __ELSE equ ..else
  JNCOND __ELSE,arg
}

macro .endif
{
  if __IF eq
   __ELSE:
  end if
  __ENDIF:
  restore __ELSE
  restore __ENDIF
  restore __IF
}

macro .while [arg]
{
  common
  local ..while
  __WHILE equ ..while
  local ..endw
  __ENDW equ ..endw
  __WHILE:
  JNCOND __ENDW,arg
}

macro .endw
{
  jmp __WHILE
  __ENDW:
  restore __ENDW
  restore __WHILE
}

macro .repeat
{
  local ..repeat
  __REPEAT equ ..repeat
  __REPEAT:
}

macro .until [arg]
{
  common
  JNCOND __REPEAT,arg
  restore __REPEAT
}

jnne equ je
jnna equ ja
jnnb equ jb
jnng equ jg
jnnl equ jl
jnnae equ jae
jnnbe equ jbe
jnnge equ jge
jnnle equ jle

macro JNCOND label,v1,c,v2
{
 match any,c
 \{
   cmp v1,v2
   jn\#c label
 \}
 match ,c
 \{
   PARSECOND parsed@cond,v1
   match cond,parsed@cond \\{ JNCONDEXPR label,cond \\}
 \}
}

gt equ >
lt equ <

macro PARSECOND parsed,cond
{
 define parsed
 define neg@cond
 define status@cond
 define nest@cond
 irps symb,cond
 \{
   define symb@cond symb
   match >,symb
   \\{
      define symb@cond gt
   \\}
   match <,symb
   \\{
      define symb@cond lt
   \\}
   current@cond equ status@cond
   match ,current@cond
   \\{
      match ~,symb
      \\\{
	  neg@cond equ neg@cond ~
	  match ~~,neg@cond
	  \\\\{
	       define neg@cond
	  \\\\}
	  define symb@cond
      \\\}
      match (,symb
      \\\{
	  parsed equ parsed neg@cond,<
	  define nest@cond +
	  define symb@cond
      \\\}
      match any,symb@cond
      \\\{
	  parsed equ parsed neg@cond,symb@cond
	  define status@cond +
      \\\}
   \\}
   match status,current@cond
   \\{
      match &,symb
      \\\{
	  parsed equ parsed,&,
	  define status@cond
	  define symb@cond
      \\\}
      match |,symb
      \\\{
	  parsed equ parsed,|,
	  define status@cond
	  define symb@cond
      \\\}
      match (,symb
      \\\{
	  define nest@cond (
      \\\}
      match ),symb
      \\\{
	  match +,nest@cond
	  \\\\{
	       parsed equ parsed>
	       define symb@cond
	  \\\\}
	  restore nest@cond
      \\\}
      match any,symb@cond
      \\\{
	  parsed equ parsed symb@cond
      \\\}
   \\}
 \}
}

macro define_JNCONDEXPR
{
 macro JNCONDEXPR elabel,[mod,cond,op]
 \{
  \common
   \local ..t,..f
   define t@cond ..t
   define f@cond ..f
  \forward
   match ,op
   \\{
      match ,mod \\\{ JNCONDEL elabel,<cond> \\\}
      match ~,mod \\\{ JCONDEL elabel,<cond> \\\}
   \\}
   match &:flabel:tlabel, op:f@cond:t@cond
   \\{
      match ,mod \\\{ JNCONDEL flabel,<cond> \\\}
      match ~,mod \\\{ JCONDEL flabel,<cond> \\\}
      tlabel:
      \\local ..tnew
      restore t@cond
      define t@cond ..tnew
   \\}
   match |:flabel:tlabel, op:f@cond:t@cond
   \\{
      match ,mod \\\{ JCONDEL tlabel,<cond> \\\}
      match ~,mod \\\{ JNCONDEL tlabel,<cond> \\\}
      flabel:
      \\local ..fnew
      restore f@cond
      define f@cond ..fnew
   \\}
  \common
   label f@cond at elabel
   t@cond:
   restore t@cond
   restore f@cond
 \}
}

macro define_JCONDEXPR
{
 macro JCONDEXPR elabel,[mod,cond,op]
 \{
  \common
   \local ..t,..f
   define t@cond ..t
   define f@cond ..f
  \forward
   match ,op
   \\{
      match ,mod \\\{ JCONDEL elabel,<cond> \\\}
      match ~,mod \\\{ JNCONDEL elabel,<cond> \\\}
   \\}
   match |:flabel:tlabel, op:f@cond:t@cond
   \\{
      match ,mod \\\{ JCONDEL flabel,<cond> \\\}
      match ~,mod \\\{ JNCONDEL flabel,<cond> \\\}
      tlabel:
      \\local ..tnew
      restore t@cond
      define t@cond ..tnew
   \\}
   match &:flabel:tlabel, op:f@cond:t@cond
   \\{
      match ,mod \\\{ JNCONDEL tlabel,<cond> \\\}
      match ~,mod \\\{ JCONDEL tlabel,<cond> \\\}
      flabel:
      \\local ..fnew
      restore f@cond
      define f@cond ..fnew
   \\}
  \common
   label f@cond at elabel
   t@cond:
   restore t@cond
   restore f@cond
 \}
}

macro define_JNCONDEL
{
 macro JNCONDEL label,cond
 \{
   \local COND
   match car=,cdr,:cond
   \\{
      define_JNCONDEXPR
      define_JCONDEXPR
      define_JCONDEL
      define_JNCONDEL
      JNCONDEXPR label,cond
      purge JNCONDEXPR,JCONDEXPR,JCONDEL,JNCONDEL
      define COND
   \\}
   match c,cond ; replace gt and lt
   \\{
      match =COND =signed v1>==v2, COND c
      \\\{
	  cmp v1,v2
	  jl label
	  define COND
      \\\}
      match =COND =signed v1<==v2, COND c
      \\\{
	  cmp v1,v2
	  jg label
	  define COND
      \\\}
      match =COND v1>==v2, COND c
      \\\{
	  cmp v1,v2
	  jb label
	  define COND
      \\\}
      match =COND v1<==v2, COND c
      \\\{
	  cmp v1,v2
	  ja label
	  define COND
      \\\}
      match =COND v1==v2, COND c
      \\\{
	  cmp v1,v2
	  jne label
	  define COND
      \\\}
      match =COND v1<>v2, COND c
      \\\{
	  cmp v1,v2
	  je label
	  define COND
      \\\}
      match =COND =signed v1>v2, COND c
      \\\{
	  cmp v1,v2
	  jle label
	  define COND
      \\\}
      match =COND =signed v1<v2, COND c
      \\\{
	  cmp v1,v2
	  jge label
	  define COND
      \\\}
      match =COND v1>v2, COND c
      \\\{
	  cmp v1,v2
	  jbe label
	  define COND
      \\\}
      match =COND v1<v2, COND c
      \\\{
	  cmp v1,v2
	  jae label
	  define COND
      \\\}
      match =COND =ZERO?, COND c
      \\\{
	  jnz label
	  define COND
      \\\}
      match =COND =CARRY?, COND c
      \\\{
	  jnc label
	  define COND
      \\\}
      match =COND =OVERFLOW?, COND c
      \\\{
	  jno label
	  define COND
      \\\}
      match =COND =SIGN?, COND c
      \\\{
	  jns label
	  define COND
      \\\}
      match =COND =PARITY?, COND c
      \\\{
	  jnp label
	  define COND
      \\\}
      match =COND v, COND c
      \\\{
	  if v eqtype 0
	   if ~ v
	    jmp label
	   end if
	  else if v eqtype eax
	   test v,v
	   jz label
	  else
	   cmp v,0
	   je label
	  end if
      \\\}
   \\}
 \}
}

macro define_JCONDEL
{
 macro JCONDEL label,cond
 \{
   \local COND
   match car=,cdr,:cond
   \\{
      define_JNCONDEXPR
      define_JCONDEXPR
      define_JCONDEL
      define_JNCONDEL
      JCONDEXPR label,cond
      purge JNCONDEXPR,JCONDEXPR,JCONDEL,JNCONDEL
      define COND
   \\}
   match c,cond ; replace gt and lt
   \\{
      match =COND =signed v1>==v2, COND c
      \\\{
	  cmp v1,v2
	  jge label
	  define COND
      \\\}
      match =COND =signed v1<==v2, COND c
      \\\{
	  cmp v1,v2
	  jle label
	  define COND
      \\\}
      match =COND v1>==v2, COND c
      \\\{
	  cmp v1,v2
	  jae label
	  define COND
      \\\}
      match =COND v1<==v2, COND c
      \\\{
	  cmp v1,v2
	  jbe label
	  define COND
      \\\}
      match =COND v1==v2, COND c
      \\\{
	  cmp v1,v2
	  je label
	  define COND
      \\\}
      match =COND v1<>v2, COND c
      \\\{
	  cmp v1,v2
	  jne label
	  define COND
      \\\}
      match =COND =signed v1>v2, COND c
      \\\{
	  cmp v1,v2
	  jg label
	  define COND
      \\\}
      match =COND =signed v1<v2, COND c
      \\\{
	  cmp v1,v2
	  jl label
	  define COND
      \\\}
      match =COND v1>v2, COND c
      \\\{
	  cmp v1,v2
	  ja label
	  define COND
      \\\}
      match =COND v1<v2, COND c
      \\\{
	  cmp v1,v2
	  jb label
	  define COND
      \\\}
      match =COND =ZERO?, COND c
      \\\{
	  jz label
	  define COND
      \\\}
      match =COND =CARRY?, COND c
      \\\{
	  jc label
	  define COND
      \\\}
      match =COND =OVERFLOW?, COND c
      \\\{
	  jo label
	  define COND
      \\\}
      match =COND =SIGN?, COND c
      \\\{
	  js label
	  define COND
      \\\}
      match =COND =PARITY?, COND c
      \\\{
	  jp label
	  define COND
      \\\}
      match =COND v, COND c
      \\\{
	  if v eqtype 0
	   if v
	    jmp label
	   end if
	  else if v eqtype eax
	   test v,v
	   jnz label
	  else
	   cmp v,0
	   jne label
	  end if
      \\\}
   \\}
 \}
}

define_JNCONDEXPR
define_JCONDEXPR
define_JNCONDEL
define_JCONDEL
