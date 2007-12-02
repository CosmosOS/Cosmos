/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_signal_h_
#define __dj_include_signal_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#include <sys/djtypes.h>

/* 256 software interrupts + 32 exceptions = 288 */

#define SIGABRT	288
#define SIGFPE	289
#define SIGILL	290
#define SIGSEGV	291
#define SIGTERM	292
#define SIGINT  295

#define SIG_DFL ((void (*)(int))(0))
#define SIG_ERR	((void (*)(int))(1))
#define SIG_IGN	((void (*)(int))(-1))

__DJ_pid_t
#undef __DJ_pid_t
#define __DJ_pid_t

typedef int sig_atomic_t;

int	raise(int _sig);
void	(*signal(int _sig, void (*_func)(int)))(int);
  
#ifndef __STRICT_ANSI__

#define SA_NOCLDSTOP	1

#define SIGALRM	293
#define SIGHUP	294
/* SIGINT is ansi */
#define SIGKILL	296
#define SIGPIPE	297
#define SIGQUIT	298
#define SIGUSR1	299
#define SIGUSR2	300

#define SIG_BLOCK	1
#define SIG_SETMASK	2
#define SIG_UNBLOCK	3

typedef struct {
  unsigned long __bits[10]; /* max 320 signals */
} sigset_t;

struct sigaction {
  int sa_flags;
  void (*sa_handler)(int);
  sigset_t sa_mask;
};

int	kill(pid_t _pid, int _sig);
int	sigaction(int _sig, const struct sigaction *_act, struct sigaction *_oact);
int	sigaddset(sigset_t *_set, int _signo);
int	sigdelset(sigset_t *_set, int _signo);
int	sigemptyset(sigset_t *_set);
int	sigfillset(sigset_t *_set);
int	sigismember(const sigset_t *_set, int _signo);
int	sigpending(sigset_t *_set);
int	sigprocmask(int _how, const sigset_t *_set, sigset_t *_oset);
int	sigsuspend(const sigset_t *_set);

#ifndef _POSIX_SOURCE

#define SIGNOFP 301
#define SIGTRAP 302
#define SIGTIMR 303	/* Internal for setitimer (SIGALRM, SIGPROF) */
#define SIGPROF 304
#define SIGMAX 320

void	__djgpp_traceback_exit(int _sig);

#define NSIG SIGMAX

extern char *sys_siglist[];

void	psignal(int _sig, const char *_msg);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_signal_h_ */
