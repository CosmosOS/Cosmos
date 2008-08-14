using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest
{
    public static class Check
    {
        private static string mText = "";
        public static string Text { get { return mText; } set { mText = value; } }

        public static void Validate(bool expression)
        {
            if (expression)
                OK();
            else
                Fail();
        }

        public static void OK()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK [" + Text + "]");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Fail()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FAIL [" + Text + "]");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
