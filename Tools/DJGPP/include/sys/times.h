/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_times_h_
#define __dj_include_sys_times_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#include <sys/djtypes.h>
__DJ_clock_t
#undef __DJ_clock_t
#define __DJ_clock_t

struct tms {
  clock_t tms_utime;
  clock_t tms_cstime;
  clock_t tms_cutime;
  clock_t tms_stime;
};

clock_t	times(struct tms *buffer);

#ifndef _POSIX_SOURCE

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_times_h_ */
