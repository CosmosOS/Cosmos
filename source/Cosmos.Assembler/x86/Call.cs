namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("call")]
    public class Call : JumpBase
    {
        public Call()
            : base("call")
        {
            mNear = false;
        }
    }
}