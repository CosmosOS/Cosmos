/* Licensed under the terms of the New BSD License.
*
* Authors:
*  Gero Landmann (gero_dev) <gero@myzeug.de>
*/
using System;
using System.Collections.Generic;

using Cosmos.Kernel;

namespace Cosmos.Hardware.SMBIOS
{
    public abstract class BaseInfo : IHardwareType 
    {
        #region IHardwareType Members

        public byte HardwareType { get; protected set; }

        public ushort Handle { get; protected set; }

        #endregion
    }
}