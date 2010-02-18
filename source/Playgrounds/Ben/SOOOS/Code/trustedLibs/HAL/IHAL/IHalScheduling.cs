using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHAL
{
    /// <summary>
    ///  this is prob machine dependent.
    /// </summary>
    public interface IHalScheduling
    {
        //Schedule Thread

        // pass in saved registers and instruction pointers. . 
        void ScheduleThread();

        void SaveThread();


    }
}
