using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stringdoubler
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Please input a string:");
            string str = Console.ReadLine();
            string orig = str;
            int i = 0;
            while (i < 10)
            {
                str = double_str(str);
                i++;
            }
            Console.WriteLine("Done.\nOriginal: " + orig + "\nDoubled: " + str);
        }

        static string double_str(string val)
        {
            string res = "";
            foreach (char c in val)
            {
                res += repeat(c, 2);
            }
            return res;
        }

        static string repeat(char c, int times)
        {
            string res = "";
            while (res.Length != times)
            {
                res += c;
            }
            return res;
        }

    }
}
