using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    public class BoolTest
    {
        public static void RunTest()
        {
            //TESTING TRUE/FALSE TOSTRING
            bool yes = true;
            bool no = false;
            Console.WriteLine("true.ToString() gives: " + yes.ToString());
            Console.WriteLine("false.ToString() gives: " + no.ToString());
        }
    }
}
