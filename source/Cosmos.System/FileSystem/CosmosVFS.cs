using System;
using System.Collections.Generic;
using System.IO;

using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem
{
    public class CosmosVFS : VFSBase
    {
        private List<Partition> mPartitions;

        private List<FileSystem> mFileSystems;

        public override void Initialize()
        {
            mPartitions = new List<Partition>();
            mFileSystems = new List<FileSystem>();

            InitializePartitions();
            if (mPartitions.Count > 0)
            {
                InitializeFileSystems();
            }
        }

        public override DirectoryEntry CreateFile(string aPath)
        {
            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length == 0)
            {
                throw new ArgumentException("aPath");
            }

            Global.mFileSystemDebugger.SendInternal($"CosmosVFS.CreateFile : aPath = {aPath}");
            if (File.Exists(aPath))
            {
                Global.mFileSystemDebugger.SendInternal($"CosmosVFS.CreateFile : Path already exists.");
                return GetFile(aPath);
            }

            Global.mFileSystemDebugger.SendInternal($"CosmosVFS.CreateFile : Path doesn't exist.");
            string xParentDirectory = Path.GetDirectoryName(aPath);
            string xFileToCreate = Path.GetFileName(aPath);
            Global.mFileSystemDebugger.SendInternal($"CosmosVFS.CreateFile : xParentDirectory = {xParentDirectory}, xFileToCreate = {xFileToCreate}");
            if (xParentDirectory != null)
            {
                if (Directory.Exists(xParentDirectory))
                {
                    var xFS = GetFileSystemFromPath(xParentDirectory);
                    var xParentEntry = GetDirectory(xParentDirectory);
                    return xFS.CreateFile(xParentEntry, xFileToCreate);
                }
            }
            return null;
        }

        public override DirectoryEntry CreateDirectory(string aPath)
        {
            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length == 0)
            {
                throw new ArgumentException("aPath");
            }

            Global.mFileSystemDebugger.SendInternal($"CosmosVFS.CreateDirectory : aPath = {aPath}");
            if (Directory.Exists(aPath))
            {
                Global.mFileSystemDebugger.SendInternal($"CosmosVFS.CreateDirectory : Path already exists.");
                return GetDirectory(aPath);
            }

            Global.mFileSystemDebugger.SendInternal($"CosmosVFS.CreateDirectory : Path doesn't exist.");
            string xParentDirectory = Path.GetDirectoryName(aPath);
            string xDirectoryToCreate = Path.GetFileName(aPath);
            Global.mFileSystemDebugger.SendInternal($"CosmosVFS.CreateDirectory : xParentDirectory = {xParentDirectory}, xDirectoryToCreate = {xDirectoryToCreate}");
            if (xParentDirectory != null)
            {
                while (!Directory.Exists(xParentDirectory))
                {
                    xParentDirectory = Path.GetDirectoryName(aPath);
                    xDirectoryToCreate = Path.GetFileName(aPath);
                    Global.mFileSystemDebugger.SendInternal($"CosmosVFS.CreateDirectory : xParentDirectory = {xParentDirectory}, xDirectoryToCreate = {xDirectoryToCreate}");
                }


                var xFS = GetFileSystemFromPath(xDirectoryToCreate);
                var xParentEntry = GetDirectory(xParentDirectory);
                return xFS.CreateDirectory(xParentEntry, xDirectoryToCreate);
            }
            return null;
        }

        public override List<DirectoryEntry> GetDirectoryListing(string aPath)
        {
            var xFS = GetFileSystemFromPath(aPath);
            var xDirectory = DoGetDirectoryEntry(aPath, xFS);
            return xFS.GetDirectoryListing(xDirectory);
        }

        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry aDirectory)
        {
            DirectoryEntry xTempEntry = aDirectory;
            string xFullPath = "";
            while (xTempEntry.mParent != null)
            {
                xFullPath = Path.Combine(xTempEntry.mName, xFullPath);
                xTempEntry = xTempEntry.mParent;
            }

            return GetDirectoryListing(xFullPath);
        }

        public override DirectoryEntry GetDirectory(string aPath)
        {
            try
            {
                var xFileSystem = GetFileSystemFromPath(aPath);
                var xEntry = DoGetDirectoryEntry(aPath, xFileSystem);
                if ((xEntry != null) && (xEntry.mEntryType == DirectoryEntryTypeEnum.Directory))
                {
                    return xEntry;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            throw new Exception($"{aPath} was found, but is not a directory.");
        }

        public override DirectoryEntry GetFile(string aPath)
        {
            try
            {
                var xFileSystem = GetFileSystemFromPath(aPath);
                var xEntry = DoGetDirectoryEntry(aPath, xFileSystem);
                if ((xEntry != null) && (xEntry.mEntryType == DirectoryEntryTypeEnum.File))
                {
                    return xEntry;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            throw new Exception($"{aPath} was found, but is not a file.");
        }

        public override List<DirectoryEntry> GetVolumes()
        {
            List<DirectoryEntry> xVolumes = new List<DirectoryEntry>();

            for (int i = 0; i < mFileSystems.Count; i++)
            {
                xVolumes.Add(GetVolume(mFileSystems[i]));
            }

            return xVolumes;
        }

        public override DirectoryEntry GetVolume(string aVolume)
        {
            var xFileSystem = GetFileSystemFromPath(aVolume);
            if (xFileSystem != null)
            {
                return GetVolume(xFileSystem);
            }

            return null;
        }

        protected virtual void InitializePartitions()
        {
            for (int i = 0; i < BlockDevice.Devices.Count; i++)
            {
                if (BlockDevice.Devices[i] is Partition)
                {
                    mPartitions.Add((Partition)BlockDevice.Devices[i]);
                    break;
                }
            }

            if (mPartitions.Count > 0)
            {
                for (int i = 0; i < mPartitions.Count; i++)
                {
                    global::System.Console.WriteLine("Partition #: " + (i + 1));
                    global::System.Console.WriteLine("Block Size: " + mPartitions[i].BlockSize + " bytes");
                    global::System.Console.WriteLine("Size: " + mPartitions[i].BlockCount * mPartitions[i].BlockSize / 1024 / 1024 + " MB");
                }
            }
            else
            {
                global::System.Console.WriteLine("No partitions found!");
            }
        }

        protected virtual void InitializeFileSystems()
        {
            for (int i = 0; i < mPartitions.Count; i++)
            {
                string xRootPath = string.Concat(i, VolumeSeparatorChar, DirectorySeparatorChar);
                switch (FileSystem.GetFileSystemType(mPartitions[i]))
                {
                    case FileSystemType.FAT:
                        mFileSystems.Add(new FatFileSystem(mPartitions[i], xRootPath));
                        break;
                    default:
                        global::System.Console.WriteLine("Unknown filesystem type!");
                        return;
                }

                if ((mFileSystems.Count > 0) && (mFileSystems[mFileSystems.Count - 1].mRootPath == xRootPath))
                {
                    string xMessage = string.Concat("Initialized ", mFileSystems.Count, "filesystem(s)...");
                    global::System.Console.WriteLine(xMessage);
                    mFileSystems[i].DisplayFileSystemInfo();
                    Directory.SetCurrentDirectory(xRootPath);
                }
                else
                {
                    string xMessage = string.Concat("No filesystem found on partition #", i);
                    global::System.Console.WriteLine(xMessage);
                }
            }
        }

        private FileSystem GetFileSystemFromPath(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal($"CosmosVFS.GetFileSystemFromPath : aPath = {aPath}");
            string xPath = Path.GetPathRoot(aPath);
            Global.mFileSystemDebugger.SendInternal($"CosmosVFS.GetFileSystemFromPath : xPath = {xPath}");
            for (int i = 0; i < mFileSystems.Count; i++)
            {
                if (mFileSystems[i].mRootPath == xPath)
                {
                    Global.mFileSystemDebugger.SendInternal($"CosmosVFS.GetFileSystemFromPath : Found filesystem.");
                    return mFileSystems[i];
                }
            }
            throw new Exception("Unable to determine filesystem for path: " + aPath);
        }

        private DirectoryEntry DoGetDirectoryEntry(string aPath, FileSystem aFS)
        {
            if (aFS == null)
            {
                throw new ArgumentNullException("aFS");
            }
            string[] xPathParts = VFSManager.SplitPath(aPath);

            if (xPathParts.Length == 1)
            {
                return GetVolume(aFS);
            }

            DirectoryEntry xBaseDirectory = null;

            // start at index 1, because 0 is the volume
            for (int i = 1; i < xPathParts.Length; i++)
            {
                var xPathPart = xPathParts[i];
                var xPartFound = false;
                var xListing = aFS.GetDirectoryListing(xBaseDirectory);

                for (int j = 0; j < xListing.Count; j++)
                {
                    var xListingItem = xListing[j];
                    if (string.Equals(xListingItem.mName, xPathPart, StringComparison.OrdinalIgnoreCase))
                    {
                        xBaseDirectory = xListingItem;
                        xPartFound = true;
                    }
                }

                if (!xPartFound)
                {
                    throw new Exception("Path part '" + xPathPart + "' not found!");
                }
            }
            return xBaseDirectory;
        }

        private DirectoryEntry GetVolume(FileSystem aFS)
        {
            return aFS.GetRootDirectory();
        }

    }
}
