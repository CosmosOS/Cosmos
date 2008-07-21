using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Application
{
    class net : IConsoleApplication
    {
        #region IConsoleApplication Members

        public int Execute(object args)
        {
            ////string[] a = ((string)args).Split(' ');
            //string[] a = new string[] {"enable", "dummy"};

            ////List<string> arguments = new List<string>(

            //if (a.Contains("enable"))
            //    Console.WriteLine("Enabling card(dummy)");

            return 0;
        }

        public string CommandName
        {
            get { return "net"; }
        }

        public string Description
        {
            get { return "Network related commands."; }
        }

        #endregion
    }
}
