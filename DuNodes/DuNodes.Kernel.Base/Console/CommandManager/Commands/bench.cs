using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DuNodes.Kernel.Base;

namespace DuNodes_Core.Terminal.CommandManager.Commands
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
            for (int i = 0; i < 10000000; i++)
            {
                al.Add("B1 => Hello I'm just here to take some ram #~##{{[[||`\\^^@@]]}}}}/*--*/¨£µ%");

            }

            Console.WriteLine("Testing availability of first var : " +al[0].ToString());
            Console.WriteLine("Testing availability of middle var : " + al[5000000].ToString());
            Console.WriteLine("Testing availability of end var : " + al[9999999].ToString());
            Console.WriteLine("Count should be 10000000 : " + al.Count);

            int manualCount = 0;
            for (int index = 0; index < al.Count; index++)
            {
                var str = al[index];
                manualCount++;
            }

            Console.WriteLine("Count should be 10000000 (manual) : " + manualCount);
            al.Clear();

            end = RTC.Now.Minute.ToString() + " " + RTC.Now.Second.ToString();
            Console.WriteLine("Started : " + start + "    Ended : " + end, ConsoleColor.Blue);
        }
    }
}
