using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.MethodTests
{
    public static class ReturnTests
    {
        public static void Execute()
        {
            int a = 0x1FFFFFFF;
            Assert.IsTrue(a == TestInt(a), "Int return value failed.");
            Assert.IsTrue(a == TestRefInt(ref a), "Int& return value failed.");

            uint b = 0x1FFFFFFF;
            Assert.IsTrue(b == TestUInt(b), "UInt return value failed.");
            Assert.IsTrue(b == TestRefUInt(ref b), "UInt& return value failed.");

            long c = 0x1FFFFFFF0FFFFFFF;
            Assert.IsTrue(c == TestLong(c), "Long return value failed.");
            Assert.IsTrue(c == TestRefLong(ref c), "Long& return value failed.");

            ulong d = 0x0FFFFFFF1FFFFFFF;
            Assert.IsTrue(d == TestULong(d), "ULong return value failed.");
            Assert.IsTrue(d == TestRefULong(ref d), "ULong& return value failed.");

            //float e = 42.42f;
            //Assert.IsTrue(e == TestSingle(e), "Single return value failed.");
            //Assert.IsTrue(e == TestRefSingle(ref e), "Single& return value failed.");

            //double f = 42.42;
            //Assert.IsTrue(f == TestDouble(f), "Double return value failed.");
            //Assert.IsTrue(f == TestRefDouble(ref f), "Double& return value failed.");

            string h = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Assert.IsTrue(h == TestString(h), "String return value failed.");
            Assert.IsTrue(h == TestRefString(ref h), "String& return value failed.");

            char i = 'A';
            Assert.IsTrue(i == TestChar(i), "Char return value failed.");
            Assert.IsTrue(i == TestRefChar(ref i), "Char& return value failed.");
        }

        private static int TestRefInt(ref int a)
        {
            int b = a;
            return b;
        }

        private static uint TestRefUInt(ref uint a)
        {
            uint b = a;
            return b;
        }

        private static long TestRefLong(ref long a)
        {
            long b = a;
            return b;
        }

        private static ulong TestRefULong(ref ulong a)
        {
            ulong b = a;
            return b;
        }

        private static float TestRefSingle(ref float a)
        {
            float b = a;
            return b;
        }

        private static double TestRefDouble(ref double a)
        {
            double b = a;
            return b;
        }

        private static string TestRefString(ref string a)
        {
            string b = a;
            return b;
        }

        private static char TestRefChar(ref char a)
        {
            char b = a;
            return b;
        }

        private static int TestInt(int a)
        {
            int b = a;
            return b;
        }

        private static uint TestUInt(uint a)
        {
            uint b = a;
            return b;
        }

        private static long TestLong(long a)
        {
            long b = a;
            return b;
        }

        private static ulong TestULong(ulong a)
        {
            ulong b = a;
            return b;
        }

        private static float TestSingle(float a)
        {
            float b = a;
            return b;
        }

        private static double TestDouble(double a)
        {
            double b = a;
            return b;
        }

        private static string TestString(string a)
        {
            string b = a;
            return b;
        }

        private static char TestChar(char a)
        {
            char b = a;
            return b;
        }
    }
}
