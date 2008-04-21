using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Shell
{
    public class Prompt
    {
        /// <summary>
        /// Retrieves the consoleprompt for the given user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static Prompt LoadPrompt(Security.User user)
        {
            return new Prompt(user);
        }

        public static Prompt LoadPrompt()
        {
            return new Prompt("Cosmos");
        }

        //Private variables
        private Security.User xUser = null;
        private string defaultPrompt = "";

        private Prompt(Security.User user)
        {
            xUser = user;
        }

        private Prompt(string prompt)
        {
            defaultPrompt = prompt;
        }

        public string PromptText()
        {
            if (xUser != null)
                return "[" + xUser.Username + "]>";
            else
                return "[" + defaultPrompt + "]>";
        }
    }
}
