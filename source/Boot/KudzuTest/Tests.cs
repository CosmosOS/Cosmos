using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class Tests {
        // yeah, its crappy, but its a hacked together basic test
        // framework. We do need a real one that should be in 
        // Test Suite project
        public delegate object TestDelegate();

        static public void DoAll() {
            //Tests.Do("String Concatenation", Tests.StringConcat);
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

			//Console.WriteLine("Dictionary test");
			//Console.WriteLine("  " + Tests.Dictionary());
			//Console.WriteLine();

			Console.WriteLine("Concat conversion test");
			//Console.WriteLine("  " + Tests.ConcatConversion());
            Console.WriteLine("  " + Tests.ConcatConversion2());
            Console.WriteLine("  " + Tests.ConcatConversion3());
            Console.WriteLine();
        }

        //Fail - Returns in hex, and also wrong value
        static public object ConcatConversion() {
            int x = 1000;
            return x + " Euros, but should be 1000. Not even correct by value";
        }

        //Fail - Crashes
        static public object ConcatConversion2() {
            UInt32 y = 100;
            UInt32 z = 100;
            return y + "-" + z;
        }

        //Fail - crashes...
        static public object ConcatConversion3() {
            UInt32 y = 100;
            UInt32 z = 100;
            return y.ToString() + "-" + z.ToString();
        }

        //Fail - IL2CPU error on compile
        static public object Dictionary() {
            var x = new Dictionary<UInt32, string>();
            x.Add(1000, "Hello");
            return null;
        }

        //Fail - Cant even use this method
        static public void Do(string aName, TestDelegate aTest) {
            Console.WriteLine("Test: " + aName);
            object xTestResult = aTest();
            if (xTestResult != null) {
                Console.WriteLine("  " + xTestResult.ToString());
            }
            Console.WriteLine();
        }

        //Fail - IL2CPU fails to compile. Missing P/Invoke IIRC
        static public object WriteLnUInt32() {
            UInt32 x = 1000;
            Console.WriteLine(x);
            return null;
        }

        //Fail, outputs partial garbage
        static public string IntToStr16() {
            UInt16 x16 = 1000;
            return "1000=" + x16.ToString();
        }

        //Fail, outputs hex instead of decimal
        static public string IntToStr32() {
            UInt32 x32 = 1000;
            return "1000=" + x32.ToString();
        }

        //Pass
        static public string StringConcat() {
            string x = "Hello";
            x = x + " world.";
            return x;
        }

        //Fail - Important
        static public string StringBuilder() {
			var xSB = new System.Text.StringBuilder("Hello");
			xSB.Append(" world.");
			return xSB.ToString();
        }
    }
}
