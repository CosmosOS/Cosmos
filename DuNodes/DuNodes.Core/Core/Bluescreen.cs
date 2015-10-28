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
using DuNodes.HAL.Core;
using DuNodes.System.Extensions;

namespace DuNodes.System.Core
{
    public static class Bluescreen
    {
//        public static void Panic(string error)
//        {
//            /* Old code
//            Console.BackgroundColor = ConsoleColor.Blue;
//            Console.Clear();
//            Console.WriteLine();
//            Extensions.Write2("FlowDOS", ConsoleColor.White, true, false);
//            Console.WriteLine();
//            Extensions.Write2("An error occured and FlowDOS has been shut down to prevent damage to your files.", ConsoleColor.White, true, false);
//            Console.WriteLine();
//            Extensions.Write2(error, ConsoleColor.White, true, false);
//            Console.WriteLine();*/

//            // From G-DOS
//            Console.BackgroundColor = ConsoleColor.DarkBlue;
//            Console.ForegroundColor = ConsoleColor.White;
//            Console.Clear();
//            // The cosmos console class sucks, I should have rewritten it like in Grunty OS
//            // but using a different class for output would create confusion so I decieded
//            // not too
//            for (int i = 0; i < (80 * 26); i++)
//                Console.Write(" ");
//            Console.CursorLeft = 0;
//            Console.CursorTop = 0;
//            Console.WriteLine("A problem has been detected and FlowDOS has been shut down to prevent damage to your computer.\n");

//            Console.WriteLine(error);
//            Console.WriteLine(@"
//If this is the first time you've seen this Stop error screen, 
//restart you computer. If this screen appears again follow
//these steps:

//Check to make sure any new hardware is properly installed.
//If this is a new installation, check your hardware to see if it is 
//compatible with your computer's BIOS.

//If problems continue, disable or remove any newly installed hardware. 
//Disable BIOS memory options such as caching or shadowing.");
//            while (true) ;
//        }
    }
}