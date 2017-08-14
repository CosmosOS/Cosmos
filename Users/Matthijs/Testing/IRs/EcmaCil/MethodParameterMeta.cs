using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public class MethodParameterMeta : BaseMeta
    {
        public TypeMeta PropertyType
        {
            get;
            set;
        }

        public bool IsByRef
        {
            get;
            set;
        }
    }
}