using Cosmos.System.FileSystem.Listing;
using System;
using System.Collections.Generic;

namespace Cosmos.System.FileSystem.VFS
{
	public abstract class VFSBase
    {
		[Obsolete("Use instance method GetDirectorySeparatorChar")]
		public static char DirectorySeparatorChar { get { return '\\'; } }

		[Obsolete("Use instance method GetAltDirectorySeparatorChar")]
		public static char AltDirectorySeparatorChar { get { return '/'; } }

		[Obsolete("Use instance method GetVolumeSeparatorChar")]
		public static char VolumeSeparatorChar { get { return ':'; } }

		public abstract void Initialize();

        public abstract DirectoryEntry CreateFile(string aPath);

        public abstract DirectoryEntry CreateDirectory(string aPath);

        public abstract bool DeleteFile(DirectoryEntry aPath);

        public abstract bool DeleteDirectory(DirectoryEntry aPath);

        public abstract DirectoryEntry GetDirectory(string aPath);

        public abstract DirectoryEntry GetFile(string aPath);

        public abstract List<DirectoryEntry> GetDirectoryListing(string aPath);

        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry aEntry);

        public abstract DirectoryEntry GetVolume(string aVolume);

        public abstract List<DirectoryEntry> GetVolumes();

		public abstract string GetFullPath(DirectoryEntry aEntry);

		public abstract List<string> GetLogicalDrives();

		public abstract char GetAltDirectorySeparatorChar();

		public abstract char GetDirectorySeparatorChar();

		public abstract DirectoryEntry GetParent(string aPath);

		public abstract char[] GetDirectorySeparators();

		public abstract string[] SplitPath(string aPath);

		public abstract char[] GetInvalidFileNameChars();

		public abstract char[] GetInvalidPathCharsWithAdditionalChecks();

		public abstract char GetPathSeparator();

		public abstract int GetMaxPath();

		public abstract char[] GetRealInvalidPathChars();

		public abstract char[] GetTrimEndChars();

		public abstract char GetVolumeSeparatorChar();

		public abstract string GetTempPath();

		public abstract string GetTempFileName();

		public abstract string GetRandomFileName();
	}
}
