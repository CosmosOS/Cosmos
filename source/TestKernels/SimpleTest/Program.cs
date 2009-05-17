using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.TestKernelHelpers;
using Cosmos.Compiler.Builder;

namespace Cosmos.SimpleTest
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {


            BuildUI.Run();
        }
        public static void Init()
        {
            // prevent interrupts from being enabled for now. 
            bool xTest = true;
            if (xTest)
            {
                var xBoot = new Cosmos.Sys.Boot();
                xBoot.Execute();
            }
            TestReporter.Initialize();
            TestReporter.WriteTestResult("SimpleTest", "Simple test to show that harness is working", true);
            TestReporter.WriteTestResult("SimpleTest1", "Simple test to show that harness is working", true);
            TestReporter.TestsCompleted();
            Console.WriteLine("Done");
            //Cosmos.Sys.Deboot.ShutDown();
        }
    }
}