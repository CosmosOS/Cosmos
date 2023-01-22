namespace ZLibrary.Machine
{
    public class ZMemory
    {
        public ZStack Stack { get; set; }
        public long PC { get; set; }
        public long StartPC { get; set; }

        private readonly byte[] _data;

        public ZMemory(byte[] aData)
        {
            _data = aData;
        }

        public void GetBytes(long aAddress, int aCount, out byte[] aResult)
        {
            aResult = new byte[aCount];
            for (int i = 0; i < aCount; i++)
            {
                aResult[i] = _data[aAddress + i];
            }
        }

        public void GetByte(long aAddress, out byte aResult)
        {
            aResult = _data[aAddress];
        }

        public void GetWord(long aAddress, out ushort aResult)
        {
            aResult = (ushort)((_data[aAddress] << 8) | _data[aAddress + 1]);
        }

        public void SetByte(long aAddress, byte aValue)
        {
            _data[aAddress] = aValue;
            ZDebug.Output($"ZMP: {aAddress} -> {aValue}");
        }

        public void SetWord(long aAddress, ushort aValue)
        {
            _data[aAddress] = (byte)(aValue >> 8);
            _data[aAddress + 1] = (byte)(aValue & 0xff);
            ZDebug.Output($"ZMP: {aAddress} -> {aValue}");
        }

        public void CodeByte(out byte aResult)
        {
            aResult = _data[PC++];
        }

        public void CodeWord(out ushort aResult)
        {
            aResult = (ushort)(_data[PC] << 8 | _data[PC + 1]);
            PC += 2;
        }

        public void Initialize(ushort aStartPC)
        {
            ZRandom.Seed(0);
            Stack = new ZStack(this);

            StartPC = aStartPC;
            PC = StartPC;

            //SetByte(HeaderOffset.CONFIG, ZMachine.Story.Header.config);
            //SetWord(HeaderOffset.FLAGS, ZMachine.Story.Header.flags);

            //if (ZMachine.Story.Header.Version >= FileVersion.V4)
            //{
            //    SetByte(HeaderOffset.INTERPRETER_NUMBER, ZMachine.Story.Header.interpreter_number);
            //    SetByte(HeaderOffset.INTERPRETER_VERSION, ZMachine.Story.Header.interpreter_version);
            //    SetByte(HeaderOffset.SCREEN_ROWS, ZMachine.Story.Header.screen_rows);
            //    SetByte(HeaderOffset.SCREEN_COLS, ZMachine.Story.Header.screen_cols);
            //}

            //ushort screenCols = ZMachine.Story.Header.screen_cols;
            //ushort screenRows = ZMachine.Story.Header.screen_rows;

            //if (ZMachine.Story.Header.Version >= FileVersion.V5)
            //{
            //    SetWord(HeaderOffset.SCREEN_WIDTH, screenCols);
            //    SetWord(HeaderOffset.SCREEN_HEIGHT, screenRows);
            //    SetByte(HeaderOffset.FONT_HEIGHT, 1);
            //    SetByte(HeaderOffset.FONT_WIDTH, 1);
            //    SetByte(HeaderOffset.DEFAULT_BACKGROUND, ZMachine.Story.Header.default_background);
            //    SetByte(HeaderOffset.DEFAULT_FOREGROUND, ZMachine.Story.Header.default_foreground);
            //}

            //if ((ZMachine.Story.Header.Version >= FileVersion.V3) && (ZMachine.Story.Header.user_name[0] != 0))
            //{
            //    for (int i = 0; i < 8; i++)
            //    {
            //        SetByte((HeaderOffset.USER_NAME + i), ZMachine.Story.Header.user_name[i]);
            //    }
            //}
            //SetByte(HeaderOffset.STANDARD_HIGH, ZMachine.Story.Header.standard_high);
            //SetByte(HeaderOffset.STANDARD_LOW, ZMachine.Story.Header.standard_low);
        }
    }
}
