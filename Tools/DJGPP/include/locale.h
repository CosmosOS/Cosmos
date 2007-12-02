/* Copyright (C) 1997 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_locale_h_
#define __dj_include_locale_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#define LC_ALL		0x1f
#define LC_COLLATE	0x01
#define LC_CTYPE	0x02
#define LC_MONETARY	0x04
#define LC_NUMERIC	0x08
#define LC_TIME		0x10
#define NULL		0

struct lconv {
  char *currency_symbol;
  char *decimal_point;
  char *grouping;
  char *int_curr_symbol;
  char *mon_decimal_point;
  char *mon_grouping;
  char *mon_thousands_sep;
  char *negative_sign;
  char *positive_sign;
  char *thousands_sep;
  char frac_digits;
  char int_frac_digits;
  char n_cs_precedes;
  char n_sep_by_space;
  char n_sign_posn;
  char p_cs_precedes;
  char p_sep_by_space;
  char p_sign_posn;
};

struct lconv *	localeconv(void);
char *		setlocale(int _category, const char *_locale);

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_locale_h_ */
