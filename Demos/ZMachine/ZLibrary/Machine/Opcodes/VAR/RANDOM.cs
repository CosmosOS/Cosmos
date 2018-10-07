namespace ZLibrary.Machine.Opcodes.VAR
{
    public class RANDOM : Opcode
    {
        public RANDOM(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x07 random range -> (result)";
        }

        public override void Execute(ushort aRange, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            if ((short)aRange <= 0)
            {
                ZRandom.Seed(-(short)aRange);
                Store(0);

            }
            else
            {
                ushort result;

                if (ZRandom.interval != 0)
                {
                    result = (ushort)ZRandom.counter++;
                    if (ZRandom.counter == ZRandom.interval)
                    {
                        ZRandom.counter = 0;
                    }
                }
                else
                {
                    ZRandom.a = 0x015a4e35 * ZRandom.a + 1;
                    result = (ushort)((ZRandom.a >> 16) & 0x7fff);
                }

                Store((ushort)(result % aRange + 1));
            }
        }
    }
}
