//#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.VFS
{
    public static class VFSManager
    {
        private static VFSBase mVFS;

		public static VFSBase VFSInstance
		{
			get
			{
				return mVFS;
			}
		}

		public static void RegisterVFS(VFSBase aVFS)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.RegisterVFS ---");

            if (mVFS != null)
            {
                throw new Exception("Virtual File System Manager already initialized!");
            }

            aVFS.Initialize();
            mVFS = aVFS;
        }

        public static DirectoryEntry CreateFile(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.CreateFile ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            return mVFS.CreateFile(aPath);
        }

        public static void DeleteFile(string aPath)
        {
            if (mVFS == null)
                throw new Exception("VFSManager isn't ready.");

            var xFile = mVFS.GetFile(aPath);

            if (xFile.mEntryType != DirectoryEntryTypeEnum.File)
                throw new UnauthorizedAccessException("The specified path isn't a file");

            mVFS.DeleteFile(xFile);
        }

        public static DirectoryEntry GetFile(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFile ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            string xFileName = Path.GetFileName(aPath);
            Global.mFileSystemDebugger.SendInternal("xFileName =");
            Global.mFileSystemDebugger.SendInternal(xFileName);

            string xDirectory = aPath.Remove(aPath.Length - xFileName.Length);
            Global.mFileSystemDebugger.SendInternal("xDirectory =");
            Global.mFileSystemDebugger.SendInternal(xDirectory);

            char xLastChar = xDirectory[xDirectory.Length - 1];
            if (xLastChar != Path.DirectorySeparatorChar)
            {
                xDirectory = xDirectory + Path.DirectorySeparatorChar;
            }

            var xList = GetDirectoryListing(xDirectory);
            for (int i = 0; i < xList.Count; i++)
            {
                var xEntry = xList[i];
                if ((xEntry != null) && (xEntry.mEntryType == DirectoryEntryTypeEnum.File)
                    && (xEntry.mName.ToUpper() == xFileName.ToUpper()))
                {
                    return xEntry;
                }
            }

            return null;
        }

        public static DirectoryEntry CreateDirectory(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.CreateDirectory ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            return mVFS.CreateDirectory(aPath);
        }

        public static void DeleteDirectory(string aPath, bool recursive)
        {
            if (mVFS == null)
                throw new Exception("VFSManager isn't ready.");

            var xDirectory = mVFS.GetDirectory(aPath);
            var xDirectoryListing = mVFS.GetDirectoryListing(xDirectory);

            if (xDirectory.mEntryType != DirectoryEntryTypeEnum.Directory)
                throw new IOException("The specified path isn't a directory");

            if (xDirectoryListing.Count > 0 && !recursive)
                throw new IOException("Directory is not empty");

            if (recursive)
            {
                foreach (var entry in xDirectoryListing)
                {
                    if (entry.mEntryType == DirectoryEntryTypeEnum.Directory)
                    {
                        DeleteDirectory(entry.mFullPath, true);
                    }
                    else if (entry.mEntryType == DirectoryEntryTypeEnum.File)
                    {
                        DeleteFile(entry.mFullPath);
                    }
                    else
                    {
                        throw new IOException("The directory contains a corrupted file");
                    }
                }
            }

            mVFS.DeleteDirectory(xDirectory);
        }

        public static DirectoryEntry GetDirectory(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetDirectory ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            return mVFS.GetDirectory(aPath);
        }

        public static List<DirectoryEntry> GetDirectoryListing(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetDirectoryListing ---");

            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException(nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("aPath =");
            Global.mFileSystemDebugger.SendInternal(aPath);

            return mVFS.GetDirectoryListing(aPath);
        }

        public static DirectoryEntry GetVolume(string aVolume)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetVolume ---");

            if (string.IsNullOrEmpty(aVolume))
            {
                throw new ArgumentNullException(nameof(aVolume));
            }

            Global.mFileSystemDebugger.SendInternal("aVolume =");
            Global.mFileSystemDebugger.SendInternal(aVolume);

            return mVFS.GetVolume(aVolume);
        }

        public static List<DirectoryEntry> GetVolumes()
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetVolumes ---");

            return mVFS.GetVolumes();
        }

        public static List<string> GetLogicalDrives()
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetLogicalDrives ---");

            return mVFS.GetLogicalDrives();
        }

        public static List<string> InternalGetFileDirectoryNames(
            string path,
            string userPathOriginal,
            string searchPattern,
            bool includeFiles,
            bool includeDirs,
            SearchOption searchOption)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.InternalGetFileDirectoryNames ---");

            return null;

            /*
            //TODO: Add SearchOption functionality
            //TODO: What is userPathOriginal?
            //TODO: Add SearchPattern functionality

            List<string> xFileAndDirectoryNames = new List<string>();

            //Validate input arguments
            if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
                throw new ArgumentOutOfRangeException("searchOption");

            searchPattern = searchPattern.TrimEnd(new char[0]);
            if (searchPattern.Length == 0)
                return new string[0];

            //Perform search in filesystem
            FilesystemEntry[] xEntries = GetDirectoryListing(path);

            foreach (FilesystemEntry xEntry in xEntries)
            {
                if (xEntry.IsDirectory && includeDirs)
                    xFileAndDirectoryNames.Add(xEntry.Name);
                else if (!xEntry.IsDirectory && includeFiles)
                    xFileAndDirectoryNames.Add(xEntry.Name);
            }

            return xFileAndDirectoryNames.ToArray();

             */
        }

        public static bool FileExists(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("VFSManager.FileExists");

            if (aPath == null)
            {
                return false;
            }

            try
            {
                Global.mFileSystemDebugger.SendInternal("aPath =");
                Global.mFileSystemDebugger.SendInternal(aPath);

                string xPath = Path.GetFullPath(aPath);
                Global.mFileSystemDebugger.SendInternal("After GetFullPath");
                Global.mFileSystemDebugger.SendInternal("xPath =");
                Global.mFileSystemDebugger.SendInternal(xPath);

                return GetFile(xPath) != null;
            }
            catch (Exception e)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(e.Message);
                return false;
            }
        }

		public static string GetTempPath()
		{
			return mVFS.GetTempPath();
		}

		public static string GetTempFileName()
		{
			return mVFS.GetTempFileName();
		}

		public static string GetRandomFileName()
		{
			return mVFS.GetRandomFileName();
		}

		public static bool FileExists(DirectoryEntry aEntry)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.FileExists ---");

            if (aEntry == null)
            {
                return false;
            }

            try
            {
                Global.mFileSystemDebugger.SendInternal("aEntry.mName =");
                Global.mFileSystemDebugger.SendInternal(aEntry.mName);

                string xPath = GetFullPath(aEntry);
                Global.mFileSystemDebugger.SendInternal("After GetFullPath");
                Global.mFileSystemDebugger.SendInternal("xPath =");
                Global.mFileSystemDebugger.SendInternal(xPath);

                return GetFile(xPath) != null;
            }
            catch (Exception e)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool DirectoryExists(string aPath)
        {
            if (String.IsNullOrEmpty(aPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aPath));
            }

            Global.mFileSystemDebugger.SendInternal("--- VFSManager.DirectoryExists ---");

            try
            {
                Global.mFileSystemDebugger.SendInternal("aPath =");
                Global.mFileSystemDebugger.SendInternal(aPath);

                string xPath = Path.GetFullPath(aPath);
                Global.mFileSystemDebugger.SendInternal("After GetFullPath");
                Global.mFileSystemDebugger.SendInternal("xPath =");
                Global.mFileSystemDebugger.SendInternal(xPath);

                return GetDirectory(xPath) != null;
            }
            catch (Exception e)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool DirectoryExists(DirectoryEntry aEntry)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.DirectoryExists ---");

            if (aEntry == null)
            {
                throw new ArgumentNullException(nameof(aEntry));
            }

            try
            {
                Global.mFileSystemDebugger.SendInternal("aEntry.mName =");
                Global.mFileSystemDebugger.SendInternal(aEntry.mName);

                string xPath = GetFullPath(aEntry);
                Global.mFileSystemDebugger.SendInternal("After GetFullPath");
                Global.mFileSystemDebugger.SendInternal("xPath =");
                Global.mFileSystemDebugger.SendInternal(xPath);

                return GetDirectory(xPath) != null;
            }
            catch (Exception e)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(e.Message);
                return false;
            }
        }

        public static string GetFullPath(DirectoryEntry aEntry)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFullPath ---");
           
            return mVFS.GetFullPath(aEntry);
        }

        public static Stream GetFileStream(string aPathname)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetFileStream ---");

            if (string.IsNullOrEmpty(aPathname))
            {
                return null;
            }

            Global.mFileSystemDebugger.SendInternal("aPathName =");
            Global.mFileSystemDebugger.SendInternal(aPathname);

            var xFileInfo = GetFile(aPathname);
            if (xFileInfo == null)
            {
                throw new Exception("File not found: " + aPathname);
            }

            return xFileInfo.GetFileStream();
        }

        public static char GetAltDirectorySeparatorChar()
        {
            return mVFS.GetAltDirectorySeparatorChar();
        }

        public static char GetDirectorySeparatorChar()
        {
            return mVFS.GetDirectorySeparatorChar();
        }

        public static char[] GetInvalidFileNameChars()
        {
			return mVFS.GetInvalidFileNameChars();
		}

        public static char[] GetInvalidPathCharsWithAdditionalChecks()
        {
			return mVFS.GetInvalidPathCharsWithAdditionalChecks();
		}

        public static char GetPathSeparator()
        {
			return mVFS.GetPathSeparator();
		}

        public static char[] GetRealInvalidPathChars()
        {
			return mVFS.GetRealInvalidPathChars();
		}

        public static char[] GetTrimEndChars()
        {
			return mVFS.GetTrimEndChars();
		}

        public static char GetVolumeSeparatorChar()
        {
			return mVFS.GetVolumeSeparatorChar();
		}

        public static int GetMaxPath()
        {
			return mVFS.GetMaxPath();
        }

        public static string[] SplitPath(string aPath)
        {
            //TODO: This should call Path.GetDirectoryName() and then loop calling Directory.GetParent(), but those aren't implemented yet.
            return mVFS.SplitPath(aPath);
        }

        private static char[] GetDirectorySeparators()
        {
            return mVFS.GetDirectorySeparators();
        }

        public static DirectoryEntry GetParent(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("--- VFSManager.GetParent ---");

			return mVFS.GetParent(aPath);
        }
    }
}
