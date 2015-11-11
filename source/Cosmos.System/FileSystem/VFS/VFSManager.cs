using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.VFS
{
    public static class VFSManager
    {
        private static VFSBase mVFS;

        public static void RegisterVFS(VFSBase aVFS)
        {
            Global.Dbg.Send("VFSManager.RegisterVFS");
            if (mVFS != null)
            {
                throw new Exception("Virtual File System Manager already initialized!");
            }

            aVFS.Initialize();
            mVFS = aVFS;
        }

        #region Helpers

        public static char GetAltDirectorySeparatorChar()
        {
            return '/';
        }

        public static char GetDirectorySeparatorChar()
        {
            return '\\';
        }

        public static char[] GetInvalidFileNameChars()
        {
            return new[]
            {
                '"',
                '<',
                '>',
                '|',
                '\0',
                '\a',
                '\b',
                '\t',
                '\n',
                '\v',
                '\f',
                '\r',
                ':',
                '*',
                '?',
                '\\',
                '/'
            };
        }

        public static char[] GetInvalidPathCharsWithAdditionalChecks()
        {
            return new[]
            {
                '"',
                '<',
                '>',
                '|',
                '\0',
                '\a',
                '\b',
                '\t',
                '\n',
                '\v',
                '\f',
                '\r',
                '*',
                '?'
            };
        }

        public static char GetPathSeparator()
        {
            return ';';
        }

        public static char[] GetRealInvalidPathChars()
        {
            return new[]
            {
                '"',
                '<',
                '>',
                '|',
                '\0',
                '\a',
                '\b',
                '\t',
                '\n',
                '\v',
                '\f',
                '\r'
            };
        }

        public static char GetVolumeSeparatorChar()
        {
            return ':';
        }

        public static int GetMaxPath()
        {
            return 260;
        }

        //public static bool IsAbsolutePath(this string aPath)
        //{
        //    return ((aPath[0] == VFSBase.DirectorySeparatorChar) || (aPath[0] == VFSBase.AltDirectorySeparatorChar));
        //}

        //public static bool IsRelativePath(this string aPath)
        //{
        //    return (aPath[0] != VFSBase.DirectorySeparatorChar || aPath[0] != VFSBase.AltDirectorySeparatorChar);
        //}

        public static string[] SplitPath(string aPath)
        {
            //TODO: This should call Path.GetDirectoryName() and then loop calling Directory.GetParent(), but those aren't implemented yet.
            return aPath.Split(GetDirectorySeparators(), StringSplitOptions.RemoveEmptyEntries);
        }

        private static char[] GetDirectorySeparators()
        {
            return new[] { GetDirectorySeparatorChar(), GetAltDirectorySeparatorChar() };
        }

        #endregion

        public static DirectoryEntry GetFile(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            string xFileName = Path.GetFileName(aPath);
            string xDirectory = Path.GetDirectoryName(aPath);
            var xLastChar = xDirectory[xDirectory.Length - 1];
            if (xLastChar != Path.DirectorySeparatorChar)
            {
                xDirectory = xDirectory + Path.DirectorySeparatorChar;
            }

            FatHelpers.Debug("VFSManager::GetFile: aPath = " + aPath);
            FatHelpers.Debug("VFSManager::GetFile: xFileName = " + xFileName);
            FatHelpers.Debug("VFSManager::GetFile: xDirectory = " + xDirectory);

            var xList = GetDirectoryListing(xDirectory);
            for (int i = 0; i < xList.Count; i++)
            {
                var xEntry = xList[i];
                if (xEntry != null)
                {
                    if (xEntry.EntryType == DirectoryEntryTypeEnum.File)
                    {
                        FatHelpers.Debug("VFSManager::GetFile: xEntry.EntryType = File");
                    }
                    else
                    {
                        FatHelpers.Debug("VFSManager::GetFile: xEntry.EntryType = Directory");
                    }
                    FatHelpers.Debug("VFSManager::GetFile: xEntry.Name = " + xEntry.Name);
                    FatHelpers.Debug("VFSManager::GetFile: xEntry.Size = " + xEntry.Size);
                }

                if ((xEntry != null) && (xEntry.EntryType == DirectoryEntryTypeEnum.File) && (xEntry.Name.ToUpper() == xFileName.ToUpper()))
                {
                    return xEntry;
                }
            }

            return null;
        }

        public static DirectoryInfo CreateDirectory(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            var entry = mVFS.GetDirectory(aPath);
            if (entry == null)
            {
                var splitPath = SplitPath(aPath);
                
            }
            if (entry != null)
            {
                var info = new DirectoryInfo(aPath);
                return info;
            }
            return null;

            //return mVFS.CreateDirectory(aPath);
        }

        public static DirectoryEntry GetDirectory(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            return mVFS.GetDirectory(aPath);
        }

        public static List<DirectoryEntry> GetDirectoryListing(string aPath)
        {
            if (string.IsNullOrEmpty(aPath))
            {
                throw new ArgumentNullException("aPath");
            }

            return mVFS.GetDirectoryListing(aPath);
        }

        public static DirectoryEntry GetVolume(string aVolume)
        {
            if (string.IsNullOrEmpty(aVolume))
            {
                throw new ArgumentNullException("aVolume");
            }

            return null;
        }

        public static List<DirectoryEntry> GetVolumes()
        {
            return null;
        }

        public static List<string> GetLogicalDrives()
        {
            //TODO: Directory.GetLogicalDrives() will call this.
            return null;

            /*
            List<string> xDrives = new List<string>();
            foreach (FilesystemEntry entry in GetVolumes())
                xDrives.Add(entry.Name + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar);

            return xDrives.ToArray();
            */
        }

        public static List<string> InternalGetFileDirectoryNames(string path, string userPathOriginal,
            string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption)
        {
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
            try
            {
                return (VFSManager.GetFile(aPath) != null);
            }
            catch (Exception E)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(E.Message);
                return false;
            }
        }

        public static bool DirectoryExists(string aPath)
        {
            try
            {
                FatHelpers.Debug("DirectoryExists. Path = '" + aPath + "'");
                string xDir = string.Concat(aPath, VFSBase.DirectorySeparatorChar);
                //xDir = Path.GetDirectoryName(xDir);
                return (VFSManager.GetDirectory(xDir) != null);
            }
            catch (Exception E)
            {
                global::System.Console.Write("Exception occurred: ");
                global::System.Console.WriteLine(E.Message);
                return false;
            }

        }

        public static Stream GetFileStream(string aPathname)
        {
            FatHelpers.Debug("VFSManager::GetFileStream: aPathName = " + aPathname);

            var xFileInfo = GetFile(aPathname);
            if (xFileInfo == null)
            {
                throw new Exception("File not found: " + aPathname);
            }

            return xFileInfo.FileSystem.GetFileStream(xFileInfo);
        }
    }

    //public static class VFSManager
    //{
    //    private static VFSBase mVFS;

    //    public static void RegisterVFS(VFSBase aVFS)
    //    {
    //        Cosmos.System.Global.Dbg.Send("VFSManager.RegisterVFS");
    //        if (mVFS != null)
    //        {
    //            throw new Exception("Virtual File System Manager already initialized!");
    //        }

    //        aVFS.Initialize();
    //        mVFS = aVFS;
    //    }

    //    #region Helpers

    //    public static char GetAltDirectorySeparatorChar()
    //    {
    //        return '/';
    //    }

    //    public static char GetDirectorySeparatorChar()
    //    {
    //        return '\\';
    //    }

    //    public static char[] GetInvalidFileNameChars()
    //    {
    //        return new[]
    //        {
    //            '"',
    //            '<',
    //            '>',
    //            '|',
    //            '\0',
    //            '\a',
    //            '\b',
    //            '\t',
    //            '\n',
    //            '\v',
    //            '\f',
    //            '\r',
    //            ':',
    //            '*',
    //            '?',
    //            '\\',
    //            '/'
    //        };
    //    }

    //    public static char[] GetInvalidPathCharsWithAdditionalChecks()
    //    {
    //        return new[]
    //        {
    //            '"',
    //            '<',
    //            '>',
    //            '|',
    //            '\0',
    //            '\a',
    //            '\b',
    //            '\t',
    //            '\n',
    //            '\v',
    //            '\f',
    //            '\r',
    //            '*',
    //            '?'
    //        };
    //    }

    //    public static char GetPathSeparator()
    //    {
    //        return ';';
    //    }

    //    public static char[] GetRealInvalidPathChars()
    //    {
    //        return new[]
    //        {
    //            '"',
    //            '<',
    //            '>',
    //            '|',
    //            '\0',
    //            '\a',
    //            '\b',
    //            '\t',
    //            '\n',
    //            '\v',
    //            '\f',
    //            '\r'
    //        };
    //    }

    //    public static char GetVolumeSeparatorChar()
    //    {
    //        return ':';
    //    }

    //    public static int GetMaxPath()
    //    {
    //        return 260;
    //    }

    //    //public static bool IsAbsolutePath(this string aPath)
    //    //{
    //    //    return ((aPath[0] == VFSBase.DirectorySeparatorChar) || (aPath[0] == VFSBase.AltDirectorySeparatorChar));
    //    //}

    //    //public static bool IsRelativePath(this string aPath)
    //    //{
    //    //    return (aPath[0] != VFSBase.DirectorySeparatorChar || aPath[0] != VFSBase.AltDirectorySeparatorChar);
    //    //}

    //    public static string[] SplitPath(string aPath)
    //    {
    //        //TODO: This should call Path.GetDirectoryName() and then loop calling Directory.GetParent(), but those aren't implemented yet.
    //        return aPath.Split(GetDirectorySeparators(), StringSplitOptions.RemoveEmptyEntries);
    //    }

    //    private static char[] GetDirectorySeparators()
    //    {
    //        return new[] { GetDirectorySeparatorChar(), GetAltDirectorySeparatorChar() };
    //    }

    //    #endregion

    //    public static Listing.File TryGetFile(string aPath)
    //    {
    //        if (aPath == null)
    //        {
    //            throw new Exception("Path can not be null.");
    //        }
    //        FatHelpers.Debug("In VFSManager.TryGetFile");
    //        FatHelpers.Debug(aPath);
    //        string xFileName = Path.GetFileName(aPath);
    //        string xDirectory = Path.GetDirectoryName(aPath);
    //        var xLastChar = xDirectory[xDirectory.Length - 1];
    //        if (xLastChar != Path.DirectorySeparatorChar)
    //        {
    //            xDirectory = xDirectory + Path.DirectorySeparatorChar;
    //        }
    //        FatHelpers.Debug("Now Try to get directory listing");
    //        var xList = GetDirectoryListing(xDirectory);
    //        for (int i = 0; i < xList.Count; i++)
    //        {
    //            var xEntry = xList[i];
    //            var xFile = xEntry as Listing.File;
    //            if (xFile != null && String.Equals(xEntry.Name, xFileName, StringComparison.OrdinalIgnoreCase))
    //            {
    //                FatHelpers.Debug("--- Returning file");
    //                FatHelpers.Debug("Name");
    //                FatHelpers.Debug(xFile.Name);
    //                return xFile;
    //            }
    //        }

    //        return null;
    //    }

    //    public static Listing.Directory TryGetDirectory(string aPath)
    //    {
    //        if (string.IsNullOrEmpty(aPath))
    //        {
    //            throw new Exception("Path can not be null.");
    //        }
    //        FatHelpers.Debug("In VFSManager.TryGetFile");
    //        string xFileName = Path.GetFileName(aPath);
    //        string xDirectory = Path.GetDirectoryName(aPath);
    //        FatHelpers.Debug("Filename: ");
    //        FatHelpers.Debug(xFileName);
    //        FatHelpers.Debug("Directory:");
    //        FatHelpers.Debug(xDirectory);
    //        var xLastChar = xDirectory[xDirectory.Length - 1];
    //        if (xLastChar != Path.DirectorySeparatorChar)
    //        {
    //            xDirectory = xDirectory + Path.DirectorySeparatorChar;
    //        }
    //        FatHelpers.Debug("Now Try to get directory listing");
    //        var xList = GetDirectoryListing(xDirectory);
    //        FatHelpers.DebugNumber((uint) xList.Count);
    //        for (int i = 0; i < xList.Count; i++)
    //        {
    //            var xEntry = xList[i];
    //            var xFile = xEntry as Listing.Directory;
    //            if (xFile != null && String.Equals(xEntry.Name, xFileName, StringComparison.OrdinalIgnoreCase))
    //            {
    //                return xFile;
    //            }
    //            else
    //            {
    //                FatHelpers.Debug("--Skipping item");
    //                if (xFile == null)
    //                {
    //                    FatHelpers.Debug("  File");
    //                }
    //                else
    //                {
    //                    FatHelpers.Debug("  Directory");
    //                }
    //                FatHelpers.Debug("  Name");
    //                FatHelpers.Debug(xEntry.Name);

    //            }
    //        }

    //        FatHelpers.Debug("Directory not found");
    //        FatHelpers.Debug(xFileName);
    //        return null;
    //    }

    //    public static List<Listing.File> GetFiles(string aPath)
    //    {
    //        if (string.IsNullOrEmpty(aPath))
    //        {
    //            throw new Exception("Path can not be null.");
    //        }

    //        return null;

    //        /*
    //        List<FilesystemEntry> xFiles = new List<FilesystemEntry>();
    //        var xDirName = Path.GetDirectoryName(aPath);
    //        var xEntries = GetDirectoryListing(xDirName);

    //        for (int i = 0; i < xEntries.Length; i++)
    //        {
    //            var entry = xEntries[i];
    //            if (!entry.IsDirectory)
    //                xFiles.Add(entry);
    //        }

    //        return xFiles.ToArray();
    //        */
    //    }

    //    public static Listing.Directory GetDirectory(string aPath)
    //    {
    //        if (string.IsNullOrEmpty(aPath))
    //        {
    //            throw new Exception("Path can not be null.");
    //        }
    //        FatHelpers.Debug("In VFSManager.GetDirectory");

    //        return mVFS.GetDirectory(aPath);
    //    }

    //    public static List<Listing.Base> GetDirectoryListing(string aPath)
    //    {
    //        if (string.IsNullOrEmpty(aPath))
    //        {
    //            throw new Exception("Path can not be null.");
    //        }

    //        return mVFS.GetDirectoryListing(aPath);
    //    }

    //    public static Listing.Directory GetVolume(string aVolume)
    //    {
    //        if (string.IsNullOrEmpty(aVolume))
    //        {
    //            throw new Exception("Path can not be null.");
    //        }

    //        return null;
    //    }

    //    public static List<Listing.Directory> GetVolumes()
    //    {
    //        return null;
    //    }



    //    public static List<string> GetLogicalDrives()
    //    {
    //        //TODO: Directory.GetLogicalDrives() will call this.
    //        return null;

    //        /*
    //        List<string> xDrives = new List<string>();
    //        foreach (FilesystemEntry entry in GetVolumes())
    //            xDrives.Add(entry.Name + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar);

    //        return xDrives.ToArray();
    //        */
    //    }

    //    public static List<string> InternalGetFileDirectoryNames(string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption)
    //    {
    //        return null;

    //        /*
    //        //TODO: Add SearchOption functionality
    //        //TODO: What is userPathOriginal?
    //        //TODO: Add SearchPattern functionality

    //        List<string> xFileAndDirectoryNames = new List<string>();

    //        //Validate input arguments
    //        if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
    //            throw new ArgumentOutOfRangeException("searchOption");

    //        searchPattern = searchPattern.TrimEnd(new char[0]);
    //        if (searchPattern.Length == 0)
    //            return new string[0];

    //        //Perform search in filesystem
    //        FilesystemEntry[] xEntries = GetDirectoryListing(path);

    //        foreach (FilesystemEntry xEntry in xEntries)
    //        {
    //            if (xEntry.IsDirectory && includeDirs)
    //                xFileAndDirectoryNames.Add(xEntry.Name);
    //            else if (!xEntry.IsDirectory && includeFiles)
    //                xFileAndDirectoryNames.Add(xEntry.Name);
    //        }

    //        return xFileAndDirectoryNames.ToArray();

    //         */
    //    }

    //    public static bool FileExists(string aPath)
    //    {
    //        try
    //        {
    //            FatHelpers.Debug("In VFSManager.FileExists");

    //            var xFile = VFSManager.TryGetFile(aPath);
    //            return (xFile != null);
    //        }
    //        catch (Exception E)
    //        {
    //            // don't ever remove this method, even if it doesn't contain any code!
    //            FatHelpers.Debug("Exception occurred in VFSManager.FileExists: ");
    //            // don't ever remove this method, even if it doesn't contain any code!
    //            FatHelpers.Debug(E.Message);
    //            return false;
    //        }
    //    }

    //    public static bool DirectoryExists(string aPath)
    //    {
    //        try
    //        {
    //            FatHelpers.Debug("DirectoryExists. Path = '" + aPath + "'");
    //            //xDir = Path.GetDirectoryName(xDir);
    //            FatHelpers.Debug("Before VFSManager.GetDirectory");

    //            var xDirectory = VFSManager.TryGetDirectory(aPath);
    //            if (xDirectory == null)
    //            {
    //                FatHelpers.Debug("Directory not found!");
    //                FatHelpers.Debug(aPath);
    //                return false;
    //            }
    //            FatHelpers.Debug("Directory.Name:");
    //            FatHelpers.Debug(xDirectory.Name);
    //            return (xDirectory != null);
    //        }
    //        catch (Exception E)
    //        {
    //            FatHelpers.Debug("Exception occurred in VFSManager.DirectoryExists: ");
    //            FatHelpers.Debug(E.Message);
    //            return false;
    //        }

    //    }

    //    public static Stream GetFileStream(string aPathname)
    //    {
    //        var xFileInfo = TryGetFile(aPathname);
    //        if (xFileInfo == null)
    //        {
    //            throw new Exception("File not found: " + aPathname);
    //        }

    //        return xFileInfo.FileSystem.GetFileStream(xFileInfo);
    //    }
    //}
}
