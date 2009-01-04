using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Indy.IL2CPU.Assembler.X86 {
    public class CosmosAssembler: Assembler {
        //TODO: COM Port info - should be in assembler? Assembler should not know about comports...
		protected byte mComNumber = 0;
		protected UInt16[] mComPortAddresses = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };

		public CosmosAssembler(byte aComNumber) {
			mComNumber = aComNumber;
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
            new Add { DestinationReg = Registers.EBX, SourceValue = 4 };
            new Move {
                DestinationReg = Registers.EAX,
                SourceReg = Registers.EBX,
                SourceIsIndirect = true
            };
            new Move { DestinationRef = new ElementReference("MultiBootInfo_Memory_Low"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
            new Add { DestinationReg = Registers.EBX, SourceValue = 4 };
            new Move {
                DestinationReg = Registers.EAX,
                SourceReg = Registers.EBX,
                SourceIsIndirect = true
            };
            new Move { DestinationRef = new ElementReference("MultiBootInfo_Memory_High"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
            new Move {
                DestinationReg = Registers.ESP,
                SourceRef = new ElementReference("Kernel_Stack")
            };
            new Comment("some more startups todo");
            new ClrInterruptFlag();
            if (mComNumber > 0) {
                UInt16 xComAddr = mComPortAddresses[mComNumber - 1];
                // 9600 baud, 8 databits, no parity, 1 stopbit
                new Move { DestinationReg = Registers.DX, SourceValue = (uint)xComAddr + 1 };
                new Move { DestinationReg = Registers.AL, SourceValue = 0 };
                new Out { DestinationReg = Registers.AL }; // disable interrupts for serial stuff
                new Move { DestinationReg = Registers.DX, SourceValue = (uint)xComAddr + 3 };
                new Move { DestinationReg = Registers.AL, SourceValue = 0x80 };
                new Out { DestinationReg =Registers.AL }; // Enable DLAB (set baud rate divisor)
                new Move { DestinationReg = Registers.DX, SourceValue = (uint)xComAddr };
                new Move { DestinationReg = Registers.AL, SourceValue = 0xC };
                new Out { DestinationReg = Registers.AL }; // Set divisor (lo byte)
                new Move { DestinationReg = Registers.DX, SourceValue = (uint)xComAddr + 1 };
                new Move { DestinationReg = Registers.AL, SourceValue = 0x0 };
                new Out { DestinationReg =Registers.AL }; //			  (hi byte)
                new Move { DestinationReg = Registers.DX, SourceValue = (uint)xComAddr + 3 };
                new Move { DestinationReg = Registers.AL, SourceValue = 0x3 };
                new Out { DestinationReg =Registers.AL }; // 8 bits, no parity, one stop bit
                new Move { DestinationReg = Registers.DX, SourceValue = (uint)xComAddr + 2 };
                new Move { DestinationReg = Registers.AL, SourceValue = 0xC7 };
                new Out { DestinationReg =Registers.AL }; // Enable FIFO, clear them, with 14-byte threshold
                new Move { DestinationReg = Registers.DX, SourceValue = (uint)xComAddr + 4 };
                new Move { DestinationReg = Registers.AL, SourceValue = 0x3 };
                new Out { DestinationReg =Registers.AL }; // IRQ-s enabled, RTS/DSR set
            }

            // SSE init
            // CR4[bit 9]=1, CR4[bit 10]=1, CR0[bit 2]=0, CR0[bit 1]=1
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.CR4 };
            new Or { DestinationReg = Registers.EAX, SourceValue = 0x100 };
            new Move { DestinationReg = Registers.CR4, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.CR4 };
            new Or { DestinationReg = Registers.EAX, SourceValue = 0x200 };
            new Move { DestinationReg = Registers.CR4, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.CR0 };

            new And { DestinationReg = Registers.EAX, SourceValue = 0xfffffffd };
            new Move { DestinationReg = Registers.CR0, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.CR0 };

            new And { DestinationReg = Registers.EAX, SourceValue = 1 };
            new Move { DestinationReg = Registers.CR0, SourceReg = Registers.EAX };

            // END SSE INIT

            new Call { DestinationLabel = EntryPointName };
            new Label(".loop");
            new ClrInterruptFlag();
            new Halt();
            new Jump { DestinationLabel = ".loop" };
            if (mComNumber > 0) {
                var xStub = new DebugStub();
                xStub.Main(mComPortAddresses[mComNumber - 1]);
            }else {
                new Label("DebugStub_Step");
                new Return();
            }
            //aOutputWriter.WriteLine("section .data");
            DataMembers.Add(new DataIfNotDefined("NASM_COMPILATION"));
            uint xFlags = 0x10003;
            DataMembers.Add(new DataMember("MultibootSignature",
                                   new uint[] { 0x1BADB002 }));
            DataMembers.Add(new DataMember("MultibootFlags",
                           xFlags));
            DataMembers.Add(new DataMember("MultibootChecksum",
                                               (int)(0 - (xFlags + 0x1BADB002))));
            DataMembers.Add(new DataMember("MultibootHeaderAddr", new ElementReference("MultibootSignature")));
            DataMembers.Add(new DataMember("MultibootLoadAddr", new ElementReference("MultibootSignature")));
            DataMembers.Add(new DataMember("MultibootLoadEndAddr", 0));
            DataMembers.Add(new DataMember("MultibootBSSEndAddr", 0));
            DataMembers.Add(new DataMember("MultibootEntryAddr", new ElementReference("Kernel_Start")));
            DataMembers.Add(new DataEndIfDefined());
            DataMembers.Add(new DataIfDefined("NASM_COMPILATION"));
            xFlags = 0x00003;
            DataMembers.Add(new DataMember("MultibootSignature",
                                   new uint[] { 0x1BADB002 }));
            DataMembers.Add(new DataMember("MultibootFlags",
                           xFlags));
            DataMembers.Add(new DataMember("MultibootChecksum",
                                               (int)(0 - (xFlags + 0x1BADB002))));
            DataMembers.Add(new DataEndIfDefined());
            DataMembers.Add(new DataMember("MultiBootInfo_Memory_High", 0));
            DataMembers.Add(new DataMember("MultiBootInfo_Memory_Low", 0));
            DataMembers.Add(new DataMember("Before_Kernel_Stack",
                           new byte[0x50000]));
            DataMembers.Add(new DataMember("Kernel_Stack",
                           new byte[0]));
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
            //aOutput.WriteLine("use32");
            //aOutput.WriteLine("%define NASM_COMPILATION 1");
            //aOutput.WriteLine("global Kernel_Start");
            aOutput.WriteLine("[map all main.map]");
            //aOutput.WriteLine("org 0x500000");
            base.FlushText(aOutput);
        }
    }
}