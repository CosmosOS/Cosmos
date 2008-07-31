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
                var xFS = mFilesystems[ParseStringToInt(xPathParts[0],
                                                        0)];
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
                throw new ArgumentNullException("aPath");
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
                    var xFS = mFilesystems[ParseStringToInt(aPath,
                                                            1)];
                    return xFS.GetDirectoryListing(xFS.RootId);
                }
                var xParentItem = GetDirectoryEntry(xParentPath);
                if (xParentItem == null) {
                    var xFS = mFilesystems[ParseStringToInt(aPath,
                                                            1)];
                    return xFS.GetDirectoryListing(xFS.RootId);
                }
                return xParentItem.Filesystem.GetDirectoryListing(xParentItem.Id);
            }
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
                        throw new Exception("Wrong number format!");
                }

                #endregion
            }
            return xResult;
        }

        public static bool FileExists(string s) {
            var xEntries = GetDirectoryListing(Path.GetDirectoryName(s));
            string xFileName = Path.GetFileName(s);
            for (int i = 0; i < xEntries.Length; i++) {
                if (xEntries[i].Name.Equals(xFileName)) {
                    return !xEntries[i].IsDirectory;
                }
            }
            return false;
        }

        public static bool DirectoryExists(string s) {
                var xEntries = GetDirectoryListing(Path.GetDirectoryName(s));
                string xDirName = Path.GetFileName(s);
                for (int i = 0; i < xEntries.Length; i++)
                {
                    if (xEntries[i].Name.Equals(xDirName))
                    {
                        return xEntries[i].IsDirectory;
                    }
                }
                return false;
        }

        public static string[] GetDirectories(string aDir)
        {
            if (aDir == null)
                throw new ArgumentNullException("aDir is null");

            if (!Directory.Exists(aDir))
                throw new DirectoryNotFoundException("Unable to find directory " + aDir);

            List<string> xDirectories = new List<string>();
            var xEntries = GetDirectoryListing(Path.GetDirectoryName(aDir));

            foreach (FilesystemEntry entry in xEntries)
                if (entry.IsDirectory)
                    xDirectories.Add(entry.Name);

            return xDirectories.ToArray();
        }

        public static string[] GetFiles(string aDir)
        {
            if (aDir == null)
                throw new ArgumentNullException("aDir is null");

            if (!Directory.Exists(aDir))
                throw new DirectoryNotFoundException("Unable to find directory " + aDir);

            List<string> xFiles = new List<string>();
            var xEntries = GetDirectoryListing(Path.GetDirectoryName(aDir));

            foreach (FilesystemEntry entry in xEntries)
                if (!entry.IsDirectory)
                    xFiles.Add(entry.Name);

            return xFiles.ToArray();
        }

        public static string[] GetLogicalDrives()
        {
            List<string> xDrives = new List<string>();
            foreach (FilesystemEntry entry in GetDirectoryListing("/"))
                xDrives.Add(entry.Name);

            return xDrives.ToArray();
        }


    }
}