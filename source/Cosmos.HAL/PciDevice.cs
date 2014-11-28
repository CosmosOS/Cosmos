using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL {
  // Needs to be renamed. IoPCI? PciSlot? Does not descend from Device and devices create
  // their hierarchy by function, not interface (ie BlockDevice, not Pci).
  // PciDevice serves a function like IOPort and can be passed to a contructor
  // of a device.
  public class PciDevice {
  }
}
