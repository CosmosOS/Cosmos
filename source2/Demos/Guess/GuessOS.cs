using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace GuessKernel {
    public class GuessOS : Sys.Kernel {
        protected override void BeforeRun() {
            Console.WriteLine("Guess");
        }

        protected override void Run() {
            string xResult = Console.ReadLine();
        }
    }