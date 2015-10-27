using System;
using System.Collections;
using DuNodes.HAL;

namespace DuNodes.System.Console.CommandManager.Commands
{
    public class bench : CommandBase
    {
        private string start;
        private string end;
        public bench()
        {
            start = RTC.Now.Minute.ToString() + " " + RTC.Now.Second.ToString();
            //TODO: REAL PIEBENCH
            //For the moment it's just a ram tester
            ArrayList al = new ArrayList();
            for (int i = 0; i < 100; i++)
            {
                al.Add("B1 => Hello I'm just here to take some ram #~##{{[[||`\\^^@@]]}}}}/*--*/¨£µ%");

            }

            Console.WriteLine("Testing availability of first var : " +al[0].ToString());
            Console.WriteLine("Testing availability of middle var : " + al[50].ToString());
          Console.WriteLine("Testing availability of end var : " + al[99].ToString());
            Console.WriteLine("Count should be 100 : " + al.Count);

            int manualCount = 0;
            for (int index = 0; index < al.Count; index++)
            {
                var str = al[index];
                manualCount++;
            }

            Console.WriteLine("Count should be 100 (manual) : " + manualCount);
            al.Clear();

            end = RTC.Now.Minute.ToString() + " " + RTC.Now.Second.ToString();
            Console.WriteLine("Started : " + start + "    Ended : " + end, ConsoleColor.Blue);
        }
    }
}
