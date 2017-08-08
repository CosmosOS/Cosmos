using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.GDB {
    public class AsmFile {
        protected List<AsmLine> mLines = new List<AsmLine>();
		// <label, linenumber>
		protected Dictionary<string, int> mLabels = new Dictionary<string, int>();

        public AsmFile(string aPathname) {
            using (var xReader = new StreamReader(aPathname)) {
                while (!xReader.EndOfStream) {
					AsmLine line = new AsmLine(xReader.ReadLine());
					if (line.ToString().Length == 0)
						continue;
					if (line.IsLabel)
					{
						mLabels.Add(line.Label, mLines.Count);
					}
					mLines.Add(line);

                }
            }
        }

		public IList<AsmLine> Lines
		{
			get
			{
				return mLines;
			}
		}

		public int GetLineOfLabel(string label)
		{
			try
			{
				return mLabels[label];
			}
			catch
			{
				return -1;
			}
		}
    }
}
