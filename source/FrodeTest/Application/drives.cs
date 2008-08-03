using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Application
{
    class drives : IConsoleApplication
    {
        #region IConsoleApplication Members

        public int Execute(string[] args)
        {
            Console.WriteLine("The following logical drives exist:");

            foreach (string xDrive in System.IO.Directory.GetLogicalDrives())
            {
                Console.WriteLine(xDrive);
            }

            return 0;
        }

        public string CommandName
        {
            get { return "drives"; }
        }

        public string Description
        {
            get { return "Displays a list of harddrives."; }
        }

        #endregion
    }
}
