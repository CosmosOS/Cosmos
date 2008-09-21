using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using System.Resources;
using System.Globalization;

namespace Indy.IL2CPU.IL.CustomImplementations.System.Resources
{
    [Plug(TargetName="System.Resources.ResourceManager, mscorlib")]
    public static class ResourceManagerImpl
    {
        public static ResourceSet
          InternalGetResourceSet( ResourceManager rm,
            CultureInfo ci, bool createIfNotExists, bool tryParents)

        {
            throw new Exception("System.Resources.ResourceManager.InternalGetResourceSet need pluging properly");
        }

    }
}
