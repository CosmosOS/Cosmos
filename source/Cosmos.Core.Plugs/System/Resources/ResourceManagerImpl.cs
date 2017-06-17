using System;
using System.Resources;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Resources
{
    [Plug(typeof(ResourceManager))]
    public static class ResourceManagerImpl
    {
        public static void CCtor()
        {
        }


        public static void Ctor(Type aResourceSource)
        {
        }

        public static string GetString(string aString)
        {
            return EnvironmentImpl.GetResourceString(aString);
        }
    }
}
