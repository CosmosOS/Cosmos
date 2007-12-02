/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_fsext_h_
#define __dj_include_sys_fsext_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#include <stdarg.h>

typedef enum {
  __FSEXT_nop,
  __FSEXT_open,
  __FSEXT_creat,
  __FSEXT_read,
  __FSEXT_write,
  __FSEXT_ready,
  __FSEXT_close,
  __FSEXT_fcntl,
  __FSEXT_ioctl,
  __FSEXT_lseek,
  __FSEXT_link,
  __FSEXT_unlink,
  __FSEXT_dup,
  __FSEXT_dup2,
  __FSEXT_fstat,
  __FSEXT_stat
} __FSEXT_Fnumber;

/* _ready gets passed a fd and should return a mask of these,
   as if we were emulating "int ready(int fd)" */
#define __FSEXT_ready_read	1
#define __FSEXT_ready_write	2
#define __FSEXT_ready_error	4

/* The return value is nonzero if the function has overridden the
   caller's functionality. */
typedef int (__FSEXT_Function)(__FSEXT_Fnumber _function_number,
			       int *_rv, va_list _args);

int               __FSEXT_alloc_fd(__FSEXT_Function *_function);
int               __FSEXT_set_function(int _fd, __FSEXT_Function *_function);
__FSEXT_Function *__FSEXT_get_function(int _fd);
void             *__FSEXT_set_data(int _fd, void *_data);
void             *__FSEXT_get_data(int _fd);

int               __FSEXT_add_open_handler(__FSEXT_Function *_function);
int               __FSEXT_call_open_handlers(__FSEXT_Fnumber _function_number,
					     int *rv, va_list _args);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_fsext_h_ */
