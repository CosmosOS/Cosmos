/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_wait_h_
#define __dj_include_sys_wait_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#include <sys/djtypes.h>

#define WEXITSTATUS(stat_val)	(  (stat_val) & 0x000ff)
#define WIFEXITED(stat_val)	(!((stat_val) & 0x3ff00))
#define WIFSIGNALED(stat_val)	( ((stat_val) & 0x3ff00))
#define WIFSTOPPED(stat_val)	0
#define WNOHANG			1
#define WSTOPSIG(stat_val)	0
#define WTERMSIG(stat_val)	( ((stat_val) >> 8 ) & 0x3ff)
#define WUNTRACED		0

__DJ_pid_t
#undef __DJ_pid_t
#define __DJ_pid_t

pid_t wait(int *stat_loc);
pid_t waitpid(pid_t pid, int *stat_loc, int options);

#ifndef _POSIX_SOURCE

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_wait_h_ */
