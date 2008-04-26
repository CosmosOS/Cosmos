using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public class Assembler: Indy.IL2CPU.Assembler.Assembler {
		public const string BreakMethodName = "_CODE_REQUESTED_BREAK_";
		private byte? mComNumber;
		protected int[] mComPortAddress = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };
		public Assembler(Func<string, string> aGetStreamForGroup, bool aInMetalMode, byte? aComNumber)
			: base(aGetStreamForGroup, aInMetalMode) {
			mComNumber = aComNumber;
		}

		public Assembler(Func<string, string> aGetStreamForGroup)
			: base(aGetStreamForGroup) {
		}

		private static string GetValidGroupName(string aGroup) {
			return aGroup.Replace('-','_').Replace('.', '_');
		}

		protected override void EmitCodeSectionHeader(string aGroup, StreamWriter aOutputWriter) {
			base.EmitCodeSectionHeader(aGroup, aOutputWriter);
			aOutputWriter.WriteLine("section .text");
			if (aGroup == MainGroup) {
				aOutputWriter.WriteLine("global Kernel_Start");
				aOutputWriter.WriteLine("Kernel_Start: ");
				aOutputWriter.WriteLine("");
				aOutputWriter.WriteLine("; MultiBoot-compliant loader (e.g. GRUB or X.exe) provides info in registers: ");
				aOutputWriter.WriteLine("; EBX=multiboot_info ");
				aOutputWriter.WriteLine("; EAX=0x2BADB002 - check if it's really Multiboot loader ");
				aOutputWriter.WriteLine("");
				aOutputWriter.WriteLine("                ;- copy mb info - some stuff for you  ");
				//aOutputWriter.WriteLine("								mov mb_info, ebx");
				aOutputWriter.WriteLine("				add ebx, 4");
				aOutputWriter.WriteLine("				mov dword eax, [ebx]");
				aOutputWriter.WriteLine("				mov dword [MultiBootInfo_Memory_Low], eax");
				aOutputWriter.WriteLine("				add ebx, 4");
				aOutputWriter.WriteLine("				mov dword eax, [ebx]");
				aOutputWriter.WriteLine("				mov dword [MultiBootInfo_Memory_High], eax");
				aOutputWriter.WriteLine("");
				aOutputWriter.WriteLine("                mov esp,Kernel_Stack ");
				aOutputWriter.WriteLine("");
				aOutputWriter.WriteLine("; some more startups todo");
				aOutputWriter.WriteLine("				 cli");
				//aOutputWriter.WriteLine("				 push ebx");
				if (mComNumber != null) {
					int xComAddr = mComPortAddress[mComNumber.Value - 1];
					aOutputWriter.WriteLine("mov dx, 0x{0}", (xComAddr + 1).ToString("X"));
					aOutputWriter.WriteLine("mov al, 0x00");
					aOutputWriter.WriteLine("out DX, AL"); // disable all interrupts
					aOutputWriter.WriteLine("mov dx, 0x{0}", (xComAddr + 3).ToString("X"));
					aOutputWriter.WriteLine("mov al, 0x80");
					aOutputWriter.WriteLine("out DX, AL");  // Enable DLAB (set baud rate divisor)
					aOutputWriter.WriteLine("mov dx, 0x{0}", (xComAddr + 0).ToString("X"));
					aOutputWriter.WriteLine("mov al, 0x01");
					aOutputWriter.WriteLine("out DX, AL");  // Set divisor (lo byte)
					aOutputWriter.WriteLine("mov dx, 0x{0}", (xComAddr + 1).ToString("X"));
					aOutputWriter.WriteLine("mov al, 0x00");
					aOutputWriter.WriteLine("out DX, AL");  //			  (hi byte)
					aOutputWriter.WriteLine("mov dx, 0x{0}", (xComAddr + 3).ToString("X"));
					aOutputWriter.WriteLine("mov al, 0x03");
					aOutputWriter.WriteLine("out DX, AL");  // 8 bits, no parity, one stop bit
					aOutputWriter.WriteLine("mov dx, 0x{0}", (xComAddr + 2).ToString("X"));
					aOutputWriter.WriteLine("mov al, 0xC7");
					aOutputWriter.WriteLine("out DX, AL");  // Enable FIFO, clear them, with 14-byte threshold
					aOutputWriter.WriteLine("mov dx, 0x{0}", (xComAddr + 4).ToString("X"));
					aOutputWriter.WriteLine("mov al, 0x03");
					aOutputWriter.WriteLine("out DX, AL");  // IRQ-s enabled, RTS/DSR set
				}
				aOutputWriter.WriteLine("				 call " + EntryPointName);
				aOutputWriter.WriteLine("			.loop:");
				aOutputWriter.WriteLine("				 cli");
				aOutputWriter.WriteLine("				 hlt");
				aOutputWriter.WriteLine("				 jmp .loop");
				aOutputWriter.WriteLine("                 ");
				aOutputWriter.WriteLine("         " + BreakMethodName + ":");
				aOutputWriter.WriteLine("              ret");
				aOutputWriter.WriteLine("                 ");
				if (mComNumber != null) {
					int xComAddr = mComPortAddress[mComNumber.Value - 1];

					aOutputWriter.WriteLine("WriteByteToComPort:");
					aOutputWriter.WriteLine("  WriteByteToComPort_Wait:");
					aOutputWriter.WriteLine("    mov dx, " + (xComAddr + 5));
					aOutputWriter.WriteLine("    in al, dx");
					aOutputWriter.WriteLine("    test al, 0x20");
					aOutputWriter.WriteLine("    je WriteByteToComPort_Wait");
					aOutputWriter.WriteLine("    mov dx, " + xComAddr);
					aOutputWriter.WriteLine("    mov eax, [esp + 4]");
					aOutputWriter.WriteLine("    out dx, al");
					aOutputWriter.WriteLine("    ret 4");

					aOutputWriter.WriteLine("DebugWriteEIP:");
					aOutputWriter.WriteLine("    mov al, [ebp+3]");
					aOutputWriter.WriteLine("    push eax");
					aOutputWriter.WriteLine("    call WriteByteToComPort");
					aOutputWriter.WriteLine("    mov al, [ebp+2]");
					aOutputWriter.WriteLine("    push eax");
					aOutputWriter.WriteLine("    call WriteByteToComPort");
					aOutputWriter.WriteLine("    mov al, [ebp+1]");
					aOutputWriter.WriteLine("    push eax");
					aOutputWriter.WriteLine("    call WriteByteToComPort");
					aOutputWriter.WriteLine("    mov al, [ebp]");
					aOutputWriter.WriteLine("    push eax");
					aOutputWriter.WriteLine("    call WriteByteToComPort");
					aOutputWriter.WriteLine("    ret");

					aOutputWriter.WriteLine("DebugPoint_WaitCmd:");
					aOutputWriter.WriteLine("    mov dx, " + (xComAddr + 5));
					aOutputWriter.WriteLine("    in al, dx");
					aOutputWriter.WriteLine("    test al, 0x01");
					aOutputWriter.WriteLine("    jz DebugPoint_WaitCmd");
					aOutputWriter.WriteLine("    jmp DebugPoint_ProcessCmd");

					aOutputWriter.WriteLine("DebugPoint__:");
					aOutputWriter.WriteLine("    PUSHAD");
					aOutputWriter.WriteLine("    mov ebp, esp");
					aOutputWriter.WriteLine("    add ebp, 32");

					// Check TraceMode
					aOutputWriter.WriteLine("    mov dword eax, [TraceMode]");
					aOutputWriter.WriteLine("    cmp al, 1"); // if 1, no tracing
					aOutputWriter.WriteLine("    je DebugPoint_NoTrace");
					//
					aOutputWriter.WriteLine("    call DebugWriteEIP");
					//
					aOutputWriter.WriteLine("    mov dword eax, [TraceMode]");
					aOutputWriter.WriteLine("    cmp al, 4");
					aOutputWriter.WriteLine("    je DebugPoint_WaitCmd");
					aOutputWriter.WriteLine("  DebugPoint_NoTrace:");

					//// Is there a new incoming command?
					aOutputWriter.WriteLine("  DebugPoint_CheckCmd:");
					aOutputWriter.WriteLine("    mov dx, " + (xComAddr + 5));
					aOutputWriter.WriteLine("    in al, dx");
					aOutputWriter.WriteLine("    test al, 0x01");
					aOutputWriter.WriteLine("    jz DebugPoint_AfterCmd");

					aOutputWriter.WriteLine("DebugPoint_ProcessCmd:");
					aOutputWriter.WriteLine("    mov dx, " + xComAddr);
					aOutputWriter.WriteLine("    in al, dx");
					aOutputWriter.WriteLine("    cmp al, 1"); // Turn Full Tracing off
					aOutputWriter.WriteLine("    jne DebugPoint_Cmd02");
					aOutputWriter.WriteLine("    mov dword [TraceMode], 1");
					aOutputWriter.WriteLine("    jmp DebugPoint_CheckCmd");
					//
					aOutputWriter.WriteLine("DebugPoint_Cmd02:");
					aOutputWriter.WriteLine("    cmp al, 2"); // Turn Full Tracing on
					aOutputWriter.WriteLine("    jne DebugPoint_Cmd03");
					aOutputWriter.WriteLine("    mov dword [TraceMode], 2");
					aOutputWriter.WriteLine("    jmp DebugPoint_CheckCmd");
					//
					aOutputWriter.WriteLine("DebugPoint_Cmd03:");
					aOutputWriter.WriteLine("    cmp al, 3"); // Step one
					aOutputWriter.WriteLine("    jne DebugPoint_Cmd04");
					aOutputWriter.WriteLine("    mov dword [TraceMode], 4");
					aOutputWriter.WriteLine("    jmp DebugPoint_AfterCmd");
					//
					aOutputWriter.WriteLine("DebugPoint_Cmd04:");
					aOutputWriter.WriteLine("    cmp al, 4"); // Immediate Break
					aOutputWriter.WriteLine("    jne DebugPoint_Cmd05");
					aOutputWriter.WriteLine("    mov dword [TraceMode], 4");
					aOutputWriter.WriteLine("    jmp DebugPoint_WaitCmd");
					//
					aOutputWriter.WriteLine("DebugPoint_Cmd05:");
					// -Evaluate variables
					// -Step to next debug call
					// Break points
					// Immediate break
					aOutputWriter.WriteLine("DebugPoint_AfterCmd:");

					// TraceMode
					// 1 - No tracing
					// 2 - Tracing
					// 3 - 
					// 4 - Break and wait

					aOutputWriter.WriteLine("    POPAD");
					aOutputWriter.WriteLine("    ret");
				}
			}
		}

		protected override void EmitDataSectionHeader(string aGroup, StreamWriter aOutputWriter) {
			base.EmitDataSectionHeader(aGroup, aOutputWriter);
			if (aGroup == MainGroup) {
				aOutputWriter.WriteLine("section .data");
				aOutputWriter.WriteLine("_start:  ");
				aOutputWriter.WriteLine("; multiboot header ");
				aOutputWriter.WriteLine("MBFLAGS equ 0x03 ; 4KB aligned modules etc., full memory info,  ");
				aOutputWriter.WriteLine("                        ; use special header (see below) ");
				aOutputWriter.WriteLine("dd 0x1BADB002           ; multiboot signature ");
				aOutputWriter.WriteLine("dd MBFLAGS              ; 4kb page aligment for modules, supply memory info ");
				aOutputWriter.WriteLine("dd -0x1BADB002-MBFLAGS  ; checksum=-(FLAGS+0x1BADB002) ");
				aOutputWriter.WriteLine("; other data - that is the additional (optional) header which helps to load  ");
				aOutputWriter.WriteLine("; the kernel. ");
				//aOutputWriter.WriteLine("        dd _start       ; header_addr ");
				//aOutputWriter.WriteLine("        dd _start       ; load_addr ");
				//aOutputWriter.WriteLine("        dd _end_data    ; load_end_addr ");
				//aOutputWriter.WriteLine("        dd _end         ; bss_end_addr ");
				//aOutputWriter.WriteLine("        dd Kernel_Start ; entry ");
				aOutputWriter.WriteLine("; end of header ");
				//aOutputWriter.WriteLine("mb_info multiboot_info");
				aOutputWriter.WriteLine("MultiBootInfo_Memory_High dd 0");
				aOutputWriter.WriteLine("MultiBootInfo_Memory_Low dd 0");
				if (Signature != null && Signature.Length > 0) {
					aOutputWriter.WriteLine("{0} db {1}", SignatureLabelName, Signature.Aggregate<byte, string>("", (r, b) => r + b + ",") + "0");
				}
				aOutputWriter.WriteLine("TraceMode dd 1");
			}

		}

		protected override void EmitIDataSectionHeader(string aGroup, StreamWriter aOutputWriter) {
		}

		protected override void EmitDataSectionFooter(string aGroup, StreamWriter aOutputWriter) {
			base.EmitDataSectionFooter(aGroup, aOutputWriter);
			if (aGroup == MainGroup) {
				aOutputWriter.WriteLine("");
				aOutputWriter.WriteLine(";--- bss --- place r*, d* ? directives here, so that you'll have a BSS. ");
				aOutputWriter.WriteLine("");
				aOutputWriter.WriteLine("Before_Kernel_Stack:");
				//aOutputWriter.WriteLine("times 50000 resb 0        ; our own stack ");
				aOutputWriter.WriteLine("TIMES 0x50000 db 0");
				aOutputWriter.WriteLine("Kernel_Stack: ");
				aOutputWriter.WriteLine("");
				aOutputWriter.WriteLine("");
				aOutputWriter.WriteLine("_end:   ; end of BSS - here's the virtual and logical end.");
			}
		}

		protected override void EmitHeader(string aGroup, StreamWriter aOutputWriter) {
			//mOutputWriter.WriteLine("format ms coff  ");
			//mOutputWriter.WriteLine("org 0220000h    ; the best place to load our kernel to. ");
			aOutputWriter.WriteLine("use32           ; the kernel will be run in 32-bit protected mode, ");
			aOutputWriter.WriteLine("");
			aOutputWriter.WriteLine("%ifdef {0}", GetValidGroupName(aGroup));
			aOutputWriter.WriteLine("%else");
			aOutputWriter.WriteLine("  %define {0} 1", GetValidGroupName(aGroup));
			aOutputWriter.WriteLine("");
		}

		protected override void EmitIncludes(string aGroup, StreamWriter aOutputWriter) {
			foreach (string xInclude in (from item in Includes
										 where String.Equals(item.Key, aGroup, StringComparison.InvariantCultureIgnoreCase)
										 select item.Value).Distinct(StringComparer.InvariantCultureIgnoreCase)) {
				aOutputWriter.WriteLine("%include '{0}'", xInclude);
			}
		}

		protected override void EmitFooter(string aGroup, StreamWriter aOutputWriter) {
			if (aGroup == MainGroup) {
				aOutputWriter.WriteLine("_end_data:      ; -- end of CODE+DATA ");
			}
			aOutputWriter.WriteLine("%endif");
		}

		protected override void EmitImportMembers(string aGroup, StreamWriter aOutputWriter) {
			if (ImportMembers.Count > 0) {
				throw new Exception("You can't use P/Invoke in OS kernels");
			}
		}
	}
}
