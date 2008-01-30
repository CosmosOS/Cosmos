using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.GdbClient
{
    /// <summary>
    /// Represents GDB packet.
    /// </summary>
    public struct GdbPacket
    {
        private string _sequenceId;

        public string SequenceId
        {
            get { return _sequenceId; }
            set { _sequenceId = value; }
        }
        private string _packetData;

        public string PacketData
        {
            get { return _packetData; }
            set { _packetData = value; }
        }

        public GdbPacket(string sequenceId, string packetData)
        {
            _sequenceId = sequenceId;
            _packetData = packetData;
        }

        public GdbPacket(string packetData)
        {
            _sequenceId = null;
            _packetData = packetData;
        }

        public static GdbPacket FromString(string s)
        {
            if (!s.StartsWith("$"))
                throw new InvalidCastException();

            // Pull off initial $.
            s = s.Substring(1);

            // Packet and checksum.
            string[] parts1 = s.Split('#');
            // Sequence-id and Packet
            string[] parts2 = parts1[0].Split(':');

            GdbPacket result = new GdbPacket();
            result.PacketData = parts1[0];

            if (parts2.Length != 1)
            {
                result.SequenceId = parts2[0];
                result.PacketData = parts2[1];
            }

            int modulo1 = ModuloChecksum(parts1[0]);
            //int modulo2 = int.Parse(parts1[2], System.Globalization.NumberStyles.HexNumber);

            //TODO: Don't know if I got this right.
            //if (modulo1 != modulo2)
            //    throw new InvalidCastException();

            return result;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("$");
            StringBuilder pdata = new StringBuilder();

            if (!string.IsNullOrEmpty(SequenceId))
            {
                pdata.Append(SequenceId);
                pdata.Append(":");
            }

            pdata.Append(PacketData);

            result.Append(pdata.ToString());
            result.Append("#");

            result.Append(ModuloChecksum(pdata.ToString()).ToString("x"));

            return result.ToString();
        }

        private static int ModuloChecksum(string data)
        {
            int result = 0;

            foreach (char c in data)
            {
                result += (int)c;
                result %= 256;
            }

            return result;
        }
    }

    public class GdbPacketEventArgs : EventArgs
    {
        public GdbPacket Packet { get; set; }

        public GdbPacketEventArgs() { }

        public GdbPacketEventArgs(GdbPacket packet)
        {
            Packet = packet;
        }
    }
}
