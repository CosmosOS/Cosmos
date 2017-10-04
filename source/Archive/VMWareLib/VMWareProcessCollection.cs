using System;
using System.Collections.Generic;
using System.Text;

namespace Vestris.VMWareLib
{
    /// <summary>
    /// A collection of vmware processes, organized by process id.
    /// </summary>
    public class VMWareProcessCollection : Dictionary<long, VMWareVirtualMachine.Process>
    {
        /// <summary>
        /// Find a process by name.
        /// </summary>
        /// <param name="processName">The name of the process.</param>
        /// <param name="comparisonType">The type of string comparison.</param>
        /// <returns></returns>
        public VMWareVirtualMachine.Process FindProcess(string processName, StringComparison comparisonType)
        {
            foreach (KeyValuePair<long, VMWareVirtualMachine.Process> process in this)
            {
                if (string.Compare(process.Value.Name, processName, comparisonType) == 0)
                {
                    return process.Value;
                }
            }

            return null;
        }
    }
}
