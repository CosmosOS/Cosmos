using Cosmos.IL2CPU.Plugs;
using System;

namespace Cosmos.System.Plugs.System.Security.Permissions
{
    [Plug(Target = typeof(global::System.Security.Permissions.FileIOPermission))]
    public static class FileIOPermissionPlug
    {
        public static void QuickDemand(global::System.Security.Permissions.FileIOPermissionAccess access, string fullPath, bool checkForDuplicates, bool needFullPath)
        {
        }
        
        // This shows a compiler bug: trying to compile this method crashes the compiler!
        public static global::System.Security.SecurityElement ToXml(global::System.Security.Permissions.FileIOPermission aThis)
        {
            throw new NotImplementedException("FileIOPermission ToXml()");
            // It is indifferent if it returns or throw...
            //return null;
        }
    }
}