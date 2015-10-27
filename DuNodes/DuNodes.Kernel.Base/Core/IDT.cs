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

// IDT code by Grunt
namespace DuNodes.Kernel.Base.Core
{
    public class IDT
    {
        public delegate void ISR();
        public static ISR[] idt = new ISR[0xFF];
        public static void Remap()
        {
            IO.PortIO.outb(0x20, 0x11);
            IO.PortIO.outb(0xA0, 0x11);
            IO.PortIO.outb(0x21, 0x20);
            IO.PortIO.outb(0xA1, 0x28);
            IO.PortIO.outb(0x21, 0x04);
            IO.PortIO.outb(0xA1, 0x02);
            IO.PortIO.outb(0x21, 0x01);
            IO.PortIO.outb(0xA1, 0x01);
            IO.PortIO.outb(0x21, 0x0);
            IO.PortIO.outb(0xA1, 0x0);
        }
        private void idt_handler()
        {
            int num = 0;
            if (idt[num] != null)
            {
                idt[num]();
            }
        }

        public static void SetGate(byte int_num, ISR handler)
        {
            idt[int_num] = handler;
        }

    }
}
