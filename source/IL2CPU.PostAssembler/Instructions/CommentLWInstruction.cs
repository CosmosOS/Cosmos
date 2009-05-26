using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
    // we use a different one so we can identify Comments and Labels
    internal class CommentLWInstruction :LWInstruction
    {
        private string mComment;


        internal CommentLWInstruction(string comment)
        {
            mComment = string.Intern(comment);
        }


        public override LWInstructionType InstructionType
        {
            get { return LWInstructionType .Comment; }
        }



        public string Comment
        {
            get { return mComment; }
        }

        public override string ToString()
        {
            return Comment;
        }


    }
}
