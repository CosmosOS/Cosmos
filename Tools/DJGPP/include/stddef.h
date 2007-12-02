/* Copyright (C) 1994 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_stddef_h_
#define __dj_include_stddef_h_

#ifdef __cplusplus
extern "C" {
#endif

#include <sys/djtypes.h>
  
#define NULL 0
#define offsetof(s_type, mbr) ((size_t) &((s_type *)0)->mbr)
typedef int ptrdiff_t;

__DJ_size_t
#undef __DJ_size_t
#define __DJ_size_t
__DJ_wchar_t
#undef __DJ_wchar_t
#define __DJ_wchar_t

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

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

#endif /* !__dj_include_stddef_h_ */
