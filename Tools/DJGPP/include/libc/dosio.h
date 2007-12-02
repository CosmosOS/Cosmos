/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1996 DJ Delorie, see COPYING.DJ for details */
/* Copyright (C) 1995 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_libc_dosio_h__
#define __dj_include_libc_dosio_h__

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* set to O_BINARY or O_TEXT */
extern char *__file_handle_modes;
extern void __file_handle_set(int fd, int mode);

int __doserr_to_errno(int doserr);

/* puts "path" in the transfer buffer, fixing
   unix-allowed multi-slashes */
int _put_path(const char *_path);
int _put_path2(const char *_path, int _offset);

/* Convenience functions for setting up transfers */

#define __tb_offset 0	/* (__tb & 15) Always paragraph aligned */
#define __tb_segment (_go32_info_block.linear_address_of_transfer_buffer / 16)

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* __dj_include_libc_dosio_h__ */
