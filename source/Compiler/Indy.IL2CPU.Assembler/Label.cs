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

        public override string ToString()
        {
            if (IsGlobal)
            {
                return "global " + Name + "\r\n" + Name + ":";
            }
            else
            {
                return Name + ":";
            }
		}

		public static string GenerateLabelName(MethodBase aMethod) {
		    return MethodInfoLabelGenerator.GenerateLabelName(aMethod);
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
