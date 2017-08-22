using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using Cosmos.Common.Extensions;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class DirectoryInfoTest
    {
        /// <summary>
        /// Tests System.IO.StreamWriter plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            var xPath = @"0:\DiTest";
            var xDi = new DirectoryInfo(xPath);

            mDebugger.Send("START TEST: Create");
            xDi.Create();

            Assert.IsTrue(Directory.Exists(xPath), "DirectoryInfo.Create failed: directory does not exists");
            mDebugger.Send("END TEST");

            Assert.IsTrue(xDi.FullName == xPath, "DirectoryInfo.FullName failed: directory has wrong path");

            /* This hits a Stack Overflow (in CallVirt?) */
            //Debugger.DoBochsBreak();
            Assert.IsTrue(xDi.Exists == true, "DirectoryInfo.Create failed: directory does not exists");
            mDebugger.Send("END TEST");

            // Stack overflow again (same issue?)
            mDebugger.Send("START TEST: Get Attributes:");
            int attrs = (int) xDi.Attributes;
            mDebugger.Send($"Directory attributes (bmask) is {attrs}");
            //bool isReallyADir = xDi.Attributes.HasFlag(FileAttributes.Directory);
            bool isReallyADir = ((xDi.Attributes & FileAttributes.Directory) == FileAttributes.Directory);
            Assert.IsTrue(isReallyADir == false, "Directory.Attributes is wrong: directory NOT a directory.");
            mDebugger.Send("END TEST");

            /*
             * This does not work too exception regarding a path called 'temp' (?) on Console and then another
             * Stack Overflow :-(
             */
            mDebugger.Send("START TEST: CreateSubdirectory");
            xDi.CreateSubdirectory("0001");

            Assert.IsTrue(Directory.Exists(xPath + Path.DirectorySeparatorChar + "0001"), "DirectoryInfo.CreateSubdirectory failed");
            mDebugger.Send("END TEST");

            mDebugger.Send("START TEST: Delete");
            xDi.Delete();

            /* This is working OK, finally */
            Assert.IsTrue(!Directory.Exists(xPath), "DirectoryInfo.Delete failed: directory continues exists");

            // DateTime is broken see: https://github.com/CosmosOS/Cosmos/pull/553/files for some fixes
#if false
            mDebugger.Send("START TEST: Get Attributes:");
            var CreationTime = xDi.CreationTimeUtc;
            mDebugger.Send("END TEST: CreationTime:" + CreationTime);
#endif

            mDebugger.Send("");
        }
    }
}
