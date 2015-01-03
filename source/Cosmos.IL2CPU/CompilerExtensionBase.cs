using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.IL2CPU
{
  public abstract class CompilerExtensionBase
  {
    public virtual bool TryCreateAppAssembler(byte debugCom, string assemblerLog, out AppAssembler result)
    {
      result = null;
      return false;
    }
  }
}
