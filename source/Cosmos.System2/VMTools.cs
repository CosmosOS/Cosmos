using Cosmos.HAL;

namespace Cosmos.System
{
    /// <summary>
	/// A class used to detect if cosmos is being ran in a virtual machine.
	/// Usefull for VM specific tasks.
	/// </summary>
    public static class VMTools
    {
        /// <summary>
		/// A boolean describing whether or not virtualbox has been detected.
		/// </summary>
        public static bool IsVirtualBox => GetIsVirtualBox();
        /// <summary>
		/// A boolean describing whether or not VMware has been detected.
		/// </summary>
        public static bool IsVMWare => GetIsVMWare();
        /// <summary>
		/// A boolean describing whether or not QEMU has been detected.
		/// </summary>
        public static bool IsQEMU => GetIsQEMU();

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
    }
}
