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

using DuNodes.Kernel.Base.Extensions;

namespace DuNodes.Kernel.Base.Core
{
    public static class PIT
    {
        public static void Mode0(uint frequency)
        {
            IDT.Remap();
            uint divisor = 1193182 / frequency;
            IO.PortIO.outb(0x43, 0x30);
            IO.PortIO.outb(0x40, (byte)(divisor & 0xFF));
            IO.PortIO.outb(0x40, (byte)((divisor >> 8) & 0xFF));
            IRQ.ClearMask(0);
            IRQ.ClearMask(15);
        }
        public static void Mode2(uint frequency)
        {
            IDT.Remap();
            uint divisor = 1193182 / frequency;
            IO.PortIO.outb(0x43, 0x36);
            IO.PortIO.outb(0x40, (byte)(divisor & 0xFF));
            IO.PortIO.outb(0x40, (byte)((divisor >> 8) & 0xFF));
            IRQ.ClearMask(0);
            IRQ.ClearMask(15);
        }
        public static void Beep(uint frequency)
        {
            uint divisor = 1193182 / frequency;
            IO.PortIO.outb(0x43, 0xB6);
            IO.PortIO.outb(0x42, (byte)(divisor & 0xFF));
            IO.PortIO.outb(0x42, (byte)((divisor >> 8) & 0xFF));
        }
        internal static bool called = false;
        public static void SleepSeconds(uint seconds)
        {
            SleepMilliseconds(seconds * 1000);
        }
        public static void SleepMilliseconds(uint milliseconds)
        {
            if (milliseconds <= 50)
            {
                called = false;
                Mode0(milliseconds.MsToHz());
                while (!called) { }
                called = false;
            }
            else
            {
                uint mod = milliseconds % 100;
                uint ms = milliseconds - mod;
                for (int i = 0; i < ms; i += 50)
                {
                    called = false;
                    Mode0(20);
                    while (!called) { }
                }
                called = false;
                ms = mod % 2;
                for (int i = 0; i < ms; i += 2)
                {
                    called = false;
                    Mode0(500);
                    while (!called) { }
                }
                called = false;
            }
        }
    }
}
