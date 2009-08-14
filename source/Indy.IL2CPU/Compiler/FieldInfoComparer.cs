using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.Compiler
{
    public class FieldInfoComparer : IComparer<FieldInfo>
    {
        #region IComparer<FieldInfo> Members

        public int Compare(FieldInfo x,
                           FieldInfo y)
        {
					return DataMember.GetStaticFieldName(x).CompareTo(DataMember.GetStaticFieldName(y));
        }

        #endregion
    }
}