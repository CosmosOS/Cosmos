/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_debug_tss_h_
#define __dj_include_debug_tss_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

typedef struct TSS {
	unsigned short tss_back_link;
	unsigned short res0;
	unsigned long  tss_esp0;
	unsigned short tss_ss0;
	unsigned short res1;
	unsigned long  tss_esp1;
	unsigned short tss_ss1;
	unsigned short res2;
	unsigned long  tss_esp2;
	unsigned short tss_ss2;
	unsigned short res3;
	unsigned long  tss_cr3;

	unsigned long  tss_eip;
	unsigned long  tss_eflags;
	unsigned long  tss_eax;
	unsigned long  tss_ecx;
	unsigned long  tss_edx;
	unsigned long  tss_ebx;
	unsigned long  tss_esp;
	unsigned long  tss_ebp;
	unsigned long  tss_esi;
	unsigned long  tss_edi;
	unsigned short tss_es;
	unsigned short res4;
	unsigned short tss_cs;
	unsigned short res5;
	unsigned short tss_ss;
	unsigned short res6;
	unsigned short tss_ds;
	unsigned short res7;
	unsigned short tss_fs;
	unsigned short res8;
	unsigned short tss_gs;
	unsigned short res9;
	unsigned short tss_ldt;
	unsigned short res10;
	unsigned short tss_trap;
	unsigned char  tss_iomap;
	unsigned char  tss_irqn;
	unsigned long  tss_error;
} TSS;

extern TSS a_tss;

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_debug_tss_h_ */
