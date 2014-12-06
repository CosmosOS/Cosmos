﻿namespace Cosmos.Assembler
{
    public class DataIfNotDefined : DataMember, IIfNotDefined
    {
        public DataIfNotDefined(string aSymbol)
            : base("define", new byte[0])
        {
            Symbol = aSymbol;
        }

        public string Symbol
        {
            get;
            set;
        }

        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(this.GetAsText());
        }
    }
}