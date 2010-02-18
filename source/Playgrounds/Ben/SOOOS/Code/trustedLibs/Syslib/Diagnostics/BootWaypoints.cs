using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syslib.Diagnostics
{
    public static class BootWaypoints
    {

        static public void Started(BootWayPoint waypointNumber)
        {

            //TODO
            //record time
        }


        static public void Finished(BootWayPoint waypointNumber)
        {

            //TODO
            //record time
        }

        static public void Reached(BootWayPoint waypointNumber)
        {

            //TODO
            //record time
        }
    }


    public enum BootWayPoint
    {
        None,
        Unknown,
        MemoryManagerInit =100


    }
}
