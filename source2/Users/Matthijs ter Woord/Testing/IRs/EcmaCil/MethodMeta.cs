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

        /// <summary>
        /// Specifies whether the method is a base method. in c# it's defined by: "public virtual void", or "public new virtual void.."
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