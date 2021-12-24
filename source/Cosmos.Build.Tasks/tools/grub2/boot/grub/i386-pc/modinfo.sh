#!/bin/sh

# User-controllable options
grub_modinfo_target_cpu=i386
grub_modinfo_platform=pc
grub_disk_cache_stats=0
grub_boot_time_stats=0
grub_have_font_source=1

# Autodetected config
grub_have_asm_uscore=0
grub_i8086_addr32="addr32"
grub_i8086_data32="data32"
grub_bss_start_symbol="__bss_start"
grub_end_symbol="end"

# Build environment
grub_target_cc='gcc-4.7'
grub_target_cc_version='gcc-4.7 (Ubuntu/Linaro 4.7.3-12ubuntu1) 4.7.3'
grub_target_cflags=' -Os -Wall -W -Wshadow -Wpointer-arith -Wundef -Wchar-subscripts -Wcomment -Wdeprecated-declarations -Wdisabled-optimization -Wdiv-by-zero -Wfloat-equal -Wformat-extra-args -Wformat-security -Wformat-y2k -Wimplicit -Wimplicit-function-declaration -Wimplicit-int -Wmain -Wmissing-braces -Wmissing-format-attribute -Wmultichar -Wparentheses -Wreturn-type -Wsequence-point -Wshadow -Wsign-compare -Wswitch -Wtrigraphs -Wunknown-pragmas -Wunused -Wunused-function -Wunused-label -Wunused-parameter -Wunused-value  -Wunused-variable -Wwrite-strings -Wnested-externs -Wstrict-prototypes -g -Wredundant-decls -Wmissing-prototypes -Wmissing-declarations  -Wextra -Wattributes -Wendif-labels -Winit-self -Wint-to-pointer-cast -Winvalid-pch -Wmissing-field-initializers -Wnonnull -Woverflow -Wvla -Wpointer-to-int-cast -Wstrict-aliasing -Wvariadic-macros -Wvolatile-register-var -Wpointer-sign -Wmissing-prototypes -Wmissing-declarations -Wformat=2 -march=i386 -m32 -mrtd -mregparm=3 -falign-jumps=1 -falign-loops=1 -falign-functions=1 -freg-struct-return -mno-mmx -mno-sse -mno-sse2 -mno-3dnow -fno-dwarf2-cfi-asm -fno-asynchronous-unwind-tables -Qn -fno-stack-protector -Wtrampolines -Werror'
grub_target_cppflags='-Wno-unused-but-set-variable -Wall -W -I$(top_srcdir)/include -I$(top_builddir)/include  -DGRUB_MACHINE_PCBIOS=1 -DGRUB_MACHINE=I386_PC -m32 -nostdinc -isystem /usr/lib/gcc/i686-linux-gnu/4.7/include'
grub_target_ccasflags=' -g -m32'
grub_target_ldflags=' -m32 -Wl,-melf_i386 -Wl,--build-id=none'
grub_target_strip='strip'
grub_target_nm='nm'
grub_target_ranlib='ranlib'
grub_target_objconf=''
grub_target_obj2elf=''

# Version
grub_version="2.02~beta2"
grub_package="grub"
grub_package_string="GRUB 2.02~beta2-9"
grub_package_version="2.02~beta2-9"
grub_package_name="GRUB"
grub_package_bugreport="bug-grub@gnu.org"
