using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.BlockDevice {
  public abstract class Ata : BlockDevice {
    public enum BusPositionEnum {Master, Slave}
    protected BusPositionEnum mBusPosition;
    public BusPositionEnum BusPosition {
      get { return mBusPosition; }
    }

  }
}
