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
            //Console.WriteLine("Inside Execute in Interface implementor...");
            
            Check.OK();
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
            var dummy = new Dummy();
            Check.Text = "Method in interface";
            Check.Validate(dummy.Execute() == "Interfaces WORKS");

            dummy.Property = "Using class";
            //Console.WriteLine("Property get: " + dummy.Property);
            Check.Validate(dummy.Property == "Using class");

            IDummyInterface dmy = new Dummy();
            Check.Text = "Interrface";
            Check.Validate(dmy.Execute() == "Interfaces WORKS");
            dmy.Property = "Using interface";
            Check.Validate(dmy.Property == "Using interface");
        }
    }
}
