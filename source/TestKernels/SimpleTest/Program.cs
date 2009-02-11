using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.TestKernelHelpers;

namespace Cosmos.SimpleTest {
    public class Program {
        static void Main(string[] args) {
        }
        public static void Init(){
            // prevent interrupts from being enabled for now. 
            bool xTest = false;
            if (xTest) {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute();
            }
            TestReporter.Initialize();
            TestReporter.WriteTestResult("SimpleTest", "Simple test to show that harness is working", true);
            TestReporter.WriteTestResult("SimpleTest1", "Simple test to show that harness is working", true);
            TestReporter.TestsCompleted();
            Console.WriteLine("Done");
            Cosmos.Sys.Deboot.ShutDown();
        }
    }
}