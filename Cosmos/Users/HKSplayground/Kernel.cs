using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Hardware;

namespace HKSplayground
{
    public class Kernel : Sys.Kernel
    {
        Mouse m = new Mouse();
        int x=0, y=0;
        int z = 0;
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            m.Initialize();
            Console.WriteLine("Mouse initialized");
        }
        protected override void Run()
        {
            while (true)
            {
                //Do nothing
                if (x != m.X || y != m.Y)
                {
                    x = m.X;
                    y = m.Y;
                    if (z != Int16.MaxValue - 1)
                    {
                        z++;
                    }
                    else
                    {
                        z = 0;
                    }
                    Console.WriteLine(z);
                }
            }
        }

    }
}
