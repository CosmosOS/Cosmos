using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest
{
    public class Interpreter
    {
        public static List<String> GetParsed(string line)
        {
            List<String> retList = new List<String>();
            string lineOld = "";

            //Read command
            String command = ReadCommand(line);
            if (!String.IsNullOrEmpty(command))
            {
                retList.Add(command);
                line = line.Substring(command.Length); //remove the command
                line = line.Trim(); //remove any whitespace
            }

            //Read all arguments
            while (line.Trim().Length > 0)
            {
                String arg = ReadArgument(line);
                if (!String.IsNullOrEmpty(arg))
                {
                    retList.Add(arg);
                    line = line.Substring(arg.Length);
                    line = line.Trim();
                }
                else
                    break;
            }

            return retList;
        }

        private static string ReadCommand(string line)
        {
            //String.Contains() doesn't work yet, so check manually...
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x].Equals(' '))
                    return line.Substring(0, x);
            }

            return line;
        }

        private static string ReadArgument(string line)
        {
            //String.Contains() doesn't work yet, so check manually...
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x].Equals(' '))
                    return line.Substring(0, x);
            }

            return line;
        }
    }
}
