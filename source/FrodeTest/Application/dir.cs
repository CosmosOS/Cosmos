using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys;
using Cosmos.FileSystem;
using System.IO;

namespace FrodeTest.Application
{
    class dir : IConsoleApplication
    {
        #region IConsoleApplication Members

        public int Execute(object args)
        {
            //Get current directory
            string xCurrentDirectory = "/1/";

            foreach (string xDirectory in Directory.GetDirectories(xCurrentDirectory))
                Console.WriteLine("<DIR>   " + xDirectory);

            foreach (string xFile in Directory.GetFiles(xCurrentDirectory))
                Console.Write("        " + xFile);

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
