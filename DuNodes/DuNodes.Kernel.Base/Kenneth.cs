/*
Copyright (c) 2012-2013, dewitcher Team
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice
   this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

namespace DuNodes.Kernel.Base
{
    /// <summary>
    /// Why so serious?
    /// </summary>
    public static class Kenneth
    {
        /// <summary>
        /// It will do -exactly- that *cough*
        /// </summary>
        public static unsafe void Optimize()
        {
            // Well, it will pretty fuck up your vram
            byte* vram = (byte*)0xB8000;
            for (int i = 0; i >= 0; i++)
            {
                vram[i] = (byte)~vram[i];
            }
        }
        /// <summary>
        /// Burn the witchcode
        /// </summary>
        public static void Burn()
        {
            Console.Console.Clear();
            Console.Console.WriteLine("\n");
            Console.Console.WriteLine("       TM");
            Console.Console.WriteLine("Kenneth   is burning your witchcode...");
            Console.Console.WriteLine("\n");
            Console.Console.CursorTop = 5;
            Console.Console.WriteLine("Preparing...");
            Console.Console.ProgressBar pb = new Console.Console.ProgressBar(0, false);
            do { pb.Increment(); RTC.SleepTicks(500000); } while (pb.Value < 100);
            Console.Console.CursorTop = 5;
            Console.Console.WriteLine("Burning witchcode...");
            pb = new Console.Console.ProgressBar(0, false);
            unsafe
            {
                int* ptr = (int*)0;
                for (int i = 0; i > -1; i++)
                {
                    pb.Increment();
                    ptr[i] = (int)~ptr[i];
                }
            }
            Console.Console.ReadLine();
            Core.ACPI.Shutdown();
        }
        public static void SetPenisBit()
        {
            Core.Bluescreen.Init("PENIS BIT SET", "You have set the penis bit. HAHAHAHAHAHA", true);
        }
        public static void CPUMeltdown()
        {
            Core.Bluescreen.Panic();
        }
    }
}
