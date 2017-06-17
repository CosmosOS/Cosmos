using System;
using System.Reflection;

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
            if (!type.GetTypeInfo().IsSubclassOf(typeof(CompilerExtensionBase)))
            {
                throw new ArgumentException("CompilerExtensions should inherit from CompilerExtensionBase!", "type");
            }
            Type = type;
        }
    }
}
