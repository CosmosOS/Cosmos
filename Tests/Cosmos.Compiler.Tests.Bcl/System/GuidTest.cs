﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class GuidTest
    {

        public static void Execute()
        {

            //dont know how else to test this

            Guid testGuid = Guid.NewGuid();
            Guid testGuid1 = Guid.NewGuid();

            Assert.IsFalse((testGuid == Guid.Empty),"Guid was Empty");

            Assert.IsFalse((testGuid == testGuid1), "Guid was not unique");

            byte[] GuidRFC4122 = testGuid.ToByteArray();

            Assert.IsTrue((GuidRFC4122.Length == 16), "invalid Length");

            Assert.IsTrue((GuidRFC4122[6].ToString() == "4"), "7th byte invalid (not 4)");

            bool test9thbyte = false;

            if (GuidRFC4122[8].ToString() == "8")
            {
                test9thbyte = true;
            }
            else if (GuidRFC4122[8].ToString() == "9")
            {
                test9thbyte = true;
            }
            else if (GuidRFC4122[8].ToString() == "A")
            {
                test9thbyte = true;
            }
            else if (GuidRFC4122[8].ToString() == "B")
            {
                test9thbyte = true;
            }

            Assert.IsTrue(test9thbyte, "9th byte invalid (not 8,9,A,B)");

        }
    }
}
