ILC=/home/valentinbreiz/.nuget/packages/runtime.linux-x64.microsoft.dotnet.ilcompiler/6.0.0-preview.7.21355.1/tools/ilc
DOTNET = dotnet
NASM = nasm
QEMU_OPTS=-s -d cpu_reset -L ".\\tools\\qemu\\" -bios bios64.bin -k en-gb -m 512M -drive file=fat:rw:"obj/isofiles",format=raw,media=disk -serial file:serial.log

ASM_FLAGS = -f elf64 

OBJ_DIR=obj
ASM_DIR=asm

_DEPS = boot.asm dotnet.asm load_end_addr.asm multiboot2.asm

OS_SOURCES = $(wildcard *.cs)
OS_DLL=bin/Debug/net5.0/Kernel.dll
OS_OBJ=obj/Kernel.o

ISO=bin/Kernel.iso
KERNEL=bin/kernel.bin
GRUB_SCRIPT=scripts/grub.cfg

DEPS = $(patsubst %,$(ASM_DIR)/%,$(_DEPS))

_OBJ = $(_DEPS:.asm=.o)
OBJ = $(patsubst %,$(OBJ_DIR)/%,$(_OBJ))

default: build

.PHONY: default build run clean debug

build: $(ISO)
	@echo Done building kernel!;

run: $(ISO)
	@echo Starting VM; \
	qemu $(QEMU_OPTS)

$(ISO): $(KERNEL)
	@echo Building EFI; \
	mkdir -p obj/isofiles/EFI/BOOT && \
	mkdir -p obj/isofiles/boot/ && \
	cp $(KERNEL) obj/isofiles/boot/kernel.bin && \
	grub-mkstandalone -O x86_64-efi -o obj/isofiles/EFI/BOOT/BOOTX64.EFI "boot/grub/grub.cfg=scripts/grub.cfg"

$(KERNEL): $(OS_OBJ) $(OBJ)
	@echo Linking $(KERNEL); \
	ld --nmagic --output=$(KERNEL) --script=scripts/linker.ld obj/multiboot2.o obj/boot.o obj/Kernel.o obj/dotnet.o obj/load_end_addr.o
	# clang-3.9 -march=i686 -ffreestanding -fno-builtin -nostdlib obj/multiboot2.o obj/boot.o obj/Kernel.o obj/dotnet.o obj/load_end_addr.o

$(OS_OBJ): $(OS_DLL)
	@echo Converting CIL to native code; \
	$(ILC) --systemmodule Kernel --out $(OS_OBJ) $(OS_DLL)

$(OBJ_DIR): $(OBJ_DIR)
	@mkdir $(OBJ_DIR)

$(OBJ_DIR)/%.o: $(ASM_DIR)/%.asm
	@echo Assembling $@; \
	$(NASM) $(ASM_FLAGS) -o $@ $<

$(OS_DLL): $(OS_SOURCES)
	@echo Building Kernel dlls; \
	$(DOTNET) restore && \
	$(DOTNET) build

clean:
	rm -rf *.log
	rm -rf *.map
	rm -rf obj/
	rm -rf bin/

re: clean build

print-%  : ; @echo $* = $($*)