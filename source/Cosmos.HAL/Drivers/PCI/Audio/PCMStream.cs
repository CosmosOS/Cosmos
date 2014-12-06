namespace Cosmos.HAL.Drivers.PCI.Audio
{
    public class PCMStream
    {
        private double freq;
        private char[] data;

        public PCMStream(double freq, char[] data)
        {
            this.freq = freq;
            this.data = data;
        }

        public char[] getData()
        {
            return data;
        }

        public double getFreq()
        {
            return freq;
        }
    }
}