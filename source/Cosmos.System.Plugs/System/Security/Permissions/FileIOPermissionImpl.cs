using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Security.Permissions
{
    [Plug(Target = typeof(global::System.Security.Permissions.FileIOPermission))]
    public static class FileIOPermissionImpl
    {
        public static void QuickDemand(global::System.Security.Permissions.FileIOPermissionAccess access, string fullPath, bool checkForDuplicates, bool needFullPath)
        {
        }

        public static global::System.Security.SecurityElement ToXml(global::System.Security.Permissions.FileIOPermission aThis)
        {
            return null;
        }

    }

}
