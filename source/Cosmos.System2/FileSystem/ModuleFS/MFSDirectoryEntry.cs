using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.ModuleFS
{
    public class MFSDirectoryEntry : DirectoryEntry
    {
        private ModuleFileSystem mfs;
        private string path;
        private string name;

        public MFSDirectoryEntry(ModuleFileSystem fs, string fullPath, string fileName, uint size, DirectoryEntryTypeEnum entryType) : base(fs, null, fullPath, fileName, size, entryType)
        {
            mfs = fs;
            path = fullPath;
            name = fileName;
        }

        public override Stream GetFileStream()
        {
            return new MemoryStream(mfs.MFSReadFile(path.Replace(mfs.rootPath, "")));
        }

        public override long GetUsedSpace() => 0;
        public override void SetName(string aName) => throw new NotImplementedException("Read-only File system");
        public override void SetSize(long aSize) => throw new NotImplementedException("Read-only File system");
    }
}
