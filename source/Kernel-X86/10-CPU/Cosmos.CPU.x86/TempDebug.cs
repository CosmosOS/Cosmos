namespace Cosmos.CPU.x86;

public static class TempDebug
{
    private static unsafe byte* mPtr = (byte*)(0xB8000 - 1);

    public static void ShowText(char aChar)
    {
        unsafe
        {
            mPtr++;
            *mPtr = (byte)aChar;

            mPtr++;
            *mPtr = 0x0A;
        }
    }
}
