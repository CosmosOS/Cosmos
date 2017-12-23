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
            if (true)
            {
                byte[] xResult = { 1, 2, 3, 4, 5, 6, 7, 8 };
                byte[] xExpectedResult = { 1, 2, 3, 4, 5, 6, 7, 1 };
                byte[] xSource = { 1 };

                Array.Copy(xSource, 0, xResult, 7, 1);

                Assert.IsTrue((xResult[7] == xExpectedResult[7]), "Array.Copy doesn't work: xResult[7] =  " + (uint)xResult[7] + " != " + (uint)xExpectedResult[7]);
            }
            
            // Single[] Test
            float[] xResult = { 1.25, 2.50, 3.51, 4.31, 9.28, 18.56 };
            float[] xExpectedResult = { 1.25, 2.598, 5.39, 4.31, 9.28, 18.56 };
            float[] xSource = { 0.49382, 1.59034, 2.598, 5.39, 7.48392, 4.2839 };
            
            Array.Copy(xSource, 2, xResult, 1, 2);
            
            Assert.IsTrue((xResult[1] + xResult[2]) == (xExpectedResult[1] + xExpectedResult[2]), "Array.Copy doesn't work with Singles: xResult[1] =  " + (uint)xResult[1] + " != " + (uint)xExpectedResult[1] " and xResult[2] =  " + (uint)xResult[2] + " != " + (uint)xExpectedResult[2]);
            
            // Double[] Test
            double[] xResult = { 0.384, 1.5823, 2.5894, 2.9328539, 3.9201, 4.295 };
            double[] xExpectedResult = { 0.384, 1.5823, 2.5894, 95.32815, 3.9201, 4.295 };
            double[] xSource = { 95.32815 };
            
            Array.Copy(xSource, 0, xResult, 3, 1);
            
            Assert.IsTrue(xResult[3] == xExpectedResult[3], "Array.Copy doesn't work with Doubles: xResult[1] =  " + (uint)xResult[3] + " != " + (uint)xExpectedResult[3]);
        }
    }
}
