/* Copyright (C) 1996 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_ioctl_h_
#define __dj_include_sys_ioctl_h_

#ifdef __cplusplus
extern "C"{
#endif


#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE
/*
**  plain ioctl functions. Do not use them.
**  Never. Really _N_E_V_E_R_. Unless you really know what
**  you are doing.
**
*/
#define DOS_PLAIN_GETDEVDATA    0x00
#define DOS_PLAIN_SETDEVDATA    0x01
#define DOS_PLAIN_RCVDATA       0x02
#define DOS_PLAIN_SNDDATA       0x03
#define DOS_PLAIN_RCVCTLDATA    0x04
#define DOS_PLAIN_SNDCTLDATA    0x05
#define DOS_PLAIN_CHKINSTAT     0x06
#define DOS_PLAIN_CHKOUTSTAT    0x07
#define DOS_PLAIN_ISCHANGEABLE  0x08
#define DOS_PLAIN_ISREDIRBLK    0x09
#define DOS_PLAIN_ISREDIRHND    0x0a
#define DOS_PLAIN_SETRETRY      0x0b
#define DOS_PLAIN_GENCHARREQ    0x0c
#define DOS_PLAIN_GENBLKREQ     0x0d
#define DOS_PLAIN_GLDRVMAP      0x0e
#define DOS_PLAIN_SLDRVMAP      0x0f
#define DOS_PLAIN_QGIOCTLCAPH   0x10
#define DOS_PLAIN_QGIOCTLCAPD   0x11
/*
**  Flags for DOS commands
*/
#define DOS_XFER        0x8000         /* use xfer buffer                   */
#define DOS_RETMASK     0x7000         /* Here we put the bits to tell      */
                                       /* what to return. 8 possible values */
#define DOS_BRAINDEAD   0x0400         /* CX does not hold the number of    */
                                       /* bytes to transfer                 */

#define DOS_XINARGS(a)  ((a & 3)<<12)  /* How many extra args we expect.    */
#define DOS_RETAX       0x1000         /* return AX as result */
#define DOS_RETDX       0x2000         /* return DX as result */
#define DOS_RETDISI     0x3000         /* return DI SI as result */
/*
** DOS ioctls we support:
*/
#define DOS_GETDEVDATA      (DOS_PLAIN_GETDEVDATA|         DOS_RETDX|DOS_XINARGS(0))
#define DOS_SETDEVDATA      (DOS_PLAIN_SETDEVDATA|                   DOS_XINARGS(1))
#define DOS_RCVDATA         (DOS_PLAIN_RCVDATA   |DOS_XFER|DOS_RETAX|DOS_XINARGS(1))
#define DOS_SNDDATA         (DOS_PLAIN_SNDDATA   |DOS_XFER|DOS_RETAX|DOS_XINARGS(1))
#define DOS_RCVCTLDATA      (DOS_PLAIN_RCVCTLDATA|DOS_XFER|DOS_RETAX|DOS_XINARGS(1))
#define DOS_SNDCTLDATA      (DOS_PLAIN_SNDCTLDATA|DOS_XFER|DOS_RETAX|DOS_XINARGS(1))
#define DOS_CHKINSTAT       (DOS_PLAIN_CHKINSTAT |         DOS_RETAX)
#define DOS_CHKOUTSTAT      (DOS_PLAIN_CHKOUTSTAT|         DOS_RETAX)
#define DOS_ISCHANGEABLE    (DOS_PLAIN_ISCHANGEABLE|       DOS_RETAX)
#define DOS_ISREDIRBLK      (DOS_PLAIN_ISREDIRBLK|         DOS_RETDX)
#define DOS_ISREDIRHND      (DOS_PLAIN_ISREDIRHND|         DOS_RETDX)
#define DOS_SETRETRY        (DOS_PLAIN_SETRETRY|                     DOS_XINARGS(1))
/*
These ones do not fit into my scheme, because they _DO_NOT_ put the size
of the xfer buffer in CX. Aaaaargh
*/
#define DOS_GENCHARREQ      (DOS_PLAIN_GENCHARREQ|DOS_BRAINDEAD|DOS_RETDISI)
#define DOS_GENBLKREQ       (DOS_PLAIN_GENBLKREQ |DOS_BRAINDEAD|DOS_RETAX)
#define DOS_GLDRVMAP        (DOS_PLAIN_GLDRVMAP|           DOS_RETAX)
#define DOS_SLDRVMAP        (DOS_PLAIN_SLDRVMAP|           DOS_RETAX)
#define DOS_QGIOCTLCAPH     (DOS_PLAIN_QGIOCTLCAPH|        DOS_RETAX)
#define DOS_QGIOCTLCAPD     (DOS_PLAIN_QGIOCTLCAPD|        DOS_RETAX)


#define __IS_UNIX_IOCTL(a) ((a) & 0xd0000000U)
#if 0
/*
** UNIX stuff
**
** This is subject to major changes in the near future.
** Do not use it yet.
*/

/*
** __WARNING__ :
** This ifdef works for DJGPP, because its io.h
** defines __djgpp_include_io_h_
*/
#ifndef _IO
/*
* Ioctl's have the command encoded in the lower word,
* and the size of any in or out parameters in the upper
* word.  The high 2 bits of the upper word are used
* to encode the in/out status of the parameter; for now
* we restrict parameters to at most 128 bytes.
*/
#define IOCPARM_MASK    0x7f            /* parameters must be < 128 bytes */
#define IOC_VOID        0x20000000      /* no parameters */
#define IOC_OUT         0x40000000      /* copy out parameters */
#define IOC_IN          0x80000000      /* copy in parameters */
#define IOC_INOUT       (IOC_IN|IOC_OUT)
/* the 0x20000000 is so we can distinguish new ioctl's from old */
#define _IO(x,y)        (IOC_VOID|(x<<8)|y)
#define _IOR(x,y,t)     (IOC_OUT|((sizeof(t)&IOCPARM_MASK)<<16)|(x<<8)|y)
#define _IOW(x,y,t)     (IOC_IN|((sizeof(t)&IOCPARM_MASK)<<16)|(x<<8)|y)
/* this should be _IORW, but stdio got there first */
#define _IOWR(x,y,t)    (IOC_INOUT|((sizeof(t)&IOCPARM_MASK)<<16)|(x<<8)|y)
#endif /* _IO */
/* Common ioctl's for all disciplines which are handled in ttiocom */
enum tty_ioctl {
    TXISATTY = ('X'<<8),    /* quick path for isatty */
    TXTTYNAME,				/* quick path for ttyname */
    TXGETLD,				/* get line discipline */
    TXSETLD,				/* set line discipline */
    TXGETCD,				/* get control disciplines */
    TXADDCD,				/* add control discipline */
    TXDELCD,				/* delete control discipline */
    TXSBAUD,				/* set integer baud rate */
    TXGBAUD,				/* get integer baud rate */
    TXSETIHOG,				/* set the input buffer limit */
    TXSETOHOG,				/* set the output buffer limit */
    TXGPGRP,				/* get p grp with posix security */
    TXSPGRP                 /* set p grp with posix security */
};

#define TTNAMEMAX 32        /* used with TXGETLD, et al */

union txname {				/* used with TXGETCD */
    int tx_which;			/* which name to get -- inbound */
    char tx_name[TTNAMEMAX];/* the name -- outbound */
};

/* 
 * window size structure used with TXSETWIN and TXGETWIN.  This is 
 * exactly the same as the Berkeley structure and can be used with 
 * TIOCSWINSZ and TIOCGWINSZ -- in fact they are defined to be the 
 * same.
 */
struct winsize {
	unsigned short	ws_row;			/* rows, in characters */
	unsigned short	ws_col;			/* columns, in characters */
	unsigned short	ws_xpixel;		/* horizontal size, pixels */
	unsigned short	ws_ypixel;		/* vertical size, pixels */
};

struct tchars {
	char	t_intrc;	/* interrupt */
	char	t_quitc;	/* quit */
	char	t_startc;	/* start output */
	char	t_stopc;	/* stop output */
	char	t_eofc;		/* end-of-file */
	char	t_brkc;		/* input delimiter (like nl) */
};
struct ltchars {
	char	t_suspc;	/* stop process signal */
	char	t_dsuspc;	/* delayed stop process signal */
	char	t_rprntc;	/* reprint line */
	char	t_flushc;	/* flush output (toggles) */
	char	t_werasc;	/* word erase */
	char	t_lnextc;	/* literal next character */
};

/*
* Structure for TIOCGETP and TIOCSETP ioctls.
*/

struct sgttyb {
	char	sg_ispeed;		/* input speed */
	char	sg_ospeed;		/* output speed */
	char	sg_erase;		/* erase character */
	char	sg_kill;		/* kill character */
	short	sg_flags;		/* mode flags */
};

/*
 * Pun for SUN.
 */
struct ttysize {
	unsigned short	ts_lines;
	unsigned short	ts_cols;
	unsigned short	ts_xxx;
	unsigned short	ts_yyy;
};
#define	TIOCGSIZE	TIOCGWINSZ
#define	TIOCSSIZE	TIOCSWINSZ





#define TIOCGETD    _IOR('t', 0, int)       /* get line discipline */
#define TIOCSETD    _IOW('t', 1, int)       /* set line discipline */
#define TIOCHPCL    _IO('t', 2)             /* hang up on last close */
#define TIOCMODG    _IOR('t', 3, int)       /* get modem control state */
#define TIOCMODS    _IOW('t', 4, int)       /* set modem control state */
#define TIOCM_LE    0001                    /* line enable */
#define TIOCM_DTR   0002                    /* data terminal ready */
#define TIOCM_RTS   0004                    /* request to send */
#define TIOCM_ST    0010                    /* secondary transmit */
#define TIOCM_SR    0020                    /* secondary receive */
#define TIOCM_CTS   0040                    /* clear to send */
#define TIOCM_CAR   0100                    /* carrier detect */
#define TIOCM_CD    TIOCM_CAR
#define TIOCM_RNG   0200                    /* ring */
#define TIOCM_RI    TIOCM_RNG
#define TIOCM_DSR   0400                    /* data set ready */
#define TIOCGETP    _IOR('t', 8,struct sgttyb)      /* get parameters -- gtty */
#define TIOCSETP    _IOW('t', 9,struct sgttyb)      /* set parameters -- stty */
#define TIOCSETN    _IOW('t',10,struct sgttyb)  /* as above, but no flushtty */
#define TIOCEXCL    _IO('t', 13)                /* set exclusive use of tty */
#define TIOCNXCL    _IO('t', 14)                /* reset exclusive use of tty */
#define TIOCFLUSH   _IOW('t', 16, int)          /* flush buffers */
#define TIOCSETC    _IOW('t',17,struct tchars)  /* set special characters */
#define TIOCGETC    _IOR('t',18,struct tchars)  /* get special characters */
#define TANDEM      0x00000001                  /* send stopc on out q full */
#define CBREAK      0x00000002                  /* half-cooked mode */
#define LCASE       0x00000004                  /* simulate lower case */
#define ECHO        0x00000008                  /* echo input */
#define CRMOD       0x00000010                  /* map \r to \r\n on output */
#define RAW         0x00000020                  /* no i/o processing */
#define ODDP        0x00000040                  /* get/send odd parity */
#define EVENP       0x00000080                  /* get/send even parity */
#define ANYP        0x000000c0                  /* get any parity/send none */
#define CRDELAY     0x00000300                  /* \r delay */
#define CR0         0x00000000
#define CR1         0x00000100                  /* tn 300 */
#define CR2         0x00000200                  /* tty 37 */
#define CR3         0x00000300                  /* concept 100 */
#define TBDELAY     0x00000c00                  /* horizontal tab delay */
#define TAB0        0x00000000
#define TAB1        0x00000400                  /* tty 37 */
#define TAB2        0x00000800
#define XTABS       0x00000c00                  /* expand tabs on output */
#define BSDELAY     0x00001000                  /* \b delay */
#define BS0         0x00000000
#define BS1         0x00001000
#define VTDELAY     0x00002000                  /* vertical tab delay */
#define FF0         0x00000000
#define FF1         0x00002000                  /* tty 37 */
#define NLDELAY     0x0000c000                  /* \n delay */
#define NL0         0x00000000
#define NL1         0x00004000                  /* tty 37 */
#define NL2         0x00008000                  /* vt05 */
#define NL3         0x0000c000
#define ALLDELAY    (NLDELAY|TBDELAY|CRDELAY|VTDELAY|BSDELAY)
#define TOSTOP      0x00010000                  /* SIGSTOP on bckgnd output */
#define PRTERA      0x00020000                  /* \ ... / erase */
#define CRTERA      0x00040000                  /* " \b " to wipe out char */
#define TILDE       0x00080000                  /* hazeltine tilde kludge */
#define FLUSHO      0x00100000                  /* flush output to terminal */
#define LITOUT      0x00200000                  /* literal output */
#define CRTBS       0x00400000                  /* do backspacing for crt */
#define MDMBUF      0x00800000                  /* dtr pacing */
#define NOHANG      0x01000000                  /* no SIGHUP on carrier drop */
#define L001000     0x02000000
#define CRTKIL      0x04000000                  /* kill line with " \b " */
#define PASS8       0x08000000
#define CTLECH      0x10000000                  /* echo control chars as ^X */
#define PENDIN      0x20000000                  /* tp->t_rawq needs reread */
#define DECCTQ      0x40000000                  /* only ^Q starts after ^S */
#define NOFLUSH     0x80000000                  /* no output flush on signal */
#define TIOCCONS    _IOW('t', 98, int)          /* become virtual console */
#ifdef	_BSD_INCLUDES
/*
 * Added for 4.3 BSD.
 */
#define     NOFLSH      NOFLUSH                 /* no output flush on signal */
#endif	/* _BSD_INCLUDES */

						/* locals, from 127 down */
#define TIOCLBIS    _IOW('t', 127, int)              /* bis local mode bits */
#define TIOCLBIC    _IOW('t', 126, int)             /* bic local mode bits */
#define TIOCLSET    _IOW('t', 125, int)             /* set entire mode word */
#define TIOCLGET    _IOR('t', 124, int)             /* get local modes */
#define LCRTBS      (CRTBS>>16)
#define LPRTERA     (PRTERA>>16)
#define LCRTERA     (CRTERA>>16)
#define LTILDE      (TILDE>>16)
#define LMDMBUF     (MDMBUF>>16)
#define LLITOUT     (LITOUT>>16)
#define LTOSTOP     (TOSTOP>>16)
#define LFLUSHO     (FLUSHO>>16)
#define LNOHANG     (NOHANG>>16)
#define LCRTKIL     (CRTKIL>>16)
#define LPASS8      (PASS8>>16)
#define LCTLECH     (CTLECH>>16)
#define LPENDIN     (PENDIN>>16)
#define LDECCTQ     (DECCTQ>>16)
#define LNOFLSH     (NOFLUSH>>16)
#define TIOCSBRK    _IO('t', 123)                   /* set break bit */
#define TIOCCBRK    _IO('t', 122)                   /* clear break bit */
#define TIOCSDTR    _IO('t', 121)                   /* set data terminal ready */
#define TIOCCDTR    _IO('t', 120)                   /* clear data terminal ready */
#define TIOCGPGRP   _IOR('t', 119, int)             /* get process group */
#define TIOCSPGRP   _IOW('t', 118, int)             /* set process gorup */
#define TIOCSLTC    _IOW('t',117,struct ltchars)   /* set local special chars */
#define TIOCGLTC    _IOR('t',116,struct ltchars)   /* get local special chars */
#define TIOCOUTQ    _IOR('t', 115, int)             /* output queue size */
#define TIOCSTI     _IOW('t', 114, char)           /* simulate terminal input */
#define TIOCNOTTY   _IO('t', 113)                   /* void tty association */
#define TIOCPKT     _IOW('t', 112, int)         /* pty: set/clear packet mode */
#define TIOCPKT_DATA        0x00                    /* data packet */
#define TIOCPKT_FLUSHREAD   0x01                    /* flush packet */
#define TIOCPKT_FLUSHWRITE  0x02                    /* flush packet */
#define TIOCPKT_STOP        0x04                    /* stop output */
#define TIOCPKT_START       0x08                    /* start output */
#define TIOCPKT_NOSTOP      0x10                    /* no more ^S, ^Q */
#define TIOCPKT_DOSTOP      0x20                    /* now do ^S ^Q */
#define TIOCSTOP    _IO('t', 111)                   /* stop output, like ^S */
#define TIOCSTART   _IO('t', 110)                   /* start output, like ^Q */
#define TIOCMSET    _IOW('t', 109, int)             /* set all modem bits */
#define TIOCMBIS    _IOW('t', 108, int)             /* bis modem bits */
#define TIOCMBIC    _IOW('t', 107, int)             /* bic modem bits */
#define TIOCMGET    _IOR('t', 106, int)             /* get all modem bits */
#define TIOCREMOTE  _IOW('t', 105, int)             /* remote input editing */
#define TIOCGWINSZ  _IOR('t', 104, struct winsize)      /* get window size */
#define TIOCSWINSZ  _IOW('t', 103, struct winsize)      /* set window size */
#define TIOCUCNTL   _IOW('t', 102, int)         /* pty: set/clr usr cntl mode */
#define UIOCCMD(n)  _IO('u', n)                         /* usr cntl op "n" */

#define OTTYDISC    0                               /* old, v7 std tty driver */
#define NETLDISC    1                               /* line discip for berk net */
#define NTTYDISC    2                               /* new tty discipline */
#define TABLDISC    3                               /* tablet discipline */
#define SLIPDISC    4                               /* serial IP discipline */

#define FIOCLEX     _IO('f', 1)                     /* set exclusive use on fd */
#define FIONCLEX    _IO('f', 2)                     /* remove exclusive use */
/* another local */

#define FIONREAD    _IOR('f', 127, int)                 /* get # bytes to read */
#define FIONBIO     _IOW('f', 126, int)         /* set/clear non-blocking i/o */
#define FIOASYNC    _IOW('f', 125, int)                 /* set/clear async i/o */

#define FIOSETOWN   _IOW('f', 124, int)                 /* set owner */
#define FIOGETOWN   _IOR('f', 123, int)                 /* get owner */


#endif /* 0 */

int ioctl( int fd, int cmd, ...);


#endif  /* ! _POSIX_SOURCE */
#endif  /* ! __STRICT_ANSI__ */
#endif  /* ! __dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */


#ifdef __cplusplus
}
#endif

#endif  /* !__dj_include_sys_ioctl_h_ */

