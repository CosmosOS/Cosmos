using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentinelKernel.System.FileSystem.VFS
{
    public abstract class VFSBase
    {
        public abstract void Initialize();

        public abstract System.FileSystem.Listing.Directory GetDirectory(string aPath);

        public abstract List<System.FileSystem.Listing.Base> GetDirectoryListing(string aPath);

        public abstract List<System.FileSystem.Listing.Base> GetDirectoryListing(System.FileSystem.Listing.Directory aEntry);

        public abstract System.FileSystem.Listing.Directory GetVolume(string aVolume);

        public abstract List<System.FileSystem.Listing.Directory> GetVolumes();

        public static char DirectorySeparatorChar { get { return '\\'; } }

        public static char AltDirectorySeparatorChar { get { return '/'; } }

        public static char VolumeSeparatorChar { get { return ':'; } }
    }
}
