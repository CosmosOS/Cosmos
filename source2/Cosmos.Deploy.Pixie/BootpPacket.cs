using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Deploy.Pixie {
  public class BootpPacket {
    protected byte[] mData;

    public BootpPacket() {
      mData = new byte[300];
    }

    public BootpPacket(byte[] aData) {
      mData = aData;

      Op = aData[0];
      HwType = aData[1];
      HwLength = aData[2];
      Hops = aData[3];

      // Dont worry about byte order, its an atomic number
      TxID = BitConverter.ToUInt32(aData, 4);

      // Dont reverse IP Addresses, byte arrays end up big endian as we write them back
      ClientAddr = BitConverter.ToUInt32(aData, 12);

      Array.Reverse(aData, 28, HwLength);
      HwAddr = aData.Skip(28).Take(HwLength).ToArray();
    }

    protected void Write(int aIdx, byte[] aBytes) {
      // See comments in ctor why this is commented out
      //Array.Reverse(aBytes);
      aBytes.CopyTo(mData, aIdx);
    }

    public byte[] GetBytes() {
      mData[0] = Op;
      mData[1] = 1;
      mData[2] = 6;
      Write(4, BitConverter.GetBytes(TxID));
      Write(16, BitConverter.GetBytes(YourAddr));
      Write(20, BitConverter.GetBytes(ServerAddr));
      Write(28, HwAddr);

      return mData;
    }

    public byte Op;
    //op      1       packet op code / message type.
    //                1 = BOOTREQUEST, 2 = BOOTREPLY

    public byte HwType;
    //htype   1       hardware address type,
    //                see ARP section in "Assigned Numbers" RFC.
    //                '1' = 10mb ethernet

    public byte HwLength;
    //hlen    1       hardware address length (eg '6' for 10mb ethernet).

    public byte Hops;
    //hops    1       client sets to zero, optionally used by gateways
    //                in cross-gateway booting.

    public UInt32 TxID;
    //xid     4       transaction ID, a random number, used to match this boot request with the
    //                responses it generates.

    //secs    2       filled in by client, seconds elapsed since client started trying to boot.

    //--      2       unused
    // Data is here.. used by later RFC?

    public UInt32 ClientAddr;
    //ciaddr  4       client IP address;
    //                filled in by client in bootrequest if known.

    public UInt32 YourAddr;
    //yiaddr  4       'your' (client) IP address;
    //                filled by server if client doesn't
    //                know its own address (ciaddr was 0).

    public UInt32 ServerAddr;
    //siaddr  4       server IP address; returned in bootreply by server.

    //giaddr  4       gateway IP address,
    //                used in optional cross-gateway booting.

    public byte[] HwAddr;
    //chaddr  16      client hardware address, filled in by client.

    //sname   64      optional server host name,
    //                null terminated string.

    // [108]
    //file    128     boot file name, null terminated string;
    //                'generic' name or null in bootrequest,
    //                fully qualified directory-path
    //                name in bootreply.

    // [236]
    //vend    64      optional vendor-specific area,
    //                e.g. could be hardware type/serial on request,
    //                or 'capability' / remote file system handle
    //                on reply.  This info may be set aside for use
    //                by a third phase bootstrap or kernel.
  }
}
