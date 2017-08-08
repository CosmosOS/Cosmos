using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TestBed
{
    class C :  IB
    {
        public void SayHello()
        {
            Console.WriteLine("Hello");
        }

        public void SayHowAreYou()
        {
            Console.WriteLine("How Are You");
        }
    }
}
