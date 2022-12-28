namespace ZLibrary.Machine
{
    public interface IZInput
    {
        bool HandleInputTimer(ushort aRoutine);
        ushort Read(ushort aBuffer, ushort aParse, ushort aTime, ushort aRoutine);
        short FilterInput(short aC);
    }
}