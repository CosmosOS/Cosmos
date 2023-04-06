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
    //[Plug(TargetName = "System.IO.FileSystem, System.IO.FileSystem")]
    public static class CosmosFileSystem
    {
        private static void CreateDirectory(string fullPath)
        {
            Global.FileSystemDebugger.SendInternal("-- CosmosFileSystem.CreateDirectory -- fullPath = " + fullPath);
            // If 'fullPath' exists already we return already without dealing with VFSManager
            if (DirectoryExists(fullPath))
                return;

            var xEntry = VFSManager.CreateDirectory(fullPath);

            if (xEntry == null)
            {
                throw new IOException("VFSManager.CreateDirectory() returns null");
            }
        }

        public static void CreateDirectory(string aPath, byte[] securityDescriptor = null)
        {
            Global.FileSystemDebugger.SendInternal("-- CosmosFileSystem.CreateDirectory(string, byte[]) -- ");
            Global.FileSystemDebugger.SendInternal("aPath = " + aPath);
            CreateDirectory(aPath);
        }

        // Never called? Cosmos goes in StackOverflow... ILCPU gets lost and do not find this native method
        public static bool DirectoryExists(string fullPath)
        {
            if (fullPath == null)
            {
                return false;
            }

            Global.FileSystemDebugger.SendInternal($"DirectoryExists : fullPath = {fullPath}");
            return VFSManager.DirectoryExists(fullPath);
        }

        public static bool FileExists(string fullPath)
        {
            if (fullPath == null)
            {
                return false;
            }

            Global.FileSystemDebugger.SendInternal($"-- CosmosFileSystem.FileExists -- : fullPath = {fullPath}");
            return VFSManager.FileExists(fullPath);
        }

        public static void RemoveDirectory(string fullPath, bool recursive)
        {
            Global.FileSystemDebugger.SendInternal($"RemoveDirectory : fullPath = {fullPath}");
            VFSManager.DeleteDirectory(fullPath, recursive);
        }

        public static void MoveDirectory(string sourceFullPath, string destFullPath)
        {
            throw new NotImplementedException("MoveDirectory not implemented");
        }

        public static void DeleteFile(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                Global.mFileSystemDebugger.SendInternal($"DeleteFile : fullPath = {fullPath}");
                VFSManager.DeleteFile(fullPath);
            }
        }

        public static void CopyFile(string sourceFullPath, string destFullPath, bool overwrite)
        {
            Global.FileSystemDebugger.SendInternal($"CopyFile {sourceFullPath} into {destFullPath} with overwrite {overwrite}");

            // The destination path may just be a directory into which the file should be copied.
            // If it is, append the filename from the source onto the destination directory
            if (Directory.Exists(destFullPath))
            {
                destFullPath = Path.Combine(destFullPath, Path.GetFileName(sourceFullPath));
            }
            if (!File.Exists(destFullPath) || overwrite)
            {
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
}
