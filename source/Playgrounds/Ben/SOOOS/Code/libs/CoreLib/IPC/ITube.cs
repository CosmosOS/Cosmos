using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.ProcessModel;
using CoreLib.Capabilities;

namespace CoreLib.IPC
{
    //[TrustedInterface]


    //TODO make internal with constructor
    public struct Tube
    {
        public STP Source { get; set; }
        public STP Destination { get; set; }   
        //TODO principal 

       
    }
}
