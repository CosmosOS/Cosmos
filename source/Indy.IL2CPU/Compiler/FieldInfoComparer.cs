using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.Compiler
{
    public class FieldInfoComparer : IComparer<FieldInfo>
    {
        #region IComparer<FieldInfo> Members

        public int Compare(FieldInfo x,
                           FieldInfo y)
        {
            return x.GetFullName().CompareTo(y.GetFullName());
        }

        #endregion
    }
}