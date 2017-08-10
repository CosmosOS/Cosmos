using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.FileSystem {
    /// <summary>
    /// Represents either a directory or a file in a filesystem. The entry also contains metadata, such as name, attributes, etc.
    /// </summary>
    public class FilesystemEntry {
        public ulong Id;
        public string Name;
        public bool IsDirectory;
        public bool IsReadonly;
        public ulong Size;
        public Filesystem Filesystem;
    }

    public abstract class Filesystem {
        public abstract uint BlockSize {
            get;
        }

        public abstract ulong RootId {
            get;
        }

        public abstract FilesystemEntry[] GetDirectoryListing(ulong aId);

        public abstract bool ReadBlock(ulong aId,
                                       ulong aBlock,
                                       byte[] aBuffer);
    }
}