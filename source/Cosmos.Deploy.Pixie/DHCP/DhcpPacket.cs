using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.Deploy.Pixie {
  public class DhcpPacket {

    public DhcpPacket() {
    }

    public DhcpPacket(byte[] aData) {
      var xReader = new BinaryReader(new MemoryStream(aData));

      Op = (OpType)xReader.ReadByte();
      if (Op != OpType.Request) {
        throw new Exception("Invalid Op");
      }

      HwType = xReader.ReadByte();
      HwLength = xReader.ReadByte();
      Hops = xReader.ReadByte();

      // Dont worry about byte order, its an atomic number
      TxID = xReader.ReadUInt32();

      //secs    2       filled in by client, seconds elapsed since client started trying to boot.
      xReader.ReadUInt16();
      Flags = xReader.ReadUInt16();

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

      if (xReader.ReadUInt32() != mMagicCookie) {
        throw new Exception("[DHCP] Magic cookie doesn't match.");
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
        Options.Add(xOption, xReader.ReadBytes(xLength));
      }

      Msg = (MsgType)Options[53][0];
    }

    public byte[] GetBytes() {
      // See comments in ctor why we dont convert to network byte order
      var xStream = new MemoryStream();
      var xWriter = new BinaryWriter(xStream);

      xWriter.Write((byte)Op);
      xWriter.Write((byte)1);
      xWriter.Write((byte)6);
      xWriter.Write((byte)0);

      xWriter.Write(TxID);
      xWriter.Write((UInt16)0);
      xWriter.Write(Flags);
      xWriter.Write(0);
      xWriter.Write(YourAddr);
      xWriter.Write(ServerAddr);
      xWriter.Write(0);
      xWriter.Write(HwAddr);

      var xBytes1 = ASCIIEncoding.ASCII.GetBytes("none");
      xWriter.Write(xBytes1);
      xWriter.Write(new byte[64 - xBytes1.Length]);

      var xBytes = ASCIIEncoding.ASCII.GetBytes(BootFile);
      xWriter.Write(xBytes);
      xWriter.Write(new byte[128 - xBytes.Length]);

      xWriter.Write(mMagicCookie);

      xWriter.Write((byte)53);
      xWriter.Write((byte)1);
      xWriter.Write((byte)Msg);

      foreach (var xOption in Options) {
        xWriter.Write(xOption.Key);
        xWriter.Write((byte)xOption.Value.Length);
        xWriter.Write(xOption.Value);
      }
      xWriter.Write((byte)255);

      var xResult = xStream.ToArray();
      return xResult;
    }

    public void AddTextOption(byte aID, string aText) {
      var xBytes = Encoding.ASCII.GetBytes(aText);
      var xBytes2 = new byte[xBytes.Length + 1];
      xBytes2[0] = (byte)xBytes.Length;
      xBytes.CopyTo(xBytes2, 1);
      Options.Add(aID, xBytes2);
    }

    protected UInt32 mMagicCookie = 0x63538263;
    public Dictionary<byte, byte[]> Options = new Dictionary<byte, byte[]>();

    public enum MsgType { Discover = 1, Offer, Request, Decline, Ack, Nak, Release };
    public MsgType Msg;

    public enum OpType { Request = 1, Reply }
    public OpType Op;
    public byte HwType;
    public byte HwLength;
    public byte Hops;
    public UInt32 TxID;
    public UInt16 Flags;
    public UInt32 ClientAddr;
    public UInt32 YourAddr;
    public UInt32 ServerAddr;
    public byte[] HwAddr;
    public string BootFile;
  }
}
