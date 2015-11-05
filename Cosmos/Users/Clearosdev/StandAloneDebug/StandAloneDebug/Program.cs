using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StandAloneDebug
{
    //this gets called form IL2CPU after its compiles the os and arguments are based on property file
    class Program
    {
        public static bool running = false;
        public static bool printtofile = false;
        public static types type;
        public static string output;
        
        public enum types
        {
            Vmware,
            Broche
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            if (args.Length < 2)
            {
                if (args[1] == "vmware")
                {
                    running = true;
                    type = types.Vmware;
                }
                else if (args[1] == "broche")
                {
                    running = true;
                    type = types.Broche;
                }
                else
                {
                    throw new ArgumentNullException("Not a correct argument");
                }
            }
 
            Debugger.Run();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            if (printtofile == true)
            {
                StreamWriter writer = new StreamWriter("debugoutput.txt");
                writer.Write(output);
                writer.Close();
            }
        }
    }
}
