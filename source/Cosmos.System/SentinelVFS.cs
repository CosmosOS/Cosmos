//using Cosmos.HAL.BlockDevice;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using Directory = Cosmos.System.FileSystem.Listing.Directory;

namespace Cosmos.System
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

    public class SentinelVFS : VFSBase
    {
        private List<Partition> mPartitions;

        private List<KVP<string, Cosmos.System.FileSystem.FileSystem>> mFileSystems;

        protected virtual void InitializePartitions()
        {
            for (int i = 0; i < BlockDevice.Devices.Count; i++)
            {
                var xBlockDevice = BlockDevice.Devices[i];
                FatHelpers.Debug("BlockDevice found: ");
                FatHelpers.Debug(xBlockDevice.ToString());

                var xItem = xBlockDevice as Partition;
                if (xItem == null && xBlockDevice.ToString() == "Partition")
                {
                    FatHelpers.Debug("BlockDevice returns partition, but type check doesn't work!");
                }
                else
                {
                    FatHelpers.Debug("Partition found");
                }
                xItem = xBlockDevice as Partition;
                if (xItem != null)
                {
                    FatHelpers.Debug("Adding partition to list");
                    mPartitions.Add(xItem);
                    break;
                }
            }

            if (mPartitions.Count > 0)
            {
                for (int i = 0; i < mPartitions.Count; i++)
                {
                    FatHelpers.Debug("Partition #: " + (i + 1));
                    FatHelpers.Debug("Block Size: " + mPartitions[i].BlockSize + " bytes");
                    FatHelpers.Debug("Size: " + mPartitions[i].BlockCount * mPartitions[i].BlockSize / 1024 / 1024 + " MB");
                }
            }
            else
            {
                FatHelpers.Debug("No partitions found!");
            }
        }

        protected virtual void InitializeFileSystems()
        {
            for (int i = 0; i < mPartitions.Count; i++)
            {
                string xRootPath = string.Concat(i, VolumeSeparatorChar, DirectorySeparatorChar);
                Cosmos.System.FileSystem.FileSystem xFileSystem = null;
                switch (Cosmos.System.FileSystem.FileSystem.GetFileSystemType(mPartitions[i]))
                {
                    case FileSystemType.FAT:
                        xFileSystem = new FatFileSystem(mPartitions[i]);
                        mFileSystems.Add(new KVP<string, Cosmos.System.FileSystem.FileSystem>(xRootPath, xFileSystem));
                        break;
                    default:
                        FatHelpers.Debug("Unknown filesystem type!");
                        return;
                }

                //global::System.Console.Write("i = ");
                //global::System.Console.WriteLine(i.ToString());
                //global::System.Console.Write("mFileSystems.Count = ");
                //global::System.Console.WriteLine(mFileSystems.Count);
                var xEntry = mFileSystems[i];
                if (xEntry.Key == xRootPath)
                {
                    var xFatFS = (FatFileSystem)xFileSystem;
                    FatHelpers.Debug("-------File System--------");
                    FatHelpers.Debug("Bytes per Cluster: " + xFatFS.BytesPerCluster);
                    FatHelpers.Debug("Bytes per Sector: " + xFatFS.BytesPerSector);
                    FatHelpers.Debug("Cluster Count: " + xFatFS.ClusterCount);
                    FatHelpers.Debug("Data Sector: " + xFatFS.DataSector);
                    FatHelpers.Debug("Data Sector Count: " + xFatFS.DataSectorCount);
                    FatHelpers.Debug("FAT Sector Count: " + xFatFS.FatSectorCount);
                    FatHelpers.Debug("FAT Type: " + xFatFS.FatType);
                    FatHelpers.Debug("Number of FATS: " + xFatFS.NumberOfFATs);
                    FatHelpers.Debug("Reserved Sector Count: " + xFatFS.ReservedSectorCount);
                    FatHelpers.Debug("Root Cluster: " + xFatFS.RootCluster);
                    FatHelpers.Debug("Root Entry Count: " + xFatFS.RootEntryCount);
                    FatHelpers.Debug("Root Sector: " + xFatFS.RootSector);
                    FatHelpers.Debug("Root Sector Count: " + xFatFS.RootSectorCount);
                    FatHelpers.Debug("Sectors per Cluster: " + xFatFS.SectorsPerCluster);
                    FatHelpers.Debug("Total Sector Count: " + xFatFS.TotalSectorCount);

                    //Console.WriteLine();
                    //Console.WriteLine("Mapping Drive C...");
                    //FatFileSystem.AddMapping("C", mFileSystem);
                    //SentinelKernel.System.Filesystem.FAT.Listing.FatDirectory dir = new Sys.Filesystem.FAT.Listing.FatDirectory(mFileSystem, "Sentinel");
                }
                else
                {
                    FatHelpers.Debug("No filesystem found.");
                }
            }
        }

        protected Cosmos.System.FileSystem.FileSystem GetFileSystemFromPath(string aPath)
        {
            FatHelpers.Debug("In SentinelVFS.GetFileSystemFromPath");
            string xPath = Path.GetPathRoot(aPath);
            FatHelpers.Debug("PathRoot retrieved");
            for (int i = 0; i < mFileSystems.Count; i++)
            {
                string xTest = mFileSystems[i].Key;
                if (String.Equals(xTest, xPath))
                {
                    return mFileSystems[i].Value;
                }
            }
            throw new Exception("Unable to determine filesystem for path: " + aPath);
        }

        public override void Initialize()
        {
            mPartitions = new List<Partition>();
            mFileSystems = new List<KVP<string, Cosmos.System.FileSystem.FileSystem>>();

            InitializePartitions();
            if (mPartitions.Count > 0)
            {
                InitializeFileSystems();
            }
        }

        public override Directory GetDirectory(string aPath)
        {
            FatHelpers.Debug("In SentinelVFS.GetDirectory");
            var xFS = GetFileSystemFromPath(aPath);
            if (xFS == null)
            {
                FatHelpers.Debug("No FS found for path!");
            }
            else
            {
                FatHelpers.Debug("Filesystem found.");
            }

            return DoGetDirectory(aPath, xFS);
        }

        private Directory DoGetDirectory(string aPath, Cosmos.System.FileSystem.FileSystem aFS)
        {
            if (aFS == null)
            {
                throw new Exception("File system can not be null.");
            }
            FatHelpers.Debug("In SentinelVFS.DoGetDirectory");
            FatHelpers.Debug("Path = " + aPath);
            string[] xPathParts = VFSManager.SplitPath(aPath);

            if (xPathParts.Length == 1)
            {
                return GetVolume(aFS, aPath);
            }

            Directory xBaseDirectory = null;

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
                        if (xListingItem is Directory)
                        {
                            xBaseDirectory = (Directory)xListingItem;
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

        public override List<Base> GetDirectoryListing(string aPath)
        {
            FatHelpers.Debug("In SentinelVFS.GetDirectoryListing");
            var xFS = GetFileSystemFromPath(aPath);
            FatHelpers.Debug("Filesystem retrieved");
            var xDirectory = DoGetDirectory(aPath, xFS);
            return xFS.GetDirectoryListing(xDirectory);
        }

        public override List<Base> GetDirectoryListing(Directory aParentDirectory)
        {
            throw new NotImplementedException();
        }

        public override Directory GetVolume(string aVolume)
        {
            throw new NotImplementedException();
        }

        public override List<Directory> GetVolumes()
        {
            throw new NotImplementedException();
        }

        public Directory GetVolume(Cosmos.System.FileSystem.FileSystem filesystem, string name)
        {
            return filesystem.GetRootDirectory(name);
        }
    }
}
