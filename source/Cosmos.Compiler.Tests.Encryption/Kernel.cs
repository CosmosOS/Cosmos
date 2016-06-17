﻿using Cosmos.TestRunner;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.Encryption
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Starting tests.");
        }

        protected override void Run()
        {
            Console.WriteLine("SHA1:");
            TestSHA1();
            TestController.Completed();
        }

        public void TestSHA1()
        {
            byte[] data = new byte[256];
            byte[] result;
            var shaM = new SHA1Managed();
            result = shaM.ComputeHash(data);
        }
    }
}
