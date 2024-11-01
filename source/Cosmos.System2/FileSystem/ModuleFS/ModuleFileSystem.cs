using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core.Multiboot;
using Cosmos.Core.Multiboot.Tags;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.ModuleFS
{
    public class ModuleFileSystem : FileSystem
    {
        private string _label = "MFSDisk";
        public override long AvailableFreeSpace => 0;
        public override long TotalFreeSpace => 0;
        public override string Type => "ModuleFS";
        public override string Label { get => _label; set => _label = value; }

        internal string rootPath;

        public ModuleFileSystem(long Size, string rootPath) : base(null, rootPath, Size)
        {
            this.rootPath = rootPath;
        }

        private unsafe string GetName(MB2Module module)
        {
            string str = "";

            for (int i = 0; module.CommandLine[i] != 0; i++)
            {
                str = str + module.CommandLine[i];
            }
            return str;
        }

        internal unsafe byte[] MFSReadFile(string path)
        {
            byte[] data = null;

            foreach (MB2Module module in Multiboot2.Modules)
            {
                if (path == GetName(module))
                {
                    byte* file_pointer = (byte*)module.ModuleStartAddress;
                    data = new byte[module.ModuleEndAddress - module.ModuleStartAddress];

                    for (int i = 0; i < module.ModuleEndAddress - module.ModuleStartAddress; i++)
                    {
                        data[i] = file_pointer[i];
                    }
                }
            }

            return data;
        }

        public override DirectoryEntry CreateDirectory(DirectoryEntry aParentDirectory, string aNewDirectory) => throw new NotImplementedException();
        public override DirectoryEntry CreateFile(DirectoryEntry aParentDirectory, string aNewFile) => throw new NotImplementedException();
        public override void DeleteDirectory(DirectoryEntry aPath) => throw new NotImplementedException();
        public override void DeleteFile(DirectoryEntry aPath) => throw new NotImplementedException();

        public override void DisplayFileSystemInfo()
        {
            global::System.Console.WriteLine("Volume label is " + _label);
        }

        public override void Format(string aDriveFormat, bool aQuick) => throw new NotImplementedException("Read-only File system");
        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory)
        {
            List<DirectoryEntry > list = new List<DirectoryEntry>();

            foreach (MB2Module module in Multiboot2.Modules)
            {
                list.Add(new MFSDirectoryEntry(this, rootPath + GetName(module), GetName(module), module.ModuleEndAddress - module.ModuleStartAddress, DirectoryEntryTypeEnum.File));
            }
            return list;
        }

        public override DirectoryEntry GetRootDirectory()
        {
            return new MFSDirectoryEntry(this, rootPath, "RootDir", 0, DirectoryEntryTypeEnum.Directory);
        }
    }
}
