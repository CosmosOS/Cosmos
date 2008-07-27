using System;
using System.Collections.Generic;
using System.Linq;
using Cosmos.FileSystem;
using Cosmos.FileSystem.Ext2;
using Cosmos.Hardware;
using Cosmos.Hardware.Storage;
using DebugUtil=Cosmos.FileSystem.DebugUtil;

namespace Cosmos.Sys {
    public static class VFSManager {
        private static List<Filesystem> mFilesystems;

        private static void DetectFilesystem(BlockDevice aDevice) {
            #region Ext2

            if (Ext2.BlockDeviceContainsExt2(aDevice)) {
                aDevice.Used = true;
                var xFS = new Ext2(aDevice);
                mFilesystems.Add(xFS);
            }

            #endregion
        }

        public static void Initialize() {
            if (mFilesystems != null) {
                throw new Exception("FSManager already initialized!");
            }
            mFilesystems = new List<Filesystem>(4);
            for (int i = 0; i < Device.Devices.Count; i++) {
                var xDevice = Device.Devices[i];
                if (xDevice.Type != Device.DeviceType.Storage) {
                    continue;
                }
                var xStorageDevice = xDevice as BlockDevice;
                if (xStorageDevice == null) {
                    continue;
                }
                if (xStorageDevice.Used) {
                    continue;
                }
                DetectFilesystem(xStorageDevice);
            }
            Hardware.DebugUtil.SendNumber("VFS",
                                          "Registered Filesystems",
                                          (uint)mFilesystems.Count,
                                          32);
        }

        //public static FilesystemEntry[] GetDirectoryListing() 
    }
}