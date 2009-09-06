using System;
using System.IO;
using System.Linq;

namespace Cosmos.IL2CPU
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
            var xAttribs = GetType().GetCustomAttributes( typeof( OpCodeAttribute ), false );
            if( xAttribs != null && xAttribs.Length > 0 )
            {
                var xAttrib = ( OpCodeAttribute )xAttribs[ 0 ];
                mMnemonic = String.Intern( xAttrib.Mnemonic ); //Ben Trying it,
            }
            aAssembler.Add( this );
        }

        public Instruction( bool aAddToAssembler )
        {
            var xAttribs = GetType().GetCustomAttributes( typeof( OpCodeAttribute ), false );
            if( xAttribs != null && xAttribs.Length > 0 )
            {
                var xAttrib = ( OpCodeAttribute )xAttribs[ 0 ];
                mMnemonic = String.Intern( xAttrib.Mnemonic ); //Ben Trying it,
            }
            if( aAddToAssembler )
            {
                Assembler.CurrentInstance.Peek().Add( this );
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