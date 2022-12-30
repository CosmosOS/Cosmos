using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Ramdisk;
using Cosmos.System.FileSystem.VFS;
using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sys = Cosmos.System;

namespace RamdiskTest
{
    public class Kernel : Sys.Kernel
    {
        private static RamdiskFileSystem mRamdisk;
        private static RamdiskFileSystem mRamdisk1;
        private static string File1 = "this is a test!";
        private static string File2 = "this a is test?";
        private static string File3 = "is this a test.";
        private static string File4 = "!this test a is";
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Lets begin the tests.");
        }
        protected override void Run()
        {
            try
            {
                TestCreation();
                TestFileCreation();
                TestFileWriting();
                TestFileWriting();
                TestFileDeleting();

                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                TestController.Failed();
            }
        }
        public static void TestCreation()
        {
            mRamdisk = RamdiskFileSystem.CreateRamdisk();
            mRamdisk1 = RamdiskFileSystem.CreateRamdisk();
            VFSManager.RegisterVFS(new CosmosVFS());
            Assert.Succeed("No errors!");
        }
        public static void TestFileCreation()
        {
            VFSManager.CreateFile(mRamdisk.RootPath + "test0.txt");
            VFSManager.CreateDirectory(mRamdisk.RootPath + "test");

            VFSManager.CreateFile(mRamdisk1.RootPath + "test34234326478325.txt");
            VFSManager.CreateDirectory(mRamdisk1.RootPath + "stes");

            for (int i = 0; i < 16; i++)
            {
                VFSManager.CreateFile(mRamdisk.RootPath + $"test/test{i}.txt");
                VFSManager.CreateFile(mRamdisk1.RootPath + $"stes/file{32 - i}.txt");
            }
            Assert.IsTrue(mRamdisk.mFileCount == (16 + 1), "Files are created successfully!");
            Assert.IsTrue(mRamdisk.mDirCount == 1, "Folders are created successfully!");
            Assert.IsTrue(mRamdisk1.mFileCount == (16 + 1), "Files are created successfully!");
            Assert.IsTrue(mRamdisk1.mDirCount == 1, "Folders are created successfully!");
        }
        public static void TestFileWriting()
        {
            File1 = Shuffle(File1);
            File2 = Shuffle(File2);
            File3 = Shuffle(File3);
            File4 = Shuffle(File4);

            var stream0 = VFSManager.GetFileStream(mRamdisk.RootPath + "test0.txt");
            var stream1 = VFSManager.GetFileStream(mRamdisk1.RootPath + "test34234326478325.txt");
            var stream2 = VFSManager.GetFileStream(mRamdisk1.RootPath + "stes/file32.txt");
            var stream3 = VFSManager.GetFileStream(mRamdisk.RootPath + "test/test0.txt");

            new StreamWriter(stream0).Write(File1);
            new StreamWriter(stream1).Write(File2);
            new StreamWriter(stream2).Write(File3);
            new StreamWriter(stream3).Write(File4);

            Assert.IsTrue(new StreamReader(stream0).ReadToEnd() == File1, "Readwrite is successful!");
            Assert.IsTrue(new StreamReader(stream1).ReadToEnd() == File2, "Readwrite is successful!");
            Assert.IsTrue(new StreamReader(stream2).ReadToEnd() == File3, "Readwrite is successful!");
            Assert.IsTrue(new StreamReader(stream3).ReadToEnd() == File4, "Readwrite is successful!");
        }
        public static void TestFileDeleting()
        {
            var path0 = mRamdisk.RootPath + "test0.txt";
            var path1 = mRamdisk.RootPath + "test/test0.txt";
            var path2 = mRamdisk1.RootPath + "test34234326478325.txt";
            var path3 = mRamdisk1.RootPath + "stes/file32.txt";

            VFSManager.DeleteFile(path0);
            VFSManager.DeleteFile(path2);
            VFSManager.DeleteDirectory(path1, true);
            VFSManager.DeleteDirectory(path3, true);

            Assert.IsTrue(!VFSManager.FileExists(path0), "Deletion is successful!");
            Assert.IsTrue(!VFSManager.FileExists(path1), "Deletion is successful!");
            Assert.IsTrue(!VFSManager.FileExists(path2), "Deletion is successful!");
            Assert.IsTrue(!VFSManager.FileExists(path3), "Deletion is successful!");
        }
        public static string Shuffle(string data)
        {
            Random rng = new Random();
            string str = "";
            for (int i = 0; i < data.Length; i++)
            {
                str += data[rng.Next(data.Length)];
            }
            return str;
        }
    }
}
