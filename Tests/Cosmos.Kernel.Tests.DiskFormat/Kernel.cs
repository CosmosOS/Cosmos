using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.TestRunner;
using Sys = Cosmos.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Tests.DiskFormat
{
    public class Kernel : Sys.Kernel
    {
        public static VFSBase mVFS;

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            mVFS = new CosmosVFS();
            VFSManager.RegisterVFS(mVFS);
        }

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                DiskFormatTest.Execute(mDebugger);

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
