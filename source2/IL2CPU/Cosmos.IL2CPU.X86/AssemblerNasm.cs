using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using System.Reflection;
using System.IO;
using Cosmos.Compiler.Assembler;
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;
using Cosmos.Debug.Common.CDebugger;
using Cosmos.Build.Common;

namespace Cosmos.IL2CPU.X86 {
    public class AssemblerNasm : CosmosAssembler
    {

    public AssemblerNasm(byte aComNumber) : base(aComNumber) { }

    public override void FlushText(TextWriter aOutput)
    {
        aOutput.WriteLine("%ifndef ELF_COMPILATION");
        {
            aOutput.WriteLine("use32");
            aOutput.WriteLine("org 0x200000");
            aOutput.WriteLine("[map all main.map]");
        }
        aOutput.WriteLine("%endif");
        aOutput.WriteLine("global Kernel_Start");
        base.FlushText(aOutput);
    }

    public bool EmitELF
    {
        get;
        set;
    }

  }
}