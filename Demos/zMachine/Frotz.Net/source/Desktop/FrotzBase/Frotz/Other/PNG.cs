// This is a very quick hack to allow me to use the Adaptive Palatte stuff

using System;
using System.Collections.Generic;
using System.IO;

namespace FrotzNetDLL.Frotz.Other
{
    public class PNGChunk
    {
        public String Type { get; set; }
        public byte[] Data { get; set; }
        public ulong CRC { get; set; }
    }


    public class PNG
    {
        private readonly byte[] _header = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };


        Stream _stream;

        List<String> _chunkOrder = new List<string>();

        Dictionary<String, PNGChunk> _chunks = new Dictionary<string, PNGChunk>();


        public Dictionary<String, PNGChunk> Chunks
        {
            get { return _chunks; }
        }

        public PNG(String FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open);
            _stream = fs;

            ParsePng();
        }

        private void ParsePng()
        {

            byte[] buffer = new byte[8];
            _stream.Read(buffer, 0, 8);

            for (int i = 0; i < 8; i++)
            {
                if (buffer[i] != _header[i])
                {
                    throw new ArgumentException("Not a valid PNG file");
                }
            }

            while (_stream.Position < _stream.Length)
            {
                readChunk();
            }
        }

        private void readChunk()
        {
            ulong len = readInt();
            String type = readType();
            byte[] buffer = new byte[len];
            _stream.Read(buffer, 0, (int)len);
            ulong crc = readInt();

            if (crc != CalcCRC(type, buffer))
            {
                Console.WriteLine("CRC Don't match! {0} {1:X}:{2:X}", type, crc, CalcCRC(type, buffer));
            }

            PNGChunk pc = new PNGChunk() { Type = type, Data = buffer, CRC = crc };
            _chunks.Add(pc.Type, pc);

            _chunkOrder.Add(pc.Type);
        }

        private ulong CalcCRC(String type, byte[] buffer)
        {
            byte[] temp = new byte[buffer.Length + 4];
            byte[] tempType = System.Text.Encoding.UTF8.GetBytes(type);
            Array.Copy(tempType, temp, 4);
            Array.Copy(buffer, 0, temp, 4, buffer.Length);
            CRC c = new CRC();
                    
            return c.CalculateCRC(temp, temp.Length);
        }

        private string readType()
        {
            byte[] buffer = new byte[4];
            _stream.Read(buffer, 0, 4);

            return System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        private ulong readInt()
        {
            byte[] buffer = new byte[4];
            _stream.Read(buffer, 0, 4);

            ulong a = buffer[0];
            ulong b = buffer[1];
            ulong c = buffer[2];
            ulong d = buffer[3];

            return ((ulong)(a << 24 | ((b) << 16) | ((c) << 8) | (d)));
        }

        public void Save(String FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Create);
            Save(fs);
        }

        public void Save(Stream Stream) {
            Stream.Write(_header, 0, 8);

            foreach (String type in _chunkOrder)
            {
                PNGChunk chunk = _chunks[type];

                WriteLong(Stream, (ulong)chunk.Data.Length);
                Stream.WriteByte((byte)type[0]);
                Stream.WriteByte((byte)type[1]);
                Stream.WriteByte((byte)type[2]);
                Stream.WriteByte((byte)type[3]);

                Stream.Write(chunk.Data, 0, chunk.Data.Length);

                WriteLong(Stream, chunk.CRC);
            }

            Stream.Close();
        }

        private void WriteLong(Stream s, ulong l)
        {
            s.WriteByte((byte)(l >> 24));
            s.WriteByte((byte)(l >> 16));
            s.WriteByte((byte)(l >> 8));
            s.WriteByte((byte)(l));
        }

        public PNG(Stream Stream)
        {
            _stream = Stream;

            ParsePng();
        }
    }
}
