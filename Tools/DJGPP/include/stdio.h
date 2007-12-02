/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */

#ifndef __dj_include_stdio_h_
#define __dj_include_stdio_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#include <sys/version.h>
#include <sys/djtypes.h>
  
#define _IOFBF    	00001
#define _IONBF    	00002
#define _IOLBF    	00004

/* Some programs think they know better... */
#undef NULL

#define BUFSIZ		16384
#define EOF		(-1)
#define FILENAME_MAX	260
#define FOPEN_MAX	20
#define L_tmpnam	260
#define NULL		0
#define TMP_MAX		999999

#define SEEK_SET	0
#define SEEK_CUR	1
#define SEEK_END	2

__DJ_va_list
#undef __DJ_va_list
#define __DJ_va_list
__DJ_size_t
#undef __DJ_size_t
#define __DJ_size_t

/* Note that the definitions of these fields are NOT guaranteed!  They
   may change with any release without notice!  The fact that they
   are here at all is to comply with ANSI specifictions. */
   
typedef struct {
  int   _cnt;
  char *_ptr;
  char *_base;
  int   _bufsiz;
  int   _flag;
  int   _file;
  char *_name_to_remove;
  int   _fillsize;
} FILE;

typedef unsigned long		fpos_t;

extern FILE __dj_stdin, __dj_stdout, __dj_stderr;
#define stdin	(&__dj_stdin)
#define stdout	(&__dj_stdout)
#define stderr	(&__dj_stderr)

void	clearerr(FILE *_stream);
int	fclose(FILE *_stream);
int	feof(FILE *_stream);
int	ferror(FILE *_stream);
int	fflush(FILE *_stream);
int	fgetc(FILE *_stream);
int	fgetpos(FILE *_stream, fpos_t *_pos);
char *	fgets(char *_s, int _n, FILE *_stream);
FILE *	fopen(const char *_filename, const char *_mode);
int	fprintf(FILE *_stream, const char *_format, ...);
int	fputc(int _c, FILE *_stream);
int	fputs(const char *_s, FILE *_stream);
size_t	fread(void *_ptr, size_t _size, size_t _nelem, FILE *_stream);
FILE *	freopen(const char *_filename, const char *_mode, FILE *_stream);
int	fscanf(FILE *_stream, const char *_format, ...);
int	fseek(FILE *_stream, long _offset, int _mode);
int	fsetpos(FILE *_stream, const fpos_t *_pos);
long	ftell(FILE *_stream);
size_t	fwrite(const void *_ptr, size_t _size, size_t _nelem, FILE *_stream);
int	getc(FILE *_stream);
int	getchar(void);
char *	gets(char *_s);
void	perror(const char *_s);
int	printf(const char *_format, ...);
int	putc(int _c, FILE *_stream);
int	putchar(int _c);
int	puts(const char *_s);
int	remove(const char *_filename);
int	rename(const char *_old, const char *_new);
void	rewind(FILE *_stream);
int	scanf(const char *_format, ...);
void	setbuf(FILE *_stream, char *_buf);
int	setvbuf(FILE *_stream, char *_buf, int _mode, size_t _size);
int	sprintf(char *_s, const char *_format, ...);
int	sscanf(const char *_s, const char *_format, ...);
FILE *	tmpfile(void);
char *	tmpnam(char *_s);
int	ungetc(int _c, FILE *_stream);
int	vfprintf(FILE *_stream, const char *_format, va_list _ap);
int	vprintf(const char *_format, va_list _ap);
int	vsprintf(char *_s, const char *_format, va_list _ap);

#ifndef __STRICT_ANSI__

#define L_ctermid
#define L_cusrid
/* #define STREAM_MAX	20 - DOS can change this */

int	fileno(FILE *_stream);
FILE *	fdopen(int _fildes, const char *_type);
int	pclose(FILE *_pf);
FILE *	popen(const char *_command, const char *_mode);

#ifndef _POSIX_SOURCE

extern FILE __dj_stdprn, __dj_stdaux;
#define stdprn	(&__dj_stdprn)
#define stdaux	(&__dj_stdaux)

#define P_tmpdir "c:/"

void	_djstat_describe_lossage(FILE *_to_where);
int	_doprnt(const char *_fmt, va_list _args, FILE *_f);
int	_doscan(FILE *_f, const char *_fmt, void **_argp);
int	_doscan_low(FILE *, int (*)(FILE *_get), int (*_unget)(int, FILE *), const char *_fmt, void **_argp);
int	fpurge(FILE *_f);
int	getw(FILE *_f);
int	mkstemp(char *_template);
char *	mktemp(char *_template);
int	putw(int _v, FILE *_f);
void	setbuffer(FILE *_f, void *_buf, int _size);
void	setlinebuf(FILE *_f);
char *	tempnam(const char *_dir, const char *_prefix);
int	_rename(const char *_old, const char *_new);	/* Simple (no directory) */
int	vfscanf(FILE *_stream, const char *_format, va_list _ap);
int	vscanf(const char *_format, va_list _ap);
int	vsscanf(const char *_s, const char *_format, va_list _ap);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_stdio_h_ */
