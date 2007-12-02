/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_dpmi_h_
#define __dj_include_dpmi_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

extern unsigned short __dpmi_error;

typedef struct {
  unsigned short offset16;
  unsigned short segment;
} __dpmi_raddr;

typedef struct {
  unsigned long  offset32;
  unsigned short selector;
} __dpmi_paddr;

typedef struct {
  unsigned long handle;			/* 0, 2 */
  unsigned long size; 	/* or count */	/* 4, 6 */
  unsigned long address;		/* 8, 10 */
} __dpmi_meminfo;

typedef union {
  struct {
    unsigned long edi;
    unsigned long esi;
    unsigned long ebp;
    unsigned long res;
    unsigned long ebx;
    unsigned long edx;
    unsigned long ecx;
    unsigned long eax;
  } d;
  struct {
    unsigned short di, di_hi;
    unsigned short si, si_hi;
    unsigned short bp, bp_hi;
    unsigned short res, res_hi;
    unsigned short bx, bx_hi;
    unsigned short dx, dx_hi;
    unsigned short cx, cx_hi;
    unsigned short ax, ax_hi;
    unsigned short flags;
    unsigned short es;
    unsigned short ds;
    unsigned short fs;
    unsigned short gs;
    unsigned short ip;
    unsigned short cs;
    unsigned short sp;
    unsigned short ss;
  } x;
  struct {
    unsigned char edi[4];
    unsigned char esi[4];
    unsigned char ebp[4];
    unsigned char res[4];
    unsigned char bl, bh, ebx_b2, ebx_b3;
    unsigned char dl, dh, edx_b2, edx_b3;
    unsigned char cl, ch, ecx_b2, ecx_b3;
    unsigned char al, ah, eax_b2, eax_b3;
  } h;
} __dpmi_regs;
  
typedef struct {
  unsigned char  major;
  unsigned char  minor;
  unsigned short flags;
  unsigned char  cpu;
  unsigned char  master_pic;
  unsigned char  slave_pic;
} __dpmi_version_ret;

typedef struct {
  unsigned long largest_available_free_block_in_bytes;
  unsigned long maximum_unlocked_page_allocation_in_pages;
  unsigned long maximum_locked_page_allocation_in_pages;
  unsigned long linear_address_space_size_in_pages;
  unsigned long total_number_of_unlocked_pages;
  unsigned long total_number_of_free_pages;
  unsigned long total_number_of_physical_pages;
  unsigned long free_linear_address_space_in_pages;
  unsigned long size_of_paging_file_partition_in_pages;
  unsigned long reserved[3];
} __dpmi_free_mem_info;

typedef struct {
  unsigned long total_allocated_bytes_of_physical_memory_host;
  unsigned long total_allocated_bytes_of_virtual_memory_host;
  unsigned long total_available_bytes_of_virtual_memory_host;
  unsigned long total_allocated_bytes_of_virtual_memory_vcpu;
  unsigned long total_available_bytes_of_virtual_memory_vcpu;
  unsigned long total_allocated_bytes_of_virtual_memory_client;
  unsigned long total_available_bytes_of_virtual_memory_client;
  unsigned long total_locked_bytes_of_memory_client;
  unsigned long max_locked_bytes_of_memory_client;
  unsigned long highest_linear_address_available_to_client;
  unsigned long size_in_bytes_of_largest_free_memory_block;
  unsigned long size_of_minimum_allocation_unit_in_bytes;
  unsigned long size_of_allocation_alignment_unit_in_bytes;
  unsigned long reserved[19];
} __dpmi_memory_info;

typedef struct {
  unsigned long data16[2];
  unsigned long code16[2];
  unsigned short ip;
  unsigned short reserved;
  unsigned long data32[2];
  unsigned long code32[2];
  unsigned long eip;
} __dpmi_callback_info;

typedef struct {
  unsigned long size_requested;
  unsigned long size;
  unsigned long handle;
  unsigned long address;
  unsigned long name_offset;
  unsigned short name_selector;
  unsigned short reserved1;
  unsigned long reserved2;
} __dpmi_shminfo;

/* Unless otherwise noted, all functions return -1 on error, setting __dpmi_error to the DPMI error code */

void	__dpmi_yield(void);									/* INT 0x2F AX=1680 */

int	__dpmi_allocate_ldt_descriptors(int _count);						/* DPMI 0.9 AX=0000 */
int	__dpmi_free_ldt_descriptor(int _descriptor);						/* DPMI 0.9 AX=0001 */
int	__dpmi_segment_to_descriptor(int _segment);						/* DPMI 0.9 AX=0002 */
int	__dpmi_get_selector_increment_value(void);						/* DPMI 0.9 AX=0003 */
int	__dpmi_get_segment_base_address(int _selector, unsigned long *_addr);			/* DPMI 0.9 AX=0006 */
int	__dpmi_set_segment_base_address(int _selector, unsigned long _address);			/* DPMI 0.9 AX=0007 */
unsigned long __dpmi_get_segment_limit(int _selector);						/* LSL instruction  */
int	__dpmi_set_segment_limit(int _selector, unsigned long _limit);				/* DPMI 0.9 AX=0008 */
int	__dpmi_get_descriptor_access_rights(int _selector);					/* LAR instruction  */
int	__dpmi_set_descriptor_access_rights(int _selector, int _rights);			/* DPMI 0.9 AX=0009 */
int	__dpmi_create_alias_descriptor(int _selector);						/* DPMI 0.9 AX=000a */
int	__dpmi_get_descriptor(int _selector, void *_buffer);					/* DPMI 0.9 AX=000b */
int	__dpmi_set_descriptor(int _selector, void *_buffer);					/* DPMI 0.9 AX=000c */
int	__dpmi_allocate_specific_ldt_descriptor(int _selector);					/* DPMI 0.9 AX=000d */

int	__dpmi_get_multiple_descriptors(int _count, void *_buffer);				/* DPMI 1.0 AX=000e */
int	__dpmi_set_multiple_descriptors(int _count, void *_buffer);				/* DPMI 1.0 AX=000f */

int	__dpmi_allocate_dos_memory(int _paragraphs, int *_ret_selector_or_max);			/* DPMI 0.9 AX=0100 */
int	__dpmi_free_dos_memory(int _selector);							/* DPMI 0.9 AX=0101 */
int	__dpmi_resize_dos_memory(int _selector, int _newpara, int *_ret_max);			/* DPMI 0.9 AX=0102 */

int	__dpmi_get_real_mode_interrupt_vector(int _vector, __dpmi_raddr *_address);		/* DPMI 0.9 AX=0200 */
int	__dpmi_set_real_mode_interrupt_vector(int _vector, __dpmi_raddr *_address);		/* DPMI 0.9 AX=0201 */
int	__dpmi_get_processor_exception_handler_vector(int _vector, __dpmi_paddr *_address);	/* DPMI 0.9 AX=0202 */
int	__dpmi_set_processor_exception_handler_vector(int _vector, __dpmi_paddr *_address);	/* DPMI 0.9 AX=0203 */
int	__dpmi_get_protected_mode_interrupt_vector(int _vector, __dpmi_paddr *_address);	/* DPMI 0.9 AX=0204 */
int	__dpmi_set_protected_mode_interrupt_vector(int _vector, __dpmi_paddr *_address);	/* DPMI 0.9 AX=0205 */

int	__dpmi_get_extended_exception_handler_vector_pm(int _vector, __dpmi_paddr *_address);	/* DPMI 1.0 AX=0210 */
int	__dpmi_get_extended_exception_handler_vector_rm(int _vector, __dpmi_paddr *_address);	/* DPMI 1.0 AX=0211 */
int	__dpmi_set_extended_exception_handler_vector_pm(int _vector, __dpmi_paddr *_address);	/* DPMI 1.0 AX=0212 */
int	__dpmi_set_extended_exception_handler_vector_rm(int _vector, __dpmi_paddr *_address);	/* DPMI 1.0 AX=0213 */

int	__dpmi_simulate_real_mode_interrupt(int _vector, __dpmi_regs *_regs);			/* DPMI 0.9 AX=0300 */
int	__dpmi_int(int _vector, __dpmi_regs *_regs); /* like above, but sets ss sp fl */	/* DPMI 0.9 AX=0300 */
extern short __dpmi_int_ss, __dpmi_int_sp, __dpmi_int_flags; /* default to zero */
int	__dpmi_simulate_real_mode_procedure_retf(__dpmi_regs *_regs);				/* DPMI 0.9 AX=0301 */
int	__dpmi_simulate_real_mode_procedure_retf_stack(__dpmi_regs *_regs, int stack_words_to_copy, const void *stack_data); /* DPMI 0.9 AX=0301 */
int	__dpmi_simulate_real_mode_procedure_iret(__dpmi_regs *_regs);				/* DPMI 0.9 AX=0302 */
int	__dpmi_allocate_real_mode_callback(void (*_handler)(void), __dpmi_regs *_regs, __dpmi_raddr *_ret); /* DPMI 0.9 AX=0303 */
int	__dpmi_free_real_mode_callback(__dpmi_raddr *_addr);					/* DPMI 0.9 AX=0304 */
int	__dpmi_get_state_save_restore_addr(__dpmi_raddr *_rm, __dpmi_paddr *_pm);		/* DPMI 0.9 AX=0305 */
int	__dpmi_get_raw_mode_switch_addr(__dpmi_raddr *_rm, __dpmi_paddr *_pm);			/* DPMI 0.9 AX=0306 */

int	__dpmi_get_version(__dpmi_version_ret *_ret);						/* DPMI 0.9 AX=0400 */

int	__dpmi_get_capabilities(int *_flags, char *vendor_info);				/* DPMI 1.0 AX=0401 */

int	__dpmi_get_free_memory_information(__dpmi_free_mem_info *_info);			/* DPMI 0.9 AX=0500 */
int	__dpmi_allocate_memory(__dpmi_meminfo *_info);						/* DPMI 0.9 AX=0501 */
int	__dpmi_free_memory(unsigned long _handle);						/* DPMI 0.9 AX=0502 */
int	__dpmi_resize_memory(__dpmi_meminfo *_info);						/* DPMI 0.9 AX=0503 */

int	__dpmi_allocate_linear_memory(__dpmi_meminfo *_info, int _commit);			/* DPMI 1.0 AX=0504 */
int	__dpmi_resize_linear_memory(__dpmi_meminfo *_info, int _commit);			/* DPMI 1.0 AX=0505 */
int	__dpmi_get_page_attributes(__dpmi_meminfo *_info, short *_buffer);			/* DPMI 1.0 AX=0506 */
int	__dpmi_set_page_attributes(__dpmi_meminfo *_info, short *_buffer);			/* DPMI 1.0 AX=0507 */
int	__dpmi_map_device_in_memory_block(__dpmi_meminfo *_info, unsigned long _physaddr);	/* DPMI 1.0 AX=0508 */
int	__dpmi_map_conventional_memory_in_memory_block(__dpmi_meminfo *_info, unsigned long _physaddr); /* DPMI 1.0 AX=0509 */
int	__dpmi_get_memory_block_size_and_base(__dpmi_meminfo *_info);				/* DPMI 1.0 AX=050a */
int	__dpmi_get_memory_information(__dpmi_memory_info *_buffer);				/* DPMI 1.0 AX=050b */

int	__dpmi_lock_linear_region(__dpmi_meminfo *_info);					/* DPMI 0.9 AX=0600 */
int	__dpmi_unlock_linear_region(__dpmi_meminfo *_info);					/* DPMI 0.9 AX=0601 */
int	__dpmi_mark_real_mode_region_as_pageable(__dpmi_meminfo *_info);			/* DPMI 0.9 AX=0602 */
int	__dpmi_relock_real_mode_region(__dpmi_meminfo *_info);					/* DPMI 0.9 AX=0603 */
int	__dpmi_get_page_size(unsigned long *_size);						/* DPMI 0.9 AX=0604 */

int	__dpmi_mark_page_as_demand_paging_candidate(__dpmi_meminfo *_info);			/* DPMI 0.9 AX=0702 */
int	__dpmi_discard_page_contents(__dpmi_meminfo *_info);					/* DPMI 0.9 AX=0703 */

int	__dpmi_physical_address_mapping(__dpmi_meminfo *_info);					/* DPMI 0.9 AX=0800 */
int	__dpmi_free_physical_address_mapping(__dpmi_meminfo *_info);				/* DPMI 0.9 AX=0801 */

/* These next four functions return the old state */
int	__dpmi_get_and_disable_virtual_interrupt_state(void);					/* DPMI 0.9 AX=0900 */
int	__dpmi_get_and_enable_virtual_interrupt_state(void);					/* DPMI 0.9 AX=0901 */
int	__dpmi_get_and_set_virtual_interrupt_state(int _old_state);				/* DPMI 0.9 AH=09   */
int	__dpmi_get_virtual_interrupt_state(void);						/* DPMI 0.9 AX=0902 */

int	__dpmi_get_vendor_specific_api_entry_point(char *_id, __dpmi_paddr *_api);		/* DPMI 0.9 AX=0a00 */

int	__dpmi_set_debug_watchpoint(__dpmi_meminfo *_info, int _type);				/* DPMI 0.9 AX=0b00 */
int	__dpmi_clear_debug_watchpoint(unsigned long _handle);					/* DPMI 0.9 AX=0b01 */
int	__dpmi_get_state_of_debug_watchpoint(unsigned long _handle, int *_status);		/* DPMI 0.9 AX=0b02 */
int	__dpmi_reset_debug_watchpoint(unsigned long _handle);					/* DPMI 0.9 AX=0b03 */

int	__dpmi_install_resident_service_provider_callback(__dpmi_callback_info *_info);		/* DPMI 1.0 AX=0c00 */
int	__dpmi_terminate_and_stay_resident(int return_code, int paragraphs_to_keep);		/* DPMI 1.0 AX=0c01 */

int	__dpmi_allocate_shared_memory(__dpmi_shminfo *_info);					/* DPMI 1.0 AX=0d00 */
int	__dpmi_free_shared_memory(unsigned long _handle);					/* DPMI 1.0 AX=0d01 */
int	__dpmi_serialize_on_shared_memory(unsigned long _handle, int _flags);			/* DPMI 1.0 AX=0d02 */
int	__dpmi_free_serialization_on_shared_memory(unsigned long _handle, int _flags);		/* DPMI 1.0 AX=0d03 */

int	__dpmi_get_coprocessor_status(void);							/* DPMI 1.0 AX=0e00 */
int	__dpmi_set_coprocessor_emulation(int _flags);						/* DPMI 1.0 AX=0e01 */

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
/* Backwards compatibility stuff						       */

#define _go32_dpmi_registers            __dpmi_regs

typedef struct {
  unsigned long available_memory;
  unsigned long available_pages;
  unsigned long available_lockable_pages;
  unsigned long linear_space;
  unsigned long unlocked_pages;
  unsigned long available_physical_pages;
  unsigned long total_physical_pages;
  unsigned long free_linear_space;
  unsigned long max_pages_in_paging_file;
  unsigned long reserved[3];
} _go32_dpmi_meminfo;

#define _go32_dpmi_get_free_memory_information(x) __dpmi_get_free_memory_information((__dpmi_free_mem_info *)(x))

#define _go32_dpmi_simulate_int		__dpmi_simulate_real_mode_interrupt
#define _go32_dpmi_simulate_fcall	__dpmi_simulate_real_mode_procedure_retf
#define _go32_dpmi_simulate_fcall_iret	__dpmi_simulate_real_mode_procedure_iret

typedef struct {
  unsigned long size;
  unsigned long pm_offset;
  unsigned short pm_selector;
  unsigned short rm_offset;
  unsigned short rm_segment;
} _go32_dpmi_seginfo;

/* returns zero if success, else dpmi error and info->size is max size */
int _go32_dpmi_allocate_dos_memory(_go32_dpmi_seginfo *info);
	/* set size to bytes/16, call, use rm_segment.  Do not
	   change anthing but size until the memory is freed.
	   If error, max size is returned in size as bytes/16. */
int _go32_dpmi_free_dos_memory(_go32_dpmi_seginfo *info);
	/* set new size to bytes/16, call.  If error, max size
	   is returned in size as bytes/16 */
int _go32_dpmi_resize_dos_memory(_go32_dpmi_seginfo *info);
	/* uses pm_selector to free memory */

/* These both use the rm_segment:rm_offset fields only */
int _go32_dpmi_get_real_mode_interrupt_vector(int vector, _go32_dpmi_seginfo *info);
int _go32_dpmi_set_real_mode_interrupt_vector(int vector, _go32_dpmi_seginfo *info);

/* These do NOT wrap the function in pm_offset in an iret handler.
   You must provide an assembler interface yourself, or alloc one below.
   You may NOT longjmp out of an interrupt handler. */
int _go32_dpmi_get_protected_mode_interrupt_vector(int vector, _go32_dpmi_seginfo *info);
	/* puts vector in pm_selector:pm_offset. */
int _go32_dpmi_set_protected_mode_interrupt_vector(int vector, _go32_dpmi_seginfo *info);
	/* sets vector from pm_offset and pm_selector */

/********** HELPER FUNCTIONS **********/

int _go32_dpmi_chain_protected_mode_interrupt_vector(int vector, _go32_dpmi_seginfo *info);
	/* sets up wrapper that calls function in pm_offset, chaining to old
	   handler when it returns */

/* These generate assember IRET-style wrappers for functions and set up stack */
int _go32_dpmi_allocate_iret_wrapper(_go32_dpmi_seginfo *info);
	/* Put function ptr in pm_offset, call, returns wrapper entry in pm_offset. */
int _go32_dpmi_free_iret_wrapper(_go32_dpmi_seginfo *info);
	/* assumes pm_offset points to wrapper, frees it */

/* RMCB functions, automatically restructure the real-mode stack for the 
   proper return type and set up correct PM stack.  The callback
   (info->pm_offset) is called as (*pmcb)(_go32_dpmi_registers *regs); */
int _go32_dpmi_allocate_real_mode_callback_retf(_go32_dpmi_seginfo *info, _go32_dpmi_registers *regs);
	/* points callback at pm_offset, returns seg:ofs of callback addr
	   in rm_segment:rm_offset.  Do not change any fields until freed.
	   Interface is added to simulate far return */
int _go32_dpmi_allocate_real_mode_callback_iret(_go32_dpmi_seginfo *info, _go32_dpmi_registers *regs);
	/* same, but simulates iret */
int _go32_dpmi_free_real_mode_callback(_go32_dpmi_seginfo *info);
	/* frees callback */

/* The following two variables may be used to change the default stack size
   for interrupts and rmcb wrappers to a user defined size from the default
   of 32Kbytes.  Each RMCB and chain/iret wrapper gets it's own stack. */

extern unsigned long _go32_interrupt_stack_size;
extern unsigned long _go32_rmcb_stack_size;

/* Convenience functions, the return value is *bytes* */
unsigned long _go32_dpmi_remaining_physical_memory(void);
unsigned long _go32_dpmi_remaining_virtual_memory(void);

/* locks memory from a specified offset within the code/data selector */
int _go32_dpmi_lock_code( void *_lockaddr, unsigned long _locksize);
int _go32_dpmi_lock_data( void *_lockaddr, unsigned long _locksize);

int __djgpp_set_page_attributes(void *our_addr, unsigned long num_bytes,
			        unsigned short attributes);
int __djgpp_map_physical_memory(void *our_addr, unsigned long num_bytes,
			        unsigned long phys_addr);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_dpmi_h_ */
