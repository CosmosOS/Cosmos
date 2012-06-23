using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.Deploy.Pixie {
  public class DhcpPacket {
    protected byte[] mData;
    public enum OpType { BootRequest = 0, BootReply = 1 }
    protected byte[] mMagicCookie = new byte[] { 99, 130, 83, 99 };
    protected List<byte[]> mOptions = new List<byte[]>();

    public DhcpPacket() {
      mData = new byte[300];
    }

    public DhcpPacket(byte[] aData) {
      mData = aData;

      var xReader = new BinaryReader(new MemoryStream(mData));

      Op = (OpType)xReader.ReadByte();
      HwType = xReader.ReadByte();
      HwLength = xReader.ReadByte();
      Hops = xReader.ReadByte();

      // Dont worry about byte order, its an atomic number
      TxID = xReader.ReadUInt32();

      //secs    2       filled in by client, seconds elapsed since client started trying to boot.
      xReader.ReadUInt16();
      //flags   2       
      xReader.ReadUInt16();

      // Dont reverse IP Addresses, byte arrays end up big endian as we write them back
      ClientAddr = xReader.ReadUInt32();

      // Your Addr
      xReader.ReadUInt32();
      // Server Addr
      xReader.ReadUInt32();
      // Gateway Addr
      xReader.ReadUInt32();

      HwAddr = xReader.ReadBytes(16);

      //sname   64      optional server host name, null terminated string.
      xReader.ReadBytes(64);

      //file    128     boot file name, null terminated string;
      //                'generic' name or null in bootrequest,
      //                fully qualified directory-path
      //                name in bootreply.
      xReader.ReadBytes(128);

      if (xReader.ReadUInt32() != 0x63538263) {
        throw new Exception("Magic cookie doesn't match.");
      }

      //options     var  Optional parameters field.  See the options
      //                documents for a list of defined options.  
      while (true) {
        byte xOption = xReader.ReadByte();
        if (xOption == 255) {
          break;
        } else if (xOption == 0) {
          continue;
        }

        byte xLength = xReader.ReadByte();
        var xBytes = new byte[xLength + 2];
        xBytes[0] = xOption;
        xBytes[1] = xLength;
        var xData = xReader.ReadBytes(xLength);
        xData.CopyTo(xBytes, 2);

        mOptions.Add(xBytes);
      }
    }

    protected void Write(int aIdx, byte[] aBytes) {
      // See comments in ctor why this is commented out
      //Array.Reverse(aBytes);
      aBytes.CopyTo(mData, aIdx);
    }

    public byte[] GetBytes() {
      mData[0] = (byte)Op;
      mData[1] = 1;
      mData[2] = 6;
      Write(4, BitConverter.GetBytes(TxID));
      Write(16, BitConverter.GetBytes(YourAddr));
      Write(20, BitConverter.GetBytes(ServerAddr));
      Write(28, HwAddr);
      Write(236, mMagicCookie);

      return mData;
    }

    public OpType Op;
    public byte HwType;
    public byte HwLength;
    public byte Hops;
    public UInt32 TxID;
    public UInt32 ClientAddr;
    public UInt32 YourAddr;
    public UInt32 ServerAddr;
    public byte[] HwAddr;
  }
}
