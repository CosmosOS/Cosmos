using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Application
{
    class help : IConsoleApplication
    {
        #region IConsoleApplication Members

        public int Execute(string[] args)
        {
            //foreach (IConsoleApplication app in new ConsoleApplicationManager().AllConsoleApplications)
            List<IConsoleApplication> xApps = new ConsoleApplicationManager().AllConsoleApplications;
            for (int i = 0; i < xApps.Count; i++)
            {
                Console.WriteLine(xApps[i].CommandName + "    " + xApps[i].Description);
            }

            return 0;
        }

        public string CommandName
        {
            get { return "help"; }
        }

        public string Description
        {
            get { return "Displays all commands"; }
        }

        #endregion
    }
}
