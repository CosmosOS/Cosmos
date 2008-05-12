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

        public static void GetResumeAndResume(ref uint aSuspend)
        {
            aSuspend = 0;
            throw new NotImplementedException();
        }
        public static int GetValue() {
            return 5; }

        public static void Init()
        {
            Console.Clear();
            Console.WriteLine("Kernel started!");
            ITest xTest = new TestImpl();
            xTest.DoMessage();
            //var xTest = new List<string>() { "String1", "String2", "String3", "String4" };
            //foreach (string xItem in xTest.ToArray()) { Console.WriteLine(xItem); }
            //Console.WriteLine();
            //Console.Write("Number: ");
            //Console.WriteLine(GetValue().ToString());
        }
    }

    public interface ITest {
        void DoMessage();}

    public class TestImpl : ITest {
        public void DoMessage() {
            Console.WriteLine("Message from interface member");
        }
    }
}