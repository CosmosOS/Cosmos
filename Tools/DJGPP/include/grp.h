/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_grp_h_
#define __dj_include_grp_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#include <sys/djtypes.h>
__DJ_gid_t
#undef __DJ_gid_t
#define __DJ_gid_t

struct group {
  gid_t		gr_gid;
  char **      	gr_mem;
  char *	gr_name;
};

struct group *	getgrgid(gid_t _gid);
struct group *	getgrnam(const char *_name);

#ifndef _POSIX_SOURCE

void		endgrent(void);
struct group *	getgrent(void);
struct group *	fgetgrent(void *_f);
void		setgrent(void);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_grp_h_ */
