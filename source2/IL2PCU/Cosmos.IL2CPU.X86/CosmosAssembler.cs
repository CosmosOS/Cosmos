using System;
using System.Collections.Generic;
using System.IO;
using CPU = Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.X86;
using System.Reflection;
using Cosmos.IL2CPU.X86;
using Indy.IL2CPU.Compiler;
using Cosmos.IL2CPU.ILOpCodes;
using Indy.IL2CPU;

namespace Cosmos.IL2CPU.X86 {
  // TODO: I think we need to later elminate this class
  // Much of it is left over from the old build stuff, and info 
  // here actually belongs else where, not in the assembler
  public abstract class CosmosAssembler : Cosmos.IL2CPU.Assembler 
  {
              //TODO: COM Port info - should be in assembler? Assembler should not know about comports...
		protected byte mComNumber = 0;
		protected UInt16[] mComPortAddresses = { 0x3F8, 0x2F8, 0x3E8, 0x2E8 };

        public CosmosAssembler( byte aComNumber )
        {
			mComNumber = aComNumber;
            
		}

		private static string GetValidGroupName(string aGroup) {
			return aGroup.Replace('-','_').Replace('.', '_');
		}
        public const string EntryPointName = "__ENGINE_ENTRYPOINT__";
        public override void Initialize() {
            base.Initialize();
            if (mComNumber > 0) {
                new Define("DEBUGSTUB");
            }
            new Label("Kernel_Start");
            new Comment(this, "MultiBoot-compliant loader (e.g. GRUB or X.exe) provides info in registers: ");
            new Comment( this, "EBX=multiboot_info " );
            new Comment( this, "EAX=0x2BADB002 - check if it's really Multiboot loader " );
            new Comment( this, "                ;- copy mb info - some stuff for you  " );
            new Add { DestinationReg = Registers.EBX, SourceValue = 4 };
            new Move {
                DestinationReg = Registers.EAX,
                SourceReg = Registers.EBX,
                SourceIsIndirect = true
            };
            new Move { DestinationRef = ElementReference.New("MultiBootInfo_Memory_Low"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
            new Add { DestinationReg = Registers.EBX, SourceValue = 4 };
            new Move {
                DestinationReg = Registers.EAX,
                SourceReg = Registers.EBX,
                SourceIsIndirect = true
            };
            new Move { DestinationRef = ElementReference.New( "MultiBootInfo_Memory_High" ), DestinationIsIndirect = true, SourceReg = Registers.EAX };
            new Move {
                DestinationReg = Registers.ESP,
                SourceRef = ElementReference.New( "Kernel_Stack" )
            };
            new Comment( this, "some more startups todo" );
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
            DataMembers.Add(new DataMember("MultibootHeaderAddr", ElementReference.New("MultibootSignature")));
            DataMembers.Add(new DataMember("MultibootLoadAddr", ElementReference.New("MultibootSignature")));
            DataMembers.Add(new DataMember("MultibootLoadEndAddr", 0));
            DataMembers.Add(new DataMember("MultibootBSSEndAddr", 0));
            DataMembers.Add(new DataMember("MultibootEntryAddr", ElementReference.New("Kernel_Start")));
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
            new Label("__ENGINE_ENTRYPOINT__");
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
            //aOutput.WriteLine("%define NASM_COMPILATION 1");
            //aOutput.WriteLine("global Kernel_Start");
            //aOutput.WriteLine("[map all main.map]");
            aOutput.WriteLine("org 0x200000");
            base.FlushText(aOutput);
        }

        protected override void Move(string aDestLabelName, int aValue) {
          new Move {
            DestinationRef = ElementReference.New(aDestLabelName),
            DestinationIsIndirect = true,
            SourceValue = (uint)aValue
          };
        }

        protected override void Push(uint aValue) {
          new Push {
            DestinationValue = aValue
          };
        }

        protected override void Push(string aLabelName) {
          new Push {
            DestinationRef = ElementReference.New(aLabelName)
          };
        }

        protected override void Call(MethodBase aMethod) {
          new IL2CPU.X86.Call {
            DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(aMethod)
          };
        }

        protected override void Jump(string aLabelName) {
          new IL2CPU.X86.Jump {
            DestinationLabel = aLabelName
          };
        }

        protected override int GetVTableEntrySize() {
          return 16; // todo: retrieve from actual type info
        }
  }
}
