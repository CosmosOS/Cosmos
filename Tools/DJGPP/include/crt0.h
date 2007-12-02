/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1996 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_crt0_h_
#define __dj_include_crt0_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/*****************************************************************************\
 * crt0.h - specific to go32 v2.0 applications, controls command line
 * argument creation.
\*****************************************************************************/

/*****************************************************************************\
 * If the application wishes to provide a wildcard expansion function,
 * it should define a __crt0_glob_function function.  It should return
 * a list of the expanded values, or 0 if no expansion will occur.
 * The startup code will free the returned pointer if it is nonzero.
 *
 * If no expander function is provided, wildcards will be expanded in
 * the POSIX.1 style.  To disable expansion, provide a __crt0_glob_function
 * that always returns 0.
 *
 * Applications that do not rely on environment variables can provide an
 * alternate version of __crt0_load_environment_file that does nothing.
 *
 * Applications that do not rely on arguments passed to main() can
 * provide an alternate version of __crt0_setup_arguments() that does
 * nothing.
\*****************************************************************************/

extern char  *__dos_argv0;
extern int    __crt0_argc;
extern char **__crt0_argv;

void   __crt0_load_environment_file(char *_app_name);
void   __crt0_setup_arguments(void);
char **__crt0_glob_function(char *_arg);

/*****************************************************************************\
 *
 *  To set any of these startup flags, add the following declaration to
 *  *your* source code:
 *   
 *	int _crt0_startup_flags = _CRT0_FLAG_* | _CRT0_FLAG_*;
 *  
 *  The default is all flags off.
 *
\*****************************************************************************/

extern int _crt0_startup_flags;

/* If set, argv[0] is left in whatever case it was.  If not set, all
** characters are mapped to lower case.  Note that if the argv0 field in
** the stubinfo structure is present, the case of that part of argv0 is not
** affected. 
*/
#define _CRT0_FLAG_PRESERVE_UPPER_CASE		0x0001

/* If set, reverse slashes (dos-style) are preserved in argv[0].  If not
** set, all reverse slashes are replaced with unix-style slashes.
*/
#define _CRT0_FLAG_USE_DOS_SLASHES		0x0002

/* If set, the .EXE suffix is removed from the file name component of
** argv[0].  If not set, the suffix remains. 
*/
#define _CRT0_FLAG_DROP_EXE_SUFFIX		0x0004

/* If set, the drive specifier (ex: `C:') is removed from the beginning of
** argv[0] (if present).  If not set, the drive specifier remains. 
*/
#define _CRT0_FLAG_DROP_DRIVE_SPECIFIER	0x0008

/* If set, response files (ex: @gcc.rf) are not expanded.  If not set, the
** contents of the response files are used to create arguments.  Note that
** if the file does not exist, that argument remains unexpanded. 
*/
#define _CRT0_FLAG_DISALLOW_RESPONSE_FILES	0x0010

/* If set, fill sbrk()'d memory with a constant value.  If not, memory
** gets whatever happens to have been in there, which breaks some
** applications.
*/
#define _CRT0_FLAG_FILL_SBRK_MEMORY		0x0020

/* If set, fill memory (above) with 0xdeadbeef, else fill with zero.
** This is especially useful for debugging uninitialized memory problems.
*/
#define _CRT0_FLAG_FILL_DEADBEEF		0x0040

/* If set, set DS limit to 4GB which allows use of near pointers to DOS
** (and other) memory.  WARNING, disables memory protection and bad pointers
** may crash the machine or wipe out your data.
*/
#define _CRT0_FLAG_NEARPTR			0x0080

/* If set, disable NULL pointer protection (if it can be controlled at all).
*/
#define _CRT0_FLAG_NULLOK			0x0100

/* If set, enabled capture of NMI in exception code.  This may cause problems
** with laptops and "green" boxes which use it to wake up.  Default is to 
** leave NMIs alone and pass through to real mode code.  You decide.
*/
#define _CRT0_FLAG_NMI_SIGNAL			0x0200

/* If set, disable usage of long file name functions even on systems
** (such as Win95) which support them.  This might be needed to work
** around program assumptions on file name format on programs written
** specifically for DOS.
*/
#define _CRT0_FLAG_NO_LFN			0x0400

/* If set, chooses an sbrk() algorithm.  If your code requires one type
** or the other, set the value (since the default may change).  The non-move
** sbrk makes sure the base of CS/DS/SS does not change.  Each new sbrk() 
** allocation is put in a different DPMI memory block.  This works best with
** DOS programs which would like to use near pointers or hardware interrupts.
** The unix sbrk resizes a single memory block, so programs making assumptions
** about unix-like sbrk behavior may run better with this choice.
*/
#define _CRT0_FLAG_NONMOVE_SBRK			0x0000		/* Default */
#define _CRT0_FLAG_UNIX_SBRK			0x0800

/* If set, locks all memory as it is allocated.  This effectively disables
** virtual memory, and may be useful if using extensive hardware interrupt
** codes in a relatively small image size.  The memory is locked after it
** is sbrk()ed, so the locking may fail.  This bit may be set or cleared
** during execution.  When sbrk() uses multiple memory zones, it can be
** difficult to lock all memory since the memory block size and location is
** impossible to determine.
*/

#define _CRT0_FLAG_LOCK_MEMORY			0x1000

/* If set, disables all filename letter-case conversion in functions that
** traverse directories (except findfirst/findnext which always return the
** filenames exactly as found in the directory entry).  When reset, all
** filenames on 8+3 MSDOS filesystems and DOS-style 8+3 filenames on LFN
** systems are converted to lower-case by functions such as `readdir',
** `getcwd', `_fixpath' and `srchpath'.  Note that when this flag is set,
** ALL filenames on MSDOS systems will appear in upper-case, which is
** both ugly and will break many Unix-born programs.  Use only if you know
** exactly what you are doing!
*/

#define _CRT0_FLAG_PRESERVE_FILENAME_CASE	0x2000

/* If set, the quote characters ', ", and \ will be retained in argv[]
** elements when processing command lines passed via `system'.  This is
** used by `redir', and should only be needed if you want to get the
** original command line exactly as it was passed by the caller.
*/

#define _CRT0_FLAG_KEEP_QUOTES			0x4000

/*****************************************************************************\
 *  Access to the memory handles used by the non-move sbrk algorithm.  
 *  The handle is the SI:DI DPMI handle; the address is the offset relative 
 *  to the application's address space.  Address will be zero unused slots > 1.
\*****************************************************************************/

typedef struct {
  long handle;
  unsigned address;
  } __djgpp_sbrk_handle;

extern __djgpp_sbrk_handle __djgpp_memory_handle_list[256];
__djgpp_sbrk_handle *__djgpp_memory_handle(unsigned address);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_crt0_h_ */
