using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleMethodsTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var xValue1 = 33;
            var xValue2 = 45;
            var xSum = Sum(xValue1, xValue2);
            var xSum2 = Sum(xSum, xValue2);
        }

        private static int Sum(int xValue1, int xValue2)
        {
            return xValue1 + xValue2;
        }
    }
}