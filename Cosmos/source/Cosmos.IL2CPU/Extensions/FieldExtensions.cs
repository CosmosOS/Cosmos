using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cosmos.IL2CPU.Extensions
{
    public static class FieldExtensions
    {
        public static string GetFullName(this FieldInfo aField)
        {
            return aField.FieldType.GetFullName() + " " + aField.DeclaringType.GetFullName() + "." + aField.Name;
        }
    }
}
