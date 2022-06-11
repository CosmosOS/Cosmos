namespace Cosmos.HAL.Drivers.PCI.Audio;

public class PCMStream
{
    private readonly char[] data;
    private readonly double freq;

    public PCMStream(double freq, char[] data)
    {
        this.freq = freq;
        this.data = data;
    }

    public char[] getData() => data;

    public double getFreq() => freq;
}
