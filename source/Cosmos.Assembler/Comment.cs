namespace Cosmos.Assembler
{
    public class Comment : Instruction
    {
        public readonly string Text;

        private char commentChar
        {
            get;
        }

        public Comment(Assembler aAssembler, string aText) : base() //HACK
        {
            commentChar = CompilerStyle.GetCommentChar(aAssembler.CompilerStyle);

            if (aText.StartsWith(commentChar.ToString()))
            {
                aText = aText.TrimStart(commentChar).TrimStart();
            }

            Text = string.Intern(aText);
        }

        public Comment(string aText) : base(true)
        {
            commentChar = CompilerStyle.GetCommentChar(Assembler.CurrentInstance.CompilerStyle);

            if (aText.StartsWith(commentChar.ToString()))
            {
                aText = aText.TrimStart(commentChar).TrimStart();
            }

            Text = string.Intern(aText);
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(commentChar + " ");
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
