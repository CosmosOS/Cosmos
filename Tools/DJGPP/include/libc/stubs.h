/* Copyright (C) 1997 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_libc_stubs_h__
#define __dj_include_libc_stubs_h__

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* POSIX functions (for when compiling an ANSI function) */

#define access __access
#define chdir __chdir
#define close __close
#define dup __dup
#define dup2 __dup2
#define fnmatch __fnmatch
#define getcwd __getcwd
#define glob __glob
#define isatty __isatty
#define lseek __lseek
#define mkdir __mkdir
#define open __open
#define read __read
#define tzset __tzset
#define write __write

/* DJGPP functions (for compiling POSIX or ANSI functions) */

#define crlf2nl __crlf2nl
#define dosmemget __dosmemget
#define dosmemput __dosmemput
#define filelength __filelength
#define findfirst __findfirst
#define findnext __findnext
#define fsync __fsync
#define getdisk __getdisk
#define getdtablesize __getdtablesize
#define getitimer __getitimer
#define gettimeofday __gettimeofday
#define modfl __modfl
#define movedata __movedata
#define pow10 __pow10
#define pow2 __pow2
#define putenv __putenv
#define sbrk __sbrk
#define setitimer __setitimer
#define setmode __setmode
#define spawnve __spawnve
#define spawnvpe __spawnvpe
#define stricmp __stricmp
#define sync __sync

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* __dj_include_libc_stubs_h__ */
