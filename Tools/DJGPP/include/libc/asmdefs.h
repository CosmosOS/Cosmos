/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_libc_asmdefs_h__
#define __dj_include_libc_asmdefs_h__

	.file	__BASE_FILE__

#ifdef USE_EBX
#define PUSHL_EBX	pushl %ebx;
#define POPL_EBX	popl %ebx;
#else
#define PUSHL_EBX	
#define POPL_EBX	
#endif

#ifdef USE_ESI
#define PUSHL_ESI	pushl %esi;
#define POPL_ESI	popl %esi;
#else
#define PUSHL_ESI	
#define POPL_ESI	
#endif

#ifdef USE_EDI
#define PUSHL_EDI	pushl %edi;
#define POPL_EDI	popl %edi;
#else
#define PUSHL_EDI	
#define POPL_EDI	
#endif

#define FUNC(x)		.globl x; x:

#define ENTER		pushl %ebp; movl %esp,%ebp; PUSHL_EBX PUSHL_ESI PUSHL_EDI

#define LEAVE		L_leave: POPL_EDI POPL_ESI POPL_EBX movl %ebp,%esp; popl %ebp; ret
#define LEAVEP(x)	L_leave: x; POPL_EDI POPL_ESI POPL_EBX movl %ebp,%esp; popl %ebp; ret

#define RET		jmp L_leave

#define ARG1		8(%ebp)
#define ARG1h		10(%ebp)
#define ARG2		12(%ebp)
#define ARG2h		14(%ebp)
#define ARG3		16(%ebp)
#define ARG4		20(%ebp)
#define ARG5		24(%ebp)
#define ARG6		28(%ebp)
#define ARG7		32(%ebp)
#define ARG8		36(%ebp)

#endif /* __dj_include_libc_asmdefs_h__ */
