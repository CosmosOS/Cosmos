using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FrodeTest.Test
{
    public class PathTest
    {
        public static void RunTest()
        {
            Check.SetHeadingText("Testing System.IO.Path");
            //Console.WriteLine("-- Testing System.IO.Path --");

            Check.Text = "Path.ChangeExtension";
            Check.Validate(Path.ChangeExtension(@"\0\Alpha\Readme.txt", ".doc").Equals(@"\0\Alpha\Readme.doc"));

            Check.Text = "Path.Combine";
            Check.Validate(Path.Combine(@"\0\Alpha", "Bravo").Equals(@"\0\Alpha\Bravo"));

            Check.Text = "Path.GetDirectoryName";
            Console.WriteLine(Path.GetDirectoryName(@"\0\Alpha\Dummy"));
            Check.Validate(Path.GetDirectoryName(@"\0\Alpha\Dummy").Equals(@"\0\Alpha"));

            Check.Text = "Path.GetExtension";
            Check.Validate(Path.GetExtension(@"\0\Alpha\Readme.txt").Equals(".txt"));

            Check.Text = "Path.GetFileName";
            Check.Validate(Path.GetFileName(@"\0\Alpha\Readme.txt").Equals("Readme.txt"));

            Check.Text = "Path.GetFileNameWithoutExtension";
            Check.Validate(Path.GetFileNameWithoutExtension(@"\0\Alpha\Readme.txt").Equals("Readme"));

            Check.Text = "Path.GetFullPath";
            Console.WriteLine(Path.GetFullPath("Alpha"));
            Check.Validate(Path.GetFullPath("Alpha").Equals(Directory.GetCurrentDirectory() + @"\Alpha"));

            Check.Text = "Path.GetInvalidFileNameChars";
            Check.Validate(Path.GetInvalidFileNameChars().Length == 41);

            Check.Text = "Path.GetInvalidPathChars";
            Check.Validate(Path.GetInvalidPathChars().Length == 36);

            Check.Text = "Path.GetPathRoot";
            Check.Validate(Path.GetPathRoot(@"\0\Alpha").Equals(@"\"));

            Check.Text = "Path.GetRandomFileName";
            Check.Validate(Path.GetRandomFileName().Length != 0);

            Check.Text = "Path.GetTempFileName";
            Check.Validate(Path.GetTempFileName().Length != 0);

            Check.Text = "Path.GetTempPath";
            Check.Validate(Path.GetTempPath().Equals(@"\0\Temp"));

            Check.Text = "Path.HasExtension";
            Check.Validate(Path.HasExtension(@"\0\Alpha\Readme.txt"));
            Check.Validate(!Path.HasExtension(@"\0\Alpha"));

            Check.Text = "Path.IsPathRooted";
            Check.Validate(Path.IsPathRooted(@"\0\Alpha"));

        }
    }
}
