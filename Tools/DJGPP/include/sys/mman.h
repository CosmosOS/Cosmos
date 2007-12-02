/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_mman_h_
#define __dj_include_sys_mman_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* protections are chosen from these bits, or-ed together */
#define PROT_NONE	0		/* no access to these pages */
#define PROT_READ	0x1		/* pages can be read */
#define PROT_WRITE	0x2		/* pages can be written */
#define PROT_EXEC	0		/* pages can be executed - not used */

extern int mprotect(void *addr, size_t len, int prot);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_mman_h_ */
