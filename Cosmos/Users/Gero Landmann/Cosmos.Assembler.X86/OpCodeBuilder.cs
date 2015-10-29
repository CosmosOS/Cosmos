using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Cosmos.Assembler.X86;

namespace Cosmos.Assembler.X86.Generator
{

    public class OpCodeBuilder
    {
        [STAThread]
        static void Main( string[] args )
        {

        }


        public Operand[] GetOperands(int OpCount, ulong[] aData )
        { 
            Operand[] ops = new Operand[OpCount];
            Operand op;
            int x = 0;
            bool OpFinish = false;
            for( int i = 0; i < OpCount; i++ )
            {
                ops[ i ] = new Operand();
                op = ops[ i ];
                for(; x < aData.Length; x++ )
                {
                    switch( aData[ x ] )
                    {
                        case OpCodeData.MEMORY:
                            break;
                        case OpCodeData.REG8:
                            break;
                        case OpCodeData.REG16:
                            break;
                        case OpCodeData.REG32:
                            break;
                        case OpCodeData.REG64:
                            break;
                        case OpCodeData.FPUREG:
                            break;
                        case OpCodeData.XMMREG:
                            break;
                        case OpCodeData.YMMREG:
                            break;
                        case OpCodeData.IMMEDIATE:
                            break;
                        case OpCodeData.BITS8:
                            break;
                        case OpCodeData.BITS16:
                            break;
                        case OpCodeData.BITS32:
                            break;
                        case OpCodeData.BITS64:
                            break;
                        case OpCodeData.BITS80:
                            break;
                        case OpCodeData.BITS128:
                            break;
                        case OpCodeData.BITS256:
                            break;

                        

                    }

                    if( OpFinish )
                        break;
                }
            }

            return ops;
        }

        public string GetInstructionVariant( OpCodeData.itemplate node )
        {
            string code = "";
            code += "new InstructionVariant()" + Environment.NewLine;
            code += "{" + Environment.NewLine;
            


            code += "    " + Environment.NewLine;
            code += "}" + Environment.NewLine;

            return code;
        }

        public string GetInstruction( )
        {
            string code = "";
            code += "public void {0} : Instruction" + Environment.NewLine;
            code += "{" + Environment.NewLine;
            code += "    public {0}()" + Environment.NewLine;
            code += "    {" + Environment.NewLine;
            code += "        Mnemonic = \"{0}\"" + Environment.NewLine;
            code += "    }" + Environment.NewLine;
            code += Environment.NewLine;
            code += "    public override bool Initialize()" + Environment.NewLine;
            code += "    {" + Environment.NewLine;

            code += "       Variants = new List<InstructionVariant>()" + Environment.NewLine;


            code += "    }" + Environment.NewLine;
            code += "}" + Environment.NewLine;

            return code;        
        }


        /*
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;

            namespace Cosmos.Assembler.X86
            {
            
                [OpCode("dec")]
                public class Dec : InstructionWithDestinationAndSize 
                {
                      aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption 
                      {
                            OpCode = new byte[] { 0xFE },
                            NeedsModRMByte = true,
                            InitialModRMByteValue = 0xC8,
                            OperandSizeByte = 0,
                            ReverseRegisters=true,
                            DestinationRegAny = true
                      }); // reg
                }
	        }

         */


    }
}

