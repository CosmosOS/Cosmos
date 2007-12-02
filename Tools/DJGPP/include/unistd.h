/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_unistd_h_
#define __dj_include_unistd_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#include <sys/types.h> /* NOT POSIX but you can't include just unistd.h without it */
#include <sys/djtypes.h>

#define SEEK_SET	0
#define SEEK_CUR	1
#define SEEK_END	2
  
/* Some programs think they know better... */
#undef NULL

#define NULL 0

#define F_OK	0x01
#define R_OK	0x02
#define W_OK	0x04
#define X_OK	0x08

#define STDIN_FILENO		0
#define STDOUT_FILENO		1
#define STDERR_FILENO		2

#define _PC_CHOWN_RESTRICTED	1
#define _PC_LINK_MAX		2
#define _PC_MAX_CANON		3
#define _PC_MAX_INPUT		4
#define _PC_NAME_MAX		5
#define _PC_NO_TRUNC		6
#define _PC_PATH_MAX		7
#define _PC_PIPE_BUF		8
#define _PC_VDISABLE		9

#define _POSIX_CHOWN_RESTRICTED	0
#undef  _POSIX_JOB_CONTROL
#define _POSIX_NO_TRUNC		0
#undef  _POSIX_SAVED_IDS
#define _POSIX_VDISABLE		-1
#define _POSIX_VERSION		199009L

#define _SC_ARG_MAX		1
#define _SC_CHILD_MAX		2
#define _SC_CLK_TCK		3
#define _SC_JOB_CONTROL		4
#define _SC_NGROUPS_MAX		5
#define _SC_OPEN_MAX		6
#define _SC_SAVED_IDS		7
#define _SC_STREAM_MAX		8
#define _SC_TZNAME_MAX		9
#define _SC_VERSION		10
 
__DJ_size_t
#undef __DJ_size_t
#define __DJ_size_t
__DJ_ssize_t
#undef __DJ_ssize_t
#define __DJ_ssize_t

extern char *optarg;
extern int optind, opterr, optopt;

void		__exit(int _status) __attribute__((noreturn));
void		_exit(int _status) __attribute__((noreturn));
int		access(const char *_path, int _amode);
unsigned int	alarm(unsigned int _seconds);
int		chdir(const char *_path);
int		chown(const char *_path, uid_t _owner, gid_t _group);
int		close(int _fildes);
char *		ctermid(char *_s);
int		dup(int _fildes);
int		dup2(int _fildes, int _fildes2);
int		execl(const char *_path, const char *_arg, ...);
int		execle(const char *_path, const char *_arg, ...);
int		execlp(const char *_file, const char *_arg, ...);
int		execv(const char *_path, char *const _argv[]);
int		execve(const char *_path, char *const _argv[], char *const _envp[]);
int		execvp(const char *_file, char *const _argv[]);
pid_t		fork(void);
long		fpathconf(int _fildes, int _name);
char *		getcwd(char *_buf, size_t _size);
gid_t		getegid(void);
uid_t		geteuid(void);
gid_t		getgid(void);
int		getgroups(int _gidsetsize, gid_t *_grouplist);
char *		getlogin(void);
int		getopt(int _argc, char *const _argv[], const char *_optstring);
pid_t		getpgrp(void);
pid_t		getpid(void);
pid_t		getppid(void);
uid_t		getuid(void);
int		isatty(int _fildes);
int		link(const char *_existing, const char *_new);
off_t		lseek(int _fildes, off_t _offset, int _whence);
long		pathconf(const char *_path, int _name);
int		pause(void);
int		pipe(int _fildes[2]);
ssize_t		read(int _fildes, void *_buf, size_t _nbyte);
int		rmdir(const char *_path);
int		setgid(gid_t _gid);
int		setpgid(pid_t _pid, pid_t _pgid);
pid_t		setsid(void);
int		setuid(uid_t uid);
unsigned int	sleep(unsigned int _seconds);
long		sysconf(int _name);
pid_t		tcgetpgrp(int _fildes);
int		tcsetpgrp(int _fildes, pid_t _pgrp_id);
char *		ttyname(int _fildes);
int		unlink(const char *_path);
ssize_t		write(int _fildes, const void *_buf, size_t _nbyte);

#ifndef _POSIX_SOURCE

/* additional access() checks */
#define D_OK	0x10

char *		basename(const char *_fn);
int		brk(void *_heaptop);
char *		dirname(const char *_fn);
int		__file_exists(const char *_fn);
int		fsync(int _fd);
int		ftruncate(int, off_t);
int		getdtablesize(void);
int		gethostname(char *buf, int size);
int		getpagesize(void);
char *		getwd(char *__buffer);
int		nice(int _increment);
void *		sbrk(int _delta);
int		symlink (const char *, const char *);
int		sync(void);
int		truncate(const char*, off_t);
unsigned int	usleep(unsigned int _useconds);
#ifndef vfork
pid_t		vfork(void);
#endif

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_unistd_h_ */
