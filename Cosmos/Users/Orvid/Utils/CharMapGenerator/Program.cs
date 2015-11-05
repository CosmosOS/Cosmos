using System;
using System.IO;
using System.Collections.Generic;

namespace CharMapGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader sr = new StreamReader("mappings");
            StreamWriter sw = new StreamWriter("Out.txt");

            string s;
            string[] s2;
            while (true)
            {
                s = sr.ReadLine();
                if (s == null)
                {
                    break;
                }
                if (!s.StartsWith("#"))
                {
                    s2 = s.Split(new char[] { '=' });
                    if (s2.Length != 2)
                    {
                        if (s2.Length == 3 && s2[1] == "" && s2[2] == "" && s2[0] == "equal")
                        {
                            s2 = new string[] { s2[0], "=" };
                        }
                        else
                        {
                            throw new Exception("Unable to split properly!");
                        }
                    }
                    if (s2[1] == "\"")
                    {
                        s2[1] = "\\\"";
                    }
                    sw.WriteLine("charMapper.Add(\"" + s2[0] + "\", \"" + s2[1] + "\");");
                }
                else
                {
                    continue;
                }
            }

            sr.Close();
            sr.Dispose();
            sw.Flush();
            sw.Close();
            sw.Dispose();

        }
    }
}
