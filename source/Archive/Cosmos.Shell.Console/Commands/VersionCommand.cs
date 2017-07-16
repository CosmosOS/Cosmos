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
            else
                Help();
        }

        private void DisplayVersion()
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Cosmos Copyright 2010 The Cosmos Project");
            System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write("Cosmos ");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Milestone 4");
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
