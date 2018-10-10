using Cosmos.TestRunner;
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
            TestSHA1();
            TestController.Completed();
        }

        public void TestSHA1()
        {
            // TODO: when we use .net standard 2.0, uncomment this
            //byte[] data = new byte[256];
            //byte[] result;
            //var shaM = new SHA1Managed();
            //result = shaM.ComputeHash(data);
        }
    }
}
