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

            Check.Text = "/0/Readme.txt";
            FileInfo file = new FileInfo(Check.Text);
            Check.Validate(file.Name == "Readme.txt");

            Check.Text = "Exists";
            Check.Validate(file.Exists);

            //Console.WriteLine("Name: " + file.Name);
            //if (file.Exists)
            //    Console.WriteLine("File exists");
            //else
            //    Console.WriteLine("File does not exist!");


        }
    }
}
