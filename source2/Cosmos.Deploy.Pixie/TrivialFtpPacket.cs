using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Deploy.Pixie {
  public class TrivialFtpPacket {
    public enum OpType { Read = 1, Write, Data, Ack, Error };
    public OpType Op;
    public string Filename;
    public string Mode;

    public TrivialFtpPacket(byte[] aData) {
      var xReader = new BinaryReader(new MemoryStream(aData));

      // Op MSB which is always 0
      xReader.ReadByte(); 
      Op = (OpType)xReader.ReadByte();
      if (Op == OpType.Read || Op == OpType.Write) {
        Filename = ReadString(xReader);
        Mode = ReadString(xReader);
      }
    }

    public string ReadString(BinaryReader aReader) {
      var xResult = new List<byte>();
      
      while (true) {
        byte xByte = aReader.ReadByte();
        if (xByte == 0) {
          break;
        }
        xResult.Add(xByte);
      }

      return Encoding.ASCII.GetString(xResult.ToArray());
    }
  }
}
