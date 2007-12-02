/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_nearptr_h_
#define __dj_include_sys_nearptr_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* Functions to enable "near" pointer access to DOS memory under DPMI
   CW Sandmann 7-95  NO WARRANTY: WARNING, since these functions disable
   memory protection, they MAY DESTROY EVERYTHING ON YOUR COMPUTER! */

int __djgpp_nearptr_enable(void);	/* Returns 0 if feature not avail */
void __djgpp_nearptr_disable(void);	/* Enables protection */
extern int __djgpp_selector_limit;	/* Limit on CS and on DS if prot */
extern int __djgpp_base_address;	/* Used in calculation below */

#define __djgpp_conventional_base (-__djgpp_base_address)

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_nearptr_h_ */
