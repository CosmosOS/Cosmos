/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_pwd_h_
#define __dj_include_pwd_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#include <sys/djtypes.h>

__DJ_gid_t
#undef __DJ_gid_t
#define __DJ_gid_t
__DJ_uid_t
#undef __DJ_uid_t
#define __DJ_uid_t

struct passwd {
  char *	pw_name;
  uid_t		pw_uid;
  gid_t		pw_gid;
  char *	pw_dir;
  char *	pw_shell;
};
  
struct passwd *	getpwuid(uid_t _uid);
struct passwd *	getpwnam(const char *_name);

#ifndef _POSIX_SOURCE

struct passwd	*getpwent(void);
void		setpwent(void);
void		endpwent(void);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_pwd_h_ */
