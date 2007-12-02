/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_dos_h_
#define __dj_include_dos_h_

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#include <pc.h>

extern int _8087;

int _detect_80387(void);

struct DWORDREGS {
  unsigned long edi;
  unsigned long esi;
  unsigned long ebp;
  unsigned long cflag;
  unsigned long ebx;
  unsigned long edx;
  unsigned long ecx;
  unsigned long eax;
  unsigned short eflags;
};

struct DWORDREGS_W {
  unsigned long di;
  unsigned long si;
  unsigned long bp;
  unsigned long cflag;
  unsigned long bx;
  unsigned long dx;
  unsigned long cx;
  unsigned long ax;
  unsigned short flags;
};

struct WORDREGS {
  unsigned short di, _upper_di;
  unsigned short si, _upper_si;
  unsigned short bp, _upper_bp;
  unsigned short cflag, _upper_cflag;
  unsigned short bx, _upper_bx;
  unsigned short dx, _upper_dx;
  unsigned short cx, _upper_cx;
  unsigned short ax, _upper_ax;
  unsigned short flags;
};

struct BYTEREGS {
  unsigned short di, _upper_di;
  unsigned short si, _upper_si;
  unsigned short bp, _upper_bp;
  unsigned long cflag;
  unsigned char bl;
  unsigned char bh;
  unsigned short _upper_bx;
  unsigned char dl;
  unsigned char dh;
  unsigned short _upper_dx;
  unsigned char cl;
  unsigned char ch;
  unsigned short _upper_cx;
  unsigned char al;
  unsigned char ah;
  unsigned short _upper_ax;
  unsigned short flags;
};

union REGS {		/* Compatible with DPMI structure, except cflag */
  struct DWORDREGS d;
#ifdef _NAIVE_DOS_REGS
  struct WORDREGS x;
#else
#ifdef _BORLAND_DOS_REGS
  struct DWORDREGS x;
#else
  struct DWORDREGS_W x;
#endif
#endif
  struct WORDREGS w;
  struct BYTEREGS h;
};

struct SREGS {
  unsigned short es;
  unsigned short ds;
  unsigned short fs;
  unsigned short gs;
  unsigned short cs;
  unsigned short ss;
};

struct ftime {
  unsigned ft_tsec:5;	/* 0-29, double to get real seconds */
  unsigned ft_min:6;	/* 0-59 */
  unsigned ft_hour:5;	/* 0-23 */
  unsigned ft_day:5;	/* 1-31 */
  unsigned ft_month:4;	/* 1-12 */
  unsigned ft_year:7;	/* since 1980 */
};

struct date {
  short da_year;
  char  da_day;
  char  da_mon;
};

struct time {
  unsigned char ti_min;
  unsigned char ti_hour;
  unsigned char ti_hund;
  unsigned char ti_sec;
};

struct dfree {
  unsigned df_avail;
  unsigned df_total;
  unsigned df_bsec;
  unsigned df_sclus;
};

#ifdef __cplusplus
extern "C" {
#endif

extern unsigned short   _osmajor, _osminor;
extern const    char  * _os_flavor;
extern int		_doserrno;

unsigned short _get_dos_version(int);


int int86(int ivec, union REGS *in, union REGS *out);
int int86x(int ivec, union REGS *in, union REGS *out, struct SREGS *seg);
int intdos(union REGS *in, union REGS *out);
int intdosx(union REGS *in, union REGS *out, struct SREGS *seg);
int bdos(int func, unsigned dx, unsigned al);
int bdosptr(int func, void *dx, unsigned al);

#define bdosptr(a, b, c) bdos(a, (unsigned)(b), c)
#define intdos(a, b) int86(0x21, a, b)
#define intdosx(a, b, c) int86x(0x21, a, b, c)

int enable(void);
int disable(void);

int getftime(int handle, struct ftime *ftimep);
int setftime(int handle, struct ftime *ftimep);

int getcbrk(void);
int setcbrk(int new_value);

void getdate(struct date *);
void gettime(struct time *);
void setdate(struct date *);
void settime(struct time *);

void getdfree(unsigned char drive, struct dfree *ptr);

void delay(unsigned msec);
/* int _get_default_drive(void);
void _fixpath(const char *, char *); */


/*
 *  For compatibility with other DOS C compilers.
 */

#define _A_NORMAL   0x00    /* Normal file - No read/write restrictions */
#define _A_RDONLY   0x01    /* Read only file */
#define _A_HIDDEN   0x02    /* Hidden file */
#define _A_SYSTEM   0x04    /* System file */
#define _A_VOLID    0x08    /* Volume ID file */
#define _A_SUBDIR   0x10    /* Subdirectory */
#define _A_ARCH     0x20    /* Archive file */

#define _enable   enable
#define _disable  disable

struct _dosdate_t {
  unsigned char  day;       /* 1-31 */
  unsigned char  month;     /* 1-12 */
  unsigned short year;      /* 1980-2099 */
  unsigned char  dayofweek; /* 0-6, 0=Sunday */
};
#define dosdate_t _dosdate_t

struct _dostime_t {
  unsigned char hour;     /* 0-23 */
  unsigned char minute;   /* 0-59 */
  unsigned char second;   /* 0-59 */
  unsigned char hsecond;  /* 0-99 */
};
#define dostime_t _dostime_t

struct _find_t {
  char reserved[21] __attribute__((packed));
  unsigned char attrib __attribute__((packed));
  unsigned short wr_time __attribute__((packed));
  unsigned short wr_date __attribute__((packed));
  unsigned long size __attribute__((packed));
  char name[256] __attribute__((packed));
};
#define find_t _find_t

struct _diskfree_t {
  unsigned short total_clusters;
  unsigned short avail_clusters;
  unsigned short sectors_per_cluster;
  unsigned short bytes_per_sector;
};
#define diskfree_t _diskfree_t

struct _DOSERROR {
  int  exterror;
  #ifdef __cplusplus
  char errclass;
  #else
  char class;
  #endif
  char action;
  char locus;
};
#define DOSERROR _DOSERROR

unsigned int   _dos_creat(const char *_filename, unsigned int _attr, int *_handle);
unsigned int   _dos_creatnew(const char *_filename, unsigned int _attr, int *_handle);
unsigned int   _dos_open(const char *_filename, unsigned int _mode, int *_handle);
unsigned int   _dos_write(int _handle, const void *_buffer, unsigned int _count, unsigned int *_result);
unsigned int   _dos_read(int _handle, void *_buffer, unsigned int _count, unsigned int *_result);
unsigned int   _dos_close(int _handle);
unsigned int   _dos_commit(int _handle);

unsigned int   _dos_findfirst(const char *_name, unsigned int _attr, struct _find_t *_result);
unsigned int   _dos_findnext(struct _find_t *_result);

void           _dos_getdate(struct _dosdate_t *_date);
unsigned int   _dos_setdate(struct _dosdate_t *_date);
void           _dos_gettime(struct _dostime_t *_time);
unsigned int   _dos_settime(struct _dostime_t *_time);

unsigned int   _dos_getftime(int _handle, unsigned int *_p_date, unsigned int *_p_time);
unsigned int   _dos_setftime(int _handle, unsigned int _date, unsigned int _time);
unsigned int   _dos_getfileattr(const char *_filename, unsigned int *_p_attr);
unsigned int   _dos_setfileattr(const char *_filename, unsigned int _attr);

void           _dos_getdrive(unsigned int *_p_drive);
void           _dos_setdrive(unsigned int _drive, unsigned int *_p_drives);
unsigned int   _dos_getdiskfree(unsigned int _drive, struct _diskfree_t *_diskspace);

int            _dosexterr(struct _DOSERROR *_p_error);
#define dosexterr(_ep) _dosexterr(_ep)

#define int386(_i, _ir, _or)         int86(_i, _ir, _or)
#define int386x(_i, _ir, _or, _sr)   int86x(_i, _ir, _or, _sr)

#ifdef __cplusplus
}
#endif

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#endif /* !__dj_include_dos_h_ */
