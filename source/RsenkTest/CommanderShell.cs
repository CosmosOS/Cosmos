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

            interpreter = new Interpreter(commands);
        }

        public override void Initialize()
        {
            while (true) //Stay in the shell until exit
            {
                Prompter.Prompt("rsenk330", "~");
                string line = Console.ReadLine();

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

        public override string Name
        {
            get { return "Cosmos Commander Shell"; }
        }

        public override void Teardown() { }
    }
}
