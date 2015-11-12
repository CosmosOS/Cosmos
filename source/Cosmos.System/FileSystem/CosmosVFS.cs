using System;
using System.Collections.Generic;
using System.IO;

using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem
{
    [Serializable]
    public struct KVP<TKey, TValue>
    {
        private readonly TKey key;
        private readonly TValue value;

        public KVP(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        public TKey Key
        {
            get { return key; }
        }

        public TValue Value
        {
            get { return value; }
        }
    }

    public class CosmosVFS : VFSBase
    {
        private List<Partition> mPartitions;
        private List<KVP<string, FileSystem>> mFileSystems;

        public override void Initialize()
        {
            mPartitions = new List<Partition>();
            mFileSystems = new List<KVP<string, FileSystem>>();

            InitializePartitions();
            if (mPartitions.Count > 0)
            {
                InitializeFileSystems();
            }
        }

        public override DirectoryEntry CreateDirectory(string aPath)
        {
            var xEntry = GetDirectory(aPath);
            if (xEntry != null)
            {
                return xEntry;
            }

            string xParentDirectory;
            if (aPath.EndsWith(DirectorySeparatorChar.ToString()))
            {
                string xStripPath = aPath.Remove(aPath.Length - 1, 1);
                xParentDirectory = xStripPath.Remove(xStripPath.IndexOf(DirectorySeparatorChar) - 1);
            }
            var xFS = GetFileSystemFromPath(aPath);

            return null;
        }

        public override List<DirectoryEntry> GetDirectoryListing(string aPath)
        {
            var xFS = GetFileSystemFromPath(aPath);
            var xDirectory = DoGetDirectory(aPath, xFS);
            return xFS.GetDirectoryListing(xDirectory);
        }

        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry aDirectory)
        {
            return null;
        }

        public override List<DirectoryEntry> GetVolumes()
        {
            return null;
        }

        public override DirectoryEntry GetDirectory(string aPath)
        {
            var xFS = GetFileSystemFromPath(aPath);

            return DoGetDirectory(aPath, xFS);
        }

        public override DirectoryEntry GetVolume(string aVolume)
        {
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
                        mFileSystems.Add(new KVP<string, FileSystem>(xRootPath, new FatFileSystem(mPartitions[i])));
                        break;
                    default:
                        global::System.Console.WriteLine("Unknown filesystem type!");
                        return;
                }

                if ((mFileSystems.Count > 0) && (mFileSystems[mFileSystems.Count - 1].Key == xRootPath))
                {
                    string xMessage = string.Concat("Initialized ", mFileSystems.Count, "filesystem(s)...");
                    global::System.Console.WriteLine(xMessage);
                    mFileSystems[i].Value.DisplayFileSystemInfo();
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
            string xPath = Path.GetPathRoot(aPath);
            for (int i = 0; i < mFileSystems.Count; i++)
            {
                string xTest = mFileSystems[i].Key;
                if (string.Equals(xTest, xPath))
                {
                    return mFileSystems[i].Value;
                }
            }
            throw new Exception("Unable to determine filesystem for path: " + aPath);
        }

        private DirectoryEntry DoGetDirectory(string aPath, FileSystem aFS)
        {
            if (aFS == null)
            {
                throw new ArgumentNullException("aFS");
            }
            string[] xPathParts = VFSManager.SplitPath(aPath);

            if (xPathParts.Length == 1)
            {
                return GetVolume(aFS, aPath);
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
                    if (String.Equals(xListingItem.Name, xPathPart, StringComparison.OrdinalIgnoreCase))
                    {
                        if (xListingItem.EntryType == DirectoryEntryTypeEnum.Directory)
                        {
                            xBaseDirectory = xListingItem;
                            xPartFound = true;
                        }
                        else
                        {
                            throw new Exception("Path part '" + xPathPart + "' found, but not a directory!");
                        }
                    }
                }

                if (!xPartFound)
                {
                    throw new Exception("Path part '" + xPathPart + "' not found!");
                }
            }
            return xBaseDirectory;
        }

        private DirectoryEntry GetVolume(FileSystem aFS, string aVolume)
        {
            return aFS.GetRootDirectory(aVolume);
        }
    }

}
