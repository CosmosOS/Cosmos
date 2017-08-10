using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;

namespace Cosmos.Shell.Console.Commands
{
    class TimeCommand : CommandBase
    {
        public override string Name
        {
            get { return "time"; }
        }

        public override string Summary
        {
            get { return "Displays the current time."; }
        }

        public override void Execute(string param)
        {
            //TODO: For some reason the values output are too high. F.instance GetSeconds goes to 90.
            System.Console.WriteLine((int)RTC.GetHours() + ":" + RTC.GetMinutes() + ":" + RTC.GetSeconds());
        }

        public override void Help()
        {
            throw new NotImplementedException();
        }
    }
}
