using System;
using System.IO;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;

namespace Cosmos.Kernel.Tests.Fat.System.IO
{
    class PathTest
    {
        /// <summary>
        /// Tests System.IO.Path plugs.
        /// </summary>
        public static void Execute(Debugger mDebugger)
        {
            mDebugger.Send("START TEST");
            string xStringResult = Path.ChangeExtension(@"0:\Kudzu.txt", ".doc");
            string xStringExpectedResult = @"0:\Kudzu.doc";
            string xMessage = "Path.ChangeExtension (no dot) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.ChangeExtension(@"0:\Kudzu.txt", "doc");
            xStringExpectedResult = @"0:\Kudzu.doc";
            xMessage = "Path.ChangeExtension (no dot) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.Combine(string, string)
            mDebugger.Send("START TEST");
            xStringResult = Path.Combine(@"0:\", "test");
            xStringExpectedResult = @"0:\test";
            xMessage = "Path.Combine (root and directory) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.Combine(@"0:\", "test.txt");
            xStringExpectedResult = @"0:\test.txt";
            xMessage = "Path.Combine (root and file) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.Combine(@"0:\test", "test2");
            xStringExpectedResult = @"0:\test\test2";
            xMessage = "Path.Combine (directory and directory) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.Combine(@"0:\test", "test2.txt");
            xStringExpectedResult = @"0:\test\test2.txt";
            xMessage = "Path.Combine (directory and file) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetDirectoryName(string)
            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\");
            xMessage = "Path.GetDirectoryName (root) failed.";
            Assert.IsTrue(xStringResult == null, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test");
            xStringExpectedResult = @"0:\";
            xMessage = "Path.GetDirectoryName (directory no trailing directory separator) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test\");
            xStringExpectedResult = @"0:\";
            xMessage = "Path.GetDirectoryName (directory with trailing directory separator) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test\test2");
            xStringExpectedResult = @"0:\test";
            xMessage = "Path.GetDirectoryName (subdirectory no trailing directory separator) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test\test2\");
            xStringExpectedResult = @"0:\test";
            xMessage = "Path.GetDirectoryName (subdirectory with trailing directory separator) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetDirectoryName(@"0:\test\ .");
            xStringExpectedResult = @"0:\test";
            xMessage = "Path.GetDirectoryName (directory with trailing directory separator and invalid path) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetExtension(string)
            mDebugger.Send("START TEST");
            xStringResult = Path.GetExtension(@"file");
            xStringExpectedResult = string.Empty;
            xMessage = "Path.GetExtension (file no extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetExtension(@"file.txt");
            xStringExpectedResult = ".txt";
            xMessage = "Path.GetExtension (file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetFileName(string aPath)
            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\");
            xStringExpectedResult = string.Empty;
            xMessage = "Path.GetFileName (root directory) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\file.txt");
            xStringExpectedResult = "file.txt";
            xMessage = "Path.GetFileName (file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\test\file");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileName (directory and file no extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\test\file.txt");
            xStringExpectedResult = "file.txt";
            xMessage = "Path.GetFileName (directory and file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileName(@"0:\test\dir\");
            xStringExpectedResult = string.Empty;
            xMessage = "Path.GetFileName (two directories and no file) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetFileNameWithoutExtension(string aPath)
            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\file");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileNameWithoutExtension (file no extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\file.txt");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileNameWithoutExtension (file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\test\file");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileNameWithoutExtension (directory and file no extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\test\file.txt");
            xStringExpectedResult = "file";
            xMessage = "Path.GetFileNameWithoutExtension (directory and file with extension) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetFileNameWithoutExtension(@"0:\test\dir\");
            xStringExpectedResult = string.Empty;
            xMessage = "Path.GetFileNameWithoutExtension (two directories and no file) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetFullPath(string)

            // Path.GetInvalidFileNameChars()
            mDebugger.Send("START TEST");
            int xIntResult = Path.GetInvalidFileNameChars().Length;
            int xIntExpectedResult = 0;
            xMessage = "Path.GetInvalidFileNameChars failed.";
            Assert.IsFalse(xIntResult == xIntExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetInvalidPathChars()
            mDebugger.Send("START TEST");
            xIntResult = Path.GetInvalidPathChars().Length;
            xIntExpectedResult = 0;
            xMessage = "Path.GetInvalidPathChars failed.";
            Assert.IsFalse(xIntResult == xIntExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetPathRoot(string)
            mDebugger.Send("START TEST");
            xStringResult = Path.GetPathRoot(@"0:\");
            xStringExpectedResult = @"0:\";
            xMessage = "Path.GetPathRoot failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetPathRoot(@"0:\test");
            xStringExpectedResult = @"0:\";
            xMessage = "Path.GetPathRoot failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetPathRoot(@"0:\test.txt");
            xStringExpectedResult = @"0:\";
            xMessage = "Path.GetPathRoot failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetPathRoot(@"0:\test\test2");
            xStringExpectedResult = @"0:\";
            xMessage = "Path.GetPathRoot failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.GetPathRoot(@"0:\test\test2.txt");
            xStringExpectedResult = @"0:\";
            xMessage = "Path.GetPathRoot failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetRandomFileName()
            mDebugger.Send("START TEST");
            xStringResult = Path.GetRandomFileName();
            xStringExpectedResult = "random.tmp";
            xMessage = "Path.GetRandomFileName failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetTempFileName()
            mDebugger.Send("START TEST");
            xStringResult = Path.GetTempFileName();
            xStringExpectedResult = "tempfile.tmp";
            xMessage = "Path.GetTempFileName failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.GetTempPath();
            mDebugger.Send("START TEST");
            xStringResult = Path.GetTempPath();
            xStringExpectedResult = @"0:\Temp";
            xMessage = "Path.GetTempPath failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.HasExtension(string)
            mDebugger.Send("START TEST");
            bool xBooleanResult = Path.HasExtension("test.txt");
            bool xBooleanExpectedResult = true;
            xMessage = "Path.HasExtension failed.";
            Assert.IsTrue(xBooleanResult == xBooleanExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xBooleanResult = Path.HasExtension("test");
            xBooleanExpectedResult = false;
            xMessage = "Path.HasExtension failed.";
            Assert.IsTrue(xBooleanResult == xBooleanExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xBooleanResult = Path.HasExtension(@"0:\test.txt");
            xBooleanExpectedResult = true;
            xMessage = "Path.HasExtension failed.";
            Assert.IsTrue(xBooleanResult == xBooleanExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xBooleanResult = Path.HasExtension(@"0:\test");
            xBooleanExpectedResult = false;
            xMessage = "Path.HasExtension failed.";
            Assert.IsTrue(xBooleanResult == xBooleanExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xBooleanResult = Path.HasExtension(@"0:\test\");
            xBooleanExpectedResult = false;
            xMessage = "Path.HasExtension failed.";
            Assert.IsTrue(xBooleanResult == xBooleanExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            // Path.IsPathRooted(string)
        }
    }
}
