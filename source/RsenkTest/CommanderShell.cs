using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel.Staging;
using RsenkTest.Commands;
using RsenkTest.Commands.ClearScreen;

namespace RsenkTest
{
    class CommanderShell : StageBase
    {
        Interpreter interpreter;
        List<CommandBase> commands = new List<CommandBase>();

        public CommanderShell()
        {
            commands.Add(new ClearScreen());
            commands.Add(new RsenkTest.Commands.Version.Version());

            interpreter = new Interpreter(commands);
        }

        public override void Initialize()
        {
            //Clear the screen
            interpreter.ParseCommand("cls").Execute();

            while (true) //Stay in the shell until exit
            {
                Prompter.Prompt("rsenk330", "~");
                string line = Console.ReadLine();

                if (line.Equals("exit"))
                    break;

                if (!String.IsNullOrEmpty(line))
                {
                    CommandBase command = interpreter.ParseCommand(line);

                    if (command != null)
                    {
                        command.Execute();
                    }
                    else
                    {
                        Prompter.PrintError("'" + line + "' is not a valid command. Type 'help' for a list of valid commands");
                    }
                }
            }
        }

        public override string Name
        {
            get { return "Cosmos Commander Shell"; }
        }

        public override void Teardown() { }
    }
}
