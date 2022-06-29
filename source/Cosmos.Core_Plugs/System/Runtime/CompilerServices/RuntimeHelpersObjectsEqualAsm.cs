using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp;
using XSharp.Assembler;
using static XSharp.XSRegisters;

namespace Cosmos.Core_Plugs.System.Runtime.CompilerServices
{
    class RuntimeHelpersObjectsEqualAsm : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            // we need
            XS.Set(EAX, EBP, sourceDisplacement: 8);
        }
    }
}
