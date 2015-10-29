﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.BlockDevice {
  public abstract class Ata : BlockDevice {

    protected Ata() {
      mBlockSize = 512;
    }

    // In future may need to add a None for PCI ATA controllers. 
    // Or maybe they all have Primary and Secondary on them as well.
    public enum ControllerIdEnum { Primary, Secondary }
    protected ControllerIdEnum mControllerID;
    public ControllerIdEnum ControllerID {
      get { return mControllerID; }
    }

    public enum BusPositionEnum { Master, Slave }
    protected BusPositionEnum mBusPosition;
    public BusPositionEnum BusPosition {
      get { return mBusPosition; }
    }

  }
}
