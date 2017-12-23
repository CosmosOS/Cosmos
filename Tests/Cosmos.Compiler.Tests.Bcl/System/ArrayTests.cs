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
            byte[] xByteResult = { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] xByteExpectedResult = { 1, 2, 3, 4, 5, 6, 7, 1 };
            byte[] xByteSource = { 1 };

            Array.Copy(xByteSource, 0, xByteResult, 7, 1);

            Assert.IsTrue((xByteResult[7] == xByteExpectedResult[7]), "Array.Copy doesn't work: xResult[7] =  " + (uint)xByteResult[7] + " != " + (uint)xByteExpectedResult[7]);
            
            // Single[] Test
            float[] xSingleResult = { 1.25, 2.50, 3.51, 4.31, 9.28, 18.56 };
            float[] xSingleExpectedResult = { 1.25, 2.598, 5.39, 4.31, 9.28, 18.56 };
            float[] xSingleSource = { 0.49382, 1.59034, 2.598, 5.39, 7.48392, 4.2839 };
            
            xSingleResult[1] = xSingleSource[2];
            xSingleResult[2] = xSingleSource[3];
            
            Assert.IsTrue(((xSingleResult[1] + xSingleResult[2]) == (xSingleExpectedResult[1] + xSingleExpectedResult[2])), "Assinging values to single array elements doesn't work: xResult[1] =  " + (uint)xSingleResult[1] + " != " + (uint)xSingleExpectedResult[1] " and xResult[2] =  " + (uint)xResult[2] + " != " + (uint)xExpectedResult[2]);
            
            // Double[] Test
            double[] xDoubleResult = { 0.384, 1.5823, 2.5894, 2.9328539, 3.9201, 4.295 };
            double[] xDoubleExpectedResult = { 0.384, 1.5823, 2.5894, 95.32815, 3.9201, 4.295 };
            double[] xDoubleSource = { 95.32815 };
            
            xDoubleResult[3] = xDoubleSource[0];
            
            Assert.IsTrue(xDoubleResult[3] == xDoubleExpectedResult[3], "Assinging values to double array elements doesn't work: xResult[1] =  " + (uint)xDoubleResult[3] + " != " + (uint)xDoubleExpectedResult[3]);
        }
    }
}
