using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Application
{
    class ConsoleApplicationManager
    {
        private List<IConsoleApplication> mApplications = new List<IConsoleApplication>();

        public ConsoleApplicationManager()
        {
            this.SetAllConsoleApplications();
        }

        private void SetAllConsoleApplications()
        {
            //Scan through and find all applications.

            //Adding them manually for now. Until we can scan the harddrive/memory.
            mApplications.Add(new net());
            mApplications.Add(new ping());
            mApplications.Add(new reboot());
        }

        /// <summary>
        /// Get the IConsoleApplication with the given name, or null if no such application found.
        /// </summary>
        public IConsoleApplication GetConsoleApplication(string name)
        {
            //Return the IConsoleApplication with the given name
            if (String.IsNullOrEmpty(name))
                return null;

            //if (name == "help")
            //{
            //    Console.WriteLine("Commands recognized:");
            //    foreach (IConsoleApplication app in mApplications)
            //        Console.WriteLine(app.CommandName + "\t" + app.Description);
            //}

            //var xApplication = (from application in mApplications
            //                                    where application.CommandName == name
            //                                    select application).First();

            //return xApplication;
            return (IConsoleApplication)mApplications[1];
        }
    }
}
