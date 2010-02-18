using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.GC
{
   // [Trusted]

    public enum CollectionState { UNKNOWN, Idle, Collecting, PendingCollect }  

    [TrustedInterface]
    public interface ICollector
    {

        /// <summary>
        /// 
        /// </summary>
        uint DomainId { get;  }

        CollectionState State { get; }

        CollectorDomainContext GetContext();
    }
}
