using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    interface IDummyInterface
    {
        string Execute();
    }

    class Dummy : IDummyInterface
    {
        public string Execute()
        {
            Console.WriteLine("Inside Execute in Interface implementor...");
            return "Interfaces WORKS";
        }
    }

    public class InterfaceTest
    {
        public static void RunTest()
        {
            Test.Dummy dummy = new FrodeTest.Test.Dummy();
            dummy.Execute();
        }
    }
}
