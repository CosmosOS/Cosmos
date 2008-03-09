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

        //Private variables
        private Security.User xUser = null;

        private Prompt(Security.User user)
        {
            xUser = user;
        }

        public string PromptText()
        {
            return "[" + xUser.Username + "]>";
        }
    }
}
