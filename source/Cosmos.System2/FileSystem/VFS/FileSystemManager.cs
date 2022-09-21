using System;
using System.Collections.Generic;

namespace Cosmos.System.FileSystem.VFS
{
    public static class FileSystemManager
    {
        private static List<FileSystemFactory> registeredFileSystems = 
            new List<FileSystemFactory>() { 
                new FAT.FatFileSystemFactory(), 
                new ISO9660.ISO9660FileSystemFactory()
            };
        
        public static List<FileSystemFactory> RegisteredFileSystems { get {
            return registeredFileSystems;
        }}

        public static bool Register(FileSystemFactory factory)
        {
            foreach (var item in registeredFileSystems)
            {
                if(item.Name == factory.Name)
                {
                    return false;
                }
            }
            registeredFileSystems.Add(factory);
            return true;
        }

        public static bool Remove(FileSystemFactory factory)
        {
            return Remove(factory.Name);
        }

        public static bool Remove(string factoryName)
        {
            foreach (var item in registeredFileSystems)
            {
                if(item.Name == factoryName)
                {
                    registeredFileSystems.Remove(item);
                    return true;
                }
            }
            return false;
        }
    }
}