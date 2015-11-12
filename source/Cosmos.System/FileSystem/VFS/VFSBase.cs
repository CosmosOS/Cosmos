﻿using System.Collections.Generic;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.VFS
{
    public abstract class VFSBase
    {
        public abstract void Initialize();

        public abstract DirectoryEntry CreateDirectory(string aPath);

        public abstract DirectoryEntry GetDirectory(string aPath);

        public abstract List<DirectoryEntry> GetDirectoryListing(string aPath);

        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry aEntry);

        public abstract DirectoryEntry GetVolume(string aVolume);

        public abstract List<DirectoryEntry> GetVolumes();

        public static char DirectorySeparatorChar { get { return '\\'; } }

        public static char AltDirectorySeparatorChar { get { return '/'; } }

        public static char VolumeSeparatorChar { get { return ':'; } }
    }
}
