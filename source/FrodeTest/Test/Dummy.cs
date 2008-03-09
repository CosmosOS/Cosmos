using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    class Dummy : IDummyInterface
    {
        public string Execute()
        {
            Console.WriteLine("Inside Execute in Interface implementor...");
            return "Interfaces WORKS";
        }
    }
}
