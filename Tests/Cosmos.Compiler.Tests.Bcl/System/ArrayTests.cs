using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class ArrayTests
    {
        public static void Execute()
        {
            byte[] xResult = { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] xExpectedResult = { 1, 2, 3, 4, 5, 6, 7, 1 };
            byte[] xSource = { 1 };

            Array.Copy(xSource, 0, xResult, 7, 1);

            Assert.IsTrue((xResult[7] == xExpectedResult[7]), "Array.Copy doesn't work: xResult[7] =  " + (uint)xResult[7] + " != " + (uint)xExpectedResult[7]);
        }
    }
}
