using System;
using System.IO;

using Cosmos.Common.Extensions;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Kernel.Tests.Fat
{
    /// <summary>
    /// The kernel implementation.
    /// </summary>
    /// <seealso cref="Cosmos.System.Kernel" />
    public class Kernel : Sys.Kernel
    {
        private VFSBase mVFS;

        private byte[] xBytes = new byte[16]
        {
            0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7,
            0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF
        };

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

                TestPath();
                TestDirectory();
                TestFile();
                TestFileStream();
                TestStreamWriter();
                TestStreamReader();
                TestBinaryWriter();
                TestBinaryReader();

                TestController.Completed();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred");
                Console.WriteLine(e.Message);
                mDebugger.Send("Exception occurred: " + e.Message);
                TestController.Failed();
            }
        }

        #region System.IO.Path Tests

        /// <summary>
        /// Tests System.IO.Path plugs.
        /// </summary>
        private void TestPath()
        {
            //Path.ChangeExtension(string, string)
            mDebugger.Send("START TEST");
            string xStringResult = Path.ChangeExtension(@"0:\Kudzu.txt", ".doc");
            string xStringExpectedResult = @"0:\Kudzu.doc";
            string xMessage = "Path.ChangeExtenstion (no dot) failed.";
            Assert.IsTrue(xStringResult == xStringExpectedResult, xMessage);
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST");
            xStringResult = Path.ChangeExtension(@"0:\Kudzu.txt", "doc");
            xStringExpectedResult = @"0:\Kudzu.doc";
            xMessage = "Path.ChangeExtenstion (no dot) failed.";
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
            xStringExpectedResult = @"\Temp";
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

        #endregion

        #region System.IO.Directory Tests

        /// <summary>
        /// Tests System.IO.Directory plugs.
        /// </summary>
        private void TestDirectory()
        {
            mDebugger.Send("START TEST: Get parent:");
            var xParent = Directory.GetParent(@"0:\test");
            Assert.IsTrue(xParent != null, "Failed to get directory parent.");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Get parent:");
            xParent = Directory.GetParent(@"0:\test\");
            Assert.IsTrue(xParent != null, "Failed to get directory parent.");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Get files:");
            var xFiles = Directory.GetFiles(@"0:\");
            mDebugger.Send("Found " + xFiles.Length + " files.");
            if (xFiles.Length > 0)
            {
                mDebugger.Send("-- File list");
                for (int i = 0; i < xFiles.Length; i++)
                {
                    mDebugger.Send("File: " + xFiles[i]);
                }
            }
            Assert.IsTrue(xFiles.Length > 0, "Failed to get files from the directory.");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Get directories:");
            var xDirectories = Directory.GetDirectories(@"0:\");
            mDebugger.Send("Found " + xDirectories.Length + " directories.");
            if (xDirectories.Length > 0)
            {
                mDebugger.Send("-- Directory list");
                for (int i = 0; i < xDirectories.Length; i++)
                {
                    mDebugger.Send("Directory: " + xDirectories[i]);
                }
            }
            Assert.IsTrue(xDirectories.Length > 0, "Failed to get directories from the directory.");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Directory exist check:");
            var xTest = Directory.Exists(@"0:\test");
            Assert.IsTrue(xTest, "Folder does not exist!");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST: Create Directory");
            var xDirectory = Directory.CreateDirectory(@"0:\test2");
            Assert.IsTrue(xDirectory != null, "Directory.CreateDirectory failed: Directory is null");
            bool xExists = Directory.Exists(@"0:\test2");
            Assert.IsTrue(xExists, "Directory.CreateDirectory failed: Directory doesn't exist after create call");
            mDebugger.Send("END TEST");

            mDebugger.Send("");

            mDebugger.Send("START TEST: Delete a directory:");
            Directory.CreateDirectory(@"0:\TestDir1");
            Assert.IsTrue(Directory.Exists(@"0:\TestDir1"), "TestDir1 wasn't created!");
            Directory.Delete(@"0:\TestDir1");
            Assert.IsFalse(Directory.Exists(@"0:\TestDir1"), "TestDir1 wasn't deleted!");
            mDebugger.Send("END TEST");

            mDebugger.Send("");
        }

        #endregion

        #region System.IO.File Tests

        /// <summary>
        /// Tests System.IO.File plugs.
        /// </summary>
        private void TestFile()
        {
            string xContents;
            // Moved this test here because if not the test can be executed only a time!
            mDebugger.Send("Write to file now");
            File.WriteAllText(@"0:\Kudzu.txt", "Hello Cosmos");
            mDebugger.Send("Text written");


            mDebugger.Send("File contents of Kudzu.txt: ");
            xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "Hello Cosmos", "Contents of Kudzu.txt was read incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Open))
            {
                xFS.SetLength(5);
            }
            mDebugger.Send("File made smaller");
            xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "Hello", "Contents of Kudzu.txt was read incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Create))
            {
                xFS.SetLength(5);
            }
            mDebugger.Send("File made smaller");
            xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "Hello", "Contents of Kudzu.txt was read incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            mDebugger.Send("Write to file now");
            File.WriteAllText(@"0:\Kudzu.txt", "Test FAT write.");
            mDebugger.Send("Text written");
            xContents = File.ReadAllText(@"0:\Kudzu.txt");
            mDebugger.Send("Contents retrieved after writing");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "Test FAT write.", "Contents of Kudzu.txt was written incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Create file:");
            // Attention! File.Create() returns a FileStream that should be Closed / Disposed on Windows trying to write to the file next gives "File in Use" exception!

            using (var xFile = File.Create(@"0:\test2.txt"))
            {
                Assert.IsTrue(xFile != null, "Failed to create a new file.");
                bool xFileExists = File.Exists(@"0:\test2.txt");
                Assert.IsTrue(xFileExists, "Failed to check existence of the new file.");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
            }

            // Possible issue: writing to another file in the same directory, the data are mixed with the other files
            mDebugger.Send("Write to another file now");
            File.WriteAllText(@"0:\test2.txt", "123");
            mDebugger.Send("Text written");
            xContents = File.ReadAllText(@"0:\test2.txt");
            mDebugger.Send("Contents retrieved after writing");
            mDebugger.Send(xContents);
            Assert.IsTrue(xContents == "123", "Contents of test2.txt was written incorrectly!");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            // Now we write in test3.txt using WriteAllLines()
            mDebugger.Send("START TEST: WriteAllLines:");
            using (var xFile = File.Create(@"0:\test3.txt"))
            {
                Assert.IsTrue(xFile != null, "Failed to create a new file.");
                bool xFileExists = File.Exists(@"0:\test3.txt");
                Assert.IsTrue(xFileExists, "Failed to check existence of the new file.");
                mDebugger.Send("END TEST");
                mDebugger.Send("");
            }


            string[] contents = { "One", "Two", "Three" };
            File.WriteAllLines(@"0:\test3.txt", contents);
            mDebugger.Send("Text written");
            mDebugger.Send("Now reading with ReadAllLines()");
            string[] readLines = File.ReadAllLines(@"0:\test3.txt");
            mDebugger.Send("Contents retrieved after writing");
            for (int i = 0; i < readLines.Length; i++)
            {
                mDebugger.Send(readLines[i]);
            }
            Assert.IsTrue(StringArrayAreEquals(contents, readLines), "Contents of test3.txt was written incorrectly!");
#if false
                // TODO maybe the more correct test is to implement ReadAllLines and then check that two arrays are equals
                        var xContents = File.ReadAllText(@"0:\test3.txt");
                        mDebugger.Send("Contents retrieved after writing");
                        mDebugger.Send(xContents);
                        String expectedResult = String.Concat("One", Environment.NewLine, "Two", Environment.NewLine, "Three");
                        mDebugger.Send("expectedResult: " + expectedResult);
                        Assert.IsTrue(xContents == expectedResult, "Contents of test3.txt was written incorrectly!");
#endif
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            //
            mDebugger.Send("START TEST: Write binary data to file now:");
            using (var xFile = File.Create(@"0:\test.dat"))
            {
                Assert.IsTrue(xFile != null, "Failed to create a new file.");
            }
            byte[] dataWritten = new byte[] { 0x01, 0x02, 0x03 };
            File.WriteAllBytes(@"0:\test.dat", dataWritten);
            mDebugger.Send("Text written");
            byte[] dataRead = File.ReadAllBytes(@"0:\test.dat");

            Assert.IsTrue(ByteArrayAreEquals(dataWritten, dataRead), "Failed to write binary data to a file.");
            mDebugger.Send("END TEST");
            mDebugger.Send("");

            mDebugger.Send("START TEST: Create a new directory with a file inside (filestream):");
            var xDirectory = Directory.CreateDirectory(@"0:\testdir");
            Assert.IsTrue(xDirectory != null, "Failed to create a new directory.");
            using (var xFile2 = File.Create(@"0:\testdir\file.txt"))
            {
                string wText = "This a test";
                byte[] xWriteBuff = wText.GetUtf8Bytes(0, (uint)wText.Length);
                xFile2.Write(xWriteBuff, 0, xWriteBuff.Length);
                mDebugger.Send("---- Data written");
                xFile2.Position = 0;
                byte[] xReadBuff = new byte[xWriteBuff.Length];
                xFile2.Read(xReadBuff, 0, xWriteBuff.Length);
                mDebugger.Send("xWriteBuff=" + xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length));
                mDebugger.Send("xReadBuff =" + xReadBuff.GetUtf8String(0, (uint)xReadBuff.Length));
                string xWriteBuffAsString = xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length);
                string xReadBuffAsString = xReadBuff.GetUtf8String(0, (uint)xReadBuff.Length);
                mDebugger.Send("xWriteBuffAsString=" + xWriteBuffAsString);
                mDebugger.Send("xReadBuffAsString =" + xReadBuffAsString);
                Assert.IsTrue(xWriteBuffAsString == xReadBuffAsString, "Failed to write and read file");
                mDebugger.Send("END TEST");
            }

            //mDebugger.Send("START TEST: Create a new directory with a file inside (File):");
            //var xDirectory2 = Directory.CreateDirectory(@"0:\testdir2");
            //Assert.IsTrue(xDirectory2 != null, "Failed to create a new directory.");
            //string WrittenText = "This a test";
            //File.WriteAllText(@"0:\testdir2\file.txt", WrittenText);
            //mDebugger.Send("Text written");
            //// now read it
            //xContents = File.ReadAllText(@"0:\testdir2\file.txt");
            //mDebugger.Send("Contents retrieved");
            //Assert.IsTrue(xContents == WrittenText, "Failed to read from file");

            //mDebugger.Send("START TEST: Append text to file:");
            //string appendedText = "Yet other text.";
            //File.AppendAllText(@"0:\Kudzu.txt", appendedText);
            //mDebugger.Send("Text appended");
            //xContents = File.ReadAllText(@"0:\Kudzu.txt");
            //mDebugger.Send("Contents retrieved after writing");
            //mDebugger.Send(xContents);
            //// XXX Use String.Concat() with Enviroment.NewLine this not Linux there are is '\n'!
            //Assert.IsTrue(xContents == "Test FAT write.\nYet other text.",
            //    "Contents of Kudzu.txt was appended incorrectly!");
            //mDebugger.Send("END TEST");
            //mDebugger.Send("");

            mDebugger.Send("START TEST: Delete a file:");
            File.Create(@"0:\test1.txt");
            Assert.IsTrue(File.Exists(@"0:\test1.txt"), "test1.txt wasn't created!");
            File.Delete(@"0:\test1.txt");
            Assert.IsFalse(File.Exists(@"0:\test1.txt"), "test1.txt wasn't deleted!");
            mDebugger.Send("END TEST");

            //mDebugger.Send("START TEST: Delete a directory with File.Delete:");
            //Simple test: create a directory, then try to delete it as a file.
            //Directory.CreateDirectory(@"0:\Dir1");

            //File.Delete(@"0:\Dir1");
            //Assert.IsTrue(Directory.Exists(@"0:\Dir1"), "Yeah, it's actually deleting the directory. That isn't right.");

            //mDebugger.Send("END TEST");
        }

        #endregion

        #region System.IO.FileStream Tests

        /// <summary>
        /// Tests System.IO.FileStream plugs.
        /// </summary>
        private void TestFileStream()
        {
            mDebugger.Send("START TEST: Filestream:");

            using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Create))
            {
                mDebugger.Send("Start writing");
                var xStr = "Test FAT Write.";
                byte[] xWriteBuff = xStr.GetUtf8Bytes(0, (uint)xStr.Length);
                xFS.Write(xWriteBuff, 0, xWriteBuff.Length);
                mDebugger.Send("---- Data written");
                xFS.Position = 0;
                byte[] xReadBuff = new byte[xWriteBuff.Length];
                xFS.Read(xReadBuff, 0, xWriteBuff.Length);
                mDebugger.Send("xWriteBuff " + xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length) + " xReadBuff " +
                               xReadBuff.GetUtf8String(0, (uint)xWriteBuff.Length));
                string xWriteBuffAsString = xWriteBuff.GetUtf8String(0, (uint)xWriteBuff.Length);
                string xReadBuffAsString = xReadBuff.GetUtf8String(0, (uint)xReadBuff.Length);
                Assert.IsTrue(xWriteBuffAsString == xReadBuffAsString, "Failed to write and read file");
                mDebugger.Send("END TEST");
            }
        }

        #endregion

        #region System.IO.StreamWriter Tests

        private void TestStreamWriter()
        {
            /*
            mDebugger.Send("START TEST: StreamWriter:");
            mDebugger.Send("Create StreamWriter");

            using (var xSW = new StreamWriter(@"0:\test.txt"))
            {
                if (xSW != null)
                {
                    try
                    {
                        mDebugger.Send("Start writing");

                        xSW.Write("A line of text for testing\nSecond line");
                    }
                    catch
                    {
                        Assert.IsTrue(false, @"Couldn't write to file 0:\test.txt using StreamWriter");
                    }
                }
                else
                {
                    Assert.IsTrue(false, @"Failed to create StreamWriter for file 0:\test.txt");
                }
            }

            mDebugger.Send("END TEST");
            */
        }

        #endregion

        #region System.IO.StreamReader Tests

        private void TestStreamReader()
        {
            /*
            mDebugger.Send("START TEST: StreamReader:");
            mDebugger.Send("Create StreamReader");

            using (var xSR = new StreamReader(@"0:\test.txt"))
            {
                if (xSR != null)
                {
                    mDebugger.Send("Start reading");

                    var content = xSR.ReadToEnd();
                    Assert.IsTrue(content == "A line of text for testing\nSecond line", "Content: " + content);
                }
                else
                {
                    Assert.IsTrue(false, @"Failed to create StreamReader for file 0:\test.txt");
                }
            }

            mDebugger.Send("END TEST");
            */
        }

        #endregion

        #region System.IO.BinaryWriter Tests

        private void TestBinaryWriter()
        {
            //TODO: Implement FileStream with FileMode.Create, currently throws a file not found exception

            /*
            mDebugger.Send("START TEST: BinaryWriter");
            mDebugger.Send("Creating FileStream: FileMode.Create");

            using (var xFS = new FileStream(@"0:\binary.bin", FileMode.Create))
            {
                mDebugger.Send("Creating BinaryWriter");

                using (var xBW = new BinaryWriter(xFS))
                {
                    if (xFS != null)
                    {
                        mDebugger.Send("Start writing");

                        xBW.Write(xBytes);
                        Assert.IsTrue(xFS.Length == xBytes.Length, "The length of the stream and the length of the bytes are different");
                    }
                    else
                    {
                        Assert.IsTrue(false, @"Failed to create StreamWriter for file 0:\binary.bin");
                    }
                }
            }

            mDebugger.Send("END TEST");
            */
        }

        #endregion

        #region System.IO.BinaryReader Tests

        private void TestBinaryReader()
        {
            /*Error Stack Trace:
             *
             *Error: Exception: System.Exception: Error compiling method 'SystemVoidSystemIOMemoryStreamDisposeSystemBoolean': System.NullReferenceException: Object reference not set to an instance of an object.
             *at Cosmos.IL2CPU.X86.IL.Leave.Execute(MethodInfo aMethod, ILOpCode aOpCode) in Cosmos\source\Cosmos.IL2CPU\IL\Leave.cs:line 17
             *at Cosmos.IL2CPU.AppAssembler.EmitInstructions(MethodInfo aMethod, List`1 aCurrentGroup, Boolean & amp; emitINT3) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 667
             *at Cosmos.IL2CPU.AppAssembler.ProcessMethod(MethodInfo aMethod, List`1 aOpCodes) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 533-- - >; System.NullReferenceException: Object reference not set to an instance of an object.
             *at Cosmos.IL2CPU.X86.IL.Leave.Execute(MethodInfo aMethod, ILOpCode aOpCode) in Cosmos\source\Cosmos.IL2CPU\IL\Leave.cs:line 17
             *at Cosmos.IL2CPU.AppAssembler.EmitInstructions(MethodInfo aMethod, List`1 aCurrentGroup, Boolean & amp; emitINT3) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 667
             *at Cosmos.IL2CPU.AppAssembler.ProcessMethod(MethodInfo aMethod, List`1 aOpCodes) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 533
             *--- End of inner exception stack trace ---
             *at Cosmos.IL2CPU.AppAssembler.ProcessMethod(MethodInfo aMethod, List`1 aOpCodes) in Cosmos\source\Cosmos.IL2CPU\AppAssembler.cs:line 540
             *at Cosmos.IL2CPU.ILScanner.Assemble() in Cosmos\source\Cosmos.IL2CPU\ILScanner.cs:line 946
             *at Cosmos.IL2CPU.ILScanner.Execute(MethodBase aStartMethod) in Cosmos\source\Cosmos.IL2CPU\ILScanner.cs:line 247
             *at Cosmos.IL2CPU.CompilerEngine.Execute() in Cosmos\source\Cosmos.IL2CPU\CompilerEngine.cs:line 252
             *
             */

            /*
            mDebugger.Send("START TEST: BinaryReader");
            mDebugger.Send("Creating FileStream: FileMode.Open");

            using (var xFS = new FileStream(@"0:\binary.bin", FileMode.Open))
            {
                mDebugger.Send("Creating BinaryReader");

                using (var xBR = new BinaryReader(xFS))
                {
                    if (xFS != null)
                    {
                        mDebugger.Send("Start reading");

                        byte[] xBuffer = xBR.ReadBytes(xBytes.Length);
                        Assert.IsTrue(ByteArrayAreEquals(xBytes, xBuffer), "Bytes changed during BinaryWriter and BinaryReader opeartions on FileStream");
                    }
                    else
                    {
                        Assert.IsTrue(false, @"Failed to create StreamReader for file 0:\binary.bin");
                    }
                }
            }

            mDebugger.Send("END TEST");
            */
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Utility method to test Byte[] equality.
        /// </summary>
        /// <param name="a1">Byte array.</param>
        /// <param name="a2">Byte array.</param>
        /// <returns>True if the elements in the arrays are equal otherwise false.</returns>
        private bool ByteArrayAreEquals(byte[] a1, byte[] a2)
        {
            if (ReferenceEquals(a1, a2))
            {
                mDebugger.Send("a1 and a2 are the same Object");
                return true;
            }

            if (a1 == null || a2 == null)
            {
                mDebugger.Send("a1 or a2 is null so are different");
                return false;
            }

            if (a1.Length != a2.Length)
            {
                mDebugger.Send("a1.Length != a2.Length so are different");
                return false;
            }

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    mDebugger.Send("In position " + i + " a byte is different");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Utility method to test string[] equality.
        /// </summary>
        /// <param name="a1">String array.</param>
        /// <param name="a2">String array.</param>
        /// <returns>True if the elements in the arrays are equal otherwise false.</returns>
        private bool StringArrayAreEquals(string[] a1, string[] a2)
        {
            if (ReferenceEquals(a1, a2))
            {
                mDebugger.Send("a1 and a2 are the same Object");
                return true;
            }

            if (a1 == null || a2 == null)
            {
                mDebugger.Send("a1 or a2 is null so are different");
                return false;
            }

            if (a1.Length != a2.Length)
            {
                mDebugger.Send("a1.Length != a2.Length so are different");
                return false;
            }

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    mDebugger.Send("In position " + i + " a String is different");
                    return false;
                }
            }

            return true;
        }

        #endregion

    }
}
