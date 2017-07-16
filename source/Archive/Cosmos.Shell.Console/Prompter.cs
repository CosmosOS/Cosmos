using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Demo.Shell {
	/// <summary>
	/// The demonstration prompter.
	/// </summary>
    public class Prompter
    {
        private List<Cosmos.Shell.Console.Commands.CommandBase> _commands;

        private bool running = true;

        public void Stop() {
            running = false;
        }

        public void Initialize() {
            _commands = new List<Cosmos.Shell.Console.Commands.CommandBase>();
            _commands.Add(new Cosmos.Shell.Console.Commands.BreakCommand());
            _commands.Add(new Cosmos.Shell.Console.Commands.ClsCommand());
            _commands.Add(new Cosmos.Shell.Console.Commands.DirCommand());
            _commands.Add(new Cosmos.Shell.Console.Commands.EchoCommand());
            _commands.Add(new Cosmos.Shell.Console.Commands.ExitCommand(Stop));
            _commands.Add(new Cosmos.Shell.Console.Commands.FailCommand());
            _commands.Add(new Cosmos.Shell.Console.Commands.HelpCommand(_commands));
            _commands.Add(new Cosmos.Shell.Console.Commands.TimeCommand());
            _commands.Add(new Cosmos.Shell.Console.Commands.TypeCommand());
            _commands.Add(new Cosmos.Shell.Console.Commands.VersionCommand());
            _commands.Add(new Cosmos.Shell.Console.Commands.LspciCommand());
            _commands.Add(new Cosmos.Shell.Console.Commands.MountCommand());

            while (running) {
                System.Console.Write("Running = ");
                System.Console.Write(running.ToString());
                System.Console.Write(" ");
                System.Console.Write("/> ");
                string line = System.Console.ReadLine();
                if (string.IsNullOrEmpty(line)) { continue; }
                int index = line.IndexOf(' ');
                string command;
                string param;
                if (index == -1) {
                    command = line;
                    param = "";
                } else {
                    command = line.Substring(0, index);
                    param = line.Substring(index + 1);
                }

                bool found = false;
                for (int i = 0; i < _commands.Count; i++) {
                    if (_commands[i].Name == command) {
                        found = true;
                        _commands[i].Execute(param);
                        break;
                    }
                }

                if (!found) {
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.Write("The command ");
                    System.Console.Write(command);
                    System.Console.WriteLine(" is not supported. Please type help for more information.");
                    System.Console.ForegroundColor = ConsoleColor.White;
                    System.Console.WriteLine();
                }
            }
        }

	}
}
