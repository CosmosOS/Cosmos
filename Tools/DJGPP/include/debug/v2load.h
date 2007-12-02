/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_debug_v2load_h_
#define __dj_include_debug_v2load_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#include <setjmp.h>

typedef struct {
  unsigned first_addr;
  unsigned last_addr;
  } AREAS;

typedef enum {
  A_text,
  A_data,
  A_bss,
  A_arena,
  A_stack
} AREA_TYPES;

#define areas _v2load_areas
#define MAX_AREA 5

extern AREAS areas[MAX_AREA];

int v2loadimage(const char *program, const char *cmdline, jmp_buf load_state);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_debug_v2load_h_ */
