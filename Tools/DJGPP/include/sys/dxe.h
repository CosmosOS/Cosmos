/* Copyright (C) 1995 Charles Sandmann (sandmann@clio.rice.edu)
   This software may be freely distributed with above copyright, no warranty.
   Based on code by DJ Delorie, it's really his, enhanced, bugs fixed. */

typedef struct {
  long magic;
  long symbol_offset;
  long element_size;
  long nrelocs;
} dxe_header;

#define DXE_MAGIC 0x31455844

/* data stored after dxe_header in file; then relocs, 4 bytes each */

#ifdef __cplusplus
extern "C" {
#endif

void *_dxe_load(char *filename);

#ifdef __cplusplus
}
#endif
