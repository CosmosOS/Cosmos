using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Shell
{
    public class Session
    {
        Security.User xUser = null;
        static ushort xSessionId;

        public Session(Security.User user)
        {
            xSessionId++;
            xUser = user;
        }

        internal void Run()
        {
            Console.Write(Prompt.LoadPrompt(xUser).PromptText());
            string command = Console.ReadLine();

            if (command.Equals("exit"))
                return;
            else
                Console.WriteLine("No such systemcommand or application: " + command);

            Run(); //Recursive call
        }

        internal static Session CreateSession(FrodeTest.Security.User currentUser)
        {
            return new Session(currentUser);
        }
    }
}
