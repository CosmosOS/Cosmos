using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Application
{
    class reboot : IConsoleApplication
    {
        #region IConsoleApplication Members

        public int Execute(object args)
        {
            Cosmos.Sys.Deboot.Reboot();

            return 0;
        }

        public string CommandName
        {
            get { return "reboot"; }
        }

        public string Description
        {
            get { return "Restarts the computer."; }
        }

        #endregion
    }
}
