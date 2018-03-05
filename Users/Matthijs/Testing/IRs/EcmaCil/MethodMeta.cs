using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil
{
    public class MethodMeta: BaseMemberMeta
    {
        public MethodMeta()
            : base()
        {
            Parameters = new MethodParameterMeta[0];
        }
        public MethodParameterMeta[] Parameters
        {
            get;
            set;
        }

        public TypeMeta ReturnType
        {
            get;
            set;
        }

        public MethodBodyMeta Body
        {
            get;
            set;
        }

        public bool IsVirtual
        {
            get;
            set;
        }

        public bool IsPublic
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies whether the method is a base method. in c# it's defined by: "virtual void", or "new virtual void.."
        /// </summary>
        public bool StartsNewVirtualTree
        {
            get;
            set;
        }

        public MethodMeta Overrides
        {
            get;
            set;
        }
    }
}