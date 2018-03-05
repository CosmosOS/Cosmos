using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtilityClasses
{
    public class HashSetUsingEquatable<T> where T: IEquatable<T>
    {
        private Dictionary<int, T> mBackend = new Dictionary<int, T>();
    }
}