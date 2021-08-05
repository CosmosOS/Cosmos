using System.Runtime.InteropServices;

namespace EfiSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EFI_INPUT_KEY
    {
        private readonly ushort ScanCode;
        public readonly char UnicodeChar;

        public EFI_INPUT_KEY(char unicodeChar)
        {
            ScanCode = 0;
            UnicodeChar = unicodeChar;
        }
    }
}
