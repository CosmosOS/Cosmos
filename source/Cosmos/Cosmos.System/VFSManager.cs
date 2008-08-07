using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cosmos.Sys.FileSystem;
using Cosmos.Sys.FileSystem.Ext2;
using Cosmos.Hardware;
using Cosmos.Hardware.Storage;
//using DebugUtil=Cosmos.FileSystem.DebugUtil;

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

        public static void Init() {
            if (mFilesystems != null) {
                throw new Exception("Virtual File System Manager already initialized!");
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

        //Path examples:

        //  1:\Dir\File.txt
        //  1:\Dir\
        //  1:\Dir
        //  1:/Dir/
        //  0:\Dir\Sub
        //  Sub
        //  Sub/
        //  Sub\File.txt
        //  ..\Other\File.txt
        //  ..\..\Other

        public static bool ContainsVolumeSeparator(this string aPath)
        {
            return (aPath[1] == Path.VolumeSeparatorChar);
            //return aPath.Contains(Path.VolumeSeparatorChar);
        }

        public static bool IsAbsolutePath(this string aPath)
        {
            return aPath.ContainsVolumeSeparator();
        }

        public static bool IsRelativePath(this string aPath)
        {
            return !aPath.ContainsVolumeSeparator();
        }

        public static string[] SplitPath(string aPath)
        {
            return aPath.Split(GetDirectorySeparators(), StringSplitOptions.RemoveEmptyEntries);
        }

        private static char[] GetDirectorySeparators()
        {
            return new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        }

        //public static bool IsADriveVolume(this string aPath)
        //{
        //    string xPath = aPath.Replace('\\', '-');//(GetDirectorySeparators());
        //    xPath = xPath.Replace('/', (char)(""[0]));

        //    Console.WriteLine(xPath);
        //    return false;
        //}

        /// <summary>
        /// Get a single directory from the given path.
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static FilesystemEntry GetDirectoryEntry(string aPath) {
            Cosmos.Hardware.DebugUtil.SendMessage("GetDirectoryEntry", "Searching for " + aPath);
            if (String.IsNullOrEmpty(aPath)) {
                throw new ArgumentNullException("aPath");
            }
            //if (aPath[0] != '/' && aPath[0] != '\\') {
            //    throw new Exception("Incorrect path, should start with / or \\!");
            //}
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
                        Cosmos.Hardware.DebugUtil.SendMessage("GetDirectoryEntry","Checking if " + xListing[j].Name + " equals " + xPathParts[i]);
                        if (xListing[j].Name.Equals(xPathParts[i])) {
                            xCurrentFSEntryId = xListing[j].Id;
                            if (i == (xPathParts.Length - 1)) {
                                Cosmos.Hardware.DebugUtil.SendMessage("GetDirectoryEntry", "Found match! : " + xListing[j].Name);
                                return xListing[j];
                            }
                            xFound = true;
                            break;
                        }
                    }
                    if (!xFound) {
                        Cosmos.Hardware.DebugUtil.SendError("GetDirectoryEntry", "Path not found(1)");
                        throw new Exception("Path not found!");
                    }
                }
                Cosmos.Hardware.DebugUtil.SendError("GetDirectoryEntry", "Path not found(2)");
                throw new Exception("Path not found!");
            }
        }

        /// <summary>
        /// Retrieves an array of FilesystemEntries (i.e. Directories and Files) in the gives Directory path.
        /// </summary>
        /// <param name="aPath">Directory to search in. Can be absolute and relative.</param>
        /// <returns>All Directories and Files in the given path.</returns>
        public static FilesystemEntry[] GetDirectoryListing(string aPath) {
            
            if (String.IsNullOrEmpty(aPath)) {
                throw new ArgumentNullException("aPath is null in GetDirectoryListing");
            }
            
            //if (aPath[0] != '/' && aPath[0] != '\\') {
            //    throw new Exception("Incorrect path, must start with / or \\!");
            //}                

            //if (aPath.Trim(Path.VolumeSeparatorChar).Length == 1)
            //{
            //    return GetVolumes(mFilesystems);
            //}

            //var xFS = GetFileSystemFromPath(aPath, 1);
            //return xFS.GetDirectoryListing(xFS.RootId);

            if (aPath.Length == 1)
            {
                return GetVolumes();
            }
            else
            {
                string xParentPath = aPath;
                if (String.IsNullOrEmpty(xParentPath))
                {
                    var xFS = GetFileSystemFromPath(aPath, 1);
                    return xFS.GetDirectoryListing(xFS.RootId);
                }
                var xParentItem = GetDirectoryEntry(aPath);
                if (xParentItem == null)
                {
                    var xFS = GetFileSystemFromPath(aPath, 1);
                    return xFS.GetDirectoryListing(xFS.RootId);
                }

                return xParentItem.Filesystem.GetDirectoryListing(xParentItem.Id);
            }
            
        }

        private static FilesystemEntry[] GetVolumes()
        {
        //    if (aFilesystems == null)
        //        throw new ArgumentNullException("mFilesystems has not been initialized");

            //Get volumes
            var xResult = new FilesystemEntry[mFilesystems.Count];
            for (int i = 0; i < mFilesystems.Count; i++)
            {
                xResult[i] = new FilesystemEntry()
                {
                    Id = (ulong)i,
                    IsDirectory = true,
                    IsReadonly = true,
                    Name = i.ToString() //Volume number
                };
            }
            return xResult;
        }

        /// <summary>
        /// Retrieves all files and directories in the given directory entry.
        /// </summary>
        /// <param name="aDirectory">Must be a Directory entry.</param>
        /// <returns></returns>
        public static FilesystemEntry[] GetDirectoryListing(FilesystemEntry aDirectory)
        {
            if (!aDirectory.IsDirectory)
                throw new ArgumentException("Only Directories are allowed");

            var xFS = aDirectory.Filesystem;
            return xFS.GetDirectoryListing(aDirectory.Id);
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
                var xDirectory = GetDirectoryEntry(Path.GetDirectoryName(s));

                Cosmos.Hardware.DebugUtil.SendMessage("FileExists", "1");
                var xEntries = GetDirectoryListing(Path.GetDirectoryName(s));
                Cosmos.Hardware.DebugUtil.SendMessage("FileExists", "2");
                string xFileName = Path.GetFileName(s);
                Cosmos.Hardware.DebugUtil.SendMessage("FileExists", "3");
                for (int i = 0; i < xEntries.Length; i++)
                {
                    Cosmos.Hardware.DebugUtil.SendMessage("FileExists", "4");
                    if (xEntries[i].Name.Equals(xFileName))
                    {
                        Cosmos.Hardware.DebugUtil.SendMessage("FileExists", "5");
                        return !xEntries[i].IsDirectory;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Cosmos.Hardware.DebugUtil.SendMessage("FileExists", e.Message);
                return false;
            }
        }

        public static string ReadFileAsString(string aFile)
        {
            var xFile = GetFileEntry(aFile);
            Hardware.DebugUtil.SendMessage("ReadFile", "Found file " + xFile.Id.ToString());
            var xFS = GetFileSystemFromPath(aFile, 1);
            Hardware.DebugUtil.SendMessage("ReadFile", "Found filesystem " + xFS.RootId.ToString());

            byte[] xSingleBlockBuffer = new byte[xFS.BlockSize];
            byte[] xAllBlocksBuffer = new byte[xFile.Size];

            for (uint i = 0; i < (xAllBlocksBuffer.Length/xSingleBlockBuffer.Length); i++)
            {
                Hardware.DebugUtil.SendMessage("ReadFile", "Reading block " + i.ToString());
                //Read the block
                if (!xFS.ReadBlock(xFile.Id, i, xSingleBlockBuffer))
                {
                    Console.Write("Error while processing file! (");
                    Console.Write(((uint)xFile.Id).ToString());
                    Console.WriteLine(")");
                    return "";
                }

                int xCurLength = xAllBlocksBuffer.Length % xSingleBlockBuffer.Length;
                if (xCurLength == 0)
                {
                    xCurLength = xSingleBlockBuffer.Length;
                }

                //Copy the single block into the full buffer
                Array.Copy(xSingleBlockBuffer, 0, xAllBlocksBuffer, i * xSingleBlockBuffer.Length, xCurLength);

                //If we read exactly to the end, then break
                if ((i + 1) == (xAllBlocksBuffer.Length / xSingleBlockBuffer.Length))
                {
                    break;
                }
            }

            if (xAllBlocksBuffer.Length > 0)
            {
                System.Text.StringBuilder xBuilder = new System.Text.StringBuilder(xAllBlocksBuffer.Length);
                for (int i = 0; i < xAllBlocksBuffer.Length; i++)
                {
                    xBuilder.Append(xAllBlocksBuffer[i].ToString());
                }
                return xBuilder.ToString();
                //return new string(xAllBlocksBuffer).ToString();
            }
            else
                throw new Exception("Unable to read contents of file " + xFile);
            
            //return "Dummy file contents";
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

        /// <summary>
        /// Retrieve multiple directories from the given directory.
        /// </summary>
        /// <param name="aDir"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Retrieve a specific file with the given path.
        /// </summary>
        /// <param name="aFile"></param>
        /// <returns></returns>
        public static FilesystemEntry GetFileEntry(String aFile)
        {
            Hardware.DebugUtil.SendMessage("GetFileEntry", "Searching for file " + aFile);
            string xFileName = Path.GetFileName(aFile);
            Hardware.DebugUtil.SendMessage("GetFileEntry", "Filename is " + xFileName);

            //Find the directory first.
            var xDirectory = VFSManager.GetDirectoryEntry(Path.GetDirectoryName(aFile)); 
            Hardware.DebugUtil.SendMessage("GetFileEntry", "Directory is " + xDirectory.Name);

            //Then find file in that directory
            //var xFS = GetFileSystemFromPath(aFile, 1);
            //Hardware.DebugUtil.SendMessage("GetFileEntry", "Got filesystem");

            //FilesystemEntry[] xEntries = xFS.GetDirectoryListing(xDirectory.Id);
            //Hardware.DebugUtil.SendMessage("GetFileEntry", "Got Directory Listing");

            FilesystemEntry[] xEntries = GetDirectoryListing(xDirectory);

            foreach (FilesystemEntry xEntry in xEntries)
            {
                Hardware.DebugUtil.SendMessage("GetFileEntry", "Matching " + xEntry.Name + " with " + xFileName);
                if (xEntry.Name == xFileName)
                    return xEntry;
            }

            //throw new FileNotFoundException();
            Hardware.DebugUtil.SendMessage("GetFileEntry", "File not found: " + aFile);
            return null;
        }

        /// <summary>
        /// Get all the files in the given directory.
        /// </summary>
        /// <param name="aDir"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the files in the given Directory entry
        /// </summary>
        /// <param name="aDir">Must be a Directory</param>
        /// <returns></returns>
        public static FilesystemEntry[] GetFiles(FilesystemEntry aDir)
        {
            if (aDir == null)
                throw new ArgumentNullException("aDir in GetFiles(FilesystemEntry)");

            if (!aDir.IsDirectory)
                throw new Exception("Must be a directory");

            List<FilesystemEntry> xFiles = new List<FilesystemEntry>();
            foreach (FilesystemEntry xEntry in GetDirectoryListing(aDir))
                if (!xEntry.IsDirectory)
                    xFiles.Add(xEntry);

            return xFiles.ToArray();
        }

        /// <summary>
        /// Get the logical drives found. Formatted as 1:/
        /// </summary>
        /// <returns></returns>
        public static string[] GetLogicalDrives()
        {
            List<string> xDrives = new List<string>();
            foreach (FilesystemEntry entry in GetDirectoryListing("/"))
                xDrives.Add(entry.Name + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar);

            return xDrives.ToArray();
        }


    }
}