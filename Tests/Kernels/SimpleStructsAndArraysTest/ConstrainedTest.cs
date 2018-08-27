using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace SimpleStructsAndArraysTest
{
    public interface ITestMutate
    {
        void Mutate();
    }

    public struct MyStruct1 : ITestMutate
    {
        public int a;

        public void Mutate()
        {
            a++;
        }
    }

    public struct MyStruct2
    {
        public int b;

        public override string ToString()
        {
            b++;
            return b.ToString();
        }
    }

    internal struct MyStruct3
    {
        public int c;

        public override string ToString()
        {
            c = 1;
            return c.ToString();
        }
    }

    public static class ConstrainedTest
    {
        public static void DoMutate<T>(ref T a) where T : ITestMutate
        {
            a.Mutate();
        }

        public static void MutateStructTest()
        {
            var xS1 = new MyStruct1();
            Assert.IsTrue(xS1.a == 0, "xS1.a == 0");
            DoMutate(ref xS1);
            Assert.IsTrue(xS1.a == 1, "xS1.a == 1");

            var xS2 = new MyStruct2();
            Assert.IsTrue(xS2.b == 0, "xS2.b == 0");
            Assert.IsTrue(xS2.ToString() == "1", "xS2.ToString() == '1'");
            Assert.IsTrue(xS2.b == 1, "xS2.b == 1");

            var xS3 = new MyStruct3();
            Assert.IsTrue(xS3.c == 0, "xS3.b == 0");
            Assert.IsTrue(xS3.ToString() == "1", "xS3.ToString() == '1'");
            Assert.IsTrue(xS3.c == 1, "xS3.b == 1");
        }
    }
}
