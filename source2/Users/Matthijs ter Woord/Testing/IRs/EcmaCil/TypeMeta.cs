using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilityClasses;

namespace EcmaCil
{
    public class TypeMeta: BaseMeta
    {
        public TypeMeta()
            : base()
        {
            //Methods = new SimpleHashSet<MethodMeta>(BaseMetaComparer<MethodMeta>.Instance);
            //Fields = new SimpleHashSet<FieldMeta>(BaseMetaComparer<FieldMeta>.Instance);
            Methods = new List<MethodMeta>();
            Fields = new List<FieldMeta>();
        }

        public TypeMeta BaseType
        {
            get;
            set;
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