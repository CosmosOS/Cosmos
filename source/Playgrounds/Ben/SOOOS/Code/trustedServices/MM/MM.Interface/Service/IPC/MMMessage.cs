using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ben.Kernel.IPC;

namespace Ben.Services.MM
{


    public abstract class MMMessage :KernelMessage
    {

           protected MMMessage(ushort id) : base(id)
    {
    }
    
    }
}
