namespace ZLibrary.Machine
{
    public interface IZScreen
    {
        string ReadLine(string aInitial, int aTimeout, ushort aTimeoutCallback, byte[] aTerminatingKeys, out byte aTerminator);
        short ReadKey(int aTimeout, ushort aTimeoutCallback);
        void WriteChar(char aC);
        void WriteString(string aS);
        void ShowStatus();
    }
}
