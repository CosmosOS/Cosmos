using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.Native {
	public class Assembler: X86.Assembler {
		public Assembler(StreamWriter aOutputWriter, bool aInMetalMode)
			: base(aOutputWriter, aInMetalMode) {
		}

		public Assembler(StreamWriter aOutputWriter)
			: base(aOutputWriter) {
		}

		protected override void EmitCodeSectionHeader() {
		}

		protected override void EmitDataSectionHeader() {
		}

		protected override void EmitIDataSectionHeader() {
		}

		protected override void EmitFooter() {
			mOutputWriter.WriteLine("_end_data:      ; -- end of CODE+DATA ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine(";--- bss --- place r*, d* ? directives here, so that you'll have a BSS. ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("mb_info multiboot_info ; just a dummy  ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("rb 50000        ; our own stack ");
			mOutputWriter.WriteLine("Kernel_Stack: ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("_end:   ; end of BSS - here's the virtual and logical end.");
		}

		protected override void EmitHeader() {
			mOutputWriter.WriteLine("format ms coff  ");
			mOutputWriter.WriteLine("org 0220000h    ; the best place to load our kernel to. ");
			mOutputWriter.WriteLine("use32           ; the kernel will be run in 32-bit protected mode, ");
			mOutputWriter.WriteLine("");
			List<string> xTheLabels = new List<string>();
			string mLastRealLabel = "";
			foreach (IL2CPU.Assembler.Instruction x in mInstructions) {
				Label xLabel = x as Label;
				if (xLabel != null) {
					if (!xLabel.Name.StartsWith(".")) {
						mLastRealLabel = xLabel.Name;
						xTheLabels.Add(xLabel.Name);
					} else {
						xTheLabels.Add(mLastRealLabel + "@@" + xLabel.Name.Substring(1));
					}
				}
			}
			if (xTheLabels.Count > 0) {
			//	mOutputWriter.WriteLine("include 'export.inc'");
			//	mOutputWriter.WriteLine("section '.edata' export data readable");
			//	mOutputWriter.WriteLine("");
			//	mOutputWriter.WriteLine("\texport 'OUTPUT.dll', \\");
				for (int i = 0; i < xTheLabels.Count; i++) {
					if (i == (xTheLabels.Count - 1)) {
			//			mOutputWriter.WriteLine("\t\t {0},'{1}'", xTheLabels[i].Replace("@@", "."), xTheLabels[i]);
					} else {
			//				mOutputWriter.WriteLine("\t\t{0},'{1}', \\", xTheLabels[i].Replace("@@", "."), xTheLabels[i]);
					}
					// public entry as 'entry'
					//mOutputWriter.WriteLine("\tpublic {0} as '{0}'", xTheLabels[i].Replace("@@", "."), xTheLabels[i]);
				}
			}
			//mOutputWriter.WriteLine("section '.code' code readable executable");
			mOutputWriter.WriteLine("macro struct name ");
			mOutputWriter.WriteLine(" { virtual at 0 ");
			mOutputWriter.WriteLine("   name name ");
			mOutputWriter.WriteLine("   sizeof.#name = $ - name ");
			mOutputWriter.WriteLine("   name equ sizeof.#name ");
			mOutputWriter.WriteLine("   end virtual } ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("struc multiboot_info ");
			mOutputWriter.WriteLine("{ ");
			mOutputWriter.WriteLine("        .flags          dd      ? ");
			mOutputWriter.WriteLine("        .mem_lower      dd      ? ");
			mOutputWriter.WriteLine("        .mem_upper      dd      ? ");
			mOutputWriter.WriteLine("        .boot_device    rb      4 ");
			mOutputWriter.WriteLine("        .cmdline        dd      ? ");
			mOutputWriter.WriteLine("        .mods_count     dd      ? ");
			mOutputWriter.WriteLine("        .mods_addr      dd      ? ");
			mOutputWriter.WriteLine("        ;-- ELF specific info: ");
			mOutputWriter.WriteLine("        .num            dd      ? ");
			mOutputWriter.WriteLine("        .size_          dd      ? ");
			mOutputWriter.WriteLine("        .addr_          dd      ? ");
			mOutputWriter.WriteLine("        .shndx          dd      ? ");
			mOutputWriter.WriteLine("        ;---- ");
			mOutputWriter.WriteLine("        .mmap_length    dd      ? ");
			mOutputWriter.WriteLine("        .mmap_addr      dd      ? ");
			mOutputWriter.WriteLine("        .size=$-.flags ");
			mOutputWriter.WriteLine("} ");
			mOutputWriter.WriteLine("struct multiboot_info ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("struc mod_list ");
			mOutputWriter.WriteLine("{ ");
			mOutputWriter.WriteLine("        .mod_start      dd      ? ");
			mOutputWriter.WriteLine("        .mod_end        dd      ?       ; (mod_end-mod_start)=len ");
			mOutputWriter.WriteLine("        .string         dd      ? ");
			mOutputWriter.WriteLine("        .reserved_      dd      ? ");
			mOutputWriter.WriteLine("} ");
			mOutputWriter.WriteLine("struct mod_list ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("struc addr_range ");
			mOutputWriter.WriteLine("{ ");
			mOutputWriter.WriteLine("        .size_          dd      ? ");
			mOutputWriter.WriteLine("        .BaseAddrLow    dd      ? ");
			mOutputWriter.WriteLine("        .BaseAddrHigh   dd      ? ");
			mOutputWriter.WriteLine("        .LengthLow      dd      ? ");
			mOutputWriter.WriteLine("        .LengthHigh     dd      ? ");
			mOutputWriter.WriteLine("        .Type_          dd      ?       ; =1 if useable ");
			mOutputWriter.WriteLine("} ");
			mOutputWriter.WriteLine("struct addr_range");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("_start:  ");
			mOutputWriter.WriteLine("; multiboot header ");
			mOutputWriter.WriteLine("MBFLAGS=0x03 or (1 shl 16) ; 4KB aligned modules etc., full memory info,  ");
			mOutputWriter.WriteLine("                        ; use special header (see below) ");
			mOutputWriter.WriteLine("dd 0x1BADB002           ; multiboot signature ");
			mOutputWriter.WriteLine("dd MBFLAGS              ; 4kb page aligment for modules, supply memory info ");
			mOutputWriter.WriteLine("dd -0x1BADB002-MBFLAGS  ; checksum=-(FLAGS+0x1BADB002) ");
			mOutputWriter.WriteLine("; other data - that is the additional (optional) header which helps to load  ");
			mOutputWriter.WriteLine("; the kernel. ");
			mOutputWriter.WriteLine("        dd _start       ; header_addr ");
			mOutputWriter.WriteLine("        dd _start       ; load_addr ");
			mOutputWriter.WriteLine("        dd _end_data    ; load_end_addr ");
			mOutputWriter.WriteLine("        dd _end         ; bss_end_addr ");
			mOutputWriter.WriteLine("        dd Kernel_Start ; entry ");
			mOutputWriter.WriteLine("; end of header ");
			//mOutputWriter.WriteLine("mb_info multiboot_info");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("Kernel_Start: ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("; MultiBoot-compliant loader (e.g. GRUB or X.exe) provides info in registers: ");
			mOutputWriter.WriteLine("; EBX=multiboot_info ");
			mOutputWriter.WriteLine("; EAX=0x2BADB002 - check if it's really Multiboot loader ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("                ;- copy mb info - some stuff for you  ");
			//mOutputWriter.WriteLine("								mov mb_info, ebx");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("                mov esp,Kernel_Stack ");
			mOutputWriter.WriteLine("");
			mOutputWriter.WriteLine("; some more startups todo");
			mOutputWriter.WriteLine("				 call " + EngineEntryPointLabelName);
			mOutputWriter.WriteLine("			.loop:");
			mOutputWriter.WriteLine("				 xchg bx, bx");
			mOutputWriter.WriteLine("				 hlt");
			mOutputWriter.WriteLine("				 jmp .loop");
			mOutputWriter.WriteLine("                 ");
		}
	}
}
