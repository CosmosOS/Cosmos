/* Licensed under the terms of the New BSD License.
*
* Authors:
*  Gero Landmann (gero_dev) <gero@myzeug.de>
*/

namespace Cosmos.Hardware.SMBIOS
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHardwareType
    {
        /// <summary>
        /// 
        /// </summary>
        byte HardwareType { get; }
        ushort Handle { get; }
    }
}