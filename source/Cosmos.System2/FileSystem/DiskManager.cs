//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem
{
    /// <summary>
    /// DiskManager class. Used to manage drives.
    /// </summary>
    public class DiskManager
    {
        /// <summary>
        /// Get drive name.
        /// </summary>
        public string Name {get; }

        /// <summary>
        /// Create new instance of <see cref="DiskManager"/> class.
        /// </summary>
        /// <param name="aDriveName">A drive name assinged to the disk.</param>
        /// <exception cref="ArgumentNullException">Thrown if aDriveName is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if aDriveName length is smaller then 2, or greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if aDriveName is invalid drive identifier / not a root dir.</exception>
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

        /// <summary>
        /// Format drive. (delete all)
        /// </summary>
        /// <param name="aDriveFormat">A drive format.</param>
        /// <param name="aQuick">Quick format.</param>
        /// <exception cref="NotImplementedException">
        /// <list type="bullet">
        /// <item>Thrown when formating to different filesystem format then current format.</item>
        /// <item>aQuick is false.</item>
        /// <item>Thrown when FAT type is unknown.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// <item>Fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when filesystem is null / memory error.</exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when drive name is null or empty.</item>
        /// <item>Data length is 0.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Unable to determine filesystem for path:  + drive name.</item>
        /// <item>Thrown when data size invalid.</item>
        /// <item>Thrown on unknown file system type.</item>
        /// <item>Thrown on fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
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

        /// <summary>
        /// Change drive letter.
        /// </summary>
        /// <param name="aNewName">A new name to be set.</param>
        /// <exception cref="ArgumentNullException">Thrown when aNewName is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if aNewName length is smaller then 2, or greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if aNewName is invalid drive identifier / not a root dir.</exception>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
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

        /// <summary>
        /// Create Partition.
        /// </summary>
        /// <param name="start">Start.</param>
        /// <param name="end">End.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if start / end is smaller then 0.</exception>
        /// <exception cref="ArgumentException">Thrown if end is smaller or equal to start.</exception>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
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
