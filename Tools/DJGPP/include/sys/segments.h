/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_sys_segments_h_
#define __dj_include_sys_segments_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

static __inline__ int
_my_cs(void)
{
  unsigned short result;
  __asm__("movw %%cs,%0" : "=r" (result));
  return result;
}

static __inline__ int
_my_ds(void)
{
  unsigned short result;
  __asm__("movw %%ds,%0" : "=r" (result));
  return result;
}

static __inline__ int
_my_ss(void)
{
  unsigned short result;
  __asm__("movw %%ss,%0" : "=r" (result));
  return result;
}

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_sys_segment_h_ */
