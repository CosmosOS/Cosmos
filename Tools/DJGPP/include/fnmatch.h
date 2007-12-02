/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_fnmatch_h_
#define __dj_include_fnmatch_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#define FNM_NOESCAPE	0x01
#define FNM_PATHNAME	0x02
#define FNM_PERIOD	0x04

#define FNM_NOMATCH	1
#define FNM_ERROR	2

int fnmatch(const char *_pattern, const char *_string, int _flags);

#ifndef _POSIX_SOURCE

#define FNM_NOCASE	0x08
#define FNM_CASEFOLD	FNM_NOCASE /* compatibility with GNU fnmatch */
#define FNM_IGNORECASE	FNM_NOCASE /* compatibility with Solaris */

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_fnmatch_h_ */
