/* Copyright (C) 1994 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_ctype_h_
#define __dj_include_ctype_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

int	isalnum(int c);
int	isalpha(int c);
int	iscntrl(int c);
int	isdigit(int c);
int	isgraph(int c);
int	islower(int c);
int	isprint(int c);
int	ispunct(int c);
int	isspace(int c);
int	isupper(int c);
int	isxdigit(int c);
int	tolower(int c);
int	toupper(int c);

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#include <inlines/ctype.ha>
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */
  
#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

int	isascii(int c);
int	toascii(int c);

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#include <inlines/ctype.hd>
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_ctype_h_ */
