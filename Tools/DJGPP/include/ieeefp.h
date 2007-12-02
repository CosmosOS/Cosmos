/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_ieeefp_h_
#define __dj_include_ieeefp_h_

#ifdef __cplusplus
extern "C" {
#endif

#include <sys/cdefs.h>

/* #include <machine/ieeefp.h> */
#include <machine/endian.h>
#if BYTE_ORDER == LITTLE_ENDIAN
# define __IEEE_LITTLE_ENDIAN
#endif

/* FLOATING ROUNDING */

typedef int fp_rnd;
#define FP_RN 0 	/* Round to nearest 		*/
#define FP_RM 1		/* Round down 			*/
#define FP_RP 2		/* Round up 			*/
#define FP_RZ 3		/* Round to zero (truncate) 	*/

fp_rnd _EXFUN(fpgetround,(void));
fp_rnd _EXFUN(fpsetround, (fp_rnd));

/* EXCEPTIONS */

typedef int fp_except;
#define FP_X_INV 0x10	/* Invalid operation 		*/
#define FP_X_DX  0x80	/* Divide by zero		*/
#define FP_X_OFL 0x04	/* Overflow exception		*/
#define FP_X_UFL 0x02	/* Underflow exception		*/
#define FP_X_IMP 0x01	/* Imprecise exception		*/

fp_except _EXFUN(fpgetmask,(void));
fp_except _EXFUN(fpsetmask,(fp_except));
fp_except _EXFUN(fpgetsticky,(void));
fp_except _EXFUN(fpsetsticky, (fp_except));

/* INTEGER ROUNDING */

typedef int fp_rdi;
#define FP_RDI_TOZ 0	/* Round to Zero 		*/
#define FP_RDI_RD  1	/* Follow float mode		*/

fp_rdi _EXFUN(fpgetroundtoi,(void));
fp_rdi _EXFUN(fpsetroundtoi,(fp_rdi));

int _EXFUN(isnan, (double));
int _EXFUN(isinf, (double));
int _EXFUN(finite, (double));

int _EXFUN(isnanf, (float));
int _EXFUN(isinff, (float));
int _EXFUN(finitef, (float));

#define __IEEE_DBL_EXPBIAS 1023
#define __IEEE_FLT_EXPBIAS 127

#define __IEEE_DBL_EXPLEN 11
#define __IEEE_FLT_EXPLEN 8


#define __IEEE_DBL_FRACLEN (64 - (__IEEE_DBL_EXPLEN + 1))
#define __IEEE_FLT_FRACLEN (32 - (__IEEE_FLT_EXPLEN + 1))

#define __IEEE_DBL_MAXPOWTWO	((double)(1L << 32 - 2) * (1L << (32-11) - 32 + 1))
#define __IEEE_FLT_MAXPOWTWO	((float)(1L << (32-8) - 1))

#define __IEEE_DBL_NAN_EXP 0x7ff
#define __IEEE_FLT_NAN_EXP 0xff


#define isnanf(x) (((*(long *)&(x) & 0x7f800000L)==0x7f800000L) && \
		   ((*(long *)&(x) & 0x007fffffL)!=0000000000L))

#define isinff(x) (((*(long *)&(x) & 0x7f800000L)==0x7f800000L) && \
		   ((*(long *)&(x) & 0x007fffffL)==0000000000L))

#define finitef(x) (((*(long *)&(x) & 0x7f800000L)!=0x7f800000L))

#ifdef __cplusplus
}
#endif

#endif /* __dj_include_ieeefp_h_ */
