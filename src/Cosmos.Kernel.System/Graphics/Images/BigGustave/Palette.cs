namespace BigGustave
{
    internal class Palette
    {
        public bool HasAlphaValues { get; private set; }

        public byte[] Data { get; }

        /// <summary>
        /// Creates a palette object. Input palette data length from PLTE chunk must be a multiple of 3.
        /// </summary>
        public Palette(byte[] data)
        {
            Data = new byte[data.Length * 4 / 3];
            var dataIndex = 0;
            for (var i = 0; i < data.Length; i += 3)
            {
                Data[dataIndex++] = data[i];
                Data[dataIndex++] = data[i + 1];
                Data[dataIndex++] = data[i + 2];
                Data[dataIndex++] = 255;
            }
        }

        /// <summary>
        /// Adds transparency values from tRNS chunk.
        /// </summary>
        public void SetAlphaValues(byte[] bytes)
        {
            HasAlphaValues = true;

            for (var i = 0; i < bytes.Length; i++)
            {
                Data[i * 4 + 3] = bytes[i];
            }
        }

        public Pixel GetPixel(int index)
        {
            var start = index * 4;
            return new Pixel(Data[start], Data[start + 1], Data[start + 2], Data[start + 3], false);
        }
    }
}
