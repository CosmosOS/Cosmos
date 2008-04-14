using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest.Commands
{
    class HelpCommand : CommandBase
    {
        public HelpCommand()
        {
            parameters = new List<CommandBase>();

            parameters.Add(new Commands.ClearScreen.ClearScreen());
            parameters.Add(new RsenkTest.Commands.Version.Version());
        }

        public override string Name
        {
            get { return "help"; }
        }

        public override string Summary
        {
            get { return "Provides help for commands"; }
        }

        public override void Execute(params ParameterBase[] args)
        {
            switch (args.Length)
            {
                case 0: //'help' parsed
                    Help();
                    break;
                case 1: //'help [command]' parsed
                    args[0].Help();
                    break;
                default:
                    Prompter.PrintCommandError(Name, true);
                    break;
            }
        }

        public override void Help()
        {
            Prompter.PrintMessage("");
            Prompter.PrintMessage("Command    Summary");
            CommanderShell.GetInstance().PrintCommands();
        }
    }
}
