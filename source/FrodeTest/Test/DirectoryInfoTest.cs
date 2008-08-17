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
            Console.WriteLine("Running DirectoryInfoTest");

            //Console.WriteLine("CurrentDirectory: " + Environment.CurrentDirectory);

            Check.Text = "ctor";
            DirectoryInfo dir = new DirectoryInfo("/0/lost+found/");
            Check.Validate(dir != null);
            Check.Validate(dir.Name == "lost+found");
            Check.Text = "Exists for /0/lost+found/";
            Check.Validate(dir.Exists);

            DirectoryInfo dir2 = new DirectoryInfo("/0/Alfa/Bravo/");
            Check.Text = "Exists for /0/Alfa/Bravo/";
            Check.Validate(dir2.Exists);
            Check.Text = "GetFiles for /0/Alfa/Bravo/";
            //Check.Validate(dir2.GetFiles().Length == 1);
            //Console.WriteLine("Files in the directory:");
            //foreach (FileInfo file in dir2.GetFiles())
            //    Console.WriteLine(file.Name);

            dir = new DirectoryInfo("/0/");
            Check.Text = "Exists for /0/";
            Check.Validate(dir.Exists);
            Check.Text = "GetFiles for /0/";
            Check.Validate(dir.GetFiles().Length == 3);
            Console.WriteLine("Files in the directory:");
            foreach (FileInfo file in dir.GetFiles())
                Console.WriteLine(file.Name);


        }
    }
}
