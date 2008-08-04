using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    public class StringTest
    {
        public static void RunTest()
        {
            Console.WriteLine("-- Testing String --");

            //Console.Write("LeftPadding: ");
            //string hex = "F";
            //hex = hex.PadLeft(2, '0');
            //Console.WriteLine(hex);

            //Add char and string
            //Bug discovered 7.june. SysFault when adding char and string.
            //string added = string.Empty;
            //added = ((char)('c')) + "oncatenating char and string works.";
            //Console.WriteLine(added);

            //StringBuilder sb = new StringBuilder();
            //sb.Append("String");
            //sb.Append("Builder");
            //sb.Append(Environment.NewLine);
            //sb.Append("Works");
            //Console.WriteLine(sb.ToString());

            //.Contains
            //Bug discovered 30.july
            //string xStringWithWorld = "HelloWorld!";
            //if (xStringWithWorld.Contains("World"))
            //    Console.WriteLine("Contains works!");
            //else
            //    Console.WriteLine("Contains doesn't work");

            //Splitting
            //string sentence = "This is a long string with many words";
            //string[] words = sentence.Split((char)' ');
            //Console.WriteLine(words[3]);
            //Console.WriteLine(words[5]);

            ////.StartsWith
            //string xBeginWithHello = "Hello world";
            //if (xBeginWithHello.StartsWith("Hello"))
            //    Console.WriteLine(".StartsWith works");
            //else
            //    Console.WriteLine(".StartsWith FAILS!");

            //.EndsWith
            //string xEndsWithWorld = "Hello World";
            //if (xEndsWithWorld.EndsWith("World"))
            //    Console.WriteLine(".EndsWith works");
            //else
            //    Console.WriteLine(".EndsWith FAILS!");

            //Bug, found 4.aug
            //Printing 0:\ will print previous string in buffer instead!
            Console.WriteLine("This should be printed ONCE");
            Console.WriteLine("\\");
            Console.WriteLine(@"0:\");
            Console.WriteLine(@"\");
            Console.WriteLine("Long string \\ Long String");
            Console.WriteLine("Back to normal");

            //.IndexOf
            Console.WriteLine("Should be  0: " + "otto".IndexOf('o'));
            Console.WriteLine("Should be  3: " + "otto".IndexOf('o', 1));
            Console.WriteLine("Should be -1: " + "otto".IndexOf('o', 1, 1));
            //Console.WriteLine("otto".IndexOf("tt"));

            //.Replace
            Console.WriteLine("Hello".Replace('l', 'E'));

            // Add \t as Tab
            //Console.WriteLine("Column1\tColumn2");

        }
    }
}
