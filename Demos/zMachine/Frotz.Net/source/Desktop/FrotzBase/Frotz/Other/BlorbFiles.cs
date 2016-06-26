using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

using System.Xml.Linq;

using Frotz.Screen;

namespace Frotz.Blorb {

    public class Blorb {
        public Dictionary<int, BlorbPicture> Pictures { get; private set; }
        public Dictionary<int, byte[]> Sounds { get; private set; }
        public byte[] ZCode { get; set; }
        public String MetaData { get; set; }
        public String StoryName { get; set; }
        public byte[] IFhd { get; set; }
        public int ReleaseNumber { get; set; }

        public ZSize StandardSize { get; set; }
        public ZSize MaxSize { get; set; }
        public ZSize MinSize { get; set; }

        public List<int> AdaptivePalatte { get; set; }

        internal Blorb() {
            Pictures = new Dictionary<int, BlorbPicture>();
            Sounds = new Dictionary<int, byte[]>();
            AdaptivePalatte = new List<int>();

            ReleaseNumber = 0;

            StandardSize = ZSize.Empty;
            MinSize = ZSize.Empty;
            MaxSize = ZSize.Empty;
        }
    }

    public class BlorbPicture {
        public byte[] Image { get; private set; }

        internal double StandardRatio { get; set; }
        internal double MinRatio { get; set; }
        internal double MaxRatio { get; set; }

        internal BlorbPicture(byte[] Image) {
            this.Image = Image;
        }
    }


    public class BlorbReader {

        private class Resource {
            internal int Id;
            internal String Usage;
            internal byte[] Data;

            internal Resource(int Id, String Usage, byte[] Data) {
                this.Id = Id;
                this.Usage = Usage;
                this.Data = Data;
            }
        }

        private struct Chunk {
            internal String Usage;
            internal int Number;
            internal int Start;

            internal Chunk(String Usage, int Number, int Start) {
                this.Usage = Usage;
                this.Number = Number;
                this.Start = Start;
            }
        }

        static Stream _stream;
        static int level = 0;

        static Dictionary<int, Chunk> _chunks;
        static Dictionary<int, Resource> _resources;

        private static Blorb _blorb;

        private static void handleForm(int start, int length) {
            level++;
            while (_stream.Position < start + length) {
                String type = ReadString();
                int len = ReadInt();
                // ReadBuffer(len);

                readChunk((int)_stream.Position, len, type);
            }
            level--;
        }

        private static void readChunk(int start, int length, String type) {
            byte[] buffer = ReadBuffer(length);
            if (_chunks.ContainsKey(start - 8)) {
                Chunk c = _chunks[start - 8];
                if (c.Usage == "Exec") {
                    _blorb.ZCode = buffer;
                } else if (c.Usage == "Pict") {
                    _blorb.Pictures[c.Number] = new BlorbPicture(buffer);
#if DEBUGPICTURES
                    // Keeping this snippet around in case I want to see the graphics in the Blorb file
                    if (buffer.Length != 8)
                    {
                        FileStream fs = new FileStream(
                            String.Format("c:\\temp\\{0:d3}.png", c.Number), FileMode.Create);
                        fs.Write(buffer, 0, buffer.Length);
                        fs.Close();
                    }
#endif
                } else if (c.Usage == "Snd ") {
                    byte[] temp = new byte[buffer.Length + 8];

                    if (buffer[0] == 'A' && buffer[1] == 'I' && buffer[2] == 'F' && buffer[3] == 'F') {
                        temp[0] = (byte)'F';
                        temp[1] = (byte)'O';
                        temp[2] = (byte)'R';
                        temp[3] = (byte)'M';
                        temp[4] = (byte)((buffer.Length >> 24) & 0xff);
                        temp[5] = (byte)((buffer.Length >> 16) & 0xff);
                        temp[6] = (byte)((buffer.Length >> 8) & 0xff);
                        temp[7] = (byte)((buffer.Length) & 0xff);

                        Array.Copy(buffer, 0, temp, 8, buffer.Length);

                        _blorb.Sounds[c.Number] = temp;
                    } else {
                        os_.fatal("Unhandled sound type in blorb file");
                    }
                } else {
                    os_.fatal("Unknown usage chunk in blorb file:" + c.Usage);
                }


            } else {
                switch (type) {
                    case "FORM":
                        String formType = ReadString();
                        handleForm(start, length);
                        break;
                    case "RIdx":
                        _stream.Position = start;
                        int numResources = ReadInt();
                        for (int i = 0; i < numResources; i++) {
                            Chunk c = new Chunk(ReadString(), ReadInt(), ReadInt());
                            _chunks.Add(c.Start, c);
                        }
                        break;
                    case "IFmd": // Metadata
                        _blorb.MetaData = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        if (_blorb.MetaData[0] != '<')
                        {
                            // TODO Make sure that this is being handled correctly
                            int index = _blorb.MetaData.IndexOf('<');
                            _blorb.MetaData = _blorb.MetaData.Substring(index);
                        }
                        break;
                    case "Loop":
                        break;
                    case "Fspc":
                        _stream.Position = start;
                        ReadInt();
                        break;
                    case "SNam": // TODO It seems that when it gets the story name, it is actually stored as 2 byte words,
                        // not one byte chars
                        _blorb.StoryName = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        break;
                    case "AUTH":
                    case "ANNO":
                    case "(c) ":
                        break;
                    case "APal":
                        int len = buffer.Length / 4;
                        for (int i = 0; i < len; i++)
                        {
                            int pos = i * 4;
                            byte a = buffer[pos + 0];
                            byte b = buffer[pos + 1];
                            byte c = buffer[pos + 2];
                            byte d = buffer[pos + 3];

                            UInt32 result = Frotz.Other.ZMath.MakeInt(a, b, c, d);

                            _blorb.AdaptivePalatte.Add( (int) result);
                        }
                        break;
                    case "IFhd":
                        _blorb.IFhd = buffer;
                        break;
                    case "RelN":
                        _blorb.ReleaseNumber = (buffer[0] << 8) + (buffer[1]);
                        break;
                    case "Reso":
                        _stream.Position = start;
                        int px = ReadInt();
                        int py = ReadInt();
                        int minx = ReadInt();
                        int miny = ReadInt();
                        int maxx = ReadInt();
                        int maxy = ReadInt();

                        _blorb.StandardSize = new ZSize(py, px);
                        _blorb.MinSize = new ZSize(miny, minx);
                        _blorb.MaxSize = new ZSize(maxy, maxx);

                        while (_stream.Position < start + length) {
                            int number = ReadInt();
                            int ratnum = ReadInt();
                            int ratden = ReadInt();
                            int minnum = ReadInt();
                            int minden = ReadInt();
                            int maxnum = ReadInt();
                            int maxden = ReadInt();

                            if (ratden != 0) _blorb.Pictures[number].StandardRatio = ratnum / ratden;
                            if (minden != 0) _blorb.Pictures[number].MinRatio = minnum / minden;
                            if (maxden != 0) _blorb.Pictures[number].MaxRatio = maxnum / maxden;
                        }
                        break;
                    case "Plte":
                        System.Diagnostics.Debug.WriteLine("Palatte");
                        break;
                    default:
                        Console.WriteLine("".PadRight(level) + ":" + "Type:" + type + ":" + length);
                        break;
                }
            }
            if (_stream.Position % 2 == 1) _stream.Position++;
        }

        internal static Blorb ReadBlorbFile(byte[] StoryData) {
            _blorb = new Blorb();
            _chunks = new Dictionary<int, Chunk>();
            _resources = new Dictionary<int, Resource>();

            // _stream = new FileStream(FileName, FileMode.Open);
            _stream = new MemoryStream(StoryData);

            String id = ReadString();
            if (id == "FORM") {
            } else {
                throw new Exception("Not a FORM");
            }

            int len = ReadInt();
            String type = ReadString();

            if (type != "IFRS") {
                throw new Exception("Not an IFRS FORM");
            }

            handleForm((int)_stream.Position - 4, len); // Backup over the Form ID so that handle form can read it


            if (_blorb.MetaData != null) {
                try
                {
                    //XDocument doc = XDocument.Parse(_blorb.MetaData);
                    //var r = doc.Root;

                    //XName n = XName.Get("title", r.Name.NamespaceName);
                    //var desc = doc.Descendants(n);
                    //foreach (var e in desc)
                    //{
                    //    _blorb.StoryName = e.Value;
                    //}
                    throw new Exception("Parsing blorb metadata not yet working");
                }
                catch (Exception ex)
                {
#if !SILVERLIGHT
                    //System.Windows.Forms.MessageBox.Show("Exception reading metadata:" + ex);
#endif
                    _blorb.MetaData = null;
                }
            }

            _stream.Close();

            return _blorb;
        }

        private static String ReadString() {
            byte[] buffer = new byte[4];
            _stream.Read(buffer, 0, 4);
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }

        private static int ReadInt() {
            byte[] buffer = new byte[4];
            _stream.Read(buffer, 0, 4);

            return ((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + (buffer[3]));
        }

        private static byte[] ReadBuffer(int length) {
            byte[] buffer = new byte[length];
            _stream.Read(buffer, 0, length);
            return buffer;
        }

    }
}

