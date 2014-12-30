using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cosmos.Debug.Common;

namespace Cosmos.VS.Package {
  public class ProfilePresets : Dictionary<string, string> {
    public ProfilePresets() {
      Add("ISO", "ISO Image");
      Add("USB", "USB Bootable Drive");
      Add("PXE", "PXE Network Boot");
      Add("VMware", "VMware");
      if (BochsSupport.BochsEnabled) { Add("Bochs", "Bochs"); }
      Add("IntelEdison", "Intel Edison Serial boot");
    }
  }
}
