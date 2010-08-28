using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.GDB {
    public class AsmFile {
        protected List<string> mLines = new List<string>();

        public AsmFile(string aPathname) {
            using (var xReader = new StreamReader(aPathname)) {
                while (!xReader.EndOfStream) {
                    mLines.Add(xReader.ReadLine());
                }
            }
        }
    }
}
