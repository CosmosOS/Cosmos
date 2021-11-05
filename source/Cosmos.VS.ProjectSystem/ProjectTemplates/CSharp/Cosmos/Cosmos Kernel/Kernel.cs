using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace $safeprojectname$
{
    public class Kernel: Sys.Kernel
    {
        /// <summary>
        /// This Method controls the Driver initialisation process and is intended for
        /// Advanced users developing their drivers and takes 4 additional booleans.
        /// 1. Mousewheel, if you experience your mouse cursors being stuck in the lower left corner set this to "false", default: true
        /// 2. PS2 Driver initialisation, true/false , default: true
        /// 3. Network Driver initialisation, true/false, default: true
        /// 4. IDE initialisation, true/false, default: true
        /// If you need anything else to be initialised really early on, place it here.
        /// </summary>
        protected override void OnBoot()
        {
        Sys.Global.Init(GetTextScreen());
        }

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }
        
        protected override void Run()
        {
            Console.Write("Input: ");
            var input = Console.ReadLine();
            Console.Write("Text typed: ");
            Console.WriteLine(input);
        }
    }
}
