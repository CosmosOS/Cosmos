using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Test
{
    public class SwitchTest
    {
        public static void RunTest()
        {
            Console.WriteLine(GetSwitchError());
        }

        public static string GetSwitchError()
        {
            byte hwverid = 255;
            switch (hwverid)
            {
                case 192:
                    return "RTL8139";
                case 224:
                    return "RTL8139A";
                case 225:
                    return "RTL8139A-G";
                case 232:
                    return "RTL8139C";
                case 235:
                    return "RTL8139B";
                default:
                    return "Unknown RTL813xxx revision";
            }
        }
    }
}
