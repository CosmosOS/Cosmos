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
            dummy.Property = "Using class";
            Console.WriteLine("Property get: " + dummy.Property);

            IDummyInterface dmy = new FrodeTest.Test.Dummy();
            Console.WriteLine(dmy.Execute());
            dmy.Property = "Using interface";
            Console.WriteLine("Property is: " + dmy.Property);
        }
    }
}
