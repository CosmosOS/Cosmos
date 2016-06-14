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
        static void Main(string[] args)
        {
            Frotz.os_.debug_mode = false;
            if (File.Exists("debug.log"))
                File.Delete("debug.log");
            string fName = "";
            while(!File.Exists(fName))
            {
                Console.WriteLine("Please type a path to a storyfile:");
                fName = Console.ReadLine();
            }
            var screen = new ZConsoleScreen(fName);
        }
    }
}
