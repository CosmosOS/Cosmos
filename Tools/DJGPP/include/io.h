/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_io_h_
#define __dj_include_io_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#include <sys/types.h>

int		chsize(int handle, long size);
int		_close(int _fd);
int		_creat(const char *_path, int _attrib);
int		_creatnew(const char *_path, int _attrib, int _mode);
ssize_t		crlf2nl(char *_buffer, ssize_t _length);
int		_dos_lock(int _fd, long _offset, long _length);
long		filelength(int _handle);
short		_get_dev_info(int _arg);
int		lock(int _fd, long _offset, long _length);
int		_open(const char *_path, int _oflag);
ssize_t		_read(int _fd, void *_buf, size_t _nbyte);
int		setmode(int _fd, int _newmode);
off_t		tell(int _fd);
int		_dos_unlock(int _fd, long _offset, long _length);
int		unlock(int _fd, long _offset, long _length);
ssize_t		_write(int _fd, const void *_buf, size_t _nbyte);
int	        _chmod(const char *_path, int _func, ...);
void		_flush_disk_cache(void);

#define sopen(path, access, shflag, mode) \
	open((path), (access)|(shflag), (mode))

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_io_h_ */
