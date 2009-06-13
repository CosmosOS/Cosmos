using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Compiler.Old
{
    partial class AssemblyCompiler: Indy.IL2CPU.IL.IServiceProvider
    {
        public T GetService<T>()
        {
            if(typeof(T) == typeof(IMetaDataInfoService)){
                return (T)(object)this;
            }
            throw new Exception("Service '" + typeof(T).FullName + "' not found!");
        }
    }
}