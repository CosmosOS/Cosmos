/* Copyright (C) 1998 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_go32_h_
#define __dj_include_go32_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

#include <sys/version.h>
#include <sys/djtypes.h>

__DJ_size_t
#undef __DJ_size_t
#define __DJ_size_t

typedef struct {
  unsigned long  size_of_this_structure_in_bytes;
  unsigned long  linear_address_of_primary_screen;
  unsigned long  linear_address_of_secondary_screen;
  unsigned long  linear_address_of_transfer_buffer;
  unsigned long  size_of_transfer_buffer; /* >= 4k */
  unsigned long  pid;
  unsigned char  master_interrupt_controller_base;
  unsigned char  slave_interrupt_controller_base;
  unsigned short selector_for_linear_memory;
  unsigned long  linear_address_of_stub_info_structure;
  unsigned long  linear_address_of_original_psp;
  unsigned short run_mode;
  unsigned short run_mode_info;
} __Go32_Info_Block;

extern __Go32_Info_Block _go32_info_block;

#define _GO32_RUN_MODE_UNDEF	0
#define _GO32_RUN_MODE_RAW	1
#define _GO32_RUN_MODE_XMS	2
#define _GO32_RUN_MODE_VCPI	3
#define _GO32_RUN_MODE_DPMI	4

#include <sys/movedata.h>
#include <sys/segments.h>

#define _go32_my_cs _my_cs
#define _go32_my_ds _my_ds
#define _go32_my_ss _my_ss
#define _go32_conventional_mem_selector() _go32_info_block.selector_for_linear_memory
#define _dos_ds _go32_info_block.selector_for_linear_memory

#define __tb _go32_info_block.linear_address_of_transfer_buffer

/* returns number of times hit since last call. (zero first time) */
unsigned _go32_was_ctrl_break_hit(void);
void     _go32_want_ctrl_break(int yes); /* auto-yes if call above function */

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_go32_h_ */
