namespace DuNodes.HAL.FileSystem.Base
{
    public class fsEntry
    {
        public byte User = (byte)6;
        public byte Group = (byte)4;
        public byte Global = (byte)4;
        public string Name;
        public byte Attributes;
        public int Pointer;
        public int Length;
        public string Owner;
        public string Time;
        public int Checksum;
    }
}
