using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Compiler.Assembler;
using x86 = Cosmos.Compiler.Assembler.X86;

namespace Orvid
{
    public static class Optimizer
    {

        public static Assembler Optimize(Assembler asmb)
        {
            Assembler asmblr = asmb;
            List<Instruction> instr = asmb.Instructions;
            //List<DataMember> dmbrs = asmb.DataMembers;

            SortedDictionary<string, Instruction> labels = new SortedDictionary<string, Instruction>();
            List<Instruction> comments = new List<Instruction>();
            List<String> usedLabels = new List<string>();
            foreach (Instruction ins in instr)
            {
                if (ins is Label)
                {
                    labels.Add(((Label)ins).QualifiedName, ins);
                }
                else if (ins is x86.JumpToSegment)
                {
                    if (((x86.JumpToSegment)ins).DestinationRef != null)
                    {
                        usedLabels.Add(((x86.JumpToSegment)ins).DestinationRef.Name);
                    }
                    else
                    {
                        usedLabels.Add(((x86.JumpToSegment)ins).DestinationLabel);
                    }
                }
                else if (ins is x86.JumpBase)
                {
                    usedLabels.Add(((x86.JumpBase)ins).DestinationLabel);
                }
                else if (ins is x86.Call)
                {
                    usedLabels.Add(((x86.Call)ins).DestinationLabel);
                }
                else if (ins is x86.Push)
                {
                    if (((x86.Push)ins).DestinationRef != null)
                    {
                        usedLabels.Add(((x86.Push)ins).DestinationRef.Name);
                    }
                }
                else if (ins is x86.Move)
                {
                    if (((x86.Move)ins).SourceRef != null)
                    {
                        usedLabels.Add(((x86.Move)ins).SourceRef.Name);
                    }
                }
            }
            foreach (string s in usedLabels)
            {
                labels.Remove(s);
            }
            usedLabels = null;
            instr.RemoveAll(
                delegate(Instruction inst)
                {
                    if (inst is Comment)
                        return true;
                    else if (inst is Label)
                    {
                        if (labels.ContainsKey(((Label)inst).QualifiedName))
                            return true;
                        return false;
                    }
                    return false;
                }
            );
            labels = null;
            comments = null;




            asmblr.Instructions = instr;
            //asmblr.DataMembers = dmbrs;
            return asmblr;

        }

    }
}
