/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_debug_syms_h_
#define __dj_include_debug_syms_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* This is file SYMS.H */
/*
** Copyright (C) 1993 DJ Delorie, 24 Kirsten Ave, Rochester NH 03867-2954
**
** This file is distributed under the terms listed in the document
** "copying.dj", available from DJ Delorie at the address above.
** A copy of "copying.dj" should accompany this file; if not, a copy
** should be available from where this file was obtained.  This file
** may not be distributed without a verbatim copy of "copying.dj".
**
** This file is distributed WITHOUT ANY WARRANTY; without even the implied
** warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*/

#ifndef _SYMS_H_
#define _SYMS_H_

void syms_init(char *fname);
void syms_list(int byval);
unsigned long syms_name2val(const char *name);
char *syms_val2name(unsigned long val, unsigned long *delta);
char *syms_val2line(unsigned long val, int *lineret, int exact);
char *syms_module(int no);
unsigned long syms_line2val(char *filename, int lnum);
void syms_listwild(char *pattern, 
  void (*handler)(unsigned long addr, char type_c, char *name, char *name2, int lnum));

extern int undefined_symbol;
extern int syms_printwhy;

#define N_INDR  0x0a
#define	N_SETA	0x14		/* Absolute set element symbol */
#define	N_SETT	0x16		/* Text set element symbol */
#define	N_SETD	0x18		/* Data set element symbol */
#define	N_SETB	0x1A		/* Bss set element symbol */
#define N_SETV	0x1C		/* Pointer to set vector in data area.  */

#endif

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_debug_syms_h_ */
