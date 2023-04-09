using Cosmos.System.FileSystem.VFS;
using IL2CPU.API.Attribs;
using Cosmos.System;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(DriveInfo))]
    public static class DriveInfoImpl
    {
        public static string NormalizeDriveName(string driveName)
        {
            string Name;

            if (driveName.Length == 1)
            {
                Name = driveName + ":\\";
            }
            else
            {
                Name = Path.GetPathRoot(driveName);

                // Disallow null or empty drive letters and UNC paths
                if (Name == null || Name.Length == 0 || Name.StartsWith("\\\\", StringComparison.Ordinal))
                {
                    throw new ArgumentException("Argument must be drive identifier or root dir");
                }
            }
            // We want to normalize to have a trailing backslash so we don't have two equivalent forms and
            // because some Win32 API don't work without it.
            if (Name.Length == 2 && Name[1] == ':')
            {
                Name += '\\';
            }

            if (!VFSManager.IsValidDriveId(Name))
            {
                throw new ArgumentException("Argument must be drive identifier or root dir");
            }

            return Name;
        }

        public static long get_AvailableFreeSpace(DriveInfo aThis)
        {
            Global.FileSystemDebugger.SendInternal($"Getting Available Free Space of {aThis.Name}");

            return VFSManager.GetAvailableFreeSpace(aThis.Name);
        }

        public static long get_TotalFreeSpace(DriveInfo aThis)
        {
            Global.FileSystemDebugger.SendInternal($"Getting Total Free Space of {aThis.Name}");

            return VFSManager.GetTotalFreeSpace(aThis.Name);
        }

        public static long get_TotalSize(DriveInfo aThis)
        {
            Global.FileSystemDebugger.SendInternal($"Getting size of {aThis.Name}");

            return VFSManager.GetTotalSize(aThis.Name);
        }

        public static string get_DriveFormat(DriveInfo aThis)
        {
            Global.FileSystemDebugger.SendInternal($"Getting format of {aThis.Name}");

            return VFSManager.GetFileSystemType(aThis.Name);
        }

        public static string get_VolumeLabel(DriveInfo aThis)
        {
            Global.FileSystemDebugger.SendInternal($"Getting label of {aThis.Name}");

            return VFSManager.GetFileSystemLabel(aThis.Name);
        }

        public static void set_VolumeLabel(DriveInfo aThis, string aLabel)
        {
            Global.FileSystemDebugger.SendInternal($"Setting label of {aThis.Name} with {aLabel}");

            VFSManager.SetFileSystemLabel(aThis.Name, aLabel);
        }

        /* For now I'm forcing IsReady to be always true as only fixed drives are supported in Cosmos for now */
        public static bool get_IsReady(DriveInfo aThis)
        {
            Global.FileSystemDebugger.SendInternal($"Getting isReady status of {aThis.Name}");
            return true;
        }

        /* For now I'm forcing DriveType to always be 'Fixed' as only fixed drives are supported in Cosmos for now */
        public static DriveType get_DriveType(DriveInfo aThis)
        {
            Global.FileSystemDebugger.SendInternal($"Getting DriveType of {aThis.Name}");
            return DriveType.Fixed;
        }

        public static DriveInfo[] GetDrives()
        {
            Global.FileSystemDebugger.SendInternal("GetDrives called");

            List<string> drives = VFSManager.GetLogicalDrives();

            DriveInfo[] result = new DriveInfo[drives.Count];
            for (int i = 0; i < drives.Count; i++)
            {
                result[i] = new DriveInfo(drives[i]);
            }

            return result;
        }
    }
}
