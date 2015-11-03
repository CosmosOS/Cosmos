using System;
using System.Collections;
using DuNodes.HAL;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class clearcls : CommandBase
    {
     
        public clearcls()
        {
           Console.Clear();
        }
    }
}
