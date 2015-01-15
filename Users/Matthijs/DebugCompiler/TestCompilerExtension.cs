using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.IL2CPU;

namespace DebugCompiler
{
    public class TestCompilerExtension: CompilerExtensionBase
    {
        public override bool TryCreateAppAssembler(byte debugCom, string assemblerLog, out AppAssembler result)
        {
            return base.TryCreateAppAssembler(debugCom, assemblerLog, out result);
        }
    }
}
