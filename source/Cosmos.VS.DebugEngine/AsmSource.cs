using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cosmos.VS.DebugEngine
{
    public static class AsmSource
    {
        // Extract out the relevant lines from the .asm file.
        public static StringBuilder GetSourceForLabels(string aAsmFile, List<string> aLabels)
        {
            var xCode = new StringBuilder();

            // Find line in ASM that starts the code block.
            using (var xSR = new StreamReader(aAsmFile, Encoding.ASCII))
            {
                string xLine;
                while (true)
                {
                    xLine = xSR.ReadLine();
                    if (xLine == null)
                    {
                        break;
                    }

                    var xParts = xLine.Trim().Split(' ');
                    if (xParts.Length > 0 && xParts[0].EndsWith(":"))
                    {
                        if (aLabels.Contains(xParts[0]))
                        {
                            // Found the first match, break.
                            break;
                        }
                    }
                }
                var lineCount = 0;
                // Extract the pertinent lines
                while (xLine != null)
                {
                    if (lineCount > 100)
                    {
                        break;
                    }
                    var xParts = xLine.Trim().Split(' ');
                    if (xParts.Length > 0 && xParts[0].EndsWith(":"))
                    {
                        // Its a label, lets check it
                        if (xParts.Length > 1 && xParts[1] == ";Asm")
                        {
                            // ASM label
                            xCode.AppendLine(xLine);
                            lineCount ++;
                        }
                        else if (aLabels.Contains(xParts[0]))
                        { 
                            // Normal label
                            xCode.AppendLine(xLine);
                            lineCount ++;
                        }
                        else
                        {
                            // Label with unrecognized comment, or IL label that doesn't match.
                            // We are done.
                            break;
                        }
                    }
                    else
                    { // Not a label, just output it
                        xCode.AppendLine(xLine);
                    }
                    xLine = xSR.ReadLine();
                }
            }
            return xCode;
        }
    }
}