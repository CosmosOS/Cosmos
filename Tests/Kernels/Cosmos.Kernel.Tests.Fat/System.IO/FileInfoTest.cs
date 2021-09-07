using System;
using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;
using Cosmos.Common.Extensions;
using static Cosmos.Kernel.Tests.Fat.System.IO.HelperMethods;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class FileInfoTest
    {
        /// <summary>
        /// Tests System.IO.FileInfo plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            var xPath = @"0:\FiTest";
            var xFi = new FileInfo(xPath);

            mDebugger.Send("xFi allocated");

            mDebugger.Send("START TEST: Create");
            xFi.Create();
            mDebugger.Send("xFi File Created");

            Assert.IsTrue(xFi.Exists == true, "FileInfo.Create failed: file does not exists");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            /* OK the File has been created, let's test some Properties */
            mDebugger.Send("START TEST: Get Name");
            Assert.IsTrue(xFi.Name == "FiTest", "FileInfo.Name failed: File has wrong name");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Get FullName");
            Assert.IsTrue(xFi.FullName == xPath, "FileInfo.FullName failed: file has wrong path");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Get Directory");
            var xDiParent = xFi.Directory;
            var ExpectedParentFullName = @"0:\";

            Assert.IsTrue(xDiParent.FullName == ExpectedParentFullName, "FileInfo.Directory is wrong");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Get DirectoryName");
            var xDiParentStr = xFi.DirectoryName;
            Assert.IsTrue(xDiParentStr == ExpectedParentFullName, "FileInfo.DirectoryName is wrong");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Get Name");
            var Name = xFi.Name;
            var ExpectedFileName = "FiTest";
            Assert.IsTrue(Name == ExpectedFileName, "FileInfo.Name is wrong");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Get Attributes");

            // This requires a plug in EnumImpl to work... but it will requires Reflection probably
            //bool isReallyADir = xDi.Attributes.HasFlag(FileAttributes.Directory);
            bool isReallyAFile = ((xFi.Attributes & FileAttributes.Normal) == FileAttributes.Normal);
            Assert.IsTrue(isReallyAFile == true, "FileInfo.Attributes is wrong: file NOT a file.");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            string writtenText = "This a test line!";
      
            mDebugger.Send("START TEST: Write text and then read it");
            /* OK let's try to write something on your xFi */
            using (StreamWriter sw = xFi.CreateText())
            {
                sw.Write(writtenText);
            }

            string readText;
            /* ... and now let's read it... */
            using (StreamReader sr = xFi.OpenText())
            {
                readText = sr.ReadToEnd();
            }

            Assert.IsTrue(writtenText == readText, "FileInfo write and read text failed");

            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Length property");
            Assert.IsTrue(xFi.Length == writtenText.Length, "FileInfo has not changed size after writing");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: CopyTo");
            var xFi2 = xFi.CopyTo(@"0:\FiTest2.txt");

            using (StreamReader sr = xFi2.OpenText())
            {
                readText = sr.ReadToEnd();
            }

            Assert.IsTrue(xFi2.Exists == true, "Copied file does not exists");

            Assert.IsTrue(writtenText == readText, "Copied file has not same content of the original");

            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Extension");
            //Console.WriteLine($"Extension is {xFi2.Extension}");
            
            Assert.IsTrue(xFi2.Extension == ".txt", "File extension is not correct");

            mDebugger.Send("END TEST");
            mDebugger.Send("");
        }
    }
}
