/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_Inline_pc_h_
#define __dj_include_Inline_pc_h_

#ifdef __cplusplus
extern "C" {
#endif

extern __inline__ unsigned char
inportb (unsigned short _port)
{
  unsigned char rv;
  __asm__ __volatile__ ("inb %1, %0"
	  : "=a" (rv)
	  : "dN" (_port));
  return rv;
}

extern __inline__ unsigned short
inportw (unsigned short _port)
{
  unsigned short rv;
  __asm__ __volatile__ ("inw %1, %0"
	  : "=a" (rv)
	  : "dN" (_port));
  return rv;
}

extern __inline__ unsigned long
inportl (unsigned short _port)
{
  unsigned long rv;
  __asm__ __volatile__ ("inl %1, %0"
	  : "=a" (rv)
	  : "dN" (_port));
  return rv;
}

extern __inline__ void
outportb (unsigned short _port, unsigned char _data)
{
  __asm__ __volatile__ ("outb %1, %0"
	  :
	  : "dN" (_port),
	    "a" (_data));
}

extern __inline__ void
outportw (unsigned short _port, unsigned short _data)
{
  __asm__ __volatile__ ("outw %1, %0"
	  :
	  : "dN" (_port),
	    "a" (_data));
}

extern __inline__ void
outportl (unsigned short _port, unsigned long _data)
{
  __asm__ __volatile__ ("outl %1, %0"
	  :
	  : "dN" (_port),
	    "a" (_data));
}

#ifdef __cplusplus
}
#endif

#endif
