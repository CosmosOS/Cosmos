using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FrodeTest.Test
{
    public class FileInfoTest
    {
        public static void RunTest()
        {
            Console.WriteLine("-- Testing System.IO.FileInfo --");

            FileInfo file = new FileInfo("/1/Readme.txt");
            Console.WriteLine("Name: " + file.Name);
        }
    }
}
