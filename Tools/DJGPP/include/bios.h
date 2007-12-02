/* Copyright (C) 1996 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_bios_h_
#define __dj_include_bios_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

int  bioscom(int _cmd, char _data, int _port);
int  biosdisk(int _cmd, int _drive, int _head, int _track, int _sector,
	      int _nsects, void *_buffer);
int  biosequip(void);
int  bioskey(int cmd);
int  biosmemory(void);
int  biosprint(int _cmd, int _byte, int _port);
long biostime(int _cmd, long _newtime);

/*
 *  For compatibility with other DOS C compilers.
 */

/* Disk parameters for _bios_disk() function. */
struct _diskinfo_t {
  unsigned drive;       /* Drive number. */
  unsigned head;        /* Head number. */
  unsigned track;       /* Track number. */
  unsigned sector;      /* Sector number. */
  unsigned nsectors;    /* Number of sectors to read/write/verify. */
  void    *buffer;      /* Buffer for reading/writing/verifying. */
};
#define diskinfo_t _diskinfo_t

/* Constants for _bios_disk() function. */
#define _DISK_RESET        0     /* Reset disk controller. */
#define _DISK_STATUS       1     /* Get disk status. */
#define _DISK_READ         2     /* Read disk sectors. */
#define _DISK_WRITE        3     /* Write disk sectors. */
#define _DISK_VERIFY       4     /* Verify disk sectors. */
#define _DISK_FORMAT       5     /* Format disk track. */

/* Constants fot _bios_serialcom() function. */
#define _COM_INIT          0     /* Init serial port. */
#define _COM_SEND          1     /* Send character. */
#define _COM_RECEIVE       2     /* Receive character. */
#define _COM_STATUS        3     /* Get serial port status. */

#define _COM_CHR7          2     /* 7 bits characters. */
#define _COM_CHR8          3     /* 8 bits characters. */

#define _COM_STOP1         0     /* 1 stop bit. */
#define _COM_STOP2         4     /* 2 stop bits. */

#define _COM_NOPARITY      0     /* No parity. */
#define _COM_ODDPARITY     8     /* Odd parity. */
#define _COM_SPACEPARITY   16    /* Space parity. */
#define _COM_EVENPARITY    24    /* Even parity. */

#define _COM_110           0     /* 110 baud. */
#define _COM_150           32    /* 150 baud. */
#define _COM_300           64    /* 300 baud. */
#define _COM_600           96    /* 600 baud. */
#define _COM_1200          128   /* 1200 baud. */
#define _COM_2400          160   /* 2400 baud. */
#define _COM_4800          192   /* 4800 baud. */
#define _COM_9600          224   /* 9600 baud. */

/* Constants for _bios_keybrd() function. */
#define _KEYBRD_READ          0     /* Read character. */
#define _KEYBRD_READY         1     /* Check character. */
#define _KEYBRD_SHIFTSTATUS   2     /* Get shift status. */

#define _NKEYBRD_READ         0x10  /* Read extended character. */
#define _NKEYBRD_READY        0x11  /* Check extended character. */
#define _NKEYBRD_SHIFTSTATUS  0x12  /* Get exteded shift status. */

/* Constans for _bios_printer() function. */
#define _PRINTER_WRITE     0     /* Write character. */
#define _PRINTER_INIT      1     /* Initialize printer. */
#define _PRINTER_STATUS    2     /* Get printer status. */

/* Constants for _bios_timeofday() function. */
#define _TIME_GETCLOCK     0     /* Get current clock count. */
#define _TIME_SETCLOCK     1     /* Set current clock count. */

#define _bios_equiplist()           ((unsigned)biosequip())
#define _bios_memsize()             ((unsigned)biosmemory())
#define _bios_printer(_c, _p, _d)   ((unsigned)biosprint(_c, _d, _p))
#define _bios_serialcom(_c, _p, _d) ((unsigned)bioscom(_c, _d, _p))
#define _bios_keybrd(_c)            ((unsigned)bioskey(_c))

unsigned _bios_disk(unsigned _cmd, struct _diskinfo_t *_di);
unsigned _bios_timeofday(unsigned _cmd, unsigned long *_timeval);

/* For int86(), int86x() and union REGS. */
#include <dos.h>

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_bios_h_ */
