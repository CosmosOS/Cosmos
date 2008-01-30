using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Shell.Console.Commands
{
    public class VersionCommand : CommandBase
    {

        public override string Name
        {
            get { return "version"; }
        }

        public override string Summary
        {
            get { return "Displays version information about Cosmos."; }
        }

        public override void Execute(string param)
        {
            if (param.CompareTo("") == 0 || param.CompareTo("ver") == 0)
            {
                DisplayVersion();
            }
            else if (param.CompareTo("dev") == 0)
            {
                DisplayDevelopers();
            }
            else
                Help();
        }

        private void DisplayDevelopers()
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Cosmos Developers");
            System.Console.WriteLine("~~~~~~~~~~~~~~~~~");
            System.Console.ForegroundColor = ConsoleColor.White;

            System.Console.WriteLine("Chad Z. Hower");
            System.Console.WriteLine("Matthijs ter Woord");
            System.Console.WriteLine("Jonathan Dickinson");
            System.Console.WriteLine();
        }

        private void DisplayVersion()
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Cosmos Copyright 2008 The Cosmos Project");
            System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("Version 0.0.0.1 - ");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Milestone 1");
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine();
        }

        public override void Help()
        {
            System.Console.WriteLine("version [ver|dev]");
            System.Console.Write("  "); System.Console.WriteLine(Summary);
            System.Console.WriteLine();
            System.Console.WriteLine("  ver: Displays the version information.");
            System.Console.WriteLine("  dev: Displays information about the developers.");
        }
    }
}
