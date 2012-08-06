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

namespace Cosmos.Assembler {
  public abstract class Assembler {
    public virtual void Initialize() { }
    public bool EmitAsmLabels { get; set; }
    //TODO: COM Port info - should be in assembler? Assembler should not know about comports...
    protected byte mComNumber = 0;
    protected UInt16 mGdCode;
    protected UInt16 mGdData;
    
    // Contains info on the current stack structure. What type are on the stack, etc
    public readonly StackContents Stack = new StackContents();

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

    protected Assembler() {
      mCurrentInstance = this;
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
      if (aReader is Label || aReader is Comment) {
      } else {
        if (EmitAsmLabels) {
          // Only issue label if its executable code.
          // Also above if statement will prevent this new label
          // from causing a stack overflow
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
    protected void OnBeforeFlush() {
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

      aOutput.WriteLine("%ifndef ELF_COMPILATION");
      aOutput.WriteLine("use32");
      aOutput.WriteLine("org 0x200000");
      aOutput.WriteLine("[map all main.map]");
      aOutput.WriteLine("%endif");
      aOutput.WriteLine("global Kernel_Start");
    }

    static public void WriteDebugVideo(string aText) {
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

  }
}