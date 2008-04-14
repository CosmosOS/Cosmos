using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest
{
    class Prompter
    {
        private const ConsoleColor NORMAL_COLOR = ConsoleColor.White;
        private const ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
        private const ConsoleColor ERROR_COLOR = ConsoleColor.Red;
        private const char SYM_ROOT = '#';
        private const char SYM_NORM = '$';

        /// <summary>
        /// The different message types possible
        /// </summary>
        private enum MessageType
        {
            Normal,
            Warning,
            Error
        }

        /// <summary>
        /// Displays the prompt string
        /// </summary>
        /// <param name="user">The current user.</param>
        /// <param name="path">The current path.</param>
        public static void Prompt(string user, string path)
        {
            Console.ForegroundColor = NORMAL_COLOR;
            Console.Write("[" + user + ":" + path + "]" + SYM_NORM + " ");
        }

        /// <summary>
        /// Prints the welcome message to the console.
        /// </summary>
        /// <param name="additional">Additional messages to be displayed.</param>
        public static void PrintWelcome(string additional)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Welcome to Cosmos Commander!\n\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Prints an error message to the console.
        /// </summary>
        public static void PrintError(string error)
        {
            PrintMessage(error, MessageType.Error);
        }

        /// <summary>
        /// Prints a warning message to the console.
        /// </summary>
        public static void PrintWarning(string warning)
        {
            PrintMessage(warning, MessageType.Warning);
        }

        /// <summary>
        /// Prints a message to the console.
        /// </summary>
        public static void PrintMessage(string message)
        {
            PrintMessage(message, MessageType.Normal);
        }

        /// <summary>
        /// Prints a message to the console and sets the foreground color depending on the message type.
        /// </summary>
        private static void PrintMessage(string message, MessageType type)
        {
            switch (type)
            {
                case MessageType.Error:
                    Console.ForegroundColor = ERROR_COLOR;
                    break;
                case MessageType.Warning:
                    Console.ForegroundColor = WARNING_COLOR;
                    break;
                case MessageType.Normal:
                default:
                    Console.ForegroundColor = NORMAL_COLOR;
                    break;
            }

            Console.WriteLine(message);
        }

        public static void PrintCommandError(string name, bool invalidArg)
        {
            if (invalidArg)
            {
                PrintError("The argument(s) for '" + name + "' were incorrect. Type 'help [command]' for help.");
            }
            else
            {
                PrintError("'" + name + "' is not a valid command. Type 'help' for a list of valid commands.");
            }
        }
    }
}
