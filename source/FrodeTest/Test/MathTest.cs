using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    class MathTest
    {
        public static void RunTest()
        {
            Check.Text = "Math.Abs";
            Check.Validate(Math.Abs(-1) == 1);
            //Check.Text = "Math.Acos and Math.PI";
            //Check.Validate(Math.Acos(-1) == Math.PI);
            Check.Text = "Math.Max";
            Check.Validate(Math.Max(3, 7) == 7);
            Math.Abs(256);
        }
    }
}
