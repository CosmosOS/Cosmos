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
                string xDir = string.Empty;
                if (args[0] != null)
                    xDir = args[0];
                else
                    xDir = Directory.GetCurrentDirectory();
                //xDir= EnvironmentVariables.GetCurrent().CurrentDirectory; //HACK

                Console.WriteLine(" Directory of " + xDir);

                foreach (string xDirectory in Directory.GetDirectories(xDir))
                    Console.WriteLine("<DIR>\t" + xDirectory);

                foreach (string xFile in Directory.GetFiles(xDir))
                    Console.WriteLine("\t\t" + xFile);
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
