using System;

using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.TestRunner;
using Sys = Cosmos.System;
using Cosmos.Kernel.Tests.Fat.System.IO;
using System.IO;

namespace Cosmos.Kernel.Tests.Fat
{
    /// <summary>
    /// The kernel implementation.
    /// </summary>
    /// <seealso cref="Cosmos.System.Kernel" />
    public class Kernel : Sys.Kernel
    {
        private VFSBase mVFS;

        /// <summary>
        /// Pre-run events
        /// </summary>
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully, now start testing");
            mVFS = new CosmosVFS();
            VFSManager.RegisterVFS(mVFS);
        }

        /// <summary>
        /// Main kernel loop
        /// </summary>
        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                PathTest.Execute(mDebugger);
                DirectoryTest.Execute(mDebugger);
                FileStreamTest.Execute(mDebugger);
                DirectoryInfoTest.Execute(mDebugger);
                StreamWriterStreamReaderTest.Execute(mDebugger);
                BinaryWriterBinaryReaderTest.Execute(mDebugger);
                FileInfoTest.Execute(mDebugger);
                DriveInfoTest.Execute(mDebugger);
                FileTest.Execute(mDebugger);
                
                TestController.Completed();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred");
                Console.WriteLine(e.ToString());
                mDebugger.Send("Exception occurred: " + e.ToString());
                TestController.Failed();
            }
        }
    }
}
