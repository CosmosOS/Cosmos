using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using Cosmos.Common.Extensions;
using System;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class DirectoryInfoTest
    {
        /// <summary>
        /// Tests System.IO.DirectoryInfo plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            var xPath = @"0:\DiTest";
            var xDi = new DirectoryInfo(xPath);

            mDebugger.Send("START TEST: Create");
            xDi.Create();

            Assert.IsTrue(xDi.Exists == true, "DirectoryInfo.Create failed: directory does not exists");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            /* OK the Directory has been created, let's test some Properties */
            mDebugger.Send("START TEST: Get Name");
            Assert.IsTrue(xDi.Name == "DiTest", "DirectoryInfo.Name failed: directory has wrong name");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Get FullName");
            Assert.IsTrue(xDi.FullName == xPath, "DirectoryInfo.FullName failed: directory has wrong path");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Get Parent");
            var xDiParent = xDi.Parent;
            var ExpectedParentFullName = @"0:\";

            Assert.IsTrue(xDiParent.FullName == ExpectedParentFullName, "DirectoryInfo.Parent is wrong");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Get Attributes:");

            // This requires a plug in EnumImpl to work... but it will requires Reflection probably
            //bool isReallyADir = xDi.Attributes.HasFlag(FileAttributes.Directory);
            bool isReallyADir = ((xDi.Attributes & FileAttributes.Directory) == FileAttributes.Directory);
            Assert.IsTrue(isReallyADir == true, "DirectoryInfo.Attributes is wrong: directory NOT a directory.");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: CreateSubdirectory");
            var xSubDi = xDi.CreateSubdirectory("SubDir");

            Assert.IsTrue(xSubDi.Exists == true, "DirectoryInfo.CreateSubdirectory failed");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: GetDirectories on xDi");
            var xDirs = xDi.GetDirectories();
            Assert.IsTrue(xDirs != null, "GetDirectories() failed it returns null array");
            Assert.IsTrue(xDirs.Length != 0, "GetDirectories() failed it returns empty array");
            Assert.IsTrue(xDirs[0].FullName == xSubDi.FullName, "GetDirectories() does not return the expected directories");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            /* Now redo the same test but for xSubDir we expect an empty array as the Directory is indeed empty */
            mDebugger.Send("START TEST: GetDirectories on xSubDi");
            xDirs = xSubDi.GetDirectories();
            Assert.IsTrue(xDirs != null, "GetDirectories() failed returns null array");
            Assert.IsTrue(xDirs.Length == 0, "GetDirectories() failed does not returns empty array");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // XXX EnumerateDirectories() and EnumerateFiles() can not be neither tested as IL2CPU crashes!
            //var xEnumDir = xDi.EnumerateDirectories();

            // Not working xFiles[0].FullName is null I'll return on it when I'll test FileInfo
#if false
            mDebugger.Send("START TEST: GetFiles on xDi");
            //var expectedFilePath = Path.Combine(xDi.FullName, "test");
            var expectedFilePath = @"0:\DiTest\test";
            File.Create(expectedFilePath);
            var xFiles = xDi.GetFiles();
            Assert.IsTrue(xFiles != null, "GetFiles() failed it returns null array");
            Assert.IsTrue(xFiles.Length != 0, "GetFiles() failed it returns empty array");
            Assert.IsTrue(xFiles[0].FullName == expectedFilePath, "GetFiles() does not return the expected directories");
            mDebugger.Send("END TEST");
#endif

            // TODO we need to implement Move at VFS level to have this working
#if false
            mDebugger.Send("START TEST: Moving SubDir");


            xSubDi.MoveTo("SubDirMoved");

            // xSubDir continues to refer to the old path so the directory does not exist anymore
            Assert.IsTrue(xSubDi.Exists == false, "DirectoryInfo.MoveTo failed directory is yet in the old path");

            // Let's change the reference of xSubDi to the new path now we should find the Directory
            xSubDi = new DirectoryInfo("SubDirMoved");
            Assert.IsTrue(xSubDi.Exists == true, "DirectoryInfo.MoveTo failed directory does not exist in the new path");
            mDebugger.Send("END TEST");
#endif
            mDebugger.Send("START TEST: Delete");
            xDi.Delete(recursive: true);

            /* This is working OK, finally */
            Assert.IsTrue(xDi.Exists == false, "DirectoryInfo.Delete failed: directory continues exists");
            mDebugger.Send("END TEST");

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
