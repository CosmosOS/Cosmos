using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.BlockDevice {
  // This class should not support selecting a device or sub device. 
  // Each instance must control exactly one device. For example with ATA
  // master/slave, each one needs its own device instance. For ATA
  // this complicates things a bit because they share IO ports, but this 
  // is an intentional decision.
  public class BlockDevice : Device {
    // TODO: Need to protect this from changes except by Hardware ring 
    static public List<BlockDevice> Devices = new List<BlockDevice>();

    protected UInt64 mBlockCount = 0;
    public UInt64 BlockCount {
      get { return mBlockCount; }
    }

    protected UInt64 mBlockSize = 0;
    public UInt64 BlockSize {
      get { return mBlockSize; }
    }

    public virtual void ReadBlock(UInt64 aSectorNo, byte[] aData) {}
    public virtual void WriteBlock(UInt64 aSectorNo, byte[] aData) {}

  }
}
