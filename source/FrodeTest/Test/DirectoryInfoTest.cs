using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FrodeTest.Test
{
    public class DirectoryInfoTest
    {
        public static void RunTest()
        {
            Console.WriteLine("-- TESTING System.IO.DirectoryInfo --");

            DirectoryInfo dir = new DirectoryInfo("/1/lost+found");
            Console.WriteLine("FullName: " + dir.FullName);
            Console.WriteLine("Name: " + dir.Name);
            //Console.WriteLine("ToString: " + dir.ToString());

            if (dir.Exists)
                Console.WriteLine("Exists");
            else
                Console.WriteLine("Doesn't exist");
        }
    }
}
