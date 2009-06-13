using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IServiceProvider=Indy.IL2CPU.IL.IServiceProvider;

namespace Indy.IL2CPU.Compiler
{
    partial class CompilerHelper: IServiceProvider
    {
        #region Implementation of IServiceProvider

        public T GetService<T>()
        {
            if(typeof(T) == typeof(IMetaDataInfoService))
            {
                return (T) (object) this;
            }
            throw new Exception("Service '" +typeof(T).FullName + "' not supported!");
        }

        #endregion
    }
}