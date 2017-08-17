using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public class ArrayTypeMeta: TypeMeta
    {
        public TypeMeta ElementType
        {
            get;
            set;
        }

        public int Dimensions
        {
            get;
            set;
        }
    }
}