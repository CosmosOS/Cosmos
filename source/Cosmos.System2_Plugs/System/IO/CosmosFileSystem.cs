//#define COSMOSDEBUG
using System;
using System.IO;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

/*
 * This plug is a little different from usual because in future while now is plugging Win32FileSystem
 * when we will compile Cosmos on Linux / MacOS this will have to plug UnixFileSystem, using the attibute
 * Inheritable we should accomplish this without problems.
 * It is for this that the name is different too the usual plugs and does not ends in "Impl"
 */
namespace Cosmos.System_Plugs.System.IO
{
    [Plug(TargetName = "System.IO.FileSystem, System.IO.FileSystem", Inheritable = true)]
    public static class CosmosFileSystem
    {
        /*
         * This "trick" will work until we have not multi threading / multi process then you must do in the correct
         * way (an "attribute" of the running thread itself?)
         */
        private static string mCurrentDirectory = string.Empty;

        public static void CreateDirectory(object aThis, string fullPath)
        {
            // If 'fullPath' exists already we return already without dealing with VFSManager
            if (DirectoryExists(aThis, fullPath))
                return;
 
            var xEntry = VFSManager.CreateDirectory(fullPath);

            if (xEntry == null)
            {
                throw new IOException("VFSManager.CreateDirectory() returns null");
            }
        }

        // Never called? Cosmos goes in StackOverflow... ILCPU gets lost and do not find this native method
        public static bool DirectoryExists(object aThis, string fullPath)
        {
            if (fullPath == null)
            {
                return false;
            }

            Global.mFileSystemDebugger.SendInternal($"DirectoryExists : fullPath = {fullPath}");
            return VFSManager.DirectoryExists(fullPath);
        }

        public static bool FileExists(object aThis, string fullPath)
        {
            if (fullPath == null)
            {
                return false;
            }

            Global.mFileSystemDebugger.SendInternal($"FileExists : fullPath = {fullPath}");
            return VFSManager.FileExists(fullPath);
        }

        public static void RemoveDirectory(object aThis, string fullPath, bool recursive)
        {
            Global.mFileSystemDebugger.SendInternal($"RemoveDirectory : fullPath = {fullPath}");
            VFSManager.DeleteDirectory(fullPath, recursive);
        }

        public static void MoveDirectory(object aThis, string sourceFullPath, string destFullPath)
        {
            throw new NotImplementedException("MoveDirectory not implemented");
        }

        public static string GetCurrentDirectory(object aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"Directory.GetCurrentDirectory : mCurrentDirectory = {mCurrentDirectory}");
            return mCurrentDirectory;
        }

        public static void SetCurrentDirectory(object aThis, string fullPath)
        {
            Global.mFileSystemDebugger.SendInternal($"Directory.SetCurrentDirectory : aPath = {fullPath}");
            mCurrentDirectory = fullPath;
        }

        public static object /* FileStreamBase */ Open(object aThis, string fullPath, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, FileStream parent)
        {
            Global.mFileSystemDebugger.SendInternal("In CosmosFileSystem.Open");
            if (fullPath == null)
            {
                Global.mFileSystemDebugger.SendInternal("In CosmosFileSystem.Open: Path == null is true");
                throw new ArgumentNullException("The file path cannot be null.");
            }
            if (fullPath.Length == 0)
            {
                Global.mFileSystemDebugger.SendInternal("In CosmosFileSystem.Open: Path.Length == 0 is true");
                throw new ArgumentException("The file path cannot be empty.");
            }

            // Naive, but working implementation of FileMode. Probably is better to do this at lower level...

            // Before let's see if aPath already exists
            bool aPathExists = File.Exists(fullPath);

            Stream aStream = null;

            Global.mFileSystemDebugger.SendInternal($"Create Mode aPath {fullPath}");

            var xEntry = VFSManager.CreateFile(fullPath);
            if (xEntry == null)
            {
                return null;
            }

            aStream = VFSManager.GetFileStream(fullPath);

#if false
            switch (mode)
            {
                case FileMode.Append:
                    // TODO it seems that GetFileStream effectively Creates the file if not exist
                    aStream = aPathExists ? VFSManager.GetFileStream(fullPath) : File.Create(fullPath);

                    if (aPathExists)
                    {
                        Global.mFileSystemDebugger.SendInternal("Append mode with aPath already existing let's seek to end of the file");
                        Global.mFileSystemDebugger.SendInternal("Actual aStream Lenght: " + aStream.Length);
                        aStream.Seek(0, SeekOrigin.End);
                    }
                    else
                    {
                        Global.mFileSystemDebugger.SendInternal("Append mode with aPath not existing let's create a new the file");
                    }
                    break;

                case FileMode.Create:
                    Global.mFileSystemDebugger.SendInternal("Create Mode aPath will be overwritten if existing");
                    // TODO it seems that GetFileStream effectively Creates the file if not exist
                    //aStream = File.Create(fullPath);
                    aStream = VFSManager.GetFileStream(fullPath);
                    break;

                case FileMode.CreateNew:
                    if (aPathExists)
                    {
                        Global.mFileSystemDebugger.SendInternal("CreateNew Mode with aPath already existing");
                        throw new IOException("File already existing but CreateNew Requested");
                    }

                    Global.mFileSystemDebugger.SendInternal("CreateNew Mode with aPath not existing new file created");
                    // TODO it seems that GetFileStream effectively Creates the file if it does not exist
                    //aStream = File.Create(fullPath);
                    aStream = VFSManager.GetFileStream(fullPath);
                    break;

                case FileMode.Open:
                    if (!aPathExists)
                    {
                        Global.mFileSystemDebugger.SendInternal("Open Mode with aPath not existing");
#warning TODO: Change IOException to FileNotFoundException, it asks for a plug
                        throw new IOException("File not existing but Open Requested");
                    }

                    Global.mFileSystemDebugger.SendInternal("Open Mode with aPath existing opening file");
                    // TODO it seems that GetFileStream effectively Creates the file if it does not exist
                    aStream = VFSManager.GetFileStream(fullPath);
                    aStream.Position = 0;
                    break;

                case FileMode.OpenOrCreate:
                    Global.mFileSystemDebugger.SendInternal("OpenOrCreate Mode with aPath not existing new file created");
                    // TODO it seems that GetFileStream effectively Creates the file if it does not exist
                    aStream = aPathExists ? VFSManager.GetFileStream(fullPath) : File.Create(fullPath);
                    break;

                case FileMode.Truncate:
                    if (!aPathExists)
                    {
                        Global.mFileSystemDebugger.SendInternal("Truncate Mode with aPath not existing");
                        throw new IOException("File not existing but Truncate Requested");
                    }

                    Global.mFileSystemDebugger.SendInternal("Truncate Mode with aPath existing change its lenght to 0 bytes");
                    // TODO it seems that GetFileStream effectively Creates the file if it does not exist
                    aStream = VFSManager.GetFileStream(fullPath);
                    aStream.SetLength(0);
                    break;

                default:
                    Global.mFileSystemDebugger.SendInternal("The mode " + mode + "is out of range");
                    throw new ArgumentOutOfRangeException("The file mode is invalid");
            }
#endif

            return aStream;
        }

        public static FileSystem Current([FieldAccess(Name = "System.IO.FileSystem System.IO.Win32FileSystem.this")]
        ref FileSystem aThis)
        {
            return aThis;
        }

        /* Other bug of IL2CPU that does not permit to use this, IEnumerableToArray() does not work :-( */
        public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(object aThis, string aPath, string searchPattern,
        SearchOption searchOption, [FieldType(Name = "System.IO.SearchTarget, System.IO.FileSystem")] int searchTarget)
        {
            // TODO only for directories for now, searchPath is ignored for now
            Global.mFileSystemDebugger.SendInternal("EnumerateFileSystemInfos");
            if (aPath == null)
            {
                throw new ArgumentNullException(aPath);
            }

            var xEntries = VFSManager.GetDirectoryListing(aPath);
            for (int i = 0; i < xEntries.Count; i++)
            {
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    yield return new DirectoryInfo(xEntries[i].mName);
                }
            }
        }

        public static void DeleteFile(object aThis, string fullPath)
        {
            Global.mFileSystemDebugger.SendInternal($"DeleteFile : fullPath = {fullPath}");
            VFSManager.DeleteFile(fullPath);
        }

        public static void CopyFile(object aThis, string sourceFullPath, string destFullPath, bool overwrite)
        {
            Global.mFileSystemDebugger.SendInternal($"CopyFile {sourceFullPath} into {destFullPath} with overwrite {overwrite}");

            // The destination path may just be a directory into which the file should be copied.
            // If it is, append the filename from the source onto the destination directory
            if (Directory.Exists(destFullPath))
            {
                destFullPath = Path.Combine(destFullPath, Path.GetFileName(sourceFullPath));
            }

            // Copy the contents of the file from the source to the destination, creating the destination in the process
            using (var src = new FileStream(sourceFullPath, FileMode.Open))
            using (var dst = new FileStream(destFullPath, overwrite ? FileMode.Create : FileMode.CreateNew))
            {
                int xSize = (int)src.Length;
                Global.mFileSystemDebugger.SendInternal($"size of {sourceFullPath} is {xSize} bytes");
                byte[] content = new byte[xSize];
                Global.mFileSystemDebugger.SendInternal($"content byte buffer allocated");
                src.Read(content, 0, xSize);
                Global.mFileSystemDebugger.SendInternal($"content byte buffer read");
                dst.Write(content, 0, xSize);
                Global.mFileSystemDebugger.SendInternal($"content byte buffer written");
            }
        }
    }
}
