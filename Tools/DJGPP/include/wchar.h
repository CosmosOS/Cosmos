/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_wchar_h_
#define __dj_include_wchar_h_

/* Bare bones header to satisfy SGI STL's basic_string<> */

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#include <stddef.h>

__DJ_wint_t
#undef __DJ_wint_t
#define __DJ_wint_t

#ifndef WEOF
#define WEOF ((wint_t)(-1))
#endif

typedef struct
{
  int shift_state;
} mbstate_t;

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_wchar_h_ */

