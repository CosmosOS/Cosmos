using Cosmos.TestRunner;
using Sys = Cosmos.System;
using System;
using Cosmos.Kernel.Tests.IO.System.IO;
using System.IO;
using Cosmos.HAL;
using Cosmos.System.FileSystem;
using XSharp.x86.Params;
using System.Collections.Generic;

namespace Cosmos.Kernel.Tests.IO
{
    public class Kernel : Sys.Kernel
    {
        private bool ExecuteFileStreamTests = true;
        private Sys.FileSystem.VFS.VFSBase mVFS;

        static readonly public byte[] xBytes = new byte[16]
        {
            0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7,
            0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF
        };

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting IO tests.");

            if (ExecuteFileStreamTests)
            {
                mVFS = new Sys.FileSystem.CosmosVFS();
                Sys.FileSystem.VFS.VFSManager.RegisterVFS(mVFS);
            }
        }

        private void DumpFolder(string aPath, string indent = "") {
            var files = Directory.GetFiles(aPath);
            foreach (var file in files) {
                mDebugger.Send(indent + "File: " + file);
            }

            var dirs = Directory.GetDirectories(aPath);
            foreach (var dir in dirs) {
                mDebugger.Send(indent + "Dir: " + dir);
                DumpFolder(aPath + "/" + dir, indent + "  ");
            }
        }

        private void TestLargeFileFromATAPI() {
            // Dump all partitions
            List<Disk> xDisks = mVFS.GetDisks();

            foreach (var xDisk in xDisks) {
                foreach (var xPart in xDisk.Partitions) {
                    mDebugger.Send("Partition: " + xPart.RootPath);
                    DumpFolder(xPart.RootPath);
                }
            }


            float secondCounter = 0;

            Global.PIT.T0Frequency = 100;
            Global.PIT.RegisterTimer(new(() => {
                secondCounter += 0.1f;
            }, 100000000 /* 100ms */, true));
            File.Copy("1:\\boot\\limine\\liminewp.bmp", "0:\\liminewp.bmp", true);

            mDebugger.Send("Took " + secondCounter + " seconds to copy large file from ATAPI to hard drive!");

            // check file identity
            var xFile1 = File.OpenRead("1:\\boot\\limine\\liminewp.bmp");
            var xFile2 = File.OpenRead("0:\\liminewp.bmp");

            if (xFile1.Length != xFile2.Length) {
                mDebugger.Send("File lengths do not match!");
                TestController.Failed();
                return;
            }

            for (int i = 0; i < Math.Min(128, xFile1.Length); i++) {
                if (xFile1.ReadByte() != xFile2.ReadByte()) {
                    mDebugger.Send("File contents do not match!");
                    TestController.Failed();
                    return;
                }
            }

            mDebugger.Send("File contents match!");
        }

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                MemoryStreamTest.Execute(mDebugger);
                BinaryWriterTest.Execute(mDebugger);
                BinaryReaderTest.Execute(mDebugger);
                StringReaderTest.Execute(mDebugger);
                StringWriterTest.Execute(mDebugger);

                // This times out in the test runner because its still pretty slow in bochs (as to be expected cause its emulated)
                //TestLargeFileFromATAPI();

                TestController.Completed();
            }
            catch(Exception e)
            {
                mDebugger.Send(e.GetType().Name + " occurred: " + e.Message);

                TestController.Failed();
            }
        }
    }
}
