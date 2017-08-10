using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace zConsole
{
    public class Program
    {

        public static byte[] GameBytes;
        static void Main(string[] args)
        {
            //byte[] xBytes;
            //using (var xGameStr = typeof(Program).Assembly.GetManifestResourceStream(typeof(Program), "ZORK1.DAT"))
            //{
            //    xBytes = new byte[xGameStr.Length];
            //    xGameStr.Read(xBytes, 0, xBytes.Length);
            //}


            Frotz.os_.debug_mode = false;
            //if (File.Exists("debug.log"))
            //    File.Delete("debug.log");
            string fName = args[0];
            //while(!File.Exists(fName))
            //{
            //    Console.WriteLine("Please type a path to a storyfile:");
            //    fName = Console.ReadLine();
            //}
            var screen = new ZConsoleScreen(fName, GameBytes);
        }
    }
}
