/* Copyright (C) 1999 DJ Delorie, see COPYING.DJ for details */
#ifndef __dj_include_debug_redir_h_
#define __dj_include_debug_redir_h_

#ifdef __cplusplus
extern "C" {
#endif

#ifndef __dj_ENFORCE_ANSI_FREESTANDING

#ifndef __STRICT_ANSI__

#ifndef _POSIX_SOURCE

/* How many handles do we track.
   WARNING: if you change this, you need to recompile dbgredir.c !!! */
#define DBG_HANDLES 3

/* Describe a single redirected handle.
   Actually only inf_handle and our_handle are currently used (and
   even they could share the same slot), but I'm keeping the rest,
   mostly because they might be useful for applications.  */
struct dbg_redirect {
  int inf_handle;
  int our_handle;
  char *file_name;
  int mode;
  off_t filepos;
};

/* cmdline_parse_args processes command lines into the following structure: */
typedef struct _cmdline {
  char *command;		    /* command line with redirection removed */
  int redirected;		    /* 1 if handles redirected for child */
  struct dbg_redirect **redirection;/* info about redirected handles */
} cmdline_t;

extern void redir_cmdline_delete (cmdline_t *);
extern int  redir_cmdline_parse (const char *, cmdline_t *);
extern int  redir_to_child (cmdline_t *);
extern int  redir_to_debugger (cmdline_t *);
extern int  redir_debug_init (cmdline_t *);

#endif /* !_POSIX_SOURCE */
#endif /* !__STRICT_ANSI__ */
#endif /* !__dj_ENFORCE_ANSI_FREESTANDING */

#ifndef __dj_ENFORCE_FUNCTION_CALLS
#endif /* !__dj_ENFORCE_FUNCTION_CALLS */

#ifdef __cplusplus
}
#endif

#endif /* !__dj_include_debug_dbgcom_h_ */
