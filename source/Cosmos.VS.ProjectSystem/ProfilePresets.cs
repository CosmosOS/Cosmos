using System.Collections.Generic;
using System.Reflection;
using Cosmos.Build.Common;

namespace Cosmos.VS.ProjectSystem
{
    public class ProfilePresets : Dictionary<string, string>
    {
        public ProfilePresets()
        {
            Logger.TraceMethod(MethodBase.GetCurrentMethod());

            Add("ISO", "ISO Image");
            Add("USB", "USB Bootable Drive");
            Add("PXE", "PXE Network Boot");
            Add("VMware", "VMware");
            if (BochsSupport.BochsEnabled)
            {
                Add("Bochs", "Bochs");
            }
            Add("IntelEdison", "Intel Edison Serial boot");
            Add("HyperV", "Hyper-V");
        }
    }
}
