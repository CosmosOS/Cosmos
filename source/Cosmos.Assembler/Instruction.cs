using System;
using System.IO;
using System.Linq;

namespace Cosmos.Assembler {
  public abstract class Instruction : BaseAssemblerElement {
    protected string mMnemonic;
    public string Mnemonic {
      get { return mMnemonic; }
    }

    protected int mMethodID;
    public int MethodID {
      get { return mMethodID; }
    }

    protected int mAsmMethodIdx;
    public int AsmMethodIdx {
      get { return mAsmMethodIdx; }
    }

    public override void WriteText(Cosmos.Assembler.Assembler aAssembler, TextWriter aOutput) {
      aOutput.Write(mMnemonic);
    }

    protected Instruction(string mnemonic = null) : this(true) {
    }

    protected Instruction(bool aAddToAssembler, string mnemonic=null) {
      if (aAddToAssembler) {
        Cosmos.Assembler.Assembler.CurrentInstance.Add(this);
      }
        mMnemonic = mnemonic;
        if (mMnemonic == null)
        {
            var xAttribs = GetType().GetCustomAttributes(typeof (OpCodeAttribute), false);
            if (xAttribs != null && xAttribs.Length > 0)
            {
                var xAttrib = (OpCodeAttribute) xAttribs[0];
                mMnemonic = xAttrib.Mnemonic;
            }
        }
    }

    public override ulong? ActualAddress {
      get {
        // TODO: for now, we dont have any data alignment
        return StartAddress;
      }
    }

    public override void UpdateAddress(Cosmos.Assembler.Assembler aAssembler, ref ulong aAddress) {
      base.UpdateAddress(aAssembler, ref aAddress);
    }

    public override bool IsComplete(Assembler aAssembler) {
      throw new NotImplementedException("Method not implemented for instruction " + this.GetType().FullName.Substring(typeof(Instruction).Namespace.Length + 1));
    }

    public override void WriteData(Cosmos.Assembler.Assembler aAssembler, Stream aOutput) {
      throw new NotImplementedException("Method not implemented for instruction " + this.GetType().FullName.Substring(typeof(Instruction).Namespace.Length + 1));
    }
  }
}