/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Info / OK / Error in console
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.HAL
{
    public class CustomConsole
    {

        public static List<string> logs = new List<string>();

        public static void WriteLineInfo(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[Info] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
            logs.Add("[Info] " + text);
        }
        public static void WriteLineOK(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[OK] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
            logs.Add("[OK] " + text);
        }
        public static void WriteLineError(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("[Error] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
            logs.Add("[Error] " + text);
        }
    }
}
