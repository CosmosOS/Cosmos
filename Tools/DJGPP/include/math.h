/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_math_h_
#define __dj_include_math_h_

#ifdef _USE_LIBM_MATH_H
#include <libm/math.h>
#else

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

extern double __dj_huge_val;
#define HUGE_VAL  __dj_huge_val

double	acos(double _x);
double	asin(double _x);
double	atan(double _x);
double	atan2(double _y, double _x);
double	ceil(double _x);
double	cos(double _x);
double	cosh(double _x);
double	exp(double _x);
double	fabs(double _x);
double	floor(double _x);
double	fmod(double _x, double _y);
double	frexp(double _x, int *_pexp);
double	ldexp(double _x, int _exp);
double	log(double _y);
double	log10(double _x);
double	modf(double _x, double *_pint);
double	pow(double _x, double _y);
double	sin(double _x);
double	sinh(double _x);
double	sqrt(double _x);
double	tan(double _x);
double	tanh(double _x);
  
#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#define M_E		2.7182818284590452354
#define M_LOG2E		1.4426950408889634074
#define M_LOG10E	0.43429448190325182765
#define M_LN2		0.69314718055994530942
#define M_LN10		2.30258509299404568402
#define M_PI		3.14159265358979323846
#define M_PI_2		1.57079632679489661923
#define M_PI_4		0.78539816339744830962
#define M_1_PI		0.31830988618379067154
#define M_2_PI		0.63661977236758134308
#define M_2_SQRTPI	1.12837916709551257390
#define M_SQRT2		1.41421356237309504880
#define M_SQRT1_2	0.70710678118654752440
#define PI		M_PI
#define PI2		M_PI_2

double	acosh(double);
double	asinh(double);
double	atanh(double);
double	cbrt(double);
double	exp10(double _x);
double	exp2(double _x);
double	expm1(double);
double	hypot(double, double);
double	log1p(double);
double	log2(double _x);
long double modfl(long double _x, long double *_pint);
double	pow10(double _x);
double	pow2(double _x);
double	powi(double, int);
void	sincos(double *, double *, double);

/* These are in libm.a (Cygnus).  You must link -lm to get these */
/* See libm/math.h for comments */

#ifndef __cplusplus
struct exception {
	int type;
	const char *name;
	double arg1;
	double arg2;
	double retval;
	int err;
};
#endif

extern double erf(double);
extern double erfc(double);
extern double gamma(double);
extern int isinf(double);
extern int isnan(double);
extern int finite(double);
extern double j0(double);
extern double j1(double);
extern double jn(int, double);
extern double lgamma(double);
extern double nan(void);
extern double y0(double);
extern double y1(double);
extern double yn(int, double);
extern double logb(double);
extern double nextafter(double, double);
extern double remainder(double, double);
extern double scalb(double, double);
#ifndef __cplusplus
extern int matherr(struct exception *);
#endif
extern double significand(double);
extern double copysign(double, double);
extern int ilogb(double);
extern double rint(double);
extern double scalbn(double, int);
extern double drem(double, double);
extern double gamma_r(double, int *);
extern double lgamma_r(double, int *);
extern float acosf(float);
extern float asinf(float);
extern float atanf(float);
extern float atan2f(float, float);
extern float cosf(float);
extern float sinf(float);
extern float tanf(float);
extern float coshf(float);
extern float sinhf(float);
extern float tanhf(float);
extern float expf(float);
extern float frexpf(float, int *);
extern float ldexpf(float, int);
extern float logf(float);
extern float log10f(float);
extern float modff(float, float *);
extern float powf(float, float);
extern float sqrtf(float);
extern float ceilf(float);
extern float fabsf(float);
extern float floorf(float);
extern float fmodf(float, float);
extern float erff(float);
extern float erfcf(float);
extern float gammaf(float);
extern float hypotf(float, float);
extern int isinff(float);
extern int isnanf(float);
extern int finitef(float);
extern float j0f(float);
extern float j1f(float);
extern float jnf(int, float);
extern float lgammaf(float);
extern float nanf(void);
extern float y0f(float);
extern float y1f(float);
extern float ynf(int, float);
extern float acoshf(float);
extern float asinhf(float);
extern float atanhf(float);
extern float cbrtf(float);
extern float logbf(float);
extern float nextafterf(float, float);
extern float remainderf(float, float);
extern float scalbf(float, float);
extern float significandf(float);
extern float copysignf(float, float);
extern int ilogbf(float);
extern float rintf(float);
extern float scalbnf(float, int);
extern float dremf(float, float);
extern float expm1f(float);
extern float log1pf(float);
extern float gammaf_r(float, int *);
extern float lgammaf_r(float, int *);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* _USE_LIBM_MATH_H */

#endif /* !__dj_include_math_h_ */
