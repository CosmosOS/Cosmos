using System;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class ping : CommandBase
    {
        public override void launch(string[] args)
        {
            Console.WriteLine("launch ping command.");
        }

        public override void cancelled()
        {
            throw new NotImplementedException();
        }

        public override void pause()
        {
            throw new NotImplementedException();
        }

        public override void finished()
        {
            throw new NotImplementedException();
        }
    }
}
