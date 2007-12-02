/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_libc_ttyprvt_h__
#define __dj_include_libc_ttyprvt_h__

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#include <dpmi.h>
#include <termios.h>
#include <unistd.h>

#define _TTY_CTRL(x) ((x) & 0x1f)
#define _TTY_QUEUE_SIZE 2048
#define _TTY_EDITLINE_SIZE ((_TTY_QUEUE_SIZE) / 2)
#define _TTY_EDITLINE_CTRL 0
#define _TTY_EDITLINE_SINGLE 1
#define _TTY_EDITLINE_INVALID -1

struct tty_queue
{
  int size;
  unsigned char *top;
  unsigned char *bottom;
  int count;
  unsigned char *rpos;
  unsigned char *wpos;
};

struct tty
{
  struct termios __libc_termios;
  struct tty_queue __libc_tty_queue;
  int __libc_tty_status;
};

struct tty_editline
{
  int col;
  char flag[_TTY_EDITLINE_SIZE];
  unsigned char buf[_TTY_EDITLINE_SIZE];
};

#if !defined (_POSIX_VDISABLE) || (_POSIX_VDISABLE == 0)
#error _POSIX_VDISABLE is undefine or zero.
#endif

#define TTYDEFAULT \
{									\
  /* struct termios __libc_termios */					\
  {									\
    /* c_cc[] */							\
    {									\
      (cc_t) 0,               /* pad */ 				\
      (cc_t) _TTY_CTRL ('d'), /* VEOF */				\
      (cc_t) _POSIX_VDISABLE, /* VEOL */				\
      (cc_t) _TTY_CTRL ('h'), /* VERASE */				\
      (cc_t) _TTY_CTRL ('c'), /* VINTR */				\
      (cc_t) _TTY_CTRL ('u'), /* VKILL */				\
      (cc_t) 1,               /* VMIN */				\
      (cc_t) _TTY_CTRL ('\\'),/* VQUIT */				\
      (cc_t) _TTY_CTRL ('q'), /* VSTART */				\
      (cc_t) _TTY_CTRL ('s'), /* VSTOP */				\
      (cc_t) _TTY_CTRL ('z'), /* VSUSP */				\
      (cc_t) 0,               /* VTIME */				\
    },									\
    (tcflag_t) (CS8|CREAD|CLOCAL), /* c_cflag */			\
    (tcflag_t) (BRKINT|ICRNL|IMAXBEL), /* c_iflag */			\
    (tcflag_t) (ISIG|ICANON|ECHO|IEXTEN|ECHOE|ECHOKE|ECHOCTL), /* c_lflag */ \
    (tcflag_t) (OPOST|ONLCR|ONOEOT), /* c_oflag */			\
    (speed_t) (B9600), /* c_ispeed */					\
    (speed_t) (B9600), /* c_ospeed */					\
  },									\
  /* struct tty_queue __libc_tty_queue */				\
  {									\
    _TTY_QUEUE_SIZE,							\
    __libc_tty_queue_buffer,						\
    __libc_tty_queue_buffer + _TTY_QUEUE_SIZE,				\
    0,									\
    __libc_tty_queue_buffer,						\
    __libc_tty_queue_buffer,						\
  },									\
  /* __libc_tty_status */						\
  0,									\
}

#define t_termios __libc_termios
#define t_iflag __libc_termios.c_iflag
#define t_oflag __libc_termios.c_oflag
#define t_cflag __libc_termios.c_cflag
#define t_lflag __libc_termios.c_lflag
#define t_ispeed __libc_termios.c_ispeed
#define t_ospeed __libc_termios.c_ospeed
#define t_cc __libc_termios.c_cc
#define t_status __libc_tty_status

#define t_size __libc_tty_queue.size
#define t_top __libc_tty_queue.top
#define t_bottom __libc_tty_queue.bottom
#define t_count __libc_tty_queue.count
#define t_rpos __libc_tty_queue.rpos
#define t_wpos __libc_tty_queue.wpos

#define _TS_LNCH 0x01 /* next character is literal */
#define _CC_EQU(v,c) (((c) == (unsigned char) __libc_tty_p->t_cc[(v)])	\
		      && ((c) != (unsigned char) _POSIX_VDISABLE))
#define _CC_NEQU(v,c) (((c) != (unsigned char)__libc_tty_p->t_cc[(v)])	\
		       && ((c) != (unsigned char) _POSIX_VDISABLE))

/* internal buffers */
extern unsigned char __libc_tty_queue_buffer[];
extern struct tty __libc_tty_internal;
extern struct tty *__libc_tty_p;
extern struct tty_editline __libc_tty_editline;

/* termios hooks */
extern ssize_t (*__libc_read_termios_hook)(int handle, void *buffer, size_t count,
	   			           ssize_t *rv);
extern ssize_t (*__libc_write_termios_hook)(int handle, const void *buffer, size_t count,
					    ssize_t *rv);
extern int __libc_termios_hook_common_count;

/* functions */
void __libc_termios_init (void);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_libc_ttyprvt_h__ */
