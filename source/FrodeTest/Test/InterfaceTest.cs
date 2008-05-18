using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    interface IDummyInterface
    {
        string Execute();
        string Property { get; set; }
    }

    class Dummy : IDummyInterface
    {
        public string Execute()
        {
            Console.WriteLine("Inside Execute in Interface implementor...");
            return "Interfaces WORKS";
        }

        private string xProperty;
        public string Property
        {
            get
            {
                return xProperty;
            }
            set
            {
                xProperty = value;
            }
        }
    }

    public class InterfaceTest
    {
        public static void RunTest()
        {
            Test.Dummy dummy = new FrodeTest.Test.Dummy();
            Console.WriteLine(dummy.Execute());
            dummy.Property = "Hello Interfaces!";
            Console.WriteLine("Property get: " + dummy.Property);
        }
    }
}
