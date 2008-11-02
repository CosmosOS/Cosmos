using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public class Assembler : Indy.IL2CPU.Assembler.Assembler {
        //TODO: COM Port info - should be in assembler? Assembler should not know about comports...
		protected byte mComNumber = 0;
		protected UInt16[] mComPortAddresses = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };

		public Assembler(Func<string, string> aGetStreamForGroup, byte aComNumber)
			: base(aGetStreamForGroup) {
			mComNumber = aComNumber;
		}

		public Assembler(Func<string, string> aGetStreamForGroup) : base(aGetStreamForGroup) {
		}

		private static string GetValidGroupName(string aGroup) {
			return aGroup.Replace('-','_').Replace('.', '_');
		}

        public override void Initialize() {
            base.Initialize();
            if (mComNumber > 0) {
                new Define("DEBUGSTUB");
            }
            new Label("Kernel_Start");
            new Comment("MultiBoot-compliant loader (e.g. GRUB or X.exe) provides info in registers: ");
            new Comment("EBX=multiboot_info ");
            new Comment("EAX=0x2BADB002 - check if it's really Multiboot loader ");
            new Comment("                ;- copy mb info - some stuff for you  ");
            new Add(Registers.EBX,
                    "4");
            new Move(Registers.EAX,
                     Registers.AtEBX);
            new Move("[MultiBootInfo_Memory_Low]", Registers.EAX);
            new Add(Registers.EBX,
                    "4");
            new Move(Registers.EAX,
                     Registers.AtEBX);
            new Move("[MultiBootInfo_Memory_High]", Registers.EAX);
            new Move(Registers.ESP,
                     "Kernel_Stack");
            new Comment("some more startups todo");
            new ClrInterruptFlag();
            if (mComNumber > 0) {
                UInt16 xComAddr = mComPortAddresses[mComNumber - 1];
                // 9600 baud, 8 databits, no parity, 1 stopbit
                new Move(Registers.DX,
                         xComAddr + 1);
                new Move(Registers.AL,
                         0);
                new Out(Registers.DX,
                        Registers.AL); // disable interrupts for serial stuff
                new Move(Registers.DX,
                         xComAddr + 3);
                new Move(Registers.AL,
                         0x80);
                new Out(Registers.DX,
                        Registers.AL); // Enable DLAB (set baud rate divisor)
                new Move(Registers.DX,
                         xComAddr);
                new Move(Registers.AL,
                         0x0C);
                new Out(Registers.DX,
                        Registers.AL); // Set divisor (lo byte)
                new Move(Registers.DX,
                         xComAddr + 1);
                new Move(Registers.AL,
                         0x00);
                new Out(Registers.DX,
                        Registers.AL); //			  (hi byte)
                new Move(Registers.DX,
                         xComAddr + 3);
                new Move(Registers.AL,
                         "0x03");
                new Out(Registers.DX,
                        Registers.AL); // 8 bits, no parity, one stop bit
                new Move(Registers.DX,
                         xComAddr + 2);
                new Move(Registers.AL,
                         0xC7);
                new Out(Registers.DX,
                        Registers.AL); // Enable FIFO, clear them, with 14-byte threshold
                new Move(Registers.DX,
                         xComAddr + 4);
                new Move(Registers.AL,
                         0x03);
                new Out(Registers.DX,
                        Registers.AL); // IRQ-s enabled, RTS/DSR set
            }

            // SSE init
            // CR4[bit 9]=1, CR4[bit 10]=1, CR0[bit 2]=0, CR0[bit 1]=1
            new Move(Registers.EAX,
                     Registers.CR4);
            new Or(Registers.EAX,
                   0x100);
            new Move(Registers.EAX,
                     Registers.CR4);

            new Move(Registers.EAX,
                     Registers.CR4);
            new Or(Registers.EAX,
                   0x200);
            new Move(Registers.EAX,
                     Registers.CR4);

            new Move(Registers.EAX,
                     Registers.CR0);
            new And(Registers.EAX,
                    0xfffffffd);
            new Move(Registers.EAX,
                     Registers.CR0);

            new Move(Registers.EAX,
                     Registers.CR0);
            new And(Registers.EAX,
                    0x1);
            new Move(Registers.EAX,
                     Registers.CR0);
            // END SSE INIT

            new Call(EntryPointName);
            new Label(".loop");
            new ClrInterruptFlag();
            new Halt();
            new Jump(".loop");
            new Label("DEBUG_STUB_");
            new Return();
            if (mComNumber > 0) {
                var xStub = new DebugStub();
                xStub.Main(mComPortAddresses[mComNumber - 1]);
            }
            //aOutputWriter.WriteLine("section .data");
            uint xFlags = 0x10003;
            DataMembers.AddRange(new DataMember[]{
                    new DataMember("MultibootSignature",
                                   new uint[]{0x1BADB002}),
                    new DataMember("MultibootFlags",
                                   xFlags),
                    new DataMember("MultibootChecksum",
                                   (int)(0-(xFlags + 0x1BADB002))),
                    new DataMember("MultibootHeaderAddr", new ElementReference("MultibootSignature")),
                    new DataMember("MultibootLoadAddr", new ElementReference("MultibootSignature")),
                    new DataMember("MultibootLoadEndAddr", 0),
                    new DataMember("MultibootBSSEndAddr", 0),
                    new DataMember("MultibootEntryAddr", new ElementReference("Kernel_Start")),
                    new DataMember("MultiBootInfo_Memory_High", 0),
                    new DataMember("MultiBootInfo_Memory_Low", 0),
                    new DataMember("Before_Kernel_Stack",
                                   new byte[0x50000]),
                    new DataMember("Kernel_Stack",
                                   new byte[0])});
            DebugStub.EmitDataSection();
        }

	    protected override void OnBeforeFlush() {
            base.OnBeforeFlush();
            DataMembers.AddRange(new DataMember[]{
                    new DataMember("_end_data",
                                   new byte[0])});
            new Label("_end_code");
        }

        public override void FlushText(TextWriter aOutput) {
            aOutput.WriteLine("use32");
            aOutput.WriteLine("%define NASM_COMPILATION 1");
            aOutput.WriteLine("global Kernel_Start");
            base.FlushText(aOutput);
        }
	}
}
