using Cosmos.TestRunner;
using Sys = Cosmos.System;
using System;
using Cosmos.Kernel.Tests.IO.System.IO;

namespace Cosmos.Kernel.Tests.IO
{
    public class Kernel : Sys.Kernel
    {
        private bool ExecuteFileStreamTests = false;
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

                TestController.Completed();
            }
            catch(Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);

                TestController.Failed();
            }
        }
    }
}
