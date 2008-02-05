using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class Tests {
        // yeah, its crappy, but its a hacked together basic test
        // framework. We do need a real one that should be in 
        // Test Suite project
        public delegate object TestDelegate();

        static public void Do(string aName, TestDelegate aTest) {
            Console.WriteLine("Test: " + aName);
            var xTestResult = aTest();
            Console.WriteLine("  " + xTestResult.ToString());
            Console.WriteLine();
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
