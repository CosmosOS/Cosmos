using System.Runtime.InteropServices;

namespace EfiSharp
{
    //TODO fix arrays and use for Data4
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EFI_GUID
    {
        private readonly uint Data1;
        private readonly ushort Data2;
        private readonly ushort Data3;
        private readonly byte Data41;
        private readonly byte Data42;
        private readonly byte Data43;
        private readonly byte Data44;
        private readonly byte Data45;
        private readonly byte Data46;
        private readonly byte Data47;
        private readonly byte Data48;

        public EFI_GUID(uint data1, ushort data2, ushort data3, byte data41, byte data42, byte data43, byte data44, byte data45, byte data46, byte data47, byte data48)
        {
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
            Data41 = data41;
            Data42 = data42;
            Data43 = data43;
            Data44 = data44;
            Data45 = data45;
            Data46 = data46;
            Data47 = data47;
            Data48 = data48;
        }

        public bool Equals(EFI_GUID other)
        {
            return Data1 == other.Data1 && Data2 == other.Data2 && Data3 == other.Data3 &&
                   Data41 == other.Data41 && Data42 == other.Data42 && Data43 == other.Data43 && Data44 == other.Data44 &&
                   Data45 == other.Data45 && Data46 == other.Data46 && Data47 == other.Data48 && Data44 == other.Data44;
        }
    }
}
