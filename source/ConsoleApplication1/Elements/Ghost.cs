using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Demo.Pacman.Elements
{
    class Ghost
    {
        char elem;
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

        #region accessors
        public char getElem()
        {
            return elem;
        }

        public void setElem(char elem)
        {
            this.elem = elem; ;
        }


        public int getSleepTime()
        {
            return sleepTime;
        }
        public int getWeakSleepTime()
        {
            return sleepWeakTime;
        }
        public void setSleepTime(int sleepTime)
        {
            this.sleepTime = sleepTime;
        }
        public void setSleepWeakTime(int sleepWeakTime)
        {
            this.sleepWeakTime = sleepWeakTime;
        }

        #endregion
    }
}
