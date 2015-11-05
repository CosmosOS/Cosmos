using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.VFS
{
    public abstract class VFSBase
    {
        public abstract void Initialize();

        public abstract Directory GetDirectory(string aPath);

        public abstract List<Base> GetDirectoryListing(string aPath);

        public abstract List<Base> GetDirectoryListing(Directory aEntry);

        public abstract Directory GetVolume(string aVolume);

        public abstract List<Directory> GetVolumes();

        public static char DirectorySeparatorChar => '\\';

        public static char AltDirectorySeparatorChar { get { return '/'; } }

        public static char VolumeSeparatorChar { get { return ':'; } }
    }
}
