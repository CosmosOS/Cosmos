using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Compiler.Tests.Bcl.Helper
{
    class EqualityHelper
    {
        public static bool DoublesAreEqual(double left, double right)
        {
            // Define the tolerance for variation in their values
            double difference = Math.Abs(left * .00001);

            if (Math.Abs(left - right) <= difference)
                return true;
            else
                return false;
        }
    }
}
