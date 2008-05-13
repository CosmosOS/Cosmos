using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;
using System.Collections;

namespace MatthijsTest
{

    public class Program
    {
        #region Cosmos Builder logic

        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        private static void Main(string[] args)
        {
            BuildUI.Run();
        }

        #endregion

       

        public static void Init()
        {
            Console.Clear();
            Console.WriteLine("Kernel started!");
            Console.WriteLine("Starting doing tests");
            DoIt();
            Console.WriteLine("Done");
        }

        public class TestType { public void DoIt(object sender, EventArgs e) { Console.WriteLine("Writeline from an instance method!"); } }

        public static void DoIt()
        {
            EventHandler xEvent = WriteMessage1;
            var xType = new TestType();
            xEvent += xType.DoIt;
            xEvent += WriteMessage2;
            xEvent(null, null);
        }

        public static void WriteMessage1(object sender, EventArgs e) { Console.WriteLine("Message 1"); }
        public static void WriteMessage2(object sender, EventArgs e) { Console.WriteLine("Message 2"); }
    }
}
