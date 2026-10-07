namespace BigGustave
{
    internal readonly struct HeaderValidationResult
    {
        public static readonly byte[] ExpectedHeader = {
            137,
            80,
            78,
            71,
            13,
            10,
            26,
            10
        };

        public int Byte1 { get; }

        public int Byte2 { get; }

        public int Byte3 { get; }

        public int Byte4 { get; }

        public int Byte5 { get; }

        public int Byte6 { get; }

        public int Byte7 { get; }

        public int Byte8 { get; }

        public bool IsValid { get; }

        public HeaderValidationResult(int byte1, int byte2, int byte3, int byte4, int byte5, int byte6, int byte7, int byte8)
        {
            Byte1 = byte1;
            Byte2 = byte2;
            Byte3 = byte3;
            Byte4 = byte4;
            Byte5 = byte5;
            Byte6 = byte6;
            Byte7 = byte7;
            Byte8 = byte8;
            IsValid = byte1 == ExpectedHeader[0] && byte2 == ExpectedHeader[1] && byte3 == ExpectedHeader[2]
                      && byte4 == ExpectedHeader[3] && byte5 == ExpectedHeader[4] && byte6 == ExpectedHeader[5]
                      && byte7 == ExpectedHeader[6] && byte8 == ExpectedHeader[7];
        }

        public override string ToString()
        {
            return $"{Byte1} {Byte2} {Byte3} {Byte4} {Byte5} {Byte6} {Byte7} {Byte8}";
        }
    }
}
