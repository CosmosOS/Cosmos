using Cosmos.HAL;

namespace Cosmos.System
{
    /// <summary>
    /// Used to get the virtualization state of the CPU.
	/// </summary>
    public static class VMTools
    {
        #region Properties

        /// <summary>
        /// Whether or not the VirtualBox virtualizer has been detected.
        /// </summary>
        public static bool IsVirtualBox => GetIsVirtualBox();

        /// <summary>
		/// Whether or not the VMware virtualizer has been detected.
		/// </summary>
        public static bool IsVMWare => GetIsVMWare();

        /// <summary>
		/// Whether or not the QEMU virtualizer has been detected.
		/// </summary>
        public static bool IsQEMU => GetIsQEMU();

        #endregion

        #region Methods

        // NOTE: @ascpixi: A better way to scan for virtualizers would be
        //       to read the CPUID leaf 0x40000000. This leaf is dedicated
        //       for hypervisors to provide information about the virtualization
        //       state of the CPU.

        private static bool GetIsVirtualBox()
        {
            for (int I = 0; I < PCI.Count; I++)
            {
                if (PCI.Devices[I].VendorID == 0x80EE)
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetIsVMWare()
        {
            for (int I = 0; I < PCI.Count; I++)
            {
                if (PCI.Devices[I].VendorID == 0x15ad)
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetIsQEMU()
        {
            for (int I = 0; I < PCI.Count; I++)
            {
                if (PCI.Devices[I].VendorID == 0x1af4)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}