using System.Reflection;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Reflection
{
    [Plug("System.Reflection.RuntimeConstructorInfo, System.Private.CoreLib")]
    class RuntimeConstructorInfoImpl
    {
        public static int get_MetadataToken(object aThis)
        {
            throw new NotImplementedException();
        }

        public static CallingConventions get_CallingConvention(object aThis)
        {
            throw new NotImplementedException();
        }

        public static ParameterInfo[] GetParametersNoCopy(object aThis)
        {
            throw new NotImplementedException();
        }
    }
}