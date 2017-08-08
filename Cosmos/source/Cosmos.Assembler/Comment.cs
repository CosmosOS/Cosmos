using System;
using System.Linq;

namespace Cosmos.Assembler
{
    public class Comment : Instruction
    {
        public readonly string Text;

        public Comment(Assembler aAssembler, string aText)
            : base() //HACK
        {
            if (aText.StartsWith(";"))
            {
                aText = aText.TrimStart(';').TrimStart();
            }
            Text = aText;
        }

        public Comment(string aText) : base(true)
        {
            if (aText.StartsWith(";"))
            {
                aText = aText.TrimStart(';').TrimStart();
            }
            Text = aText;
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write("; ");
            aOutput.Write(Text);
        }

        public override void UpdateAddress(Assembler aAssembler, ref ulong aAddress)
        {
            base.UpdateAddress(aAssembler, ref aAddress);
        }

        public override bool IsComplete(Assembler aAssembler)
        {
            return true;
        }
    }
}
