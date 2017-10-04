using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    /// <summary>
    /// 
    /// </summary>
    //public class DstSrcInstructionData : Cosmos.Assembler.X86.IInstructionData
    //{
    //    /// <summary>
    //    /// Gets or sets the size.
    //    /// </summary>
    //    /// <value>The size.</value>
    //    public InstructionSize Size { get; set; }

    //    /// <summary>
    //    /// Gets or sets the target instruction set.
    //    /// </summary>
    //    /// <value>The target instruction set.</value>
    //    public InstructionSet InstructionSet { get; set; }

    //    #region Source options
    //    public RegistersEnum? SourceReg { get; set; }
    //    //public Cosmos.Assembler.ElementReference SourceRef { get; set; }
    //    public uint? SourceValue { get; set; }
    //    public bool SourceIsIndirect { get; set; }
    //    public int SourceDisplacement { get; set; }
    //    #endregion

    //    #region Destination options
    //    public RegistersEnum? DestinationReg { get; set; }
    //    //public Cosmos.Assembler.ElementReference DestinationRef { get; set; }
    //    public uint? DestinationValue { get; set; }
    //    public bool DestinationIsIndirect { get; set; }
    //    public int DestinationDisplacement { get; set; }
    //    #endregion

    //    public override string ToString( InstructionOutputFormat aFormat )
    //    {
    //        string Instruction = "";
    //        string tmp;
    //        switch( aFormat )
    //        {
    //            case InstructionOutputFormat.ASM:
    //                tmp = this.SizeToString();
    //                if( tmp == "" )
    //                    return Instruction;
    //                Instruction += tmp + " ";

                    
    //                tmp = this.GetDestinationAsString();
    //                if( tmp == "" )
    //                    return Instruction;
    //                Instruction += tmp + " ";

    //                tmp = this.GetSourceAsString();
    //                if( tmp == "" )
    //                    return Instruction;
    //                Instruction += ", " + tmp;

    //                return Instruction;
    //        }

    //        return "Format not supported.";
    //    }
    //}
}
