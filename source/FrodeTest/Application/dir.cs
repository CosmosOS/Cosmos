using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys;
using Cosmos.FileSystem;
using System.IO;
using FrodeTest.Shell;

namespace FrodeTest.Application
{
    class dir : IConsoleApplication
    {
        #region IConsoleApplication Members

        public int Execute(string[] args)
        {
            try
            {
                //Get current directory
                string xCurrentDirectory = EnvironmentVariables.GetCurrent().CurrentDirectory;
                Console.WriteLine(" Directory of " + xCurrentDirectory);

                foreach (string xDirectory in Directory.GetDirectories(xCurrentDirectory))
                    Console.WriteLine("<DIR>   " + xDirectory);

                foreach (string xFile in Directory.GetFiles(xCurrentDirectory))
                    Console.WriteLine("        " + xFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
                return 0;
        }

        public string CommandName
        {
            get { return "dir"; }
        }

        public string Description
        {
            get { return "Show content of current directory"; }
        }

        #endregion
    }
}
