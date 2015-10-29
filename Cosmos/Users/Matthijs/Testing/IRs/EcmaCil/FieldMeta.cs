using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public class FieldMeta: BaseMemberMeta
    {
        public FieldMeta()
            : base()
        {
        }

        public TypeMeta FieldType
        {
            get;
            set;
        }
    }
}