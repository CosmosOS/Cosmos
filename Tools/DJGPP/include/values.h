/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_values_h_
#define __dj_include_values_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#define BITSPERBYTE 8

#define CHARBITS    8
#define SHORTBITS   16
#define INTBITS     32
#define LONGBITS    32
#define PTRBITS     32
#define DOUBLEBITS  64
#define FLOATBITS   32

#define MINSHORT    ((short)0x8000)
#define MININT      ((int)0x80000000L)
#define MINLONG     ((long)0x80000000L)

#define MAXSHORT    ((short)0x7fff)
#define MAXINT      ((int)0x7fffffff)
#define MAXLONG     ((long)0x7fffffff)

#define MAXDOUBLE   1.79769313486231570e+308
#define MAXFLOAT    ((float)3.40282346638528860e+38)
#define MINDOUBLE   2.22507385850720140e-308
#define MINFLOAT    ((float)1.17549435082228750e-38)
#define _IEEE       0
#define _DEXPLEN    11
#define _FEXPLEN    8
#define _HIDDENBIT  1
#define DMAXEXP     ((1 << _DEXPLEN - 1) - 1 + _IEEE)
#define FMAXEXP     ((1 << _FEXPLEN - 1) - 1 + _IEEE)
#define DMINEXP     (-DMAXEXP)
#define FMINEXP     (-FMAXEXP)

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_values_h_ */
