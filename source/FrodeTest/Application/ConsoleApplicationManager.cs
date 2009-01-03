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
             //Scan through and find all applications.

            //Adding them manually for now. Until we can scan the harddrive/memory.
            mApplications.Add(new cd());
            mApplications.Add(new dir());
            mApplications.Add(new drives());
            mApplications.Add(new help());
            mApplications.Add(new lspci());
            mApplications.Add(new net());
            mApplications.Add(new ping());
            mApplications.Add(new reboot());
            mApplications.Add(new type());
        }

        public List<IConsoleApplication> AllConsoleApplications { get { return mApplications; } }

        /// <summary>
        /// Get the IConsoleApplication with the given name, or null if no such application found.
        /// </summary>
        public IConsoleApplication GetConsoleApplication(string name)
        {
            //Return the IConsoleApplication with the given name
            if (String.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < mApplications.Count; i++)
            {
                if (mApplications[i].CommandName == name)
                    return mApplications[i];
            }

            return null;
            //    if (app.CommandName.Equals(name))
            //        return app;

            //return null;

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
            //return (IConsoleApplication)mApplications[0];
        }
        
        public class CommandLineSentence
        {
            public CommandLineSentence(string command)
            {
                string[] words = command.Split((char)' ');
                mCommand = words[0];
                mArguments = new string[words.Length - 1];

                for (int i = 1; i < words.Length; i++)
                {
                    Console.WriteLine("Length");
                    mArguments[i - 1] = words[i];
                }
            }

            private string mCommand;
            public string Command { get { return mCommand; } }

            private string[] mArguments;
            public string[] Arguments { get { return mArguments; } }

        }
    }
}
