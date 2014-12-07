//using Cosmos.HAL.BlockDevice;

using Cosmos.HAL.BlockDevice;
using Cosmos.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = global::System.Console;

namespace SentinelKernel.System.FileSystem.VFS
{
    [Serializable]
    public struct KVP<TKey, TValue>
    {
        private TKey key;
        private TValue value;

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

        private List<KVP<string, FileSystem>> mFileSystems;

        protected virtual void InitializePartitions()
        {
            for (int i = 0; i < BlockDevice.Devices.Count; i++)
            {
                if (BlockDevice.Devices[i] is Partition)
                {
                    mPartitions.Add((Partition)BlockDevice.Devices[i]);
                }
            }

            if (mPartitions.Count > 0)
            {
                for (int i = 0; i < mPartitions.Count; i++)
                {
                    Console.WriteLine("Partition #: " + (i + 1));
                    Console.WriteLine("Block Size: " + mPartitions[i].BlockSize + " bytes");
                    Console.WriteLine("Size: " + mPartitions[i].BlockCount * mPartitions[i].BlockSize / 1024 / 1024 + " MB");
                }
            }
            else
            {
                Console.WriteLine("No partitions found!");
            }
        }

        protected virtual void InitializeFileSystems()
        {
            for (int i = 0; i < mPartitions.Count; i++)
            {
                string xRootPath = string.Concat(i, VFSBase.VolumeSeparatorChar, VFSBase.DirectorySeparatorChar);
                switch (FileSystem.GetFileSystemType(mPartitions[i]))
                {
                    case FileSystemType.FAT:
                        mFileSystems.Add(new KVP<string, FileSystem>(xRootPath, new System.FileSystem.FAT.FatFileSystem(mPartitions[i])));
                        break;
                }

                if (mFileSystems[i].Key == xRootPath)
                {
                    var xFatFS = mFileSystems[i].Value as System.FileSystem.FAT.FatFileSystem;
                    Console.WriteLine("-------File System--------");
                    Console.WriteLine("Bytes per Cluster: " + xFatFS.BytesPerCluster);
                    Console.WriteLine("Bytes per Sector: " + xFatFS.BytesPerSector);
                    Console.WriteLine("Cluster Count: " + xFatFS.ClusterCount);
                    Console.WriteLine("Data Sector: " + xFatFS.DataSector);
                    Console.WriteLine("Data Sector Count: " + xFatFS.DataSectorCount);
                    Console.WriteLine("FAT Sector Count: " + xFatFS.FatSectorCount);
                    Console.WriteLine("FAT Type: " + xFatFS.FatType);
                    Console.WriteLine("Number of FATS: " + xFatFS.NumberOfFATs);
                    Console.WriteLine("Reserved Sector Count: " + xFatFS.ReservedSectorCount);
                    Console.WriteLine("Root Cluster: " + xFatFS.RootCluster);
                    Console.WriteLine("Root Entry Count: " + xFatFS.RootEntryCount);
                    Console.WriteLine("Root Sector: " + xFatFS.RootSector);
                    Console.WriteLine("Root Sector Count: " + xFatFS.RootSectorCount);
                    Console.WriteLine("Sectors per Cluster: " + xFatFS.SectorsPerCluster);
                    Console.WriteLine("Total Sector Count: " + xFatFS.TotalSectorCount);

                    //Console.WriteLine();
                    //Console.WriteLine("Mapping Drive C...");
                    //FatFileSystem.AddMapping("C", mFileSystem);
                    //SentinelKernel.System.Filesystem.FAT.Listing.FatDirectory dir = new Sys.Filesystem.FAT.Listing.FatDirectory(mFileSystem, "Sentinel");
                }
                else
                {
                    Console.WriteLine("No filesystem found.");
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

        protected FileSystem GetFileSystemFromPath(string aPath)
        {
            string xPath = Path.GetPathRoot(aPath);
            for (int i = 0; i < mFileSystems.Count; i++)
            {
                string xTest = mFileSystems[i].Key;
                if (mFileSystems[i].Key == xPath)
                {
                    return mFileSystems[i].Value;
                }
            }
            return null;
        }

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

        public override System.FileSystem.Listing.Directory GetDirectory(string aPath)
        {
            string[] xPathParts = VFSManager.SplitPath(aPath);
            var xFS = GetFileSystemFromPath(aPath);

            if (xFS != null)
            {
                if (xPathParts.Length == 1)
                {
                    return GetVolume(xPathParts[0]);
                }

                for (int i = 0; i < xPathParts.Length; i++)
                {
                    var xListing = GetDirectoryListing(xPathParts[i]);
                    for (int j = 0; j < xListing.Count; j++)
                    {
                        if (xListing[j].Name == aPath)
                        {
                            return (System.FileSystem.Listing.Directory)xListing[j];
                        }
                    }
                }
            }
            return null;
        }

        public override List<System.FileSystem.Listing.Base> GetDirectoryListing(string aPath)
        {
            throw new NotImplementedException();
        }

        public override List<System.FileSystem.Listing.Base> GetDirectoryListing(System.FileSystem.Listing.Directory aEntry)
        {
            throw new NotImplementedException();
        }

        public override System.FileSystem.Listing.Directory GetVolume(string aVolume)
        {
            throw new NotImplementedException();
        }

        public override List<System.FileSystem.Listing.Directory> GetVolumes()
        {
            throw new NotImplementedException();
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
    }
}
