using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Demo.Pacman.Elements
{
    public class Pacman
    {
        public Direction direction;
        readonly char elem;
        int sleepTime;
        public Pacman(char elem, int sleepTime)
        {
            this.elem = elem;
            this.sleepTime = sleepTime;
        }
    }
}
