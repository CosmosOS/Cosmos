using System;
using System.IO;
using System.Linq;

namespace Cosmos.Compiler.Assembler
{
    public abstract class Instruction : BaseAssemblerElement
    {
        protected string mMnemonic;

        public string Mnemonic
        {
            get { return mMnemonic; }
        }

        public override void WriteText( Assembler aAssembler, TextWriter aOutput )
        {
            aOutput.Write( mMnemonic );
        }

        public Instruction()
            : this( true )
        {
        }

        public Instruction( Assembler aAssembler )
        {
            aAssembler.Add( this );
        }

        public Instruction( bool aAddToAssembler )
        {
            if( aAddToAssembler )
            {
                Assembler.CurrentInstance.Add( this );
            }
        }

        public override ulong? ActualAddress
        {
            get
            {
                // TODO: for now, we dont have any data alignment
                return StartAddress;
            }
        }

        public override void UpdateAddress( Assembler aAssembler, ref ulong aAddress )
        {
            base.UpdateAddress( aAssembler, ref aAddress );
        }

        public override bool IsComplete( Assembler aAssembler )
        {
            throw new NotImplementedException( "Method not implemented for instruction " + this.GetType().FullName.Substring( typeof( Instruction ).Namespace.Length + 1 ) );
        }

        public override void WriteData( Assembler aAssembler, Stream aOutput )
        {
            throw new NotImplementedException( "Method not implemented for instruction " + this.GetType().FullName.Substring( typeof( Instruction ).Namespace.Length + 1 ) );
        }

        [Obsolete]
        public override byte[] GetData( Assembler aAssembler )
        {
            throw new NotImplementedException( "Method not implemented for instruction " + this.GetType().FullName.Substring( typeof( Instruction ).Namespace.Length + 1 ) );
        }
    }
}