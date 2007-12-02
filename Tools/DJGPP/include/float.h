/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_float_h_
#define __dj_include_float_h_

#ifdef __cplusplus
extern "C" {
#endif

extern float __dj_float_epsilon;
extern float __dj_float_max;
extern float __dj_float_min;

#define FLT_DIG		6
#define FLT_EPSILON	__dj_float_epsilon
#define FLT_MANT_DIG	24
#define FLT_MAX		__dj_float_max
#define FLT_MAX_10_EXP	38
#define FLT_MAX_EXP	128
#define FLT_MIN		__dj_float_min
#define FLT_MIN_10_EXP	(-37)
#define FLT_MIN_EXP	(-125)
#define FLT_RADIX	2
#define FLT_ROUNDS	1

extern double __dj_double_epsilon;
extern double __dj_double_max;
extern double __dj_double_min;

#define DBL_DIG		15
#define DBL_EPSILON	__dj_double_epsilon
#define DBL_MANT_DIG	53
#define DBL_MAX		__dj_double_max
#define DBL_MAX_10_EXP	308
#define DBL_MAX_EXP	1024
#define DBL_MIN		__dj_double_min
#define DBL_MIN_10_EXP	(-307)
#define DBL_MIN_EXP	(-1021)

extern long double __dj_long_double_epsilon;
extern long double __dj_long_double_max;
extern long double __dj_long_double_min;

#define LDBL_DIG	18
#define LDBL_EPSILON	__dj_long_double_epsilon
#define LDBL_MANT_DIG	64
#define LDBL_MAX	__dj_long_double_max
#define LDBL_MAX_10_EXP	4932
#define LDBL_MAX_EXP	16384
#define LDBL_MIN	__dj_long_double_min
#define LDBL_MIN_10_EXP	(-4931)
#define LDBL_MIN_EXP	(-16381)

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* These indicate the results of the last operation */
#define SW_INVALID	0x0001	/* Invalid operation */
#define SW_DENORMAL	0x0002	/* Denormalized operand */
#define SW_ZERODIVIDE	0x0004	/* Division by zero */
#define SW_OVERFLOW	0x0008	/* Overflow */
#define SW_UNDERFLOW	0x0010	/* Underflow (computational) */
#define SW_INEXACT	0x0020	/* Precision (computational) */
#define SW_STACKFAULT	0x0040	/* Stack Fault (over/under flow) */
#define SW_ERRORSUMMARY	0x0080	/* Error summary */
#define SW_COND		0x4700	/* Condition Code */
#define SW_C0		0x0100	/* Condition 0 bit */
#define SW_C1		0x0200	/* Condition 1 bit (also 0=stack underflow, 1=stack overflow) */
#define SW_C2		0x0400	/* Condition 2 bit */
#define SW_C3		0x4000	/* Condition 3 bit */
#define SW_TOP		0x3800	/* Top of stack */
#define SW_TOP_SHIFT	11	/* Shift to move TOS to LSB */
#define SW_BUSY		0x8000	/* FPU busy */

#define MCW_EM		0x003f	/* Exception masks (0=fault, 1=handle) */
#define EM_INVALID	0x0001	/* Invalid operation */
#define EM_DENORMAL	0x0002	/* Denormalized operand */
#define EM_ZERODIVIDE	0x0004	/* Division by zero */
#define EM_OVERFLOW	0x0008	/* Overflow */
#define EM_UNDERFLOW	0x0010	/* Underflow */
#define EM_INEXACT	0x0020	/* Precision */

#define MCW_PC		0x0300	/* precision control */
#define PC_24		0x0000	/* 24 bits (single precision) */
#define PC_53		0x0200	/* 53 bits (double precision) */
#define PC_64		0x0300	/* 64 bits (extended precision) */

#define MCW_RC		0x0c00	/* Rounding control */
#define RC_NEAR		0x0000	/* Round to nearest or even */
#define RC_DOWN		0x0400	/* Round towards -Inf */
#define RC_UP		0x0800	/* Round towards +Inf */
#define RC_CHOP		0x0c00	/* Truncate towards zero */

#define MCW_IC		0x1000	/* obsolete; i486 is always affine */
#define IC_AFFINE	0x1000	/* -Inf < +Inf */
#define IC_PROJECTIVE	0x0000	/* -Inf == +Inf */

unsigned int _clear87(void);
unsigned int _control87(unsigned int newcw, unsigned int mask);
void         _fpreset(void);
unsigned int _status87(void);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_float_h_ */
