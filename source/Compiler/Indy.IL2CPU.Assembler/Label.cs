using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Indy.IL2CPU.Assembler {
	public class Label: Instruction {
		public static string GetFullName(MethodBase aMethod) {
		    return MethodInfoLabelGenerator.GenerateLabelName(aMethod);
		}

        public static string FilterStringForIncorrectChars(string aName) {
            return String.Intern(DataMember.FilterStringForIncorrectChars(aName.Replace(".", "__DOT__")));
        }


        public Label(MethodBase aMethod)
            : this(MethodInfoLabelGenerator.GenerateLabelName(aMethod))
        {
        }

        public Label(string aName)
        {
            if (aName == "System_UInt32__Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject_System_UInt32_")
            {
                Console.Write("");
            }
            mName = aName;
            if (!aName.StartsWith("."))
            {
                LastFullLabel = aName;
                QualifiedName = aName;
            }
            else
            {
                QualifiedName = LastFullLabel + aName;
            }
        }



    

        public static string GetLabel(object aObject)
        {
            Label xLabel = aObject as Label;
            if (xLabel == null)
                return "";
            return xLabel.Name;
        }

        public static string LastFullLabel
        {
            get;
            set;
        }


        public string QualifiedName
        {
            get;
            private set;
        }


        public bool IsGlobal
        {
            get;
            set;
        }

        public string Name
        {
            get { return mName; }
        }

	    private string mName;

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            if (IsGlobal)
            {
                aOutput.Write("global ");
                aOutput.WriteLine(FilterStringForIncorrectChars(QualifiedName));
            }
            aOutput.Write(FilterStringForIncorrectChars(QualifiedName));
            aOutput.Write(":");
		}

        public override bool IsComplete(Assembler aAssembler)
        {
            return true;
        }

        public override void UpdateAddress(Assembler aAssembler, ref ulong aAddress)
        {
            base.UpdateAddress(aAssembler, ref aAddress);
        }


        public override void WriteData(Assembler aAssembler, System.IO.Stream aOutput)
        {
        }
    }
}
