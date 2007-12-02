/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_system_h__
#define __dj_include_sys_system_h__

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

extern int _shell_command  (const char *_prog, const char *_cmdline);
extern int _is_unixy_shell (const char *_prog);
extern int _is_dos_shell   (const char *_prog);

/* Checking for special executable formats */

typedef struct {
  char magic[16];
  int struct_length;
  char go32[16];
  unsigned char buffer[0];
} _v1_stubinfo;


typedef struct {
  union {
    unsigned version:8; /* The version of DJGPP created that COFF exe */
    struct {
      unsigned minor:4; /* The minor version of DJGPP */
      unsigned major:4; /* The major version of DJGPP */
    } v;
  } version;

  unsigned object_format:4; /* What an object format */
# define _V2_OBJECT_FORMAT_UNKNOWN 0x00
# define _V2_OBJECT_FORMAT_COFF    0x01

  unsigned exec_format:4; /* What an executable format */
# define _V2_EXEC_FORMAT_UNKNOWN    0x00
# define _V2_EXEC_FORMAT_COFF       0x01
# define _V2_EXEC_FORMAT_STUBCOFF   0x02
# define _V2_EXEC_FORMAT_EXE        0x03
# define _V2_EXEC_FORMAT_UNIXSCRIPT 0x04

  unsigned valid:1; /* Only when nonzero all the information is valid */

  unsigned has_stubinfo:1; /* When nonzero the stubinfo info is valid */

  unsigned unused:14;

  _v1_stubinfo *stubinfo;
} _v2_prog_type;

/* When program == NULL you have to pass a valid file handle
   in fd, otherwise the file is opened and closed by the function */
const _v2_prog_type *_check_v2_prog(const char *program, int fd);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* __dj_include_sys_system_h__ */
