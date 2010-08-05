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

namespace Cosmos.Compiler.Assembler {

  public abstract class Assembler {
    public virtual void Initialize() {
    }
    // Contains info on the current stack structure. What type are on the stack, etc
    public readonly StackContents Stack = new StackContents();

    private static Assembler mCurrentInstance;
    protected internal List<Instruction> mInstructions = new List<Instruction>();
    private List<DataMember> mDataMembers = new List<DataMember>();
    
      #region Properties
    public List<DataMember> DataMembers {
      get {
        return mDataMembers;
      }
    }

    public List<Instruction> Instructions {
      get {
        return mInstructions;
      }
    }
    public static Assembler CurrentInstance {
      get {
        return mCurrentInstance;
      }
    }

    internal int AllAssemblerElementCount {
      get {
        return mInstructions.Count + mDataMembers.Count;
      }
    }

    #endregion

    public Assembler() {
      mCurrentInstance = this;
    }

    public void Dispose() {
      // MtW: I know, IDisposable usage for this isn't really nice, but for now this should be fine.
      //		Anyhow, we need a way to clear the CurrentInstance property
      //mInstructions.Clear();
      //mDataMembers.Clear();
      //if (mAllAssemblerElements != null)
      //{
      //    mAllAssemblerElements.Clear();
      //}
    }

    public BaseAssemblerElement GetAssemblerElement(int aIndex) {
      if (aIndex >= mInstructions.Count) {
        return mDataMembers[aIndex - mInstructions.Count];
      }
      return mInstructions[aIndex];
    }

    public BaseAssemblerElement TryResolveReference(ElementReference aReference) {
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

    public void Add(params Instruction[] aReaders) {
      foreach (Instruction xInstruction in aReaders) {
        mInstructions.Add(xInstruction);
      }
    }


    /// <summary>
    /// allows to emit footers to the code and datamember sections
    /// </summary>
    protected virtual void OnBeforeFlush() {
    }
    private uint mDataMemberCounter = 0;
    public string GetIdentifier(string aPrefix) {
      mDataMemberCounter++;
      return aPrefix + mDataMemberCounter.ToString("X8").ToUpper();
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
      if (mDataMembers.Count > 0) {
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
      }
      if (mInstructions.Count > 0) {
        for (int i = 0; i < mInstructions.Count; i++) {
          //foreach (Instruction x in mInstructions) {
          var x = mInstructions[i];
          string prefix = "\t\t\t";
          Label xLabel = x as Label;
          if (xLabel != null) {
            aOutput.WriteLine();
            if (xLabel.Name[0] == '.') {
              prefix = "\t\t";
            } else {
              prefix = "\t";
            }
            //string xFullName;
            aOutput.Write(prefix);
            x.WriteText(this, aOutput);
            aOutput.WriteLine();
            //aOutput.WriteLine(prefix + Label.FilterStringForIncorrectChars(xFullName) + ":");
            continue;
          }
          aOutput.Write(prefix);
          x.WriteText(this, aOutput);
          aOutput.WriteLine();
        }
      }
    }

  }
}