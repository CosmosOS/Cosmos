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

            DirectoryInfo dir = new DirectoryInfo("/0/Frode/");
            //Console.WriteLine("FullName: " + dir.FullName);
            //Console.WriteLine("Name: " + dir.Name);
            
            //Console.WriteLine("ToString: " + dir.ToString());

            //Console.WriteLine("Files in the directory:");
            //foreach (FileInfo file in dir.GetFiles())
            //    Console.WriteLine(file.Name);


            if (dir.Exists)
                Console.WriteLine("Directory exists");
            else
                Console.WriteLine("Directory doesn't exist");
        }
    }
}
