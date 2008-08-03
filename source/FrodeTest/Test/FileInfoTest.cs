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

            FileInfo file = new FileInfo("/0/Readme.txt");
            Console.WriteLine("Name: " + file.Name);
            if (file.Exists)
                Console.WriteLine("File exists");
            else
                Console.WriteLine("File does not exist!");


        }
    }
}
