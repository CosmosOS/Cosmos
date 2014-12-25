using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace TestBed
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {

            C oba = new C();
            IA ob = oba;
            ob.SayHello();
            IB ob2 = oba;
            ob2.SayHowAreYou();
        }

        protected override void Run()
        {
           
        }
    }
}
