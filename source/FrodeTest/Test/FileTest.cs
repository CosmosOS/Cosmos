using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FrodeTest.Test
{
    public class FileTest
    {
        public static void RunTest()
        {
            Console.WriteLine("Running FileTest");
            Check.Text = "Exists";
            Check.Validate(File.Exists("/0/Readme.txt"));
            Check.Validate(File.Exists("/0/Alfa/Bravo/slide_puzzle.lua"));
            Check.Validate(!File.Exists("/0/invalidfile.txt"));
            Check.Validate(!File.Exists("/0/InvalidDir/dummy.txt"));
        }
    }
}
