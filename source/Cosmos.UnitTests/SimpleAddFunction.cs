using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.UnitTests
{
    public class SimpleAddFunction
    {

        public int Test()
        {
            int theValue = Add(1, 2);

             if (theValue == 3) {
                return 1;
             } else {
                return 0;
             }
            
        }

        private int Add(int a, int b)
        {
            return a + b;
        }
    }
}
