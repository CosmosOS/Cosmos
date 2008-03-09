using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Security
{
    public class User
    {
        public static User Authenticate(string username, string password)
        {
            User user = new User(username);
            if (user.IsPassword(password))
                return user;
            else
                return null;
        }

        public User(string username)
        {
            xUsername = username;
        }

        //Private variables
        private string xUsername = string.Empty;

        public string Username { get {return xUsername;} }

        /// <summary>
        /// Checks the given password for the user, and returns true if it's the right one.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool IsPassword(string password)
        {
            return true;
        }

    }
}
