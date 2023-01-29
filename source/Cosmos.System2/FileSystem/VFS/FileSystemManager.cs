using System;
using System.Collections.Generic;

namespace Cosmos.System.FileSystem.VFS
{
    public static class FileSystemManager
    {
        public static List<FileSystemFactory> DefaultFileSystems {
            get {
                return new List<FileSystemFactory>() {
                    new FAT.FatFileSystemFactory(),
                    new ISO9660.ISO9660FileSystemFactory()
                };
            }
        }

        public static List<FileSystemFactory> RegisteredFileSystems { get; } = DefaultFileSystems;

        public static bool Register(FileSystemFactory factory)
        {
            foreach (var item in RegisteredFileSystems)
            {
                if(item.Name == factory.Name)
                {
                    return false;
                }
            }
            RegisteredFileSystems.Add(factory);
            return true;
        }

        public static bool Remove(FileSystemFactory factory)
        {
            return Remove(factory.Name);
        }

        public static bool Remove(string factoryName)
        {
            foreach (var item in RegisteredFileSystems)
            {
                if(item.Name == factoryName)
                {
                    RegisteredFileSystems.Remove(item);
                    return true;
                }
            }
            return false;
        }
    }
}
