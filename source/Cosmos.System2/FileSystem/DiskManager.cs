#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem
{
    public class DiskManager
    {
        public string Name {get; }

        public DiskManager(string aDriveName)
        {
            if (aDriveName == null)
            {
                throw new ArgumentNullException(nameof(aDriveName));
            }

            if (!VFSManager.IsValidDriveId(aDriveName))
            {
                throw new ArgumentException("Argument must be drive identifier or root dir");
            }

            Global.mFileSystemDebugger.SendInternal($"Creating DriskManager for drive {aDriveName}");

            Name = aDriveName;
        }

        public void Format(string aDriveFormat, bool aQuick = true)
        {
            /* For now we do the more easy thing: quick format of a drive with the same filesystem */
            if (VFSManager.GetFileSystemType(Name) != aDriveFormat)
            {
                throw new NotImplementedException($"Formatting in {aDriveFormat} drive {Name} with Filesystem {VFSManager.GetFileSystemType(Name)} not yet supported");
            }

            if (aQuick == false)
            {
                throw new NotImplementedException("Slow format not implemented yet");
            }

            VFSManager.Format(Name, aDriveFormat, aQuick);
        }

        public void ChangeDriveLetter(string aNewName)
        {
            if (aNewName == null)
            {
                throw new ArgumentNullException(nameof(aNewName));
            }

            if (!VFSManager.IsValidDriveId(aNewName))
            {
                throw new ArgumentException("Argument must be drive identifier or root dir");
            }

            /*
             * 1. Add new method in VFSManager to change identifier of a drive
             * 2. Update 'Name' to be 'aNewName'
             */
            throw new NotImplementedException("ChangeDriveLetter");
        }

        public void CreatePartion(long start, long end)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (end < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (end <= start)
            {
                throw new ArgumentException("end is <= start");
            }

            throw new NotImplementedException("CreatePartion");
        }
    }
}
