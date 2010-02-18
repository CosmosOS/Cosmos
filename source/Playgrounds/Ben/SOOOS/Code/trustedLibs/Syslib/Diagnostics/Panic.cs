using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syslib.Diagnostics
{

    /// <summary>
    ///  THis is calls our screen of death or a debugger if attached 
    /// </summary>
    public class Panic :SystemException
    {



        static public void Invoke(string reason)
        {

            KernelDebugger.Break();
        }


   
    }
}
