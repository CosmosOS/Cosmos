using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FrodeTest.Test
{
    public class DirectoryTest
    {
        public static void RunTest()
        {
            Console.WriteLine("Running DirectoryTest");

            Check.Text = "Exists";
            Check.Validate(Directory.Exists("/0/"));
            Check.Validate(Directory.Exists("/0/Alfa/"));
            Check.Validate(Directory.Exists("/0/Alfa/Bravo/"));
            Check.Validate(!Directory.Exists("/0/InvalidDir/"));

            Check.Text = "GetDirectories";
            //Check.Validate(Directory.GetDirectories("/0/").Length == 3);
            Check.Validate(Directory.GetDirectories("/0/Alfa/").Length == 1);
            try //Should throw a DirectoryNotFoundException
            {
                Check.Text = "GetDirectories - Exception";
                Directory.GetDirectories("/0/InvalidDir");
                Check.Fail();
            }
            catch (DirectoryNotFoundException)
            {
                Check.OK();
            }
            catch (Exception)
            {
                Check.Fail();
            }

            Check.Text = "GetFiles";
            Check.Validate(Directory.GetFiles("/0/Alfa/").Length == 0);
            Check.Validate(Directory.GetFiles("/0/Alfa/Bravo/").Length == 1);

            Check.Text = "GetLogicalDrives";
            Check.Validate(Directory.GetLogicalDrives().Length == 2);

            Check.Text = "GetParent";
            Check.Validate(Directory.GetParent("/0/Alfa/Bravo").Name == "Alfa");

            Check.Text = "New VolumeSeparator";
            Check.Validate(Directory.Exists(@"0:/Alfa/"));
        }
    }
}
