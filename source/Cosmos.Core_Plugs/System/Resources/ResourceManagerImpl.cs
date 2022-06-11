using System;
using System.Resources;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Resources;

[Plug(typeof(ResourceManager))]
public static class ResourceManagerImpl
{
    public static void CCtor()
    {
    }


    public static void Ctor(Type aResourceSource)
    {
    }

    public static string GetString(ResourceManager aThis, string aString) => SRImpl.InternalGetResourceString(aString);
}
