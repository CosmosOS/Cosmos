using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.ELF {
    public abstract class ELFFileContentBase: BaseDataStructure {
        public File File {
            get;
            set;
        }

        protected string TryGetString(uint aStringTable, uint aString) {
            if(File == null || File.StringTables.Count == 0) {
                return "String(" + aStringTable + "," + aString + ")";
            }
            string xResult;
            return File.StringTables[(int)aStringTable].GetString(aString);
        }
    }
}