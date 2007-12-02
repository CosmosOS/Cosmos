/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_ftw_h_
#define __dj_include_ftw_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#define FTW_F   1
#define FTW_D   2
#define FTW_NS  3
#define FTW_DNR 4
#define FTW_VL  5

#include <sys/stat.h>

int     ftw(const char *_dir,
            int (*_fn)(const char *_file, struct stat *_sb, int _flag),
            int _depth);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_ftw_h_ */
