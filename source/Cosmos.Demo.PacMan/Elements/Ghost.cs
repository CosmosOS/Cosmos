using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Demo.Pacman.Elements
{
    class Ghost
    {
        readonly char elem;
        int weakTime;
        int sleepTime;
        int sleepWeakTime;
        public Ghost(char elem, int sleepTime, int sleepWeakTime, int weakTime)
        {
            this.elem = elem;
            this.sleepTime = sleepTime;
            this.sleepWeakTime = sleepWeakTime;
            this.weakTime=weakTime;
        }
    }
}
