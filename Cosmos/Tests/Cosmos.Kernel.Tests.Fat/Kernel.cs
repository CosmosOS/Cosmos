﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmos.Common.Extensions;
using Cosmos.System.FileSystem.VFS;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Kernel.Tests.Fat
{
    public class TestClass
    {
        private Stream xStream;
        private static global::Cosmos.Debug.Kernel.Debugger mDebugger = new global::Cosmos.Debug.Kernel.Debugger("User", "Test");

        public TestClass(string aTest)
        {
         //   throw new Exception("Test can not be null.");
            //throw new Exception();

            //mDebugger.Send("In TestClass::ctor(string)");
            Console.WriteLine("Before Initialize");
            xStream = Initialize(aTest);
        }
        
        private Stream Initialize(string aTest)
        {
            //mDebugger.Send("In TestClass::Intialize(string)");
            if (aTest == null)
            {
            //    mDebugger.Send("In TestClass::Intialize(string). aTest is null.");
                throw new ArgumentNullException("aTest", "Test can not be null.");
            }

            //if (aTest.Length == 0)
            //{
            //    mDebugger.Send("In TestClass::Intialize(string). aTest is empty.");
            //    throw new ArgumentException("Test can not be empty.");
            //}

            return Stream.Null;
        }
    }

    public class Kernel : Sys.Kernel
    {
        private VFSBase myVFS;

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully, now start testing");
            //myVFS = new Sys.SentinelVFS();
            //VFSManager.RegisterVFS(myVFS);
        }

        private global::Cosmos.Debug.Kernel.Debugger mDebugger = new global::Cosmos.Debug.Kernel.Debugger("User", "Test");

        protected override void Run()
        {
            try
            {
                mDebugger.Send("Run");

                string xString = null;
                var x = new TestClass(xString);
                //var x = new TestClass();
                //string xContents;
                //Assert.IsTrue(Path.GetDirectoryName(@"0:\test") == @"0:\", @"Path.GetDirectoryName(@'0:\test') == @'0:\'");
                //Assert.IsTrue(Path.GetFileName(@"0:\test") == @"test", @"Path.GetFileName(@'0:\test') == @'test'");

                //mDebugger.Send("File exist check:");
                //xTest = File.Exists(@"0:\Kudzu.txt");
                //Assert.IsTrue(xTest, @"\Kudzu.txt not found!");

                //mDebugger.Send("Directory exist check:");
                //xTest = Directory.Exists(@"0:\test");
                //Console.WriteLine("After test");
                //Assert.IsTrue(xTest, "Folder does not exist!");

                //mDebugger.Send("File contents of Kudzu.txt: ");
                //xContents = File.ReadAllText(@"0:\Kudzu.txt");
                //mDebugger.Send("Contents retrieved");
                //mDebugger.Send(xContents);
                //Assert.IsTrue(xContents == "Hello Cosmos", "Contents of Kudzu.txt was read incorrectly!");

                // Test if null or empty string throws exception.
                //try
                //{
                //    using (var xFS = new FileStream(null, FileMode.Open))
                //    {
                //        xFS.SetLength(5);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    mDebugger.Send("Exception occurred:" + ex.Message);
                //}

                //using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Open))
                //{
                //    xFS.SetLength(5);
                //}
                //mDebugger.Send("File made smaller");
                //xContents = File.ReadAllText(@"0:\Kudzu.txt");
                //mDebugger.Send("Contents retrieved");
                //mDebugger.Send(xContents);
                //Assert.IsTrue(xContents == "Hello", "Contents of Kudzu.txt was read incorrectly!");

                //using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Create))
                //{
                //    xFS.SetLength(5);
                //}
                //mDebugger.Send("File made smaller");
                //xContents = File.ReadAllText(@"0:\Kudzu.txt");
                //mDebugger.Send("Contents retrieved");
                //mDebugger.Send(xContents);
                //Assert.IsTrue(xContents == "Hello", "Contents of Kudzu.txt was read incorrectly!");

                ////using (var xFS = new FileStream(@"0:\Kudzu.txt", FileMode.Create))
                ////{
                ////    mDebugger.Send("Start writing");
                ////    var xStr = "Test FAT Write.";
                ////    var xBuff = xStr.GetUtf8Bytes(0, (uint) xStr.Length);
                ////    xFS.Write(xBuff, 0, xBuff.Length);
                ////    mDebugger.Send("---- Data written");
                ////    xFS.Position = 0;
                ////    xFS.Read(xBuff, 0, xBuff.Length);
                ////    mDebugger.Send(xBuff.GetUtf8String(0, (uint)xBuff.Length));
                ////}


                ////mDebugger.Send("Write to file now");
                ////File.WriteAllText(@"0:\Kudzu.txt", "Test FAT write.");
                ////mDebugger.Send("Text written");
                ////xContents = File.ReadAllText(@"0:\Kudzu.txt");

                ////mDebugger.Send("Contents retrieved after writing");
                ////mDebugger.Send(xContents);
                ////Assert.IsTrue(xContents == "Test FAT write.", "Contents of Kudzu.txt was written incorrectly!");

                TestController.Completed();
            }
            catch (Exception E)
            {
                mDebugger.Send("Exception occurred");
                mDebugger.Send(E.Message);
                TestController.Failed();
            }
        }
    }
}
