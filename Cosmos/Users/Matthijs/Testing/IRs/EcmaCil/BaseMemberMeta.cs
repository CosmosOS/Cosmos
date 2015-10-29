using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public class BaseMemberMeta: BaseMeta
    {
        protected BaseMemberMeta()
            : base()
        {
        }

        public TypeMeta DeclaringType
        {
            get;
            set;
        }

        public bool IsStatic
        {
            get;
            set;
        }
    }
}
