using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentinelKernel
{
    public class SentinelVFS : VFSBase
    {
        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override Cosmos.System.FileSystem.Listing.Directory GetDirectory(string aPath)
        {
            throw new NotImplementedException();
        }

        public override List<Cosmos.System.FileSystem.Listing.Base> GetDirectoryListing(string aPath)
        {
            throw new NotImplementedException();
        }

        public override List<Cosmos.System.FileSystem.Listing.Base> GetDirectoryListing(Cosmos.System.FileSystem.Listing.Directory aEntry)
        {
            throw new NotImplementedException();
        }

        public override Cosmos.System.FileSystem.Listing.Directory GetVolume(string aVolume)
        {
            throw new NotImplementedException();
        }

        public override List<Cosmos.System.FileSystem.Listing.Directory> GetVolumes()
        {
            throw new NotImplementedException();
        }

        /*
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

        protected void InitializePartitions()
        {
            try
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
                    Console.WriteLine();
                    Console.WriteLine("--------Partitions--------");
                    for (int i = 0; i < mPartitions.Count; i++)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Partition #: " + (i + 1));
                        Console.WriteLine("Block Size: " + mPartitions[i].BlockSize + " bytes");
                        Console.WriteLine("Size: " + mPartitions[i].BlockCount * mPartitions[i].BlockSize / 1024 / 1024 + " MB");
                    }
                    Console.WriteLine();
                    Console.WriteLine("--------------------------");
                }
                else
                {
                    Console.WriteLine("No partitions found!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            Console.WriteLine("Complete...");
        }

        protected void InitializeFileSystems()
        {
            for (int i = 0; i < mPartitions.Count; i++)
            {
                string xRootPath = string.Concat(i, Path.VolumeSeparatorChar, Path.DirectorySeparatorChar);
                switch (FileSystem.GetFileSystemType(mPartitions[i]))
                {
                    case FileSystemType.FAT:
                        mFileSystems.Add(new KVP<string, FileSystem>(xRootPath, new FatFileSystem(mPartitions[i])));
                        break;
                }

                if (mFileSystems[0] != null)
                {
                    Console.WriteLine("-------File System--------");
                    Console.WriteLine("Bytes per Cluster: " + mFileSystems[0].BytesPerCluster);
                    Console.WriteLine("Bytes per Sector: " + mFileSystems[0].BytesPerSector);
                    Console.WriteLine("Cluster Count: " + mFileSystem.ClusterCount);
                    Console.WriteLine("Data Sector: " + mFileSystem.DataSector);
                    Console.WriteLine("Data Sector Count: " + mFileSystem.DataSectorCount);
                    Console.WriteLine("FAT Sector Count: " + mFileSystem.FatSectorCount);
                    Console.WriteLine("FAT Type: " + mFileSystem.FatType);
                    Console.WriteLine("Number of FATS: " + mFileSystem.NumberOfFATs);
                    Console.WriteLine("Reserved Sector Count: " + mFileSystem.ReservedSectorCount);
                    Console.WriteLine("Root Cluster: " + mFileSystem.RootCluster);
                    Console.WriteLine("Root Entry Count: " + mFileSystem.RootEntryCount);
                    Console.WriteLine("Root Sector: " + mFileSystem.RootSector);
                    Console.WriteLine("Root Sector Count: " + mFileSystem.RootSectorCount);
                    Console.WriteLine("Sectors per Cluster: " + mFileSystem.SectorsPerCluster);
                    Console.WriteLine("Total Sector Count: " + mFileSystem.TotalSectorCount);

                    Console.WriteLine();
                    Console.WriteLine("Mapping Drive C...");
                    FatFileSystem.AddMapping("C", mFileSystem);
                    SentinelKernel.System.Filesystem.FAT.Listing.FatDirectory dir = new Sys.Filesystem.FAT.Listing.FatDirectory(mFileSystem, "Sentinel");
                }
                else
                {
                    Console.WriteLine("No filesystem found.");
                }
            }
            
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

        public override SentinelKernel.System.FileSystem.Listing.Directory GetDirectory(string aPath)
        {
            string[] xPathParts = VFSManager.SplitPath(aPath);
            var xFS = GetFileSystemFromPath(aPath);
            int a = (int)Path.DirectorySeparatorChar;

            if (xFS != null)
            {
                if (xPathParts.Length == 1)
                {
                    return GetVolume(xPathParts[0]);
                }

                for (int i = 0; i < xPathParts.Length; i++)
                {
                    var xListing = GetDirectoryListing(xPathParts[i]);
                    for (int j = 0; j < xListing.Length; j++)
                    {
                        if (xListing[j].Name == aPath)
                        {
                            return (SentinelKernel.System.FileSystem.Listing.Directory)xListing[j];
                        }
                    }
                }
            }
            return null;
        }

        public override SentinelKernel.System.FileSystem.Listing.Base[] GetDirectoryListing(SentinelKernel.System.FileSystem.Listing.Directory aEntry)
        {
            if (!(aEntry is SentinelKernel.System.FileSystem.Listing.Directory))
            {
                throw new ArgumentException("Only Directories are allowed");
            }

            var xFS = ((SentinelKernel.System.FileSystem.Listing.Directory)aEntry).mFileSystem;
            return xFS.GetDirectoryListing(aEntry.Name);
        }

        public override SentinelKernel.System.FileSystem.Listing.Base[] GetDirectoryListing(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath is null in GetDirectoryListing");
            }

            if (aPath.Length == 1)
            {
                return GetVolumes();
            }
            else
            {
                var xParentItem = GetDirectory(aPath);

                var xResult = xParentItem.mFileSystem.GetDirectoryListing(xParentItem.Name);
                return xResult;
            }
        }

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
