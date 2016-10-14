using System;

namespace Cosmos.IL2CPU.Plugs
{
    /// <summary>
    /// This attribute is used on plug parameters, that need the unsafe pointer to an object's data area
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ObjectPointerAccessAttribute: Attribute
    {

    }
}
