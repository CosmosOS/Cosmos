using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Playground.Kudzu {
    public class Tests {
        // yeah, its crappy, but its a hacked together basic test
        // framework. We do need a real one that should be in 
        // Test Suite project
        public delegate object TestDelegate();

        static public void DoAll() {
            // Delegate version
            //Tests.Do("String Concatenation", Tests.StringConcat);
            //return;

            Console.WriteLine("String test");
			Console.WriteLine("  " + Tests.StringConcat());
            Console.WriteLine();

            Console.WriteLine("StringBuilder test");
			Console.WriteLine(Tests.StringBuilder());
			Console.WriteLine();

			Console.WriteLine("IntToStr 16 test");
			Console.WriteLine("  " + Tests.IntToStr16());
			Console.WriteLine();

			Console.WriteLine("IntToStr 32 test");
			Console.WriteLine("  " + Tests.IntToStr32());
			Console.WriteLine();

            Console.WriteLine("WriteLnUInt32 test");
			Console.WriteLine("  " + Tests.WriteLnUInt32());
			Console.WriteLine();

			Console.WriteLine("Dictionary test");
            //M - uncomment this and IL2CPU fails on compile
			//Console.WriteLine("  " + Tests.Dictionary());
			Console.WriteLine();

            Console.WriteLine("Concat conversion test");
			Console.WriteLine("  " + Tests.ConcatConversion());
            Console.WriteLine("  " + Tests.ConcatConversion2());
            Console.WriteLine("  " + Tests.ConcatConversion3());
            Console.WriteLine();
        }

        static public object ConcatConversion() {
            int x = 1000;
            return x + " Euros, but should be 1000. Not even correct by value";
        }

        static public object ConcatConversion2() {
            UInt32 y = 100;
            UInt32 z = 100;
            return y + "-" + z;
        }

        static public object ConcatConversion3() {
            UInt32 y = 100;
            UInt32 z = 100;
            return y.ToString() + "-" + z.ToString();
        }

        static public object Dictionary() {
            var x = new Dictionary<UInt32, string>();
            x.Add(1000, "Hello");
            return x[1000];
        }

        static public void Do(string aName, TestDelegate aTest) {
            Console.WriteLine("Test: " + aName);
            object xTestResult = aTest();
            if (xTestResult != null) {
                Console.WriteLine("  " + xTestResult.ToString());
            }
            Console.WriteLine();
        }

        static public object WriteLnUInt32() {
            UInt32 x = 1000;
            Console.WriteLine(x);
            return null;
        }

        static public string IntToStr16() {
            UInt16 x16 = 1000;
            return "1000=" + x16.ToString();
        }

        static public string IntToStr32() {
            UInt32 x32 = 1000;
            return "1000=" + x32.ToString();
        }

        static public string StringConcat() {
            string x = "Hello";
            x = x + " world.";
            return x;
        }

        static public string StringBuilder() {
			var xSB = new System.Text.StringBuilder("Hello");
			xSB.Append(" world.");
			return xSB.ToString();
        }
    }
}
