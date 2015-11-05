using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilityClasses;
using System.Diagnostics;

namespace EcmaCil
{
#if DEBUG
    [DebuggerDisplay("Type({Data[1]})")]
#endif
    public class TypeMeta: BaseMeta
    {
        private static readonly FieldMeta[] EmptyFields = new FieldMeta[0];
        private static readonly MethodMeta[] EmptyMethods = new MethodMeta[0];
        public TypeMeta()
            : base()
        {
            //Methods = new SimpleHashSet<MethodMeta>(BaseMetaComparer<MethodMeta>.Instance);
            //Fields = new SimpleHashSet<FieldMeta>(BaseMetaComparer<FieldMeta>.Instance);
            Methods = new List<MethodMeta>();
            Fields = new List<FieldMeta>();
        }

        private TypeMeta mBaseType;
        public TypeMeta BaseType
        {
            get
            {
                return mBaseType;
            }
            set
            {
                if (value != mBaseType)
                {
                    if (mBaseType != null)
                    {
                        mBaseType.mDescendants.Remove(this);
                    }
                    mBaseType = value;
                    if (mBaseType != null)
                    {
                        mBaseType.mDescendants.Add(this);
                    }
                }
            }
        }

        private List<TypeMeta> mDescendants = new List<TypeMeta>();
        public IList<TypeMeta> Descendants
        {
            get
            {
                return mDescendants;
            }
        }


        public List<MethodMeta> Methods
        {
            get;
            set;
        }

        public List<FieldMeta> Fields
        {
            get;
            set;
        }
    }
}