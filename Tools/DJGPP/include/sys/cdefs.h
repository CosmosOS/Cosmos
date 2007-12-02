/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1994 DJ Delorie, see COPYING.DJ for details */
#undef __P
#if defined(__STDC__) || defined(__cplusplus)
#define __P(p) p
#else
#define __P(p)
#endif
#define	_PTR		void *
#define	_AND		,
#define	_NOARGS		void
#define	_CONST		const
#define	_VOLATILE	volatile
#define	_SIGNED		signed
#define	_DOTS		, ...
#define	_VOID void
#define	_EXFUN(name, proto)		name proto
#define	_DEFUN(name, arglist, args)	name(args)
#define	_DEFUN_VOID(name)		name(_NOARGS)
#define	_CAST_VOID (void)
#ifndef	_LONG_DOUBLE
#define	_LONG_DOUBLE long double
#endif
#ifndef	_PARAMS
#define	_PARAMS(paramlist)		paramlist
#endif

/* Support gcc's __attribute__ facility.  */

#define _ATTRIBUTE(attrs) __attribute__ ((attrs))

#if defined(__cplusplus)
#define __BEGIN_DECLS	extern "C" {
#define __END_DECLS	}
#else
#define __BEGIN_DECLS
#define __END_DECLS
#endif
