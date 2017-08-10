using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands
{
    public class HelpCommand : CommandBase
    {
        public override string Name
        {
            get { return "help"; }
        }

        public HelpCommand(List<CommandBase> commands)
        {
            _commands = commands;
        }

        private List<CommandBase> _commands;

        public override void Execute(string param)
        {
            if (param.CompareTo("") == 0)
                DisplayCommands();
            else
                CommandHelp(param);
        }

        private void CommandHelp(string command)
        {
            bool found = false;
            for (int i = 0; i < _commands.Count; i++)
            {
                if (_commands[i].Name.CompareTo(command) == 0)
                {
                    found = true;
                    _commands[i].Help();
                    System.Console.WriteLine();
                    break;
                }
            }

            if (!found)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Write("The command ");
                System.Console.Write(command);
                System.Console.WriteLine(" is not supported. Please type help for more information.");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine();
            }
        }

        private void DisplayCommands()
        {
            System.Console.WriteLine("Supported Commands:");
            for (int i = 0; i < _commands.Count; i++)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Write("  ");
                System.Console.Write(_commands[i].Name);
                System.Console.Write(":  ");
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine(_commands[i].Summary);
            }
            System.Console.WriteLine("Please type help [command] for more information.");
            System.Console.WriteLine();
            
        }

        public override void Help()
        {
            System.Console.WriteLine("help [command]");
            System.Console.WriteLine("  Gets help on a specific command.");
            System.Console.WriteLine("  [command]:The command to look up.");
        }

        public override string Summary
        {
            get { return "Gets help on a specific command."; }
        }
    }
}
