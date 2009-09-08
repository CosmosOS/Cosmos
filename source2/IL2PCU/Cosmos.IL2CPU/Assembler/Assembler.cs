using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.IO;

namespace Cosmos.IL2CPU {

  public abstract class Assembler {
    protected ILOp[] mILOpsLo = new ILOp[256];
    protected ILOp[] mILOpsHi = new ILOp[256];
    public virtual void Initialize() {
    }
    // Contains info on the current stack structure. What type are on the stack, etc
    public readonly StackContents Stack = new StackContents();

    private static Assembler mCurrentInstance;
    protected internal List<Instruction> mInstructions = new List<Instruction>();
    private List<DataMember> mDataMembers = new List<DataMember>();
    private System.IO.TextWriter mLog;
    #region Properties
    public List<DataMember> DataMembers {
      get { return mDataMembers; }
    }

    public List<Instruction> Instructions {
      get { return mInstructions; }
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
      mLog = new System.IO.StreamWriter("Cosmos.Assembler.Log");
      InitILOps();
      mCurrentInstance = this;
    }

    public static ulong ConstructLabel(uint aMethod, uint aOpCode, byte aSubLabel) {
      /* Explanation:
       * * This method generates labels. labels are 64bit:
       * * First 24 bits (high to low) is the method number
       * * then 32 bits is the opcode offset in the il
       * * then 8 bits for a sub label.
       */
      if (aMethod > 0x00FFFFFF) {
        throw new Exception("Error Method id too high!");
      }
      ulong xResult = aMethod << 40;
      xResult |= aOpCode << 8;
      xResult |= aSubLabel;
      return xResult;
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

    protected virtual void MethodBegin(MethodInfo aMethod) {
      new Comment(this, "---------------------------------------------------------");
      new Comment(this, "Type: " + aMethod.MethodBase.DeclaringType.ToString());
      new Comment(this, "Name: " + aMethod.MethodBase.Name);
      new Comment(this, "Plugged: " + (aMethod.PlugMethod == null ? "No" : "Yes"));
    }

    protected virtual void MethodEnd(MethodInfo aMethod) {
      new Comment(this, "End Method: " + aMethod.MethodBase.Name);
    }

    public void ProcessMethod(MethodInfo aMethod, List<ILOpCode> aOpCodes) {
      // We check this here and not scanner as when scanner makes these
      // plugs may still have not yet been scanned that it will depend on.
      // But by the time we make it here, they have to be resolved.
      if (aMethod.Type == MethodInfo.TypeEnum.NeedsPlug && aMethod.PlugMethod == null) {
        throw new Exception("Method needs plug, but no plug was assigned.");
      }

      // todo: MtW: how to do this? we need some extra space.
      //		see ConstructLabel for extra info
      if (aMethod.UID > 0x00FFFFFF) {
        throw new Exception("For now, too much methods");
      }

      MethodBegin(aMethod);
      foreach (var xOpCode in aOpCodes) {
        uint xOpCodeVal = (uint)xOpCode.OpCode;
        ILOp xILOp;
        if (xOpCodeVal <= 0xFF) {
          xILOp = mILOpsLo[xOpCodeVal];
        } else {
          xILOp = mILOpsHi[xOpCodeVal & 0xFF];
        }
        //mLog.Write ( "[" + xILOp.ToString() + "] \t Stack start: " + Stack.Count.ToString() );
        new Comment(this, "[" + xILOp.ToString() + "]");
        BeforeOp(aMethod, xOpCode);
        xILOp.Execute(aMethod, xOpCode);
        //mLog.WriteLine( " end: " + Stack.Count.ToString() );
        //mLog.Flush(); 
      }
      MethodEnd(aMethod);
    }

    protected virtual void BeforeOp(MethodInfo aMethod, ILOpCode aOpCode) {
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
          xMember.WriteText(this, aOutput);
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

    protected abstract void InitILOps();

    protected virtual void InitILOps(Type aAssemblerBaseOp) {
      foreach (var xType in aAssemblerBaseOp.Assembly.GetExportedTypes()) {
        if (xType.IsSubclassOf(aAssemblerBaseOp)) {
          var xAttribs = (OpCodeAttribute[])xType.GetCustomAttributes(typeof(OpCodeAttribute), false);
          foreach (var xAttrib in xAttribs) {
            var xOpCode = (ushort)xAttrib.OpCode;
            var xCtor = xType.GetConstructor(new Type[] { typeof(Assembler) });
            var xILOp = (ILOp)xCtor.Invoke(new Object[] { this });
            if (xOpCode <= 0xFF) {
              mILOpsLo[xOpCode] = xILOp;
            } else {
              mILOpsHi[xOpCode & 0xFF] = xILOp;
            }
          }
        }
      }
    }

  }
}