using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.BlockDevice {
  public class Partition {
    BlockDevice mHost;

    public Partition(BlockDevice aHost) {
      mHost = aHost;
    }

  }
}
