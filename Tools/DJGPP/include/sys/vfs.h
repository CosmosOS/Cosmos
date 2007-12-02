/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_vfs_h_
#define __dj_include_sys_vfs_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

typedef long	fsid_t[2];

#define	MOUNT_UFS	0
#define	MOUNT_NFS	1	/* Not possible on DOS */
#define	MOUNT_CDFS	2	/* Not possible on DOS */

#define	FS_MAGIC	0x11954	/* Taken from HP-UX */

struct statfs
{
    long	f_type;
    long	f_bsize;
    long	f_blocks;
    long	f_bfree;
    long	f_bavail;
    long	f_files;
    long	f_ffree;
    fsid_t	f_fsid;
    long	f_magic;
};

extern int	statfs(const char *, struct statfs *);
extern int	fstatfs(int, struct statfs *);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_vfs_h_ */
