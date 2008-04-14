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
        private Interpreter interpreter;
        private List<CommandBase> commands = new List<CommandBase>();
        private static CommanderShell commander;

        private CommanderShell()
        {
            commands.Add(new ClearScreen());
            commands.Add(new RsenkTest.Commands.Version.Version());
            commands.Add(new HelpCommand());

            interpreter = new Interpreter(commands);
        }

        public static CommanderShell GetInstance()
        {
            if (commander == null)
                commander = new CommanderShell();

            return commander;
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
                        ParameterBase[] parameters = interpreter.ParseParameters(command, line);
                        command.Execute(parameters);
                    }
                    else
                    {
                        Prompter.PrintCommandError(line, false);
                    }
                }
            }
        }

        public override string Name
        {
            get { return "Cosmos Commander Shell"; }
        }

        public override void Teardown() { }

        public void PrintCommands()
        {
            string toPrint = "";
            for (int x = 0; x < commands.Count; x++)
            {
                toPrint = commands[x].Name;

                for (int i = commands[x].Name.Length; i < 11; i++)
                {
                    toPrint += " ";
                }

                toPrint += commands[x].Summary;

                Prompter.PrintMessage(toPrint);
            }

            Prompter.PrintMessage("exit       Exits Commander\n");
            Prompter.PrintMessage("Type 'help [command]' for more help.\n");
        }

        public List<CommandBase> Commands
        {
            get
            {
                return commands;
            }
        }
    }
}
