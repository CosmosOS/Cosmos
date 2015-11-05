using System;
using System.Collections;
using DuNodes.HAL;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class clearcls : CommandBase
    {
     
        public override void launch(string[] args)
        {
            Console.Clear();
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
