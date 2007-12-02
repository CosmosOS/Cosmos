#ifndef __dj_include_inline_ctype_ha_
#define __dj_include_inline_ctype_ha_

#define __dj_ISALNUM 	0x0001
#define __dj_ISALPHA	0x0002
#define __dj_ISCNTRL	0x0004
#define __dj_ISDIGIT	0x0008
#define __dj_ISGRAPH	0x0010
#define __dj_ISLOWER	0x0020
#define __dj_ISPRINT	0x0040
#define __dj_ISPUNCT	0x0080
#define __dj_ISSPACE	0x0100
#define __dj_ISUPPER	0x0200
#define __dj_ISXDIGIT	0x0400

extern unsigned short __dj_ctype_flags[];
extern unsigned char __dj_ctype_toupper[];
extern unsigned char __dj_ctype_tolower[];

#define isalnum(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISALNUM)
#define isalpha(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISALPHA)
#define iscntrl(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISCNTRL)
#define isdigit(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISDIGIT)
#define isgraph(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISGRAPH)
#define islower(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISLOWER)
#define isprint(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISPRINT)
#define ispunct(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISPUNCT)
#define isspace(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISSPACE)
#define isupper(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISUPPER)
#define isxdigit(c) (__dj_ctype_flags[(int)(c)+1] & __dj_ISXDIGIT)

#define tolower(c) (__dj_ctype_tolower[(int)(c)+1])
#define toupper(c) (__dj_ctype_toupper[(int)(c)+1])

#endif /* __dj_include_inline_ctype_hi_ */
