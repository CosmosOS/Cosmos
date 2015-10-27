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

using System;

namespace DuNodes.Kernel.Base.Console
{
    public static class Bootscreen
    {
        public enum Effect : byte
        { SlideFromLeft, SlideFromRight, SlideFromTop, SlideFromBottom, Typewriter, Matrix }
        public static void Show(string OSname, Effect efx, ConsoleColor color, int ms_sleep = 20)
        {
            if (ms_sleep % 2 == 1) ms_sleep++;
            switch (efx)
            {
                case Effect.SlideFromLeft:

                    for (int i = 0; i < ((Console.WindowWidth / 2) - (OSname.Length / 2)); i++)
                    {
                        Console.Clear();
                        Console.CursorLeft = i;
                        string fill = "";
                        for (int x = 0; x < i; x++) fill += " ";
                        Console.Write(fill);
                        Console.Write(OSname, color, false, true);
                        Core.PIT.SleepMilliseconds((uint)ms_sleep);
                    } break;

                case Effect.SlideFromRight:

                    for (int i = (Console.WindowWidth - OSname.Length);
                        i > ((Console.WindowWidth / 2) - (OSname.Length / 2)); i--)
                    {
                        Console.Clear();
                        Console.CursorLeft = i;
                        Console.Write(OSname, color, false, true);
                        Core.PIT.SleepMilliseconds((uint)ms_sleep);
                    } break;

                case Effect.SlideFromTop:

                    for (int i = 0; i < (Console.WindowHeight / 2); i++)
                    {
                        Console.Clear();
                        Console.CursorTop = i;
                        Console.WriteLine(OSname, color, true, false);
                        Core.PIT.SleepMilliseconds((uint)ms_sleep);
                    } break;

                case Effect.SlideFromBottom:

                    for (int i = (Console.WindowHeight - 1); i > (Console.WindowHeight / 2); i--)
                    {
                        Console.Clear();
                        Console.CursorTop = i;
                        Console.WriteLine(OSname, color, true, false);
                        Core.PIT.SleepMilliseconds((uint)ms_sleep);
                    } break;

                case Effect.Typewriter:

                    Console.CursorLeft = ((Console.WindowWidth / 2) - (OSname.Length / 2));
                    foreach (char chr in OSname)
                    {
                        Console.Write(chr.ToString(), color, false, true);
                        Core.PIT.SleepMilliseconds((uint)ms_sleep);
                    } break;

                case Effect.Matrix:

                    int sec1 = RTC.Now.Second;
                    int sec2 = sec1;
                    do { sec2 = RTC.Now.Second; } while (sec1 == sec2);
                    int sec3;
                    if (sec2 <= 56) sec3 = sec2 + 3;
                    else if (sec2 == 57) sec3 = 1;
                    else if (sec2 == 58) sec3 = 2;
                    else if (sec2 == 59) sec3 = 3;
                    else sec3 = 3;
                    int tmr = 0;
                    int tmrx = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        for (int ih = 0; ih < Console.WindowHeight; ih++)
                        {
                            for (int iw = 0; iw < Console.WindowWidth; iw++)
                            {
                                if (tmr == 11) tmr = 0;
                                if (tmrx == 4) tmrx = 0;
                                tmr++;
                                if (tmr == 0) Console.Write("#", ConsoleColor.Magenta);
                                if (tmrx == 3) Console.Write("*", ConsoleColor.Green);
                                if (tmr == 2) { Console.Write(";", ConsoleColor.Red); ++tmrx; }
                                if (tmrx == 1) Console.Write("+", ConsoleColor.Yellow);
                                if (tmr == 4) { Console.Write("~", ConsoleColor.Blue); ++tmrx; }
                                if (tmrx == 2) Console.Write("&", ConsoleColor.Cyan);
                                Console.Write(OSname, ConsoleColor.White, true, true);
                            }
                        }
                    } break;
            }
        }
    }
}
