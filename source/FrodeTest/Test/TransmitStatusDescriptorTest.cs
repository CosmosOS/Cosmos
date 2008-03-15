using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Driver.RTL8139.Register;

namespace FrodeTest.Test
{
    class TransmitStatusDescriptorTest
    {
        public static void RunTest()
        {
            //Testing that the TSD is rotating between the four Descriptors
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(TransmitStatusDescriptor.GetCurrentTSDescriptor());
                TransmitStatusDescriptor.IncrementTSDescriptor();
            }
            
        }
    }
}
