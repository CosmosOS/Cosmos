using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.IL2CPU
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited=false, AllowMultiple=true)]
    public class CompilerExtensionAttribute: Attribute
    {
        public readonly Type Type;

        public CompilerExtensionAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!type.IsSubclassOf(typeof(CompilerExtensionBase)))
            {
                throw new ArgumentException("CompilerExtensions should inherit from CompilerExtensionBase!", "type");
            }
            Type = type;
        }
    }
}
