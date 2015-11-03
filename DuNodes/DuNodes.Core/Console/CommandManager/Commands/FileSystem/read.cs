using System;
using System.Collections.Generic;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class read : CommandBase
    {
        public override void launch(string[] args)
        {
            Console.WriteLine("launch config command.");
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
