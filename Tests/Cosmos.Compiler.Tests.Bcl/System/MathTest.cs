using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class MathTest
    {
        public static void Execute()
        {
            double acceptedError = 0.000001;
            double result;
            double expectedResult;
            double resultError;

            // Test with whole number
            result = Math.Sqrt(16);
            expectedResult = 4;
            resultError = Math.Abs(result - expectedResult);

            Assert.IsTrue((resultError < acceptedError), "Sqrt does not produce accurate result");

            // Test with rational number
            result = Math.Sqrt(6.5);
            expectedResult = 2.54950975679639241;
            resultError = Math.Abs(result - expectedResult);

            Assert.IsTrue((resultError < acceptedError), "Sqrt does not produce accurate result");

            // Test with zero
            result = Math.Sqrt(0);
            expectedResult = 0;

            Assert.IsTrue((resultError == 0), "Sqrt of zero must be zero");
        }
    }
}
