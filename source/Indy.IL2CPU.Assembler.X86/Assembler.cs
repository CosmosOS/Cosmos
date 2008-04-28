using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public class Assembler : Indy.IL2CPU.Assembler.Assembler {
        protected class Y86 : X.Y86 {

            public void DebugStub(TextWriter aOutputWriter, UInt16 aComAddr) {
                UInt16 xComStatusAddr = (UInt16)(aComAddr + 5);
                Label = "WriteByteToComPort";
                Label = "WriteByteToComPort_Wait";
                DX = xComStatusAddr;
                AL = Port[DX];
                AL.Test(0x20);
                JumpIf(Flags.Zero, "WriteByteToComPort_Wait");
                DX = aComAddr;
                AL = Memory[ESP + 4];
                Port[DX] = AL;
                Return(4);

                Label = "DebugWriteEIP";
                AL = Memory[EBP + 3];
                EAX.Push();
                Call("WriteByteToComPort");
                AL = Memory[EBP + 2];
                EAX.Push();
                Call("WriteByteToComPort");
                AL = Memory[EBP + 1];
                EAX.Push();
                Call("WriteByteToComPort");
                AL = Memory[EBP];
                EAX.Push();
                Call("WriteByteToComPort");
                Return();

                Label = "DebugPoint_WaitCmd";
                DX = xComStatusAddr;
                AL = Port[DX];
                AL.Test(0x01);
                JumpIf(Flags.Zero, "DebugPoint_WaitCmd");
                Jump("DebugPoint_ProcessCmd");

                Label = "DebugPoint__";
                PushAll32();
                EBP = ESP;
                EBP.Add(32);

                // Check TraceMode
                EAX = Memory["TraceMode"];
                AL.Compare(1);
                JumpIf(Flags.Equal, "DebugPoint_NoTrace");
                //
                Call("DebugWriteEIP");
                //
                EAX = Memory["TraceMode"];
                AL.Compare(4);
                JumpIf(Flags.Equal, "DebugPoint_WaitCmd");
                Label = "DebugPoint_NoTrace";

                // Is there a new incoming command?
                Label = "DebugPoint_CheckCmd";
                DX = xComStatusAddr;
                AL = Port[DX];
                AL.Test(0x01);
                JumpIf(Flags.Zero, "DebugPoint_AfterCmd");

                Label = "DebugPoint_ProcessCmd";
                DX = aComAddr;
                AL = Port[DX];
                AL.Compare(1);
                JumpIf(Flags.NotEqual, "DebugPoint_Cmd02");
                Memory["TraceMode", 32] = 1;
                Jump("DebugPoint_CheckCmd");
                //
                Label = "DebugPoint_Cmd02";
                AL.Compare(2);
                JumpIf(Flags.NotEqual, "DebugPoint_Cmd03");
                Memory["TraceMode", 32] = 2;
                Jump("DebugPoint_CheckCmd");
                //
                Label = "DebugPoint_Cmd03";
                AL.Compare(3);
                JumpIf(Flags.NotEqual, "DebugPoint_Cmd04");
                Memory["TraceMode", 32] = 4;
                Jump("DebugPoint_AfterCmd");
                //
                Label = "DebugPoint_Cmd04";
                AL.Compare(4);
                JumpIf(Flags.NotEqual, "DebugPoint_Cmd05");
                Memory["TraceMode", 32] = 4;
                Jump("DebugPoint_WaitCmd");
                //
                Label = "DebugPoint_Cmd05";
                // -Evaluate variables
                // -Step to next debug call
                // Break points
                // Immediate break
                Label = "DebugPoint_AfterCmd";

                // TraceMode
                // 1 - No tracing
                // 2 - Tracing
                // 3 - 
                // 4 - Break and wait

                PopAll32();
                Return();
            }
        }

		public const string BreakMethodName = "_CODE_REQUESTED_BREAK_";
		protected byte mComNumber = 0;
		protected UInt16[] mComPortAddresses = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };

		public Assembler(Func<string, string> aGetStreamForGroup, bool aInMetalMode, byte aComNumber)
			: base(aGetStreamForGroup, aInMetalMode) {
			mComNumber = aComNumber;
		}

		public Assembler(Func<string, string> aGetStreamForGroup) : base(aGetStreamForGroup) {
		}

		private static string GetValidGroupName(string aGroup) {
			return aGroup.Replace('-','_').Replace('.', '_');
		}

		protected override void EmitCodeSectionHeader(string aGroup, TextWriter aOutputWriter) {
			base.EmitCodeSectionHeader(aGroup, aOutputWriter);
            using (var xAsm = new RawAssembler()) {
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
				    if (mComNumber > 0) {
					    UInt16 xComAddr = mComPortAddresses[mComNumber - 1];
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
				    if (mComNumber > 0) {
                        var y = new Y86();
                        y.DebugStub(aOutputWriter, mComPortAddresses[mComNumber - 1]);
                    }
                }
                aOutputWriter.WriteLine(xAsm.GetContents());
            }
        }

		protected override void EmitDataSectionHeader(string aGroup, TextWriter aOutputWriter) {
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

		protected override void EmitIDataSectionHeader(string aGroup, TextWriter aOutputWriter) {
		}

		protected override void EmitDataSectionFooter(string aGroup, TextWriter aOutputWriter) {
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

		protected override void EmitHeader(string aGroup, TextWriter aOutputWriter) {
			//mOutputWriter.WriteLine("format ms coff  ");
			//mOutputWriter.WriteLine("org 0220000h    ; the best place to load our kernel to. ");
			aOutputWriter.WriteLine("use32           ; the kernel will be run in 32-bit protected mode, ");
			aOutputWriter.WriteLine("");
			aOutputWriter.WriteLine("%ifdef {0}", GetValidGroupName(aGroup));
			aOutputWriter.WriteLine("%else");
			aOutputWriter.WriteLine("  %define {0} 1", GetValidGroupName(aGroup));
			aOutputWriter.WriteLine("");
		}

		protected override void EmitIncludes(string aGroup, TextWriter aOutputWriter) {
			foreach (string xInclude in (from item in Includes
										 where String.Equals(item.Key, aGroup, StringComparison.InvariantCultureIgnoreCase)
										 select item.Value).Distinct(StringComparer.InvariantCultureIgnoreCase)) {
				aOutputWriter.WriteLine("%include '{0}'", xInclude);
			}
		}

		protected override void EmitFooter(string aGroup, TextWriter aOutputWriter) {
			if (aGroup == MainGroup) {
				aOutputWriter.WriteLine("_end_data:      ; -- end of CODE+DATA ");
			}
			aOutputWriter.WriteLine("%endif");
		}

	}
}
