using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.TestRunner;
using Sys = Cosmos.System;

namespace Cosmos.Compiler.Tests.MethodTests
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }
        public static void BranchStackCorruption2()
        {
            char c = 'l';
            char c2 = (c <= 'Z' && c >= 'A') ? ((char)(c - 65 + 97)) : c;
            c = 'L';
            char c3 = (c <= 'Z' && c >= 'A') ? ((char)(c - 65 + 97)) : c;
        }

        public static void BranchStackCorruption()
        {
            bool flag1 = true;
            bool flag2 = false;
            bool flag3 = true;

            if (flag1 && (flag2 || flag3))
            {
                return;
            }
            else
            {
                throw new Exception("This should not occur");
            }
        }

        protected override void Run()
        {
            BranchStackCorruption();
            BranchStackCorruption2();
            DelegatesTest.Execute();
            try
            {
                ReturnTests.Execute();
                TestController.Completed();
            }
            catch (Exception E)
            {
                Console.WriteLine("Exception");
                Console.WriteLine(E.ToString());
            }
        }
    }
}
