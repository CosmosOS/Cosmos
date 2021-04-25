﻿using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using System;
using Cosmos.System.FileSystem;
using System.Collections.Generic;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.Kernel.Tests.DiskManager
{
    public class DiskManagerTest
    {
        /// <summary>
        /// Tests DiskManager.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            string driveName = @"0:\";
            var MyDrive = new System.FileSystem.DiskManager(driveName);

            mDebugger.Send("START TEST: Get Name");

            Assert.IsTrue(MyDrive.Name == driveName, "DiskManager.Name failed drive has wrong name");

            mDebugger.Send("END TEST");

            /* How to really test this? I fear the other tests relies on the fact that there are files on 0: */

            mDebugger.Send("START TEST: Format");

            MyDrive.Format("FAT32", aQuick: true);

            mDebugger.Send("Format done testing HDD is really empty");

            var xDi = new DriveInfo(driveName);

            /* If the drive is emptry all Space should be free */
            Assert.IsTrue(xDi.TotalSize == xDi.TotalFreeSpace, "DiskManager.Format (quick) failed TotalFreeSpace is not the same of TotalSize");

            /* Let's try to create a new file on the Root Directory */
            File.Create(@"0:\newFile.txt");

            Assert.IsTrue(File.Exists(@"0:\newFile.txt") == true, "Failed to create new file after disk format");

            mDebugger.Send("END TEST");

            mDebugger.Send("Testing if you can create directories");

            Directory.CreateDirectory(@"0:\SYS\");
            Assert.IsTrue(Directory.GetDirectories(@"0:\SYS\").Length == 0, "Can create a directory and its content is emtpy");

            mDebugger.Send("END TEST");

            //while (true) ;
        }
    }
}
