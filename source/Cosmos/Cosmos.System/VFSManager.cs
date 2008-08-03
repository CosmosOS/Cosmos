using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cosmos.FileSystem;
using Cosmos.FileSystem.Ext2;
using Cosmos.Hardware;
using Cosmos.Hardware.Storage;
using DebugUtil=Cosmos.FileSystem.DebugUtil;

namespace Cosmos.Sys {
    public static class VFSManager {
        private static List<Filesystem> mFilesystems;

        private static void DetectFilesystem(BlockDevice aDevice) {
            #region Ext2

            if (Ext2.BlockDeviceContainsExt2(aDevice)) {
                aDevice.Used = true;
                var xFS = new Ext2(aDevice);
                mFilesystems.Add(xFS);
            }

            #endregion
        }

        public static void Initialize() {
            if (mFilesystems != null) {
                throw new Exception("FSManager already initialized!");
            }
            mFilesystems = new List<Filesystem>(4);
            for (int i = 0; i < Device.Devices.Count; i++) {
                var xDevice = Device.Devices[i];
                if (xDevice.Type != Device.DeviceType.Storage) {
                    continue;
                }
                var xStorageDevice = xDevice as BlockDevice;
                if (xStorageDevice == null) {
                    continue;
                }
                if (xStorageDevice.Used) {
                    continue;
                }
                DetectFilesystem(xStorageDevice);
            }
            Hardware.DebugUtil.SendNumber("VFS",
                                          "Registered Filesystems",
                                          (uint)mFilesystems.Count,
                                          32);
        }

        public static FilesystemEntry GetDirectoryEntry(string aPath) {
            if (String.IsNullOrEmpty(aPath)) {
                throw new ArgumentNullException("aPath");
            }
            if (aPath[0] != '/' && aPath[0] != '\\') {
                throw new Exception("Incorrect path, should start with / or \\!");
            }
            if (aPath.Length == 1) {
                return null;
            } else {
                string[] xPathParts;
                if (aPath[0] == '/') {
                    xPathParts = aPath.Split(new char[] {'/'},
                                             StringSplitOptions.RemoveEmptyEntries);
                } else {
                    xPathParts = aPath.Split(new char[] {'\\'},
                                             StringSplitOptions.RemoveEmptyEntries);
                }
                // first get the correct FS
                var xFS = GetFileSystemFromPath(xPathParts[0], 0);
                //var xFS = mFilesystems[ParseStringToInt(xPathParts[0], 0)];
                var xCurrentFSEntryId = xFS.RootId;
                if (xPathParts.Length == 1) {
                    return null;
                }
                for (int i = 1; i < (xPathParts.Length); i++) {
                    var xListing = xFS.GetDirectoryListing(xCurrentFSEntryId);
                    bool xFound = false;
                    for (int j = 0; j < xListing.Length; j++) {
                        if (xListing[j].Name.Equals(xPathParts[i])) {
                            xCurrentFSEntryId = xListing[j].Id;
                            if (i == (xPathParts.Length - 1)) {
                                return xListing[j];
                            }
                            xFound = true;
                            break;
                        }
                    }
                    if (!xFound) {
                        throw new Exception("Path not found!");
                    }
                }
                throw new Exception("Path not found!");
            }
        }

        /// <summary>
        /// Retrieves an array of FilesystemEntries (i.e. Directories and Files) in the gives Directory path.
        /// </summary>
        /// <param name="aPath">Directory to search in.</param>
        /// <returns>All Directories and Files in the given path.</returns>
        public static FilesystemEntry[] GetDirectoryListing(string aPath) {
            if (String.IsNullOrEmpty(aPath)) {
                throw new ArgumentNullException("aPath is null in GetDirectoryListing");
            }
            if (aPath[0] != '/' && aPath[0] != '\\') {
                throw new Exception("Incorrect path, must start with / or \\!");
            }
            if (aPath.Length == 1) {
                // get listing of all drives:
                var xResult = new FilesystemEntry[mFilesystems.Count];
                for (int i = 0; i < mFilesystems.Count; i++) {
                    xResult[i] = new FilesystemEntry() {
                                                           Id = (ulong)i,
                                                           IsDirectory = true,
                                                           IsReadonly = true,
                                                           Name = i.ToString()
                                                       };
                }
                return xResult;
            } else {
                string xParentPath = aPath;
                if (String.IsNullOrEmpty(xParentPath)) {
                    var xFS = GetFileSystemFromPath(aPath, 1);
                    return xFS.GetDirectoryListing(xFS.RootId);
                }
                var xParentItem = GetDirectoryEntry(xParentPath);
                if (xParentItem == null) {
                    var xFS = GetFileSystemFromPath(aPath,1);
                    return xFS.GetDirectoryListing(xFS.RootId);
                }
                return xParentItem.Filesystem.GetDirectoryListing(xParentItem.Id);
            }
        }

        /// <summary>
        /// Retrieves the drive filesystem from the drivenumber in the path.
        /// </summary>
        /// <param name="aPath">The posistion of the drive number in the path.</param>
        /// <returns>A filesystem</returns>
        private static Filesystem GetFileSystemFromPath(string aPath, int aOffset)
        {
            return mFilesystems[ParseStringToInt(aPath, aOffset)];
        }

        private static int ParseStringToInt(string aString,
                                            int aOffset) {
            int xResult = 0;
            for (int i = aOffset; i < aString.Length; i++) {
                if (i > 0) {
                    xResult *= 10;
                }

                #region actual parsing

                switch (aString[i]) {
                    case '0':
                        break;
                    case '1':
                        xResult += 1;
                        break;
                    case '2':
                        xResult += 2;
                        break;
                    case '3':
                        xResult += 3;
                        break;
                    case '4':
                        xResult += 4;
                        break;
                    case '5':
                        xResult += 5;
                        break;
                    case '6':
                        xResult += 6;
                        break;
                    case '7':
                        xResult += 7;
                        break;
                    case '8':
                        xResult += 8;
                        break;
                    case '9':
                        xResult += 9;
                        break;
                    default:
                        throw new Exception("Wrong number format! " + aString[i] + " is not a number.");
                }

                #endregion
            }
            return xResult;
        }

        public static bool FileExists(string s) {
            try
            {
                var xEntries = GetDirectoryListing(Path.GetDirectoryName(s));
                string xFileName = Path.GetFileName(s);
                for (int i = 0; i < xEntries.Length; i++)
                {
                    if (xEntries[i].Name.Equals(xFileName))
                    {
                        return !xEntries[i].IsDirectory;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string ReadFileAsString(string aFile)
        {
            //var xFile = GetFileEntry(aFile);
            //var xFS = GetFileSystemFromPath(aFile, 1);

            //byte[] xFileBuffer = new byte[255];
            //if (xFS.ReadBlock(xFile.Id, 0, xFileBuffer))
            //    return xFileBuffer.ToString();
            //else
            //    throw new Exception("Unable to read contents of file " + xFile);
            
            return "Dummy file contents";
        }

        /// <summary>
        /// Checks if the given directory exists on disk.
        /// </summary>
        /// <param name="aDir">Can be both relative and absolute path.</param>
        /// <returns></returns>
        public static bool DirectoryExists(string aDir) {
            try
            {
                var xEntries = GetDirectoryListing(Path.GetDirectoryName(aDir));
                string xDirName = Path.GetFileName(aDir);
                for (int i = 0; i < xEntries.Length; i++)
                {
                    if (xEntries[i].Name.Equals(xDirName))
                    {
                        return xEntries[i].IsDirectory;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Cosmos.Hardware.DebugUtil.SendError("VFSManager.cs", e.Message);
                return false;
            }
             
        }

        public static FilesystemEntry[] GetDirectories(string aDir)
        {
            if (aDir == null)
                throw new ArgumentNullException("aDir is null");

            //if (!Directory.Exists(aDir))
            //    throw new DirectoryNotFoundException("Unable to find directory " + aDir);

            List<FilesystemEntry> xDirectories = new List<FilesystemEntry>();
            var xEntries = GetDirectoryListing(Path.GetDirectoryName(aDir));

            foreach (FilesystemEntry entry in xEntries)
                if (entry.IsDirectory)
                    xDirectories.Add(entry);

            //return (from xEntry in GetDirectoryListing(aDir) where xEntry.IsDirectory select xEntry).ToArray();

            return xDirectories.ToArray();
        }

        public static FilesystemEntry GetFileEntry(String aFile)
        {
            string xFileName = Path.GetFileName(aFile);

            //Find the directory first.
            var xDirectory = VFSManager.GetDirectoryEntry(Path.GetDirectoryName(aFile));

            //Then find file in that directory
            var xFS = GetFileSystemFromPath(aFile, 1);

            FilesystemEntry[] xEntries = xFS.GetDirectoryListing(xDirectory.Id);
            foreach (FilesystemEntry xEntry in xEntries)
                if (xEntry.Name == xFileName)
                    return xEntry;

            //throw new FileNotFoundException();
            return null;
        }

        public static FilesystemEntry[] GetFiles(string aDir)
        {
            if (aDir == null)
                throw new ArgumentNullException("aDir is null");

            //var xDirectory = VFSManager.GetDirectoryEntry(Path.GetDirectoryName(aDir));
            //Cosmos.Hardware.DebugUtil.SendMessage("GetFiles", "Directory found");
            ////var xFS = xDirectory.Filesystem;
            //var xFS = GetFileSystemFromPath(Path.GetDirectoryName(aDir), 1);
            //Cosmos.Hardware.DebugUtil.SendMessage("GetFiles", "Filesystem set");
            
            //List<FilesystemEntry> xFiles = new List<FilesystemEntry>();
            //Cosmos.Hardware.DebugUtil.SendMessage("GetFiles", "Going to search directory with ID " + xDirectory.Id);
            //var xEntries = xFS.GetDirectoryListing(xDirectory.Id);
            //foreach (FilesystemEntry xEntry in xEntries)
            //{
            //    Cosmos.Hardware.DebugUtil.SendMessage("GetFiles", "Foreach");
            //    if (!xEntry.IsDirectory)
            //        xFiles.Add(xEntry);
            //}

            //Cosmos.Hardware.DebugUtil.SendMessage("GetFiles", "Converting to array");
            //return xFiles.ToArray();


            //if (!Directory.Exists(aDir))
            //    throw new DirectoryNotFoundException("Unable to find directory " + aDir);

            List<FilesystemEntry> xFiles = new List<FilesystemEntry>();
            var xEntries = GetDirectoryListing(Path.GetDirectoryName(aDir));

            foreach (FilesystemEntry entry in xEntries)
                if (!entry.IsDirectory)
                    xFiles.Add(entry);

            return xFiles.ToArray();

            //return (from xEntry in GetDirectoryListing(aDir) where !xEntry.IsDirectory select xEntry).ToArray();
        }

        public static string[] GetLogicalDrives()
        {
            List<string> xDrives = new List<string>();
            foreach (FilesystemEntry entry in GetDirectoryListing("/"))
                xDrives.Add(entry.Name + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar);

            return xDrives.ToArray();
        }


    }
}