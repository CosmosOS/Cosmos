using System;
using System.Collections.Generic;
using Interop.VixCOM;

namespace Vestris.VMWareLib
{
    /// <summary>
    /// Helper functions for VMWare interop.
    /// </summary>
    public abstract class VMWareInterop
    {
        /// <summary>
        /// Default timeouts for VMWare operations.
        /// </summary>
        public static VMWareTimeouts Timeouts = new VMWareTimeouts();

        /// <summary>
        /// Checks whether an error indicates failure and throws an exception in that case.
        /// </summary>
        /// <param name="errCode">Error code.</param>
        public static void Check(ulong errCode)
        {
            if (new VixLib().ErrorIndicatesFailure(errCode))
            {
                throw new VMWareException(errCode);
            }
        }

        /// <summary>
        /// VMWare VIX date/time is expressed in UNIX EPOCH (number of seconds since January 1st, 1970).
        /// Convert VIX date/time into .NET DateTime.
        /// </summary>
        /// <param name="dt">Unix epoch date/time.</param>
        /// <returns>DateTime in .NET format.</returns>
        public static DateTime FromUnixEpoch(long dt)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(dt);
        }
    }
}
