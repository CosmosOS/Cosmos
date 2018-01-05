using System;

using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.TestRunner;
using Sys = Cosmos.System;
using Cosmos.Kernel.Tests.Fat.System.IO;

/*
 * Split FAT Tests in two separate kernels because it would take too much time ( > 47 minutes)
 * and make go AppVoyer in timeout. There is a bug somewhere or in Bochs or in the debug
 * connector for sure...
 * VmWare works correctly!
 */
namespace Cosmos.Kernel.Tests.Fat2
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
