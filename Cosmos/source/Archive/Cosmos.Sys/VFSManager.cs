using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using Cosmos.Sys.FileSystem;
using Cosmos.Sys.FileSystem.Ext2;
using Cosmos.Hardware2;
using Cosmos.Hardware2.Storage;
//using DebugUtil=Cosmos.FileSystem.DebugUtil;

namespace Cosmos.Sys {
    public static class VFSManager {
        private static List<Filesystem> mFilesystems;
        //TODO: Remove when done with ext2 testing.
        public static List<Filesystem> Filesystems { get { return mFilesystems; } }

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
                Console.WriteLine("Check device: " + i);
                var xDevice = Device.Devices[i];
                if (xDevice.Type != Device.DeviceType.Storage) {
                    continue;
                }
                var xStorageDevice = (BlockDevice)xDevice;
                if (xStorageDevice.Used) {
                    continue;
                }
                Console.WriteLine("Detect Filesystem");
                DetectFilesystem(xStorageDevice);
                Console.WriteLine("Detection went ok");
            }
            Console.WriteLine("Checked all devices");
            //// Cosmos.Debug.Debugger.SendNumber("VFS","Registered Filesystems",(uint)mFilesystems.Count,32);
            Console.WriteLine("End check");
            if (mFilesystems.Count == 0)
            {
                Console.WriteLine("WARNING: No filesystems found in VFS init!");
            }
            else
            {
                Console.WriteLine("  Found " + mFilesystems.Count + " filesystems!");
            }
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

        //public static bool ContainsVolumeSeparator(this string aPath)
        //{
        //    return (aPath[1] == Path.VolumeSeparatorChar);
        //}

        public static bool IsAbsolutePath(this string aPath)
        {
            return ((aPath[0] == Path.DirectorySeparatorChar) || aPath[0] == Path.AltDirectorySeparatorChar);
            //return aPath.ContainsVolumeSeparator();
        }

        public static bool IsRelativePath(this string aPath)
        {
            return ((aPath[0] != Path.DirectorySeparatorChar) || aPath[0] != Path.AltDirectorySeparatorChar);
            //return !aPath.ContainsVolumeSeparator();
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
        /// <param name="aPath">Absolute path</param>
        /// <returns></returns>
        public static FilesystemEntry GetDirectoryEntry(string aPath) {
            if (String.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }
            if (!aPath.IsAbsolutePath())
                throw new ArgumentException("Path must be absolute, not relative");

            if (aPath.Length == 1)
            { //Uber-root (/)
                //// Cosmos.Debug.Debugger.SendMessage("GetDirectoryEntry", "aPath is 1 long!");
                return null;
            }
            else
            {
                string[] xPathParts = SplitPath(aPath);
            
                // first get the correct FS
                //// Cosmos.Debug.Debugger.SendMessage("GetDirectoryEntry", "Searching for filesystem: " + xPathParts[0]);
                var xFS = GetFileSystemFromPath(ParseStringToInt(xPathParts[0]));
                //// Cosmos.Debug.Debugger.SendMessage("GetDirectoryEntry", "Found filesystem " + xFS.RootId.ToString());

                var xCurrentFSEntryId = xFS.RootId;
                //// Cosmos.Debug.Debugger.SendMessage("GetDirectoryEntry", "Found filesystem " + xCurrentFSEntryId);
                if (xPathParts.Length == 1)
                {
                    //// Cosmos.Debug.Debugger.SendMessage("GetDirectoryEntry", "Returning root entry");
                    //// Cosmos.Debug.Debugger.SendMessage("GetDirectoryEntry", "String parsed");
                    return GetVolumeEntry(ParseStringToInt(xPathParts[0]));
                    //return null;
                }
                for (int i = 1; i < (xPathParts.Length); i++)
                {
                    var xListing = xFS.GetDirectoryListing(xCurrentFSEntryId);
                    bool xFound = false;
                    for (int j = 0; j < xListing.Length; j++)
                    {
                        //// Cosmos.Debug.Debugger.SendMessage("GetDirectoryEntry", "Checking if " + xListing[j].Name + " equals " + xPathParts[i]);
                        if (xListing[j].Name.Equals(xPathParts[i]))
                        {
                            xCurrentFSEntryId = xListing[j].Id;
                            if (i == (xPathParts.Length - 1))
                            {
                                //// Cosmos.Debug.Debugger.SendMessage("GetDirectoryEntry", "Found match! : " + xListing[j].Name);
                                return xListing[j];
                            }
                            xFound = true;
                            break;
                        }
                    }
                    if (!xFound)
                    {
                        //// Cosmos.Debug.Debugger.SendError("GetDirectoryEntry", "Path not found(1)");
                        throw new Exception("Path not found: " + aPath);
                    }
                }
                //// Cosmos.Debug.Debugger.SendError("GetDirectoryEntry", "Path not found(2)");
                throw new Exception("Path not found: " + aPath);
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
            //    return GetVolumeEntry(0);
            //}

            //var xFS = GetFileSystemFromPath(aPath, 1);
            //return xFS.GetDirectoryListing(xFS.RootId);
            if (aPath.Length == 1)
            {
                return GetVolumes();
            }
            else
            {
                //string xParentPath = aPath;
                //if (String.IsNullOrEmpty(xParentPath))
                //{
                //    // Cosmos.Debug.Debugger.SendMessage("GetDirectoryListing", "Should never come here!");
                //    var xFS = GetFileSystemFromPath(aPath, 1);
                //    // Cosmos.Debug.Debugger.SendMessage("GetDirectoryListing", "1-RootId=" + xFS.RootId.ToString());
                //    return xFS.GetDirectoryListing(xFS.RootId);
                //}

                var xParentItem = GetDirectoryEntry(aPath);
                //if (xParentItem == null)
                //{
                //    var xFS = GetFileSystemFromPath(aPath, 1);
                //    return xFS.GetDirectoryListing(xFS.RootId);
                //}
                var xResult= xParentItem.Filesystem.GetDirectoryListing(xParentItem.Id);
                return xResult;
            }
            
        }


        /// <summary>
        /// Get a single volume
        /// </summary>
        public static FilesystemEntry GetVolumeEntry(int volumeId)
        {
            var xFS = GetFileSystemFromPath(volumeId);
            
            return new FilesystemEntry()
            {
                Name = volumeId.ToString(),
                Filesystem = xFS,
                IsDirectory = true,
                IsReadonly = true,
                Id = (ulong)xFS.RootId
            };
        }

        public static FilesystemEntry[] GetVolumes()
        {
            //if (aFilesystems == null)
            //    throw new ArgumentNullException("mFilesystems has not been initialized");

            //Get volumes
            var xResult = new FilesystemEntry[mFilesystems.Count];
            for (int i = 0; i < mFilesystems.Count; i++)
            {
                xResult[i] = GetVolumeEntry(i);
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
            //// Cosmos.Debug.Debugger.SendMessage("GetDirectorylisting", "ID is " + aDirectory.Id);
            //aDirectory.
            return xFS.GetDirectoryListing(aDirectory.Id);
        }

        /// <summary>
        /// Retrieves the drive filesystem from the drivenumber in the path.
        /// </summary>
        /// <param name="aPath">The posistion of the drive number in the path.</param>
        /// <returns>A filesystem</returns>
        private static Filesystem GetFileSystemFromPath(int aPath)
        {
            //// Cosmos.Debug.Debugger.SendMessage("GetFileSystemFromPath", aPath.ToString());
            if (mFilesystems.Count == 0)
                throw new Exception("No filesystems found");
            else
            {
                int xId = aPath;
                return mFilesystems[xId];
            }
        }

        private static int ParseStringToInt(string aString) {
            int xResult = 0;
            for (int i = 0; i < aString.Length; i++) {
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

        public static bool FileExists(string aPath) {
            try
            {
                //var xDir = Path.GetDirectoryName(aPath) + Path.DirectorySeparatorChar;
                return (GetFileEntry(aPath) != null);

                //var xDirectory = GetDirectoryEntry(Path.GetDirectoryName(s));

                //// Cosmos.Debug.Debugger.SendMessage("FileExists", "1");
                //var xEntries = GetDirectoryListing(Path.GetDirectoryName(s));
                //// Cosmos.Debug.Debugger.SendMessage("FileExists", "2");
                //string xFileName = Path.GetFileName(s);
                //// Cosmos.Debug.Debugger.SendMessage("FileExists", "3");
                //for (int i = 0; i < xEntries.Length; i++)
                //{
                //    // Cosmos.Debug.Debugger.SendMessage("FileExists", "4");
                //    if (xEntries[i].Name.Equals(xFileName))
                //    {
                //        // Cosmos.Debug.Debugger.SendMessage("FileExists", "5");
                //        return !xEntries[i].IsDirectory;
                //    }
                //}
                //return false;
            }
            catch (Exception)
            {
                //// Cosmos.Debug.Debugger.SendMessage("FileExists", "Error!: " + e.Message);
                return false;
            }
        }

        public static string ReadFileAsString(string aFile)
        {
            //// Cosmos.Debug.Debugger.SendMessage("ReadFile", "Start reading file now");
            var xFile = GetFileEntry(aFile);
            //// Cosmos.Debug.Debugger.SendMessage("ReadFile", "Found file " + xFile.Id.ToString());
            var xFS = xFile.Filesystem;//GetFileSystemFromPath(aFile, 1);
            //// Cosmos.Debug.Debugger.SendMessage("ReadFile", "Found filesystem " + xFS.RootId.ToString());

            byte[] xSingleBlockBuffer = new byte[xFS.BlockSize];
            byte[] xAllBlocksBuffer = new byte[xFile.Size];
            int xBlockCount = (xAllBlocksBuffer.Length / xSingleBlockBuffer.Length);
            if ((xAllBlocksBuffer.Length % xSingleBlockBuffer.Length) > 0) {
                xBlockCount++;
            }
            // Cosmos.Debug.Debugger.SendNumber("ReadFile", "xBlockCount", (uint)xBlockCount, 32);
            for (uint i = 0; i < xBlockCount; i++)
            {
                // Cosmos.Debug.Debugger.SendMessage("ReadFile", "Reading block " + i.ToString());
                //Read the block
                if (!xFS.ReadBlock(xFile.Id, i, xSingleBlockBuffer))
                {
                    return "";
                }
                // Cosmos.Debug.Debugger.SendMessage("ReadFile", "After ReadBlock");
                //int xCurLength = xAllBlocksBuffer.Length % xSingleBlockBuffer.Length;
                int xCurLength = (int)xFS.BlockSize;
                if(i == (xBlockCount-1)) {
                    if(xFile.Size % xFS.BlockSize != 0) {
                        // if the last block is read, we should only read the last couple of bytes (not always full block)
                        xCurLength = (int)(xFile.Size % xFS.BlockSize);
                    }
                }
                // Cosmos.Debug.Debugger.SendNumber("ReadFile", "xCurLength", (uint)xCurLength, 32);
                //if (xCurLength == 0)
                //{
                //    xCurLength = xSingleBlockBuffer.Length;
                //}

                //Copy the single block into the full buffer
                Array.Copy(xSingleBlockBuffer, 0, xAllBlocksBuffer, i * xSingleBlockBuffer.Length, xCurLength);

                //If we read exactly to the end, then break
                if ((i + 1) == xBlockCount)
                {
                    break;
                }
            }

            if (xAllBlocksBuffer.Length > 0)
            {
                System.Text.StringBuilder xBuilder = new System.Text.StringBuilder(xAllBlocksBuffer.Length);
                for (int i = 0; i < xAllBlocksBuffer.Length; i++)
                {
                    xBuilder.Append(((char)xAllBlocksBuffer[i]).ToString());
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
                string xDir = aDir + Path.DirectorySeparatorChar;
                return (VFSManager.GetDirectoryEntry(Path.GetDirectoryName(xDir)) != null);

                //string xDir = aDir + Path.DirectorySeparatorChar;
                //var xEntries = GetDirectoryListing(Path.GetDirectoryName(xDir));
                //string xDirName = Path.GetFileName(xDir);
                //for (int i = 0; i < xEntries.Length; i++)
                //{
                //    if (xEntries[i].Name.Equals(xDirName))
                //    {
                //        return xEntries[i].IsDirectory;
                //    }
                //}
                //return false;
            }
            catch (Exception)
            {
                // Cosmos.Debug.Debugger.SendError("VFSManager.cs", e.Message);
                return false;
            }
             
        }

        /// Retrieve multiple directories from the given directory.
        //public static FilesystemEntry[] GetDirectories(string aDir)
        //{
        //    // Cosmos.Debug.Debugger.SendMessage("GetDirectories", "Checking for nullreference");
        //    if (aDir == null)
        //    {
        //        throw new ArgumentNullException("aDir is null");
        //    }

        //    // Cosmos.Debug.Debugger.SendMessage("GetDirectories", "Checking if " + aDir + " exists");
        //    //if (!Directory.Exists(aDir))
        //    //{
        //    //    throw new DirectoryNotFoundException("Unable to find directory " + aDir);
        //    //}

        //    // Cosmos.Debug.Debugger.SendMessage("GetDirectories", "About to GetDirectoryListing");

        //    var xDir = VFSManager.GetDirectoryEntry(Path.GetDirectoryName(aDir));
        //    ///// Cosmos.Debug.Debugger.SendMessage("GetDirectories", xDir.Name + " with ID " + xDir.Id.ToString());
        //    var xEntries = VFSManager.GetDirectoryListing(xDir);
        //    //var xEntries = VFSManager.GetDirectoryListing(Path.GetDirectoryName(aDir));
        //    //List<FilesystemEntry> xDirectories = new List<FilesystemEntry>();
        //    //for (int i = 0; i < xEntries.Length; i++)
        //    //{
        //    //    if (xEntries[i].IsDirectory)
        //    //    {
        //    //        xDirectories.Add(xEntries[0]);
        //    //    }
        //    //}
            
        //    ////foreach (FilesystemEntry entry in xEntries)
        //    ////    if (entry.IsDirectory)
        //    ////        xDirectories.Add(entry);


        //    ////return (from xEntry in GetDirectoryListing(aDir) where xEntry.IsDirectory select xEntry).ToArray();
        //    //// Cosmos.Debug.Debugger.SendMessage("GetDirectories", "Returning");
        //    //return xDirectories.ToArray();
        //    return new FilesystemEntry[0];
        //}


        /// <summary>
        /// Retrieve a specific file with the given path.
        /// </summary>
        /// <param name="aFile"></param>
        /// <returns></returns>
        public static FilesystemEntry GetFileEntry(String aFile)
        {
            // Cosmos.Debug.Debugger.SendMessage("GetFileEntry", "Searching for file " + aFile);
            string xFileName = Path.GetFileName(aFile);
            // Cosmos.Debug.Debugger.SendMessage("GetFileEntry", "Filename is " + xFileName);

            //Find the directory first.
            var xDirectory = VFSManager.GetDirectoryEntry(Path.GetDirectoryName(aFile) + Path.DirectorySeparatorChar); 
            // Cosmos.Debug.Debugger.SendMessage("GetFileEntry", "Directory is " + xDirectory.Name);

            //Then find file in that directory
            //var xFS = GetFileSystemFromPath(aFile, 1);
            //// Cosmos.Debug.Debugger.SendMessage("GetFileEntry", "Got filesystem");

            //FilesystemEntry[] xEntries = xFS.GetDirectoryListing(xDirectory.Id);
            //// Cosmos.Debug.Debugger.SendMessage("GetFileEntry", "Got Directory Listing");

            FilesystemEntry[] xEntries = VFSManager.GetDirectoryListing(xDirectory);
            // Cosmos.Debug.Debugger.SendMessage("GetFileEntry", "Found " + xEntries.Length + " entries");

            foreach (FilesystemEntry xEntry in xEntries)
            {
                // Cosmos.Debug.Debugger.SendMessage("GetFileEntry", "Matching " + xEntry.Name + " with " + xFileName);
                if (xEntry.Name.Equals(xFileName))
                    return xEntry;
            }

            //throw new FileNotFoundException();
            // Cosmos.Debug.Debugger.SendMessage("GetFileEntry", "File not found: " + aFile);
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
            //// Cosmos.Debug.Debugger.SendMessage("GetFiles", "Directory found");
            ////var xFS = xDirectory.Filesystem;
            //var xFS = GetFileSystemFromPath(Path.GetDirectoryName(aDir), 1);
            //// Cosmos.Debug.Debugger.SendMessage("GetFiles", "Filesystem set");
            
            //List<FilesystemEntry> xFiles = new List<FilesystemEntry>();
            //// Cosmos.Debug.Debugger.SendMessage("GetFiles", "Going to search directory with ID " + xDirectory.Id);
            //var xEntries = xFS.GetDirectoryListing(xDirectory.Id);
            //foreach (FilesystemEntry xEntry in xEntries)
            //{
            //    // Cosmos.Debug.Debugger.SendMessage("GetFiles", "Foreach");
            //    if (!xEntry.IsDirectory)
            //        xFiles.Add(xEntry);
            //}

            //// Cosmos.Debug.Debugger.SendMessage("GetFiles", "Converting to array");
            //return xFiles.ToArray();


            //if (!Directory.Exists(aDir))
            //    throw new DirectoryNotFoundException("Unable to find directory " + aDir);

            List<FilesystemEntry> xFiles = new List<FilesystemEntry>();
            var xDirName = Path.GetDirectoryName(aDir);
            var xEntries = VFSManager.GetDirectoryListing(xDirName);

            for (int i = 0; i < xEntries.Length; i++)
            {
                var entry = xEntries[i];
                if (!entry.IsDirectory)
                    xFiles.Add(entry);
            }

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
            // Cosmos.Debug.Debugger.SendMessage("GetFiles(FileystemEntry)", aDir.Name);

            if (aDir == null)
                throw new ArgumentNullException("aDir in GetFiles(FilesystemEntry)");

            if (!aDir.IsDirectory)
                throw new Exception("Must be a directory");

            List<FilesystemEntry> xFiles = new List<FilesystemEntry>();
            foreach (FilesystemEntry xEntry in VFSManager.GetDirectoryListing(aDir))
            {
                // Cosmos.Debug.Debugger.SendMessage("GetFiles(FileystemEntry)", "Found " + xEntry.Name);
                if (!xEntry.IsDirectory)
                    xFiles.Add(xEntry);
            }

            return xFiles.ToArray();
        }

        /// <summary>
        /// Get the logical drives found. Formatted as 1:\
        /// </summary>
        /// <returns></returns>
        public static string[] GetLogicalDrives()
        {
            List<string> xDrives = new List<string>();
            foreach (FilesystemEntry entry in GetVolumes())
                xDrives.Add(entry.Name + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar);

            return xDrives.ToArray();
        }

        //Mimics the behaviour of System.IO.Directory.InternalGetFileDirectoryNames
        public static string[] InternalGetFileDirectoryNames(string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption)
        {
            //TODO: Add SearchOption functionality
            //TODO: What is userPathOriginal?
            //TODO: Add SearchPattern functionality

            List<string> xFileAndDirectoryNames = new List<string>();

            //Validate input arguments
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
                throw new ArgumentOutOfRangeException("searchOption");

            searchPattern = searchPattern.TrimEnd(new char[0]);
            if (searchPattern.Length == 0)
                return new string[0];
            
            //Perform search in filesystem
            FilesystemEntry[] xEntries = VFSManager.GetDirectoryListing(path);

            foreach (FilesystemEntry xEntry in xEntries)
            {
                if (xEntry.IsDirectory && includeDirs)
                    xFileAndDirectoryNames.Add(xEntry.Name);
                else if (!xEntry.IsDirectory && includeFiles)
                    xFileAndDirectoryNames.Add(xEntry.Name);
            }

            return xFileAndDirectoryNames.ToArray();
        }
    }
}