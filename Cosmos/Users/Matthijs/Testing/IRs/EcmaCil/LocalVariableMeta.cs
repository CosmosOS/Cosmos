using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public class LocalVariableMeta: BaseMeta
    {
        public TypeMeta LocalType
        {
            get;
            set;
        }
    }
}