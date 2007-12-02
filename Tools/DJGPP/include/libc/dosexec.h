/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_libc_dosexec_h__
#define __dj_include_libc_dosexec_h__

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

extern char *__dosexec_find_on_path(const char *program, char *envp[], char *buf);
extern int __dosexec_in_system;

#define scan_ptr() \
	const char **ptr; \
	union { const char **ccpp; const char *ccp; } u; \
	for (ptr = &argv0; *ptr; ptr++); \
	u.ccp = *++ptr; \
	ptr = u.ccpp;

extern int __dosexec_command_exec(const char *program, char **argv, char **envp);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* __dj_include_libc_dosexec_h__ */
