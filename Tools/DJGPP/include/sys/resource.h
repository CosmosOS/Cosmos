/* Copyright (C) 1996 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_resource_h_
#define __dj_include_sys_resource_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#include <time.h>

#define RUSAGE_SELF     0               /* calling process */
#define RUSAGE_CHILDREN -1              /* terminated child processes */

struct rusage {
  struct timeval ru_utime;	/* user time used */
  struct timeval ru_stime;	/* system time used */
  long ru_maxrss;		/* integral max resident set size */
  long ru_ixrss;		/* integral shared text memory size */
  long ru_idrss;		/* integral unshared data size */
  long ru_isrss;		/* integral unshared stack size */
  long ru_minflt;		/* page reclaims */
  long ru_majflt;		/* page faults */
  long ru_nswap;		/* swaps */
  long ru_inblock;		/* block input operations */
  long ru_oublock;		/* block output operations */
  long ru_msgsnd;		/* messages sent */
  long ru_msgrcv;		/* messages received */
  long ru_nsignals;		/* signals received */
  long ru_nvcsw;		/* voluntary context switches */
  long ru_nivcsw;		/* involuntary context switches */
};

#define RLIMIT_CPU	0	/* cpu time in milliseconds */
#define RLIMIT_FSIZE	1	/* maximum file size */
#define RLIMIT_DATA	2	/* data size */
#define RLIMIT_STACK	3	/* stack size */
#define RLIMIT_CORE	4	/* core file size */
#define RLIMIT_RSS	5	/* resident set size */
#define RLIMIT_MEMLOCK	6	/* locked-in-memory address space */
#define RLIMIT_NPROC	7	/* number of processes */
#define RLIMIT_NOFILE	8	/* number of open files */

#define RLIM_NLIMITS	9	/* number of resource limits */
#define RLIM_INFINITY	((long) ((1UL << 31) - 1UL))

struct rlimit {
  long rlim_cur;		/* current (soft) limit */
  long rlim_max;		/* maximum value for rlim_cur */
};

int getrusage(int _who, struct rusage *_rusage);
int getrlimit(int _rltype, struct rlimit *_rlimit);
int setrlimit(int _rltype, const struct rlimit *_rlimit);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_resource_h_ */
