/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_limits_h_
#define __dj_include_limits_h_

#ifdef __cplusplus
extern "C" {
#endif

#define CHAR_BIT 8
#define CHAR_MAX 127
#define CHAR_MIN (-128)
#define INT_MAX 2147483647
#define INT_MIN (-2147483647-1)
#define LONG_MAX 2147483647L
#define LONG_MIN (-2147483647L-1L)
#define MB_LEN_MAX 5
#define SCHAR_MAX 127
#define SCHAR_MIN (-128)
#define SHRT_MAX 32767
#define SHRT_MIN (-32768)
#define UCHAR_MAX 255
#define UINT_MAX 4294967295U
#define ULONG_MAX 4294967295UL
#define USHRT_MAX 65535
#define WCHAR_MIN 0
#define WCHAR_MAX 127
#define WINT_MIN 0
#define WINT_MAX 32767

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#define _POSIX_ARG_MAX		16384	/* but only for exec's to other djgpp programs */
#define _POSIX_CHILD_MAX	7	/* limited by memory; 7 for 386MAX */
#define _POSIX_LINK_MAX		1	/* POSIX says 8, but DOS says 1 */
#define _POSIX_MAX_CANON	126	/* POSIX says 255, but DOS says 126 */
#define _POSIX_MAX_INPUT	126	/* POSIX says 255, but DOS says 126 */
#define _POSIX_NAME_MAX		12	/* 8.3 */
#define _POSIX_NGROUPS_MAX	0
#define _POSIX_OPEN_MAX		20	/* can be bigger in DOS, but defaults to 20 */
#define _POSIX_PATH_MAX		256	/* 80 for canonical paths */
#define _POSIX_PIPE_BUF		512	/* but there aren't any pipes */
#define _POSIX_SSIZE_MAX	2147483647
#define _POSIX_STREAM_MAX	20	/* can be bigger in DOS */
#define _POSIX_TZNAME_MAX	5

#define NGROUPS_MAX		0

/* #define ARG_MAX			4096 -- depends on tb size; use sysconf */
#define CHILD_MAX		6
/* #define OPEN_MAX		20 - DOS can change this */
/* #define STREAM_MAX		20 - DOS can change this */
#define TZNAME_MAX		3

#define LINK_MAX		1
#define MAX_CANON		126
#define MAX_INPUT		126
#define NAME_MAX		12	/* 8.3 */
#define PATH_MAX		512	/* for future expansion */
#define PIPE_BUF		512	/* but there aren't any pipes */

#define SSIZE_MAX		2147483647

#ifndef _POSIX_SOURCE

/* constants used in Solaris */
#define LLONG_MIN       (-9223372036854775807LL-1LL)
#define LLONG_MAX       9223372036854775807LL
#define ULLONG_MAX      18446744073709551615ULL
/* gnuc ones */
#define LONG_LONG_MIN	LLONG_MIN
#define LONG_LONG_MAX	LLONG_MAX
#define ULONG_LONG_MAX	ULLONG_MAX

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_limits_h_ */
