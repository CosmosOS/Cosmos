using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.FileSystem {
    public class FilesystemEntry {
        public string Name;
        public bool IsDirectory;
        public bool IsReadonly;
        public ulong Length;
        public ulong Id;
    }
    public abstract class Filesystem {
        public abstract uint BlockSize {
            get;
        }

        public abstract FilesystemEntry[] GetDirectoryListing(ulong aId);

        public abstract void ReadBlock(ulong aId,
                                       ulong aBlock,
                                       byte[] aBuffer);
    }
}