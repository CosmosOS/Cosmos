// please leave the next directive (and related code) in, just disable the directive
//#define VMT_DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using Cosmos.Assembler.x86;

namespace Cosmos.Assembler {
  public class Assembler {
    public Assembler(int aComPort) {
      mCurrentInstance = this;
      mComPort = aComPort;
    }

    public bool EmitAsmLabels { get; set; }
    //TODO: COM Port info - should be in assembler? Assembler should not know about comports...
    protected int mComPort = 0;
    protected UInt16 mGdCode;
    protected UInt16 mGdData;

    // Contains info on the current stack structure. What type are on the stack, etc
    //public readonly StackContents Stack = new StackContents();

    // This is a hack, hope to fix it in the future
    // as it will also cause problems when we thread the compiler
    private static Assembler mCurrentInstance;

    private static string GetValidGroupName(string aGroup) {
      return aGroup.Replace('-', '_').Replace('.', '_');
    }
    public const string EntryPointName = "__ENGINE_ENTRYPOINT__";

    protected byte[] GdtDescriptor(UInt32 aBase, UInt32 aSize, bool aCode) {
      // Limit is a confusing word. Is it the max physical address or size?
      // In fact it is the size, and 286 docs actually refer to it as size
      // rather than limit.
      // It is also size - 1, else there would be no way to specify
      // all of RAM, and a limit of 0 is invalid.

      var xResult = new byte[8];

      // Check the limit to make sure that it can be encoded
      if ((aSize > 65536) && (aSize & 0x0FFF) != 0x0FFF) {
        // If larger than 16 bit, must be an even page (4kb) size
        throw new Exception("Invalid size in GDT descriptor.");
      }
      // Flags nibble
      // 7: Granularity
      //    0 = bytes
      //    1 = 4kb pages
      // 6: 1 = 32 bit mode
      // 5: 0 - Reserved
      // 4: 0 - Reserved
      xResult[6] = 0x40;
      if (aSize > 65536) {
        // Set page sizing instead of byte sizing
        aSize = aSize >> 12;
        xResult[6] = (byte)(xResult[6] | 0x80);
      }

      xResult[0] = (byte)(aSize & 0xFF);
      xResult[1] = (byte)((aSize >> 8) & 0xFF);
      xResult[6] = (byte)(xResult[6] | ((aSize >> 16) & 0x0F));

      xResult[2] = (byte)(aBase & 0xFF);
      xResult[3] = (byte)((aBase >> 8) & 0xFF);
      xResult[4] = (byte)((aBase >> 16) & 0xFF);
      xResult[7] = (byte)((aBase >> 24) & 0xFF);

      xResult[5] = (byte)(
        // Bit 7: Present, must be 1
        0x80 |
        // Bit 6-5: Privilege, 0=kernel, 3=user
        0x00 |
        // Reserved, must be 1
        0x10 |
        // Bit 3: 1=Code, 0=Data
        (aCode ? 0x08 : 0x00) |
        // Bit 2: Direction/Conforming
        0x00 |
        // Bit 1: R/W  Data (1=Writeable, 0=Read only) Code (1=Readable, 0=Not readable)
        0x02 |
        // Bit 0: Accessed - Set to 0. Updated by CPU later.
        0x00
        );

      return xResult;
    }

    protected string mCurrentIlLabel;
    public string CurrentIlLabel {
      get { return mCurrentIlLabel; }
      set {
        mCurrentIlLabel = value;
        mAsmIlIdx = 0;
      }
    }

    protected int mAsmIlIdx;
    public int AsmIlIdx {
      get { return mAsmIlIdx; }
    }

    protected List<DataMember> mDataMembers = new List<DataMember>();
    public List<DataMember> DataMembers {
      get { return mDataMembers; }
      set { mDataMembers = value; }
    }

    protected internal List<Instruction> mInstructions = new List<Instruction>();
    public List<Instruction> Instructions {
      get { return mInstructions; }
      set { mInstructions = value; }
    }

    public static Assembler CurrentInstance {
      get { return mCurrentInstance; }
    }

    internal int AllAssemblerElementCount {
      get { return mInstructions.Count + mDataMembers.Count; }
    }

    public BaseAssemblerElement GetAssemblerElement(int aIndex) {
      if (aIndex >= mInstructions.Count) {
        return mDataMembers[aIndex - mInstructions.Count];
      }
      return mInstructions[aIndex];
    }

    public BaseAssemblerElement TryResolveReference(Cosmos.Assembler.ElementReference aReference) {
      foreach (var xInstruction in mInstructions) {
        var xLabel = xInstruction as Label;
        if (xLabel != null) {
          if (xLabel.QualifiedName.Equals(aReference.Name, StringComparison.InvariantCultureIgnoreCase)) {
            return xLabel;
          }
        }
      }
      foreach (var xDataMember in mDataMembers) {
        if (xDataMember.Name.Equals(aReference.Name, StringComparison.InvariantCultureIgnoreCase)) {
          return xDataMember;
        }
      }
      return null;
    }

    public void Add(Instruction aReader) {
        if (EmitAsmLabels)
        {
          if (!(aReader is Label)
              && !(aReader is Comment))
          {
            // Only issue label if its executable code.
            new Label("." + AsmIlIdx.ToString("X2"), "Asm");
            mAsmIlIdx++;
          }
        }
        mInstructions.Add(aReader);
    }

    public void Add(params Instruction[] aReaders) {
      mInstructions.Capacity += aReaders.Length;
      foreach (Instruction xInstruction in aReaders) {
        mInstructions.Add(xInstruction);
      }
    }

    // Allows to emit footers to the code and datamember sections
    protected virtual void OnBeforeFlush() {
      DataMembers.AddRange(new DataMember[] { new DataMember("_end_data", new byte[0]) });
      new Label("_end_code");
    }

    private uint mDataMemberCounter = 0;
    public string GetIdentifier(string aPrefix) {
      mDataMemberCounter++;
      return aPrefix + mDataMemberCounter.ToString("X4");
    }
    private bool mFlushInitializationDone = false;
    protected void BeforeFlush() {
      if (mFlushInitializationDone) {
        return;
      }
      mFlushInitializationDone = true;
      OnBeforeFlush();
      //MergeAllElements();
    }

    public virtual void FlushBinary(Stream aOutput, ulong aBaseAddress) {
      BeforeFlush();
      var xMax = AllAssemblerElementCount;
      var xCurrentAddresss = aBaseAddress;
      for (int i = 0; i < xMax; i++) {
        GetAssemblerElement(i).UpdateAddress(this, ref xCurrentAddresss);
      }
      aOutput.SetLength(aOutput.Length + (long)(xCurrentAddresss - aBaseAddress));
      for (int i = 0; i < xMax; i++) {
        var xItem = GetAssemblerElement(i);
        if (!xItem.IsComplete(this)) {
          throw new Exception("Incomplete element encountered.");
        }
        //var xBuff = xItem.GetData(this);
        //aOutput.Write(xBuff, 0, xBuff.Length);
        xItem.WriteData(this, aOutput);
      }
    }

    public virtual void FlushText(TextWriter aOutput) {
      BeforeFlush();

      aOutput.WriteLine("%ifndef ELF_COMPILATION");
      aOutput.WriteLine("use32");
      aOutput.WriteLine("org 0x1000000");
      aOutput.WriteLine("[map all main.map]");
      aOutput.WriteLine("%endif");

      // Write out data declarations
      aOutput.WriteLine();
      foreach (DataMember xMember in mDataMembers) {
        aOutput.Write("\t");
        if (xMember.IsComment) {
          aOutput.Write(xMember.Name);
        } else {
          xMember.WriteText(this, aOutput);
        }
        aOutput.WriteLine();
      }
      aOutput.WriteLine();

      // Write out code
      for (int i = 0; i < mInstructions.Count; i++) {
        var xOp = mInstructions[i];
        string prefix = "\t\t\t";
        if (xOp is Label) {
          var xLabel = (Label)xOp;
          aOutput.WriteLine();
          prefix = "\t\t";
          aOutput.Write(prefix);
          xLabel.WriteText(this, aOutput);
          aOutput.WriteLine();
        } else {
          aOutput.Write(prefix);
          xOp.WriteText(this, aOutput);
          aOutput.WriteLine();
        }
     }
      aOutput.WriteLine("global Kernel_Start");
      aOutput.Flush();
    }

    public virtual void WriteDebugVideo(string aText) {
      // This method emits a lot of ASM, but thats what we want becuase
      // at this point we need ASM as simple as possible and completely transparent.
      // No stack changes, no register mods, etc.

      // TODO: Add an option on the debug project properties to turn this off.
      // Also see TokenPatterns.cs Checkpoint in X#
      var xPreBootLogging = true;
      if (xPreBootLogging) {
        UInt32 xVideo = 0xB8000;
        for (UInt32 i = xVideo; i < xVideo + 80 * 2; i = i + 2) {
          new LiteralAssemblerCode("mov byte [0x" + i.ToString("X") + "], 0");
          new LiteralAssemblerCode("mov byte [0x" + (i + 1).ToString("X") + "], 0x02");
        }

        foreach (var xChar in aText) {
          new LiteralAssemblerCode("mov byte [0x" + xVideo.ToString("X") + "], " + (byte)xChar);
          xVideo = xVideo + 2;
        }
      }
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
      if (mComPort > 0) {
        SetIdtDescriptor(1, "DebugStub_TracerEntry", false);
        SetIdtDescriptor(3, "DebugStub_TracerEntry", false);

        for (int i = 0; i < 256; i++)
        {
          if (i == 1 || i == 3)
          {
            continue;
          }

          SetIdtDescriptor(i, "DebugStub_Interrupt_" + i.ToString(), true);
        }
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

    public void Initialize() {
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

      if (mComPort > 0) {
        new Define("DEBUGSTUB");
      }

      // This is our first entry point. Multiboot uses this as Cosmos entry point.
      new Label("Kernel_Start", isGlobal: true);
      new Mov
      {
        DestinationReg = Registers.ESP,
        SourceRef = Cosmos.Assembler.ElementReference.New("Kernel_Stack")
      };

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
      new LiteralAssemblerCode("%ifndef EXCLUDE_MULTIBOOT_MAGIC");
      new Comment(this, "MultiBoot compliant loader provides info in registers: ");
      new Comment(this, "EBX=multiboot_info ");
      new Comment(this, "EAX=0x2BADB002 - check if it's really Multiboot-compliant loader ");
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
      new Comment(this, "END - Multiboot Info");
      new LiteralAssemblerCode("%endif EXCLUDE_MULTIBOOT_MAGIC");
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

      //WriteDebugVideo("Initializing SSE.");
      //new Comment(this, "BEGIN - SSE Init");
      //// CR4[bit 9]=1, CR4[bit 10]=1, CR0[bit 2]=0, CR0[bit 1]=1
      //new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.CR4 };
      //new Or { DestinationReg = Registers.EAX, SourceValue = 0x100 };
      //new Mov { DestinationReg = Registers.CR4, SourceReg = Registers.EAX };
      //new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.CR4 };
      //new Or { DestinationReg = Registers.EAX, SourceValue = 0x200 };
      //new Mov { DestinationReg = Registers.CR4, SourceReg = Registers.EAX };
      //new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.CR0 };

      //new And { DestinationReg = Registers.EAX, SourceValue = 0xfffffffd };
      //new Mov { DestinationReg = Registers.CR0, SourceReg = Registers.EAX };
      //new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.CR0 };

      //new And { DestinationReg = Registers.EAX, SourceValue = 1 };
      //new Mov { DestinationReg = Registers.CR0, SourceReg = Registers.EAX };
      //new Comment(this, "END - SSE Init");

      if (mComPort > 0) {
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

      if (mComPort > 0) {
        var xGen = new XSharp.Compiler.AsmGenerator();
        foreach (var xFile in Directory.GetFiles(Cosmos.Build.Common.CosmosPaths.DebugStubSrc, "*.xs")) {
          var xAsm = xGen.Generate(xFile);
          foreach (var xData in xAsm.Data) {
            Cosmos.Assembler.Assembler.CurrentInstance.DataMembers.Add(new DataMember() { RawAsm = xData });
          }
          foreach (var xCode in xAsm.Code) {
            new LiteralAssemblerCode(xCode);
          }
        }
        OnAfterEmitDebugStub();
      } else {
        new Label("DebugStub_Step");
        new Return();
      }
      // Start emitting assembly labels
      Cosmos.Assembler.Assembler.CurrentInstance.EmitAsmLabels = true;
    }

    protected virtual void OnAfterEmitDebugStub()
    {
      //
    }
  }
}
