using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHAL
{

    public interface IHalProcessors
    {
        uint TotalMaxThreadsRun { get; }
        uint MaxCores { get; }
        IHalProcessor[] Processors { get; }
        bool AllCoresShareSingleCache { get; }  // must use volatile if all cores do not share.

        //Only on the CPU that handles interups
        void DisableInterupts(); //cli
        void EnableInterupts(); 
    }

    public interface IHalProcessor
    {
        uint Id { get; }
        uint MaxThreadsRun { get; }
    }
}
