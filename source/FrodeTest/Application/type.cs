using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FrodeTest.Application
{
    public class type : IConsoleApplication
    {
        #region IConsoleApplication Members

        public int Execute(string[] args)
        {
            //Locate the file
            if (File.Exists(args[0]))
            {
                Console.WriteLine("Contents of file: " + args[0]);
                Console.WriteLine(File.ReadAllText(args[0]));
            }
            else
            {
                Console.WriteLine("File not found: " + args[0]);
                return -1;
            }

            return 0;
        }

        public string CommandName
        {
            get { return "type"; }
        }

        public string Description
        {
            get { return "Displays contents of a file"; }
        }

        #endregion
    }
}
