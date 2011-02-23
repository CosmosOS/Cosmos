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

  public abstract class BlockDevice : Device {
    // TODO: Need to protect this from changes except by Hardware ring 
    static public List<BlockDevice> Devices = new List<BlockDevice>();
    
    //TODO:UInt64
    protected UInt32 mBlockCount = 0;
    public UInt32 BlockCount {
      get { return mBlockCount; }
    }

    //TODO:UInt64
    protected UInt32 mBlockSize = 0;
    public UInt32 BlockSize {
      get { return mBlockSize; }
    }

    //TODO:UInt64
    public abstract void ReadBlock(UInt32 aBlockNo, byte[] aData);
    //TODO:UInt64
    public abstract void WriteBlock(UInt32 aBlockNo, byte[] aData);

    //TODO:UInt64
    protected void CheckBlockNo(UInt32 aBlockNo) {
      if (aBlockNo >= mBlockCount) {
        throw new Exception("Invalid block number.");
      }
    }

  }
}
