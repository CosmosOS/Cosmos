/* Copyright (C) 1996 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_mntent_h_
#define __dj_include_mntent_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#include <stdio.h>

#define	MNT_MNTTAB	"/etc/mnttab"

struct mntent
{
  char *mnt_fsname;
  char *mnt_dir;
  char *mnt_type;
  char *mnt_opts;
  int  mnt_freq;
  int  mnt_passno;
  long mnt_time;
};

extern FILE		*setmntent(const char *, const char *);
extern struct mntent	*getmntent(FILE *);
extern int		addmntent(FILE *, const struct mntent *);
extern char		*hasmntopt(const struct mntent *, const char *);
extern int		endmntent(FILE *);


#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_mntent_h_ */
