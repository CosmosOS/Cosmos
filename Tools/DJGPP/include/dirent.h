/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_dirent_h_
#define __dj_include_dirent_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

/* Definition of DIR requires many other headers; not included here to
   avoid namespace pollution. */
typedef struct __dj_DIR DIR;

struct dirent {
  char d_namlen;
  char d_name[256];
};

int		closedir(DIR *dirp);
DIR *		opendir(const char *_dirname);
struct dirent *	readdir(DIR *_dirp);
void		rewinddir(DIR *_dirp);

#ifndef _POSIX_SOURCE

extern int __opendir_flags; /* default is zero, used only by opendir */
#define __OPENDIR_PRESERVE_CASE	0001
#define __OPENDIR_FIND_HIDDEN	0002 /* ignored; on by default */
#define __OPENDIR_FIND_LABEL	0004
#define __OPENDIR_NO_HIDDEN	0x08 /* NOT 0002 for back-compatibility */

void seekdir(DIR *_dir, long _loc);
long telldir(DIR *_dir);
void __set_need_fake_dot_dotdot(DIR *_dir);
void _lfn_find_close(int _handle);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_dirent_h_ */
