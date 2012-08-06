using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CPU = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.X86;
using System.Reflection;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using XSharp.Nasm;
using Cosmos.Compiler.XSharp;

namespace Cosmos.IL2CPU.X86 {
  // Merge with base - started but there are circular refs when merging
  public class CosmosAssembler : Cosmos.Assembler.Assembler {

    public CosmosAssembler(byte aComNumber) {
      mComNumber = aComNumber;
    }

    public void CreateGDT() {
      new Comment(this, "BEGIN - Create GDT");
      var xGDT = new List<byte>();
      // Null Segment - Selector 0x00
      // Not used, but required by many emulators.
      xGDT.AddRange(new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
      // Code Segment
      mGdCode = (byte)xGDT.Count;
      xGDT.AddRange(GdtDescriptor(0x00000000, 0xFFFFFFFF, true));
      // Data Segment - Selector
      mGdData = (byte)xGDT.Count;
      xGDT.AddRange(GdtDescriptor(0x00000000, 0xFFFFFFFF, false));
      DataMembers.Add(new DataMember("_NATIVE_GDT_Contents", xGDT.ToArray()));

      new Comment("Tell CPU about GDT");
      var xGdtPtr = new UInt16[3];
      // Size of GDT Table - 1
      xGdtPtr[0] = (UInt16)(xGDT.Count - 1);
      DataMembers.Add(new DataMember("_NATIVE_GDT_Pointer", xGdtPtr));
      new Mov {
        DestinationRef = Cosmos.Assembler.ElementReference.New("_NATIVE_GDT_Pointer"),
        DestinationIsIndirect = true,
        DestinationDisplacement = 2,
        SourceRef = Cosmos.Assembler.ElementReference.New("_NATIVE_GDT_Contents")
      };
      new Mov { DestinationReg = Registers.EAX, SourceRef = Cosmos.Assembler.ElementReference.New("_NATIVE_GDT_Pointer") };
      new Lgdt { DestinationReg = Registers.EAX, DestinationIsIndirect = true };

      new Comment("Set data segments");
      new Mov { DestinationReg = Registers.EAX, SourceValue = mGdData };
      new Mov { DestinationReg = Registers.DS, SourceReg = Registers.EAX };
      new Mov { DestinationReg = Registers.ES, SourceReg = Registers.EAX };
      new Mov { DestinationReg = Registers.FS, SourceReg = Registers.EAX };
      new Mov { DestinationReg = Registers.GS, SourceReg = Registers.EAX };
      new Mov { DestinationReg = Registers.SS, SourceReg = Registers.EAX };

      new Comment("Force reload of code segment");
      new JumpToSegment { Segment = mGdCode, DestinationLabel = "Boot_FlushCsGDT" };
      new Label("Boot_FlushCsGDT");
      new Cosmos.Assembler.Comment(this, "END - Create GDT");
    }

    protected void SetIdtDescriptor(int aNo, string aLabel, bool aDisableInts) {
      int xOffset = aNo * 8;
      new Mov { DestinationReg = Registers.EAX, SourceRef = Cosmos.Assembler.ElementReference.New(aLabel) };
      var xIDT = Cosmos.Assembler.ElementReference.New("_NATIVE_IDT_Contents");
      new Mov { DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset, SourceReg = Registers.AL };
      new Mov { DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 1, SourceReg = Registers.AH };
      new ShiftRight { DestinationReg = Registers.EAX, SourceValue = 16 };
      new Mov { DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 6, SourceReg = Registers.AL };
      new Mov { DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 7, SourceReg = Registers.AH };

      // Code Segment
      new Mov { DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 2, SourceValue = mGdCode, Size = 16 };

      // Reserved
      new Mov { DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 4, SourceValue = 0x00, Size = 8 };

      // Type
      new Mov { DestinationRef = xIDT, DestinationIsIndirect = true, DestinationDisplacement = xOffset + 5, SourceValue = (byte)(aDisableInts ? 0x8E : 0x8F), Size = 8 };
    }

    public void CreateIDT() {
      new Comment(this, "BEGIN - Create IDT");

      // Create IDT
      UInt16 xIdtSize = 8 * 256;
      DataMembers.Add(new DataMember("_NATIVE_IDT_Contents", new byte[xIdtSize]));
      //
      if (mComNumber > 0) {
        SetIdtDescriptor(1, "DebugStub_TracerEntry", false);
        SetIdtDescriptor(3, "DebugStub_TracerEntry", false);
      }
      //SetIdtDescriptor(1, "DebugStub_INT0"); - Change to GPF

      // Set IDT
      DataMembers.Add(new DataMember("_NATIVE_IDT_Pointer", new UInt16[] { xIdtSize, 0, 0 }));
      new Mov {
        DestinationRef = Cosmos.Assembler.ElementReference.New("_NATIVE_IDT_Pointer"),
        DestinationIsIndirect = true,
        DestinationDisplacement = 2,
        SourceRef = Cosmos.Assembler.ElementReference.New("_NATIVE_IDT_Contents")
      };
      new Mov { DestinationReg = Registers.EAX, SourceRef = Cosmos.Assembler.ElementReference.New("_NATIVE_IDT_Pointer") };
      new Lidt { DestinationReg = Registers.EAX, DestinationIsIndirect = true };

      new Comment(this, "END - Create IDT");
    }

    public override void Initialize() {
      base.Initialize();

      uint xSig = 0x1BADB002;

      DataMembers.Add(new DataIfNotDefined("ELF_COMPILATION"));
      DataMembers.Add(new DataMember("MultibootSignature", new uint[] { xSig }));
      uint xFlags = 0x10003;
      DataMembers.Add(new DataMember("MultibootFlags", xFlags));
      DataMembers.Add(new DataMember("MultibootChecksum", (int)(0 - (xFlags + xSig))));
      DataMembers.Add(new DataMember("MultibootHeaderAddr", Cosmos.Assembler.ElementReference.New("MultibootSignature")));
      DataMembers.Add(new DataMember("MultibootLoadAddr", Cosmos.Assembler.ElementReference.New("MultibootSignature")));
      DataMembers.Add(new DataMember("MultibootLoadEndAddr", Cosmos.Assembler.ElementReference.New("_end_code")));
      DataMembers.Add(new DataMember("MultibootBSSEndAddr", Cosmos.Assembler.ElementReference.New("_end_code")));
      DataMembers.Add(new DataMember("MultibootEntryAddr", Cosmos.Assembler.ElementReference.New("Kernel_Start")));
      DataMembers.Add(new DataEndIfDefined());

      DataMembers.Add(new DataIfDefined("ELF_COMPILATION"));
      xFlags = 0x00003;
      DataMembers.Add(new DataMember("MultibootSignature", new uint[] { xSig }));
      DataMembers.Add(new DataMember("MultibootFlags", xFlags));
      DataMembers.Add(new DataMember("MultibootChecksum", (int)(0 - (xFlags + xSig))));
      DataMembers.Add(new DataEndIfDefined());

      // graphics info fields 
      DataMembers.Add(new DataMember("MultibootGraphicsRuntime_VbeModeInfoAddr", Int32.MaxValue));
      DataMembers.Add(new DataMember("MultibootGraphicsRuntime_VbeControlInfoAddr", Int32.MaxValue));
      DataMembers.Add(new DataMember("MultibootGraphicsRuntime_VbeMode", Int32.MaxValue));
      // memory
      DataMembers.Add(new DataMember("MultiBootInfo_Memory_High", 0));
      DataMembers.Add(new DataMember("MultiBootInfo_Memory_Low", 0));
      DataMembers.Add(new DataMember("Before_Kernel_Stack", new byte[0x50000]));
      DataMembers.Add(new DataMember("Kernel_Stack", new byte[0]));
      DataMembers.Add(new DataMember("MultiBootInfo_Structure", new uint[1]));

      if (mComNumber > 0) {
        new Define("DEBUGSTUB");
      }

      // This is our first entry point. Multiboot uses this as Cosmos entry point.
      new Label("Kernel_Start") { IsGlobal = true };

      // Displays "Cosmos" in top left. Used to make sure Cosmos is booted in case of hang.
      // ie bootloader debugging. This must be the FIRST code, even before setup so we know
      // we are being called properly by the bootloader and that if there are problems its
      // somwhere in our code, not the bootloader.
      WriteDebugVideo("Cosmos pre boot");

      // For when using Bochs, causes a break ASAP on entry after initial Cosmos display.
      new LiteralAssemblerCode("xchg bx, bx");

      // CLI ASAP
      WriteDebugVideo("Clearing interrupts.");
      new ClrInterruptFlag();

      WriteDebugVideo("Begin multiboot info.");
      new Comment(this, "MultiBoot compliant loader provides info in registers: ");
      new Comment(this, "EBX=multiboot_info ");
      new Comment(this, "EAX=0x2BADB002 - check if it's really Multiboot loader ");
      new Comment(this, "                ;- copy mb info - some stuff for you  ");
      new Comment(this, "BEGIN - Multiboot Info");
      new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("MultiBootInfo_Structure"), DestinationIsIndirect = true, SourceReg = Registers.EBX };
      new Add { DestinationReg = Registers.EBX, SourceValue = 4 };
      new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.EBX, SourceIsIndirect = true };
      new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("MultiBootInfo_Memory_Low"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
      new Add { DestinationReg = Registers.EBX, SourceValue = 4 };
      new Mov {
        DestinationReg = Registers.EAX,
        SourceReg = Registers.EBX,
        SourceIsIndirect = true
      };
      new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("MultiBootInfo_Memory_High"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
      new Mov {
        DestinationReg = Registers.ESP,
        SourceRef = Cosmos.Assembler.ElementReference.New("Kernel_Stack")
      };
      new Comment(this, "END - Multiboot Info");

      WriteDebugVideo("Creating GDT.");
      CreateGDT();

      WriteDebugVideo("Creating IDT.");
      CreateIDT();

#if LFB_1024_8
            new Comment("Set graphics fields");
            new Move { DestinationReg = Registers.EBX, SourceRef = Cosmos.Assembler.ElementReference.New("MultiBootInfo_Structure"), SourceIsIndirect = true };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EBX, SourceIsIndirect = true, SourceDisplacement = 72 };
            new Move { DestinationRef = Cosmos.Assembler.ElementReference.New("MultibootGraphicsRuntime_VbeControlInfoAddr"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EBX, SourceIsIndirect = true, SourceDisplacement = 76 };
            new Move { DestinationRef = Cosmos.Assembler.ElementReference.New("MultibootGraphicsRuntime_VbeModeInfoAddr"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EBX, SourceIsIndirect = true, SourceDisplacement = 80 };
            new Move { DestinationRef = Cosmos.Assembler.ElementReference.New("MultibootGraphicsRuntime_VbeMode"), DestinationIsIndirect = true, SourceReg = Registers.EAX };
#endif

      WriteDebugVideo("Initializing SSE.");
      new Comment(this, "BEGIN - SSE Init");
      // CR4[bit 9]=1, CR4[bit 10]=1, CR0[bit 2]=0, CR0[bit 1]=1
      new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.CR4 };
      new Or { DestinationReg = Registers.EAX, SourceValue = 0x100 };
      new Mov { DestinationReg = Registers.CR4, SourceReg = Registers.EAX };
      new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.CR4 };
      new Or { DestinationReg = Registers.EAX, SourceValue = 0x200 };
      new Mov { DestinationReg = Registers.CR4, SourceReg = Registers.EAX };
      new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.CR0 };

      new And { DestinationReg = Registers.EAX, SourceValue = 0xfffffffd };
      new Mov { DestinationReg = Registers.CR0, SourceReg = Registers.EAX };
      new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.CR0 };

      new And { DestinationReg = Registers.EAX, SourceValue = 1 };
      new Mov { DestinationReg = Registers.CR0, SourceReg = Registers.EAX };
      new Comment(this, "END - SSE Init");

      if (mComNumber > 0) {
        WriteDebugVideo("Initializing DebugStub.");
        new Call { DestinationLabel = "DebugStub_Init" };
      }

      // Jump to Kernel entry point
      WriteDebugVideo("Jumping to kernel.");
      new Call { DestinationLabel = EntryPointName };

      new Comment(this, "Kernel done - loop till next IRQ");
      new Label(".loop");
      new ClrInterruptFlag();
      new Halt();
      new Jump { DestinationLabel = ".loop" };

      if (mComNumber > 0) {
        var xGen = new AsmGenerator();
        foreach (var xFile in Directory.GetFiles(Cosmos.Build.Common.CosmosPaths.DebugStubSrc, "*.xs")) {
          var xAsm = xGen.Generate(xFile);
          foreach (var xData in xAsm.Data) {
            Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember() { RawAsm = xData });
          }
          foreach (var xCode in xAsm.Code) {
            new LiteralAssemblerCode(xCode);
          }
        }
      } else {
        new Label("DebugStub_Step");
        new Return();
      }
      // Start emitting assembly labels
      Cosmos.Assembler.Assembler.CurrentInstance.EmitAsmLabels = true;
    }

  }
}
