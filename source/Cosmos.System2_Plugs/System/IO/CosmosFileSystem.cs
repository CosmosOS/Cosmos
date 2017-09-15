//#define COSMOSDEBUG
using System;
using System.IO;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.API;
using Cosmos.IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System_Plugs.System.IO
{
    //[Plug(TargetName = "System.IO.Win32FileSystem")]
    [Plug(TargetName = "System.IO.Win32FileSystem, System.IO.FileSystem") ]
    public static class CosmosFileSystem
    {
        public static void CreateDirectory(object aThis, string fullPath)
        {
            // TODO check if 'fullPath' exists already and return in this case

            // XXX If the Directory 'aPath' already exits this method must not do nothing
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

        public static void RemoveDirectory(object aThis, string fullPath, bool recursive)
        {
            Global.mFileSystemDebugger.SendInternal($"RemoveDirectory : fullPath = {fullPath}");
            VFSManager.DeleteDirectory(fullPath, recursive);
        }

        public static object /* FileStreamBase */ Open(object aThis, string fullPath, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, FileStream parent)
        {
            Global.mFileSystemDebugger.SendInternal("In FileStream.InitializeStream");
            if (fullPath == null)
            {
                Global.mFileSystemDebugger.SendInternal("In FileStream.Ctor: Path == null is true");
                throw new ArgumentNullException("The file path cannot be null.");
            }
            if (fullPath.Length == 0)
            {
                Global.mFileSystemDebugger.SendInternal("In FileStream.Ctor: Path.Length == 0 is true");
                throw new ArgumentException("The file path cannot be empty.");
            }

            // Naive, but working implementation of FileMode. Probably is better to do this at lower level...

            // Before let's see if aPath already exists
            bool aPathExists = File.Exists(fullPath);

            Stream aStream = null;

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
                    aStream = File.Create(fullPath);
                    break;

                case FileMode.CreateNew:
                    if (aPathExists)
                    {
                        Global.mFileSystemDebugger.SendInternal("CreateNew Mode with aPath already existing");
                        throw new IOException("File already existing but CreateNew Requested");
                    }

                    Global.mFileSystemDebugger.SendInternal("CreateNew Mode with aPath not existing new file created");
                    // TODO it seems that GetFileStream effectively Creates the file if it does not exist
                    aStream = File.Create(fullPath);
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

            return aStream;
        }
    }
}
