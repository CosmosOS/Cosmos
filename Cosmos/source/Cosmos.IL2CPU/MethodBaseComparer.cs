using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cosmos.IL2CPU.Extensions;

namespace Cosmos.IL2CPU
{
    public class MethodBaseComparer : IComparer<MethodBase>, IEqualityComparer<MethodBase>
    {
        #region IComparer<MethodBase> Members
        public int Compare(MethodBase x, MethodBase y)
        {
            return x.GetFullName().CompareTo(y.GetFullName());
        }
        #endregion

        public bool Equals(MethodBase x, MethodBase y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(MethodBase obj)
        {
            return obj.GetFullName().GetHashCode();
        }
    }

}
