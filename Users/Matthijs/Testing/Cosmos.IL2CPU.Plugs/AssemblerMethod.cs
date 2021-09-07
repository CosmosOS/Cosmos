using System;
using System.Collections.Generic;
using System.Text;

namespace IL2CPU.API {
    public abstract class AssemblerMethod
    {
        public abstract void AssembleNew(object aAssembler, object aMethodInfo);
    }
}