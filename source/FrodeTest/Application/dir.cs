using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys;
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
                //Get directory
                string xDir = string.Empty;
                if (args[0] != null)
                    xDir = args[0];
                else
                    xDir = Directory.GetCurrentDirectory();

                if (!Directory.Exists(xDir))
                {
                    Console.WriteLine("No such directory: " + xDir);
                    return -1;
                }
                else
                {
                    Console.WriteLine(" Directory of " + xDir);

                    foreach (string xDirectory in Directory.GetDirectories(xDir))
                        Console.WriteLine("<DIR>\t" + xDirectory);

                    foreach (string xFile in Directory.GetFiles(xDir))
                        Console.WriteLine("\t\t" + xFile);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR in dir command: " + e.Message);
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
