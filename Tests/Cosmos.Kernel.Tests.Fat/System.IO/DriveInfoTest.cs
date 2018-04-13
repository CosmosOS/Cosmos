using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using System;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    public class DriveInfoTest
    {
        /// <summary>
        /// Tests System.IO.DriveInfo plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            string driveName = @"0:\";
            var MyDrive = new DriveInfo(driveName);

            mDebugger.Send("START TEST: Get Name");

            Assert.IsTrue(MyDrive.Name == driveName, "DriveInfo.Name failed drive has wrong name");

            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Get TotalSize");
            long MyDriveSize = MyDrive.TotalSize;

            mDebugger.Send($"Size is {MyDriveSize}");

            Assert.IsTrue(MyDriveSize > 0, "DriveInfo.TotalSize failed: invalid size");
            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Get AvailableFreeSpace");
            long MyDriveAvailableFreeSpace = MyDrive.AvailableFreeSpace;

            mDebugger.Send($"AvailableFreeSpace {MyDriveAvailableFreeSpace}");

            Assert.IsTrue(MyDriveAvailableFreeSpace >= 0, "DriveInfo.AvailableFreeSpace failed: invalid size");

            Assert.IsFalse(MyDriveAvailableFreeSpace > MyDrive.TotalSize, "DriveInfo.AvailableFreeSpace failed: more than TotalSize");

            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Get TotalFreeSpace");
            long MyDriveTotalFreeSpace = MyDrive.TotalFreeSpace;

            mDebugger.Send($"TotalFreeSpace {MyDriveTotalFreeSpace}");

            Assert.IsTrue(MyDriveTotalFreeSpace >= 0, "DriveInfo.TotalFreeSpace failed: invalid size");

            Assert.IsFalse(MyDriveTotalFreeSpace > MyDrive.TotalSize, "DriveInfo.TotalFreeSpace failed: more than TotalSize");

            /*
             * If disk quotas are enabled AvailableFreeSpace and TotalFreeSpace could be different numbers but TotalFreeSpace
             * should be always >= of AvailableFreeSpace
             */
            Assert.IsTrue(MyDriveTotalFreeSpace >= MyDriveAvailableFreeSpace, "DriveInfo.MyDriveTotalFreeSpace failed: less than AvailableFreeSpace");

            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Get RootDirectory");

            var xDi = MyDrive.RootDirectory;

            Assert.IsTrue(xDi.Name == MyDrive.Name, "RootDirectory failed");
            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Get DriveFormat");

            Assert.IsTrue(MyDrive.DriveFormat == "FAT32", "DriveFormat failed");
            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Get VolumeLabel");

            string OriginalVolumeLabel = MyDrive.VolumeLabel;

            mDebugger.Send($"Volume Label of {MyDrive.Name} is {MyDrive.VolumeLabel}");

            Assert.IsTrue(OriginalVolumeLabel == "COSMOS", "VolumeLabel Get failed not the expected value");

            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Set VolumeLabel to <TEST>");
            // Now try to change it...
            String NewVolumeLabel = "TEST";
            mDebugger.Send($"Changing Volume Label to {NewVolumeLabel}...");
            MyDrive.VolumeLabel = NewVolumeLabel;

            string SetVolumeLabel = MyDrive.VolumeLabel;

            mDebugger.Send($"Volume Label of {MyDrive.Name} is {SetVolumeLabel}");

            Assert.IsTrue(SetVolumeLabel == NewVolumeLabel, "VolumeLabel Set failed: not the expected value");

            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Set VolumeLabel (restoring original label)");
            // Now try to change it...
            mDebugger.Send($"Changing Volume Label to {OriginalVolumeLabel}...");
            MyDrive.VolumeLabel = OriginalVolumeLabel;

            SetVolumeLabel = MyDrive.VolumeLabel;

            mDebugger.Send($"Volume Label of {MyDrive.Name} is {SetVolumeLabel}");

            Assert.IsTrue(SetVolumeLabel == OriginalVolumeLabel, "VolumeLabel Set failed: not the expected value");

            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Testing isReady status of the Drive");

            Assert.IsTrue(MyDrive.IsReady, "IsReady failed drive not ready");

            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Testing DriveType");

            Assert.IsTrue(MyDrive.DriveType == DriveType.Fixed, "DriveType failed drive not of Fixed type");

            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Getting all drives info");

            DriveInfo[] xDrives = DriveInfo.GetDrives();

            Assert.IsFalse(xDrives == null, "GetDrives failed: null array returned");

            // I suppose that at least a drive is recognized by Cosmos
            Assert.IsTrue(xDrives.Length > 0, "GetDrives failed: no drive recognized");

            /* Check that only the Name property of the first one is the same of driveName */
            Assert.IsTrue(xDrives[0].Name == driveName, "GetDrives failed: first drive is not the same of MyDrive (0:)");

            mDebugger.Send("END TEST");
        }
    }
}
