/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1996 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_config_h_
#define __dj_include_sys_config_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* config.h for DJGPP.

   This is usually generated automatically from config.h.in by the
   configure script.  However, it is very hard to run that script under
   MS-DOS, because of its extensive use of Unix shell features.  This
   header file is provided so you can skip the autoconfigure step
   altogether and go directly to the compilation step (after copying
   Makefile.in to Makefile and setting the defaults there).

   There are several parts in this header file, which closely follow the
   GNU Autoconf procedures.

   The first part checks for things which depend on the specific programs
   from your programming environment which you use to compile a package.

   The second part mentions all the header files in the include hierarchy,
   even those which every C installation must have.  The only headers
   files which are omitted are those which are specific to the PC
   architecture or to DJGPP, because no GNU package should ever look for
   those.  (Some header files which DJGPP includes only for compatibility,
   and which could cause a conflict with the mainstream header file, are
   also excluded.)

   The third part mentions all the library functions which aren't included
   in every C library.  Obviously, it isn't practical to mention every
   library function here, so this part has somewhat ad-hoc nature in that
   the macros which should go there were assembled by actually porting
   some GNU packages.

   The fourth part defines macros which are related to the header files,
   like definitions of some structures and specific member fields in some
   structures.

   The fifth part deals with some typedefs which aren't standardized
   enough between different systems, or might be missing from your header
   files.

   The sixth part defines some macros necessary to deal with differences
   between compiler and architectural characteristics of various systems.

   In the seventh part, some system services which might be required by
   some packages, are mentioned.

   The last part includes all kinds if miscellaneous macros required by
   some GNU packages to be successfully compiled under DJGPP.

              Prepared by Eli Zaretskii <eliz@is.elta.co.il>
              with help from Morten Welinder <terra@diku.dk>
           and using some information from GNU Autoconf package.
*/

/* ---------------------------------------------------------------------
                       Program-related stuff.
   --------------------------------------------------------------------- */ 

/* Define if `yytext' is a `char *' instead of a `char []'.  This is
   true if you use Flex.  */
#undef  YYTEXT_POINTER
#define YYTEXT_POINTER  1

/* ---------------------------------------------------------------------
                               Header files.
   --------------------------------------------------------------------- */

/* Define if you have the ANSI C header files.  */
#undef  STDC_HEADERS
#define STDC_HEADERS    1

/* Define if you have the <ar.h> header file.  */
#undef  HAVE_AR_H
#define HAVE_AR_H   1

/* Define if you have the <assert.h> header file.  */
#undef  HAVE_ASSERT_H
#define HAVE_ASSERT_H   1

/* Define if you have the <coff.h> header file.  */
#undef  HAVE_COFF_H
#define HAVE_COFF_H 1

/* Define if you have the <ctype.h> header file.  */
#undef  HAVE_CTYPE_H
#define HAVE_CTYPE_H    1

/* Define if you have the <dirent.h> header file.  */
#undef  DIRENT
#define DIRENT          1

#undef  HAVE_DIRENT_H
#define HAVE_DIRENT_H   1

/* Define if you have the <errno.h> header file.  */
#undef  HAVE_ERRNO_H
#define HAVE_ERRNO_H    1

/* Define if you have the <fcntl.h> header file.  */
#undef  HAVE_FCNTL_H
#define HAVE_FCNTL_H    1

/* Define if you have the <float.h> header file.  */
#undef  HAVE_FLOAT_H
#define HAVE_FLOAT_H    1

/* Define if you have the <fnmatch.h> header file.  */
#undef  HAVE_FNMATCH_H
#define HAVE_FNMATCH_H  1

/* Define if you have the <ftw.h> header file.  */
#undef  HAVE_FTW_H
#define HAVE_FTW_H  1

/* Define if you have the <glob.h> header file.  */
#undef  HAVE_GLOB_H
#define HAVE_GLOB_H 1

/* Define if you have the <grp.h> header file.  */
#undef  HAVE_GRP_H
#define HAVE_GRP_H  1

/* Define if you have the <io.h> header file.  */
#undef  HAVE_IO_H
#define HAVE_IO_H   1

/* Define if you have the <limits.h> header file.  */
#undef  HAVE_LIMITS_H
#define HAVE_LIMITS_H   1

/* Define if you have the <locale.h> header file.  */
#undef  HAVE_LOCALE_H
#define HAVE_LOCALE_H   1

/* Define if you have the <math.h> header file.  */
#undef  HAVE_MATH_H
#define HAVE_MATH_H     1

/* Define if you have the <mntent.h> header file.  */
#undef  HAVE_MNTENT_H
#define HAVE_MNTENT_H   1

/* Define if you have the <pwd.h> header file.  */
#undef  HAVE_PWD_H
#define HAVE_PWD_H      1

/* Define if you have the <search.h> header file.  */
#undef  HAVE_SEARCH_H
#define HAVE_SEARCH_H   1

/* Define if you have the <setjmp.h> header file.  */
#undef  HAVE_SETJMP_H
#define HAVE_SETJMP_H   1

/* Define if you have the <signal.h> header file.  */
#undef  HAVE_SIGNAL_H
#define HAVE_SIGNAL_H   1

#undef  HAVE_SYS_SIGLIST
#define HAVE_SYS_SIGLIST 1

/* Define if you have the <stdarg.h> header file.  */
#undef  HAVE_STDARG_H
#define HAVE_STDARG_H   1

/* Define if you have the <stddef.h> header file.  */
#undef  HAVE_STDDEF_H
#define HAVE_STDDEF_H   1

/* Define if you have the <stdio.h> header file (is there ANY C
   installation that doesn't??).  */
#undef  HAVE_STDIO_H
#define HAVE_STDIO_H    1

/* Define if you have the <stdlib.h> header file.  */
#undef  HAVE_STDLIB_H
#define HAVE_STDLIB_H   1

/* Define if you have the <string.h> header file.  */
#undef  HAVE_STRING_H
#define HAVE_STRING_H   1

/* Define if you have the <termios.h> header file.  */
#undef  HAVE_TERMIOS_H  /* we have, but the functions aren't implemented */
/* #define HAVE_TERMIOS_H  1 */

/* Define if you have the <time.h> header file.  */
#undef  HAVE_TIME_H
#define HAVE_TIME_H     1

/* Define if you have the <sys/time.h> header file.  */
#undef  HAVE_SYS_TIME_H
#define HAVE_SYS_TIME_H     1

/* Define this if your <time.h> and <sys/time.h> can both be
   included with no conflicts.  */
#undef  TIME_WITH_SYS_TIME
#define TIME_WITH_SYS_TIME  1

/* Define if you have the <unistd.h> header file.  */
#undef  HAVE_UNISTD_H
#define HAVE_UNISTD_H   1

/* Define if you have the <utime.h> header file.  */
#undef  HAVE_UTIME_H
#define HAVE_UTIME_H    1

/* Define if you have the values.h header file.  */
#undef  HAVE_VALUES_H
#define HAVE_VALUES_H 1

/* Define if you have the <varargs.h> header file.  */
#undef  HAVE_VARARGS_H
#define HAVE_VARARGS_H  1

/* Define if you have the <netinet/in.h> header file.  */
#undef  HAVE_NETINET_IN_H
#define HAVE_NETINET_IN_H   1

/* Define if you have the <sys/file.h> header file.  */
#undef  HAVE_SYS_FILE_H
#define HAVE_SYS_FILE_H     1

/* Define if you have the <sys/ioctl.h> header file.  */
#undef  HAVE_SYS_IOCTL_H
#define HAVE_SYS_IOCTL_H 1

/* Define if you have the <sys/param.h> header file.  */
#undef  HAVE_SYS_PARAM_H
#define HAVE_SYS_PARAM_H    1

/* Define if you have the <sys/resource.h> header file.  */
#undef  HAVE_SYS_RESOURCE_H
#define HAVE_SYS_RESOURCE_H 1

/* Define if you have the <sys/stat.h> header file.  */
#undef  HAVE_SYS_STAT_H
#define HAVE_SYS_STAT_H 1

/* Define if you have the <sys/time.h> header file.  */
#undef  HAVE_SYS_TIME_H
#define HAVE_SYS_TIME_H 1

/* Define if you have the <sys/timeb.h> header file.  */
#undef  HAVE_SYS_TIMEB_H
#define HAVE_SYS_TIMEB_H    1

/* Define if you have the <sys/times.h> header file.  */
#undef  HAVE_SYS_TIMES_H
#define HAVE_SYS_TIMES_H    1

/* Define if you have the <sys/types.h> header file.  */
#undef  HAVE_SYS_TYPES_H
#define HAVE_SYS_TYPES_H    1

/* Define if you have the <sys/utsname.h> header file.  */
#undef  HAVE_SYS_UTSNAME_H
#define HAVE_SYS_UTSNAME_H  1

/* Define if you have the <sys/vfs.h> header file.  */
#undef  HAVE_SYS_VFS_H
#define HAVE_SYS_VFS_H  1

/* Define if you have the <sys/wait.h> header file.  */
#undef  HAVE_SYS_WAIT_H
#define HAVE_SYS_WAIT_H 1


/* ---------------------------------------------------------------------
                   Library functions and related stuff.
   --------------------------------------------------------------------- */

/* Define if using alloca.c.  */
#undef  C_ALLOCA

/* Define if you have bcmp() and bcopy() library functions.  */
#undef  HAVE_BCMP
#define HAVE_BCMP   1
#undef  HAVE_BCOPY
#define HAVE_BCOPY  1

/* Define if you have closedir() function in your library.  */
#undef  HAVE_CLOSEDIR
#define HAVE_CLOSEDIR   1

/* Define if you have dup2() library function.  */
#undef  HAVE_DUP2
#define HAVE_DUP2       1

/* Define if you have the endgrent function.  */
#undef  HAVE_ENDGRENT
#define HAVE_ENDGRENT 1

/* Define if you have the endpwent function.  */
#undef  HAVE_ENDPWENT
#define HAVE_ENDPWENT 1

/* Define if you have fnmatch() function in your library.  */
#undef  HAVE_FNMATCH
#define HAVE_FNMATCH    1

/* Define if you have frexp() function in your library.  */
#undef  HAVE_FREXP
#define HAVE_FREXP  1

/* Define if you have ftime() function in your library.  */
#undef  HAVE_FTIME
#define HAVE_FTIME  1

/* Define if you have the the ftruncate() library function.  */
#undef  HAVE_FTRUNCATE
#define HAVE_FTRUNCATE  1

/* Define if you have ftw() function in your library.  */
#undef  HAVE_FTW
#define HAVE_FTW    1

/* Define if you have getcwd() function in your library.  */
#undef  HAVE_GETCWD
#define HAVE_GETCWD 1

/* Define if you have getdtablesize() function in your library.  */
#undef  HAVE_GETDTABLESIZE
#define HAVE_GETDTABLESIZE  1

/* Define if you have the getgroups function.  */
#undef  HAVE_GETGROUPS
#define HAVE_GETGROUPS	1

/* Define if you have gethostname() function in your library.  */
#undef  HAVE_GETHOSTNAME
#define HAVE_GETHOSTNAME  1

/* Define if you have getmntent() function in your library.  */
#undef  HAVE_GETMNTENT
#define HAVE_GETMNTENT      1

/* Define if you have getpagesize() function in your library.  */
#undef  HAVE_GETPAGESIZE
#define HAVE_GETPAGESIZE      1

/* Define this if your getpgrp() function takes no argument (the
   POSIX.1 version).  */
#undef  GETPGRP_VOID
#define GETPGRP_VOID        1

/* Define if your getmntent() function accepts one argument.  */
#undef  MOUNTED_GETMNTENT1
#define MOUNTED_GETMNTENT1  1

/* Define if you have gettimeofday() function in your library.  */
#undef  HAVE_GETTIMEOFDAY
#define HAVE_GETTIMEOFDAY   1

/* Define if you have the glob() function in your library.  */
#undef  HAVE_GLOB
#define HAVE_GLOB   1

/* Define if you have the isascii function.  */
#undef  HAVE_ISASCII
#define HAVE_ISASCII 1

/* Define if you have memchr() in your library.  */
#undef  HAVE_MEMCHR
#define HAVE_MEMCHR 1

/* Define if you have the memcpy function.  */
#undef  HAVE_MEMCPY
#define HAVE_MEMCPY 1

/* Define if you have mkdir() function in your library.  */
#undef  HAVE_MKDIR
#define HAVE_MKDIR  1

/* Define if you have the mkfifo function.  */
#undef  HAVE_MKFIFO
#define HAVE_MKFIFO 1

/* Define if you have mktime() function in your library.  */
#undef  HAVE_MKTIME
#define HAVE_MKTIME 1

/* Define if you have the pow function.  */
#undef  HAVE_POW
#define HAVE_POW 1

/* Define if you have the putenv function.  */
#undef  HAVE_PUTENV
#define HAVE_PUTENV 1

/* Define if you have random() function in your library.  */
#undef  HAVE_RANDOM
#define HAVE_RANDOM 1

/* Define if you have rename() function in your library.  */
#undef  HAVE_RENAME
#define HAVE_RENAME 1

/* Define if you have rmdir() function in your library.  */
#undef  HAVE_RMDIR
#define HAVE_RMDIR  1

/* Define if you have the setenv function.  */
#undef  HAVE_SETENV
#define HAVE_SETENV 1

/* Define if you have setlinebuf() function in your library.  */
#undef  HAVE_SETLINEBUF
#define HAVE_SETLINEBUF 1

/* Define if you have the setlocale function.  */
#undef  HAVE_SETLOCALE
#define HAVE_SETLOCALE 1

/* Define if you have sigaction() function in your library.  */
#undef  HAVE_SIGACTION
#define HAVE_SIGACTION  1

/* Define if your statfs() function accepts 2 arguments and
   struct statfs has f_bsize field.  */
#undef  STAT_STATFS2_BSIZE
#define STAT_STATFS2_BSIZE  1

/* Define if you have the stpcpy function.  */
#undef  HAVE_STPCPY
#define HAVE_STPCPY 1

/* Define if you have strcasecmp() function in your library.  */
#undef	HAVE_STRCASECMP
#define HAVE_STRCASECMP	1

/* Define if you have strchr() function in your library.  */
#undef	HAVE_STRCHR
#define HAVE_STRCHR	1

/* Define if you have strrchr() function in your library.  */
#undef	HAVE_STRRCHR
#define HAVE_STRRCHR	1

/* Define if you have strcoll() function in your library.  */
#undef  HAVE_STRCOLL
#define HAVE_STRCOLL    1

/* Define if you have strdup() function in your library.  */
#undef	HAVE_STRDUP
#define HAVE_STRDUP	1

/* Define if you have strftime() function in your library.  */
#undef  HAVE_STRFTIME
#define HAVE_STRFTIME   1

/* Define if you have strerror.  */
#undef  HAVE_STRERROR
#define HAVE_STRERROR   1

/* Define if your utime() library function accepts NULL as its second
   argument (meaning use current time).  */
#undef  HAVE_UTIME_NULL
#define HAVE_UTIME_NULL 1

/* Define vfork as fork if vfork() does not work.  */
#undef  vfork
#define vfork   fork

/* Define if you have the vprintf() library function.  */
#undef  HAVE_VPRINTF
#define HAVE_VPRINTF    1

/* Define if you have waitpid.  */
#undef  HAVE_WAITPID    /* we do, but it always fails :-( */

/* ---------------------------------------------------------------------
                                Structures
   --------------------------------------------------------------------- */

/* Define if your struct stat has st_blksize.  */
#undef  HAVE_ST_BLKSIZE
#define HAVE_ST_BLKSIZE 1

/* Define if your struct stat has st_blocks.  */
#undef  HAVE_ST_BLOCKS

/* Define if your struct stat has st_rdev member.  */
#undef  HAVE_ST_RDEV
#define HAVE_ST_RDEV    1

/* Define if you have `struct utimbuf' declared in <utime.h>.  */
#undef  HAVE_STRUCT_UTIMBUF
#define HAVE_STRUCT_UTIMBUF 1

/* Define if you have struct timeval defined in your <time.h> header file.  */
#undef  HAVE_TIMEVAL
#define HAVE_TIMEVAL    1

/* Define if you have tm_zone field in your struct tm definition (in
   <time.h> header file).  */
#undef  HAVE_TM_ZONE
#define HAVE_TM_ZONE    1

/* ---------------------------------------------------------------------
                                 Typedefs
   --------------------------------------------------------------------- */

/* Define to the type of elements in the array set by `getgroups'.
   Usually this is either `int' or `gid_t'.  */
#undef  GETGROUPS_T
#define GETGROUPS_T     gid_t

/* Define as the return type of signal handlers (int or void).  */
#undef  RETSIGTYPE
#define RETSIGTYPE      void


/* ---------------------------------------------------------------------
                         Compiler Characteristics
   --------------------------------------------------------------------- */

/* Define `inline' to `__inline__' if your compiler accepts it.  */
#undef  inline
#define inline  __inline__

/* Define this if the C compiler supports the `long double' type.  */
#undef  HAVE_LONG_DOUBLE
#define HAVE_LONG_DOUBLE    1

/* Sizes of built-in types and pointers known to the compiler.  */
#define SIZEOF_CHAR             1
#define SIZEOF_CHAR_P           4
#define SIZEOF_SHORT            2
#define SIZEOF_SHORT_P          4
#define SIZEOF_INT              4
#define SIZEOF_INT_P            4
#define SIZEOF_LONG             4
#define SIZEOF_LONG_P           4
#define SIZEOF_LONG_LONG        8
#define SIZEOF_LONG_LONG_P      4
#define SIZEOF_FLOAT            4
#define SIZEOF_FLOAT_P          4
#define SIZEOF_DOUBLE           8
#define SIZEOF_DOUBLE_P         4
#define SIZEOF_LONG_DOUBLE      10
#define SIZEOF_LONG_DOUBLE_P    4
#define SIZEOF_VOID_P           4

/* If using the C implementation of alloca, define if you know the
   direction of stack growth for your system; otherwise it will be
   automatically deduced at run-time.
	STACK_DIRECTION > 0 => grows toward higher addresses
	STACK_DIRECTION < 0 => grows toward lower addresses
	STACK_DIRECTION = 0 => direction of growth unknown
 */
#undef  STACK_DIRECTION

/* Define to empty if the `const' keyword does not work.  */
#undef  const

/* ---------------------------------------------------------------------
                              System Services
   --------------------------------------------------------------------- */

/* Define this to be the name of your NULL device.  */
#undef  NULL_DEVICE
#define NULL_DEVICE "nul"

/* Do we have long filenames?  */
#undef  HAVE_LONG_FILE_NAMES    /* not yet, but Win95 might have them... */

/* ---------------------------------------------------------------------
                             Misc definitions
   --------------------------------------------------------------------- */

/* Define both _LIBC and __GNU_LIBRARY__ if you use GNU C library,
   but want link in the version of getopt, regex, fnmatch (and other
   routines which are part of GNU C library) which came with the
   package.  Define _LIBC alone if you use non-GNU C library which
   might be incompatible with GNU (e.g., getopt()).  Define __GNU_LIBRARY__
   alone if you want the code for the above functions to be effectively
   commented out, so you will get the code from the GNU C library.
*/
#undef  _LIBC
#undef  __GNU_LIBRARY__
/* #define _LIBC           1 */
/* #define __GNU_LIBRARY__ 1 */



#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_config_h_ */
