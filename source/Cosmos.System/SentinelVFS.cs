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
                global::System.Console.Write("BlockDevice found: ");
                global::System.Console.WriteLine(xBlockDevice.ToString());
                if (xBlockDevice is Partition)
                {
                    mPartitions.Add((Partition)xBlockDevice);
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
                Cosmos.System.FileSystem.FileSystem xFileSystem = null;
                switch (Cosmos.System.FileSystem.FileSystem.GetFileSystemType(mPartitions[i]))
                {
                    case FileSystemType.FAT:
                        xFileSystem = new FatFileSystem(mPartitions[i]);
                        mFileSystems.Add(new KVP<string, Cosmos.System.FileSystem.FileSystem>(xRootPath, xFileSystem));
                        break;
                    default:
                        global::System.Console.WriteLine("Unknown filesystem type!");
                        return;
                }

                global::System.Console.Write("i = ");
                global::System.Console.WriteLine(i.ToString());
                global::System.Console.Write("mFileSystems.Count = ");
                global::System.Console.WriteLine(mFileSystems.Count);
                var xEntry = mFileSystems[i];
                if (xEntry.Key == xRootPath)
                {
                    var xFatFS = (FatFileSystem)xEntry.Value;
                    global::System.Console.WriteLine("-------File System--------");
                    global::System.Console.WriteLine("Bytes per Cluster: " + xFatFS.BytesPerCluster);
                    global::System.Console.WriteLine("Bytes per Sector: " + xFatFS.BytesPerSector);
                    global::System.Console.WriteLine("Cluster Count: " + xFatFS.ClusterCount);
                    global::System.Console.WriteLine("Data Sector: " + xFatFS.DataSector);
                    global::System.Console.WriteLine("Data Sector Count: " + xFatFS.DataSectorCount);
                    global::System.Console.WriteLine("FAT Sector Count: " + xFatFS.FatSectorCount);
                    global::System.Console.WriteLine("FAT Type: " + xFatFS.FatType);
                    global::System.Console.WriteLine("Number of FATS: " + xFatFS.NumberOfFATs);
                    global::System.Console.WriteLine("Reserved Sector Count: " + xFatFS.ReservedSectorCount);
                    global::System.Console.WriteLine("Root Cluster: " + xFatFS.RootCluster);
                    global::System.Console.WriteLine("Root Entry Count: " + xFatFS.RootEntryCount);
                    global::System.Console.WriteLine("Root Sector: " + xFatFS.RootSector);
                    global::System.Console.WriteLine("Root Sector Count: " + xFatFS.RootSectorCount);
                    global::System.Console.WriteLine("Sectors per Cluster: " + xFatFS.SectorsPerCluster);
                    global::System.Console.WriteLine("Total Sector Count: " + xFatFS.TotalSectorCount);

                    //Console.WriteLine();
                    //Console.WriteLine("Mapping Drive C...");
                    //FatFileSystem.AddMapping("C", mFileSystem);
                    //SentinelKernel.System.Filesystem.FAT.Listing.FatDirectory dir = new Sys.Filesystem.FAT.Listing.FatDirectory(mFileSystem, "Sentinel");
                }
                else
                {
                    global::System.Console.WriteLine("No filesystem found.");
                }
            }

            /*
            Console.WriteLine("Mapping...");
            TheFatFileSystem.AddMapping("C", xFS);

            Console.WriteLine();
            Console.WriteLine("Root directory");

            var xListing = xFS.GetRoot();
            TheFatFile xRootFile = null;
            TheFatFile xKudzuFile = null;


            for (int j = 0; j < xListing.Count; j++)
            {
                var xItem = xListing[j];
                if (xItem is FAT.Listing.MyFatDirectory)
                {
                    //Detecting Dir in HDD
                    Console.WriteLine("<DIR> " + xListing[j].Name);
                }
                else if (xItem is FAT.Listing.MyFatFile)
                {
                    //Detecting File in HDD
                    Console.WriteLine("<FILE> " + xListing[j].Name + " (" + xListing[j].Size + ")");
                    if (xListing[j].Name == "Root.txt")
                    {
                        xRootFile = (TheFatFile)xListing[j];
                        Console.WriteLine("Root file found");
                    }
                    else if (xListing[j].Name == "Kudzu.txt")
                    {
                        xKudzuFile = (TheFatFile)xListing[j];
                        Console.WriteLine("Kudzu file found");
                    }
                }
            }

            try
            {
                Console.WriteLine();
                Console.WriteLine("StreamReader - Root File");
                if (xRootFile == null)
                {
                    Console.WriteLine("RootFile not found!");
                }
                var xStream = new TheFatStream(xRootFile);
                var xData = new byte[xRootFile.Size];
                var size = (int)xRootFile.Size;
                Console.WriteLine("Size: " + size);
                var sizeInt = (int)xRootFile.Size;
                xStream.Read(xData, 0, sizeInt);
                var xText = Encoding.ASCII.GetString(xData);
                Console.WriteLine(xText);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            if (xKudzuFile == null)
            {
                Console.WriteLine("KudzuFile not found!");
            }
            var xKudzuStream = new TheFatStream(xKudzuFile);
            var xKudzuData = new byte[xKudzuFile.Size];
            xKudzuStream.Read(xKudzuData, 0, (int)xKudzuFile.Size);

            var xFile = new System.IO.FileStream(@"c:\Root.txt", System.IO.FileMode.Open);


            Console.WriteLine("Complete...");
            */
        }

        protected Cosmos.System.FileSystem.FileSystem GetFileSystemFromPath(string aPath)
        {
            string xPath = Path.GetPathRoot(aPath);
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
            var xFS = GetFileSystemFromPath(aPath);

            return DoGetDirectory(aPath, xFS);
        }

        private Directory DoGetDirectory(string aPath, Cosmos.System.FileSystem.FileSystem aFS)
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
            var xFS = GetFileSystemFromPath(aPath);
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

        /*
        public override SentinelKernel.System.FileSystem.Listing.Directory GetVolume(string aVolume)
        {
            var xFS = GetFileSystemFromPath(aVolumeId);
            if (xFS != null)
            {
                return new FileSystemEntry()
                {
                    Name = aVolumeId.ToString(),
                    Filesystem = xFS,
                    IsDirectory = true,
                    IsReadonly = true,
                    Id = (ulong)aVolumeId
                };
            }

            return null;
        }

        public override SentinelKernel.System.FileSystem.Listing.Directory[] GetVolumes()
        {
            var xResult = new SentinelKernel.System.FileSystem.Listing.Directory[mFileSystems.Count];
            for (int i = 0; i < mFileSystems.Count; i++)
            {
                xResult[i] = GetVolume(mFileSystems[i].Key);
            }
            return xResult;
        }
        */

        /*
        protected void InitializeHardware()
        {
            Console.WriteLine();
            Console.WriteLine("Initializing hardware...");
            if (BlockDevice.Devices.Count > 0)
            {
                Console.WriteLine("Block devices found: " + BlockDevice.Devices.Count);
                InitializeATADevices();
                if (this.mATA != null)
                {
                    InitializePartitions();
                }
            }
            else
            {
                Console.WriteLine("No block devices found!");
            }
            Console.WriteLine("Complete...");
            Console.WriteLine("Press enter.");
            Console.ReadLine();
        }
        */

        /*
        protected void InitializeATADevices()
        {
            Console.WriteLine("Initializing ATA Dedices...");
            try
            {
                for (int i = 0; i < BlockDevice.Devices.Count; i++)
                {
                    if (BlockDevice.Devices[i] is AtaPio)
                    {
                        this.mATA = (AtaPio)BlockDevice.Devices[i];
                        break;
                    }
                }

                if (mATA != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("--------ATA Devices-------");
                    Console.WriteLine("Type: " + (mATA.DriveType == AtaPio.SpecLevel.ATA ? "ATA" : "ATAPI"));
                    Console.WriteLine("Serial No: " + mATA.SerialNo);
                    Console.WriteLine("Firmware Rev: " + mATA.FirmwareRev);
                    Console.WriteLine("Model No: " + mATA.ModelNo);
                    Console.WriteLine("Block Size: " + mATA.BlockSize + " bytes");
                    Console.WriteLine("Size: " + mATA.BlockCount * mATA.BlockSize / 1024 / 1024 + " MB");
                    Console.WriteLine("--------------------------");
                }
                else
                {
                    Console.WriteLine("No ATA devices found!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            Console.WriteLine("Complete...");
            Console.WriteLine("Press enter.");
            Console.ReadLine();
        }
        */
    }
}
