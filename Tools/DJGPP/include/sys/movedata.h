/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_movedata_h_
#define __dj_include_sys_movedata_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#include <sys/djtypes.h>

__DJ_size_t
#undef __DJ_size_t
#define __DJ_size_t

/* This header is intended to be included only by other
   headers, like <go32.h> and <string.h>.  You may
   include this directly, but it will be non-portable. */

/* These lengths are in bytes, optimized for speed */
void dosmemget(unsigned long _offset, size_t _length, void *_buffer);
void dosmemput(const void *_buffer, size_t _length, unsigned long _offset);

/* The lengths here are in TRANSFERS, not bytes! */
void _dosmemgetb(unsigned long _offset, size_t _xfers, void *_buffer);
void _dosmemgetw(unsigned long _offset, size_t _xfers, void *_buffer);
void _dosmemgetl(unsigned long _offset, size_t _xfers, void *_buffer);
void _dosmemputb(const void *_buffer, size_t _xfers, unsigned long _offset);
void _dosmemputw(const void *_buffer, size_t _xfers, unsigned long _offset);
void _dosmemputl(const void *_buffer, size_t _xfers, unsigned long _offset);


/* This length is in bytes, optimized for speed */
void movedata(unsigned _source_selector, unsigned _source_offset,
	       unsigned _dest_selector, unsigned _dest_offset,
	       size_t _length);

/* The lengths here are in TRANSFERS, not bytes! */
void _movedatab(unsigned, unsigned, unsigned, unsigned, size_t);
void _movedataw(unsigned, unsigned, unsigned, unsigned, size_t);
void _movedatal(unsigned, unsigned, unsigned, unsigned, size_t);
  
#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_movedata_h_ */
