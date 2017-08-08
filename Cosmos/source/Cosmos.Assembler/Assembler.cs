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

namespace Cosmos.Assembler
{
  public class Assembler
  {
    public Assembler()
    {
      mCurrentInstance = this;
      mInstances.Push(this);
    }

    public Assembler(bool addWhitespaceWhileFlushing) : this()
    {
      mAddWhitespaceWhileFlushing = addWhitespaceWhileFlushing;
    }

    private static readonly Stack<Assembler> mInstances = new Stack<Assembler>();

    public bool EmitAsmLabels { get; set; }

    protected UInt16 mGdCode;
    protected UInt16 mGdData;

    // Contains info on the current stack structure. What type are on the stack, etc
    //public readonly StackContents Stack = new StackContents();

    // This is a hack, hope to fix it in the future
    // as it will also cause problems when we thread the compiler
    private static Assembler mCurrentInstance;


    protected string mCurrentIlLabel;

    public string CurrentIlLabel
    {
      get
      {
        return mCurrentIlLabel;
      }
      set
      {
        mCurrentIlLabel = value;
        mAsmIlIdx = 0;
      }
    }

    protected int mAsmIlIdx;

    public int AsmIlIdx
    {
      get
      {
        return mAsmIlIdx;
      }
    }

    protected List<DataMember> mDataMembers = new List<DataMember>();

    public List<DataMember> DataMembers
    {
      get
      {
        return mDataMembers;
      }
      set
      {
        mDataMembers = value;
      }
    }

    protected internal List<Instruction> mInstructions = new List<Instruction>();

    public List<Instruction> Instructions
    {
      get
      {
        return mInstructions;
      }
      set
      {
        mInstructions = value;
      }
    }

    public static Assembler CurrentInstance
    {
      get
      {
        return mCurrentInstance;
      }
    }

    internal int AllAssemblerElementCount
    {
      get
      {
        return mInstructions.Count + mDataMembers.Count;
      }
    }

    public BaseAssemblerElement GetAssemblerElement(int aIndex)
    {
      if (aIndex >= mInstructions.Count)
      {
        return mDataMembers[aIndex - mInstructions.Count];
      }
      return mInstructions[aIndex];
    }

    public BaseAssemblerElement TryResolveReference(Cosmos.Assembler.ElementReference aReference)
    {
      foreach (var xInstruction in mInstructions)
      {
        var xLabel = xInstruction as Label;
        if (xLabel != null)
        {
          if (xLabel.QualifiedName.Equals(aReference.Name, StringComparison.OrdinalIgnoreCase))
          {
            return xLabel;
          }
        }
      }
      foreach (var xDataMember in mDataMembers)
      {
        if (xDataMember.Name.Equals(aReference.Name, StringComparison.OrdinalIgnoreCase))
        {
          return xDataMember;
        }
      }
      return null;
    }

    public void Add(Instruction aReader)
    {
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

    public void Add(params Instruction[] aReaders)
    {
      mInstructions.Capacity += aReaders.Length;
      foreach (Instruction xInstruction in aReaders)
      {
        mInstructions.Add(xInstruction);
      }
    }

    // Allows to emit footers to the code and datamember sections
    protected virtual void OnBeforeFlush()
    {

    }

    private uint mDataMemberCounter = 0;

    public string GetIdentifier(string aPrefix)
    {
      mDataMemberCounter++;
      return aPrefix + mDataMemberCounter.ToString("X4");
    }

    private bool mFlushInitializationDone = false;

    protected void BeforeFlush()
    {
      if (mFlushInitializationDone)
      {
        return;
      }
      mFlushInitializationDone = true;
      OnBeforeFlush();
      //MergeAllElements();
    }

    public virtual void FlushBinary(Stream aOutput, ulong aBaseAddress)
    {
      BeforeFlush();
      var xMax = AllAssemblerElementCount;
      var xCurrentAddresss = aBaseAddress;
      for (int i = 0; i < xMax; i++)
      {
        GetAssemblerElement(i).UpdateAddress(this, ref xCurrentAddresss);
      }
      aOutput.SetLength(aOutput.Length + (long) (xCurrentAddresss - aBaseAddress));
      for (int i = 0; i < xMax; i++)
      {
        var xItem = GetAssemblerElement(i);
        if (!xItem.IsComplete(this))
        {
          throw new Exception("Incomplete element encountered.");
        }
        //var xBuff = xItem.GetData(this);
        //aOutput.Write(xBuff, 0, xBuff.Length);
        xItem.WriteData(this, aOutput);
      }
    }

    public virtual void FlushText(TextWriter aOutput)
    {
      BeforeFlush();
      BeforeFlushText(aOutput);
      // Write out data declarations
      aOutput.WriteLine();
      foreach (DataMember xMember in mDataMembers)
      {
        if (mAddWhitespaceWhileFlushing)
        {
          aOutput.Write("\t");
        }
        if (xMember.IsComment)
        {
          aOutput.Write(xMember.Name);
        }
        else
        {
          xMember.WriteText(this, aOutput);
        }
        aOutput.WriteLine();
      }
      aOutput.WriteLine();

      // Write out code
      for (int i = 0; i < mInstructions.Count; i++)
      {
        var xOp = mInstructions[i];
        string prefix = null;
        if (mAddWhitespaceWhileFlushing)
        {
          prefix = "\t\t\t";
        }
        if (xOp is Label)
        {
          var xLabel = (Label) xOp;
          aOutput.WriteLine();
          if (mAddWhitespaceWhileFlushing)
          {
            prefix = "\t\t";
          }
          aOutput.Write(prefix);
          xLabel.WriteText(this, aOutput);
          aOutput.WriteLine();
        }
        else
        {
          aOutput.Write(prefix);
          xOp.WriteText(this, aOutput);
          aOutput.WriteLine();
        }
      }
      OnFlushTextAfterEmitEverything(aOutput);
      aOutput.Flush();
    }

    private bool mAddWhitespaceWhileFlushing = true;

    protected virtual void BeforeFlushText(TextWriter aOutput)
    {

    }

    protected virtual void OnFlushTextAfterEmitEverything(TextWriter aOutput)
    {

    }

    public static void ClearCurrentInstance()
    {
      var xPopped = mInstances.Pop();
      if (xPopped != mCurrentInstance)
      {
        throw new InvalidOperationException("Popped assembler isn't the correct one!");
      }
      if (mInstances.Count == 0)
      {
        mCurrentInstance = null;
      }
      else
      {
        mCurrentInstance = mInstances.Peek();
      }
    }
  }
}
