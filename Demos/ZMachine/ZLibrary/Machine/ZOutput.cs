namespace ZLibrary.Machine
{
    public class ZOutput : IZOutput
    {
        private readonly IZScreen _screen;

        public ZOutput(IZScreen aScreen)
        {
            _screen = aScreen;
        }

        public void PrintZSCII(short aChar)
        {
            if (aChar == 0)
            {
                return;
            }

            char ch = ZText.CharFromZSCII(aChar);
            _screen.WriteChar(ch);
        }

        public void PrintString(string aString)
        {
            _screen.WriteString(aString);
        }
    }
}
