using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FrodeTest.Shell;
using System.IO;

namespace FrodeTest.Application
{
    class cd : IConsoleApplication
    {
        #region IConsoleApplication Members

        public int Execute(object args)
        {
            if (String.IsNullOrEmpty(args.ToString()))
                return 0;

            if (args.ToString() == "..")
            {
                //Go up one directory
                return 0;
            }

            EnvironmentVariables.GetCurrent().CurrentDirectory = EnvironmentVariables.GetCurrent().CurrentDirectory + Path.DirectorySeparatorChar + args.ToString();

            return 0;
        }

        public string CommandName
        {
            get { return "cd"; }
        }

        public string Description
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
