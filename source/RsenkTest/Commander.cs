using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest
{
    public class Commander
    {
        private static Commander commanderShell;
        /// <summary>
        /// Will be false when the user exits the shell
        /// </summary>
        private bool runShell;

        private Commander()
        {
            runShell = true; //Tells the shell to start running
        }

        /// <summary>
        /// Gets the current instance of the shell. If it does not exist, create it.
        /// </summary>
        /// <returns>The commander shell.</returns>
        public static Commander GetInstance()
        {
            if(commanderShell == null)
                commanderShell = new Commander();

            return commanderShell;
        }

        /// <summary>
        /// Starts the shell and prompts the user.
        /// </summary>
        public void Start()
        {
            while (runShell) //Keep prompting until the user exits
            {
                Prompter.Prompt("root", "~");
                String command = Console.ReadLine();

                this.Execute(command);
            }
        }

        private void Execute(string command)
        {
            //Use the interpreter to break apart the program to execute and the arguments to pass in
            List<String> comm = Interpreter.GetParsed(command);

            Prompter.PrintMessage("Interpreted command:");

            for(int x = 0; x < comm.Count; x++)
                Prompter.PrintMessage(comm[x]);
        }
    }
}
