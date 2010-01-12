using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cosmos.Sys.FileSystem.FAT32;
using Cosmos.Sys.FileSystem.ISO9660;
using Cosmos.Sys;
using Cosmos.Sys.Network;
using Cosmos.Kernel.ManagedMemory;
using Cosmos.Hardware.Storage.ATA;
using Cosmos.Debug;
using Cosmos.Hardware;

namespace Iso_Reader
{
    public unsafe partial class ReadBytes : ISO9660
    {
        Stream Stream;
        Cosmos.Sys.FileSystem.FilesystemEntry FSE;
        Cosmos.Sys.FileSystem.FilesystemEntry[] FSEList;
        Cosmos.Sys.FileSystem.FSDirectory FSD;
        Cosmos.Sys.FileSystem.FSFile FSF;
        string HeaderData;
        byte[] data;
        byte[] DetailedData;
        string First_DirectoryName;
        public string ReadBytes(uint ISO_Start_Sector, string ISO_Directory, string ISO_Name)
        {
            //Virtual File System Manager Setup
            VFSManager.Filesystems.Add(FSE.Filesystem);
            VFSManager.Init();

            Cosmos.Sys.FileSystem.PartitionManager.DriversInit();

            FSD.GetDirectory(ISO_Directory);
            FSF = FSD.GetFile(ISO_Name);
            data = ReadFully(FSF.GetStream(), (int)FSF.GetStream().Length);
            for (int i = 0; i < data.Length; i++)
            {

            }
            return HeaderData;
        }
    }
}
