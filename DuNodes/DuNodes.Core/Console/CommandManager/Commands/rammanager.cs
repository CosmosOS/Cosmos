using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DuNodes.System.Console.CommandManager;

namespace DuNodes_Core.Terminal.CommandManager.Commands
{
    public class rammanager : CommandBase
    {
        public override void launch(string[] args)
        {
            System.Console.WriteLine("Cleared ram");
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
