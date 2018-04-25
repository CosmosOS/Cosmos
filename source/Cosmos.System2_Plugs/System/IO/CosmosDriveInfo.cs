//#define COSMOSDEBUG
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(DriveInfo))]
    public static class DriveInfoImpl
    {
        public static string NormalizeDriveName(string driveName)
        {
            string name;

            if (driveName.Length == 1)
                name = driveName + ":\\";
            else
            {
                name = Path.GetPathRoot(driveName);
                // Disallow null or empty drive letters and UNC paths
                if (name == null || name.Length == 0 || name.StartsWith("\\\\", StringComparison.Ordinal))
                    throw new ArgumentException("Argument must be drive identifier or root dir");
            }
            // We want to normalize to have a trailing backslash so we don't have two equivalent forms and
            // because some Win32 API don't work without it.
            if (name.Length == 2 && name[1] == ':')
            {
                name = name + "\\";
            }

            if (!VFSManager.IsValidDriveId(name))
            {
                throw new ArgumentException("Argument must be drive identifier or root dir");
            }

            return name;
        }

        public static long get_AvailableFreeSpace(DriveInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"Getting Available Free Space of {aThis.Name}");

            return VFSManager.GetAvailableFreeSpace(aThis.Name);
        }

        public static long get_TotalFreeSpace(DriveInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"Getting Total Free Space of {aThis.Name}");

            return VFSManager.GetTotalFreeSpace(aThis.Name);
        }

        public static long get_TotalSize(DriveInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"Getting size of {aThis.Name}");

            return VFSManager.GetTotalSize(aThis.Name);
        }

        public static string get_DriveFormat(DriveInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"Getting format of {aThis.Name}");

            return VFSManager.GetFileSystemType(aThis.Name);
        }

        public static string get_VolumeLabel(DriveInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"Getting label of {aThis.Name}");

            return VFSManager.GetFileSystemLabel(aThis.Name);
        }

        public static void set_VolumeLabel(DriveInfo aThis, string aLabel)
        {
            Global.mFileSystemDebugger.SendInternal($"Setting label of {aThis.Name} with {aLabel}");

            VFSManager.SetFileSystemLabel(aThis.Name, aLabel);
        }

        /* For now I'm forcing IsReady to be always true as only fixed drives are supported in Cosmos for now */
        public static bool get_IsReady(DriveInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"Getting isReady status of {aThis.Name}");
            return true;
        }

        /* For now I'm forcing DriveType to always be 'Fixed' as only fixed drives are supported in Cosmos for now */
        public static DriveType get_DriveType(DriveInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"Getting DriveType of {aThis.Name}");
            return DriveType.Fixed;
        }

        public static DriveInfo[] GetDrives()
        {
            Global.mFileSystemDebugger.SendInternal("GetDrives called");

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
