/* Copyright (C) 1996 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_libc_environ_h_
#define __dj_include_libc_environ_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* This starts at 1 and gets incremented every time some
   variable in the environment is added, deleted, or changes
   its value.  It is meant to be used by functions that depend
   on values of environment variables, but don't want to call
   `getenv' unnecessarily (example: `__use_lfn').

   To use this feature, define a static variable that is
   initialized to zero, and compare its value with the value of
   `__environ_changed': if they differ, you should call `getenv'
   (and record the last value of `__environ_changed' you've seen).  */
extern unsigned __environ_changed;

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_environ_h_ */
