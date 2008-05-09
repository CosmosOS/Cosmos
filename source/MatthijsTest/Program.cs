using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;
using System.Collections;

namespace MatthijsTest {
   public class Program {
        #region Cosmos Builder logic

        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        private static void Main(string[] args) {
            BuildUI.Run();
        }

        #endregion

        public class MessageContainer : IEnumerable
        {
            public IEnumerator GetEnumerator() {
                yield return "String1";
                yield return "String2";
                yield return "String3";
                yield return "String4";
                yield return "String5";

            }
        }



        public static void GetResumeAndResume(ref uint aSuspend)
        {
            aSuspend = 0;
            throw new NotImplementedException();
        }

        public static void Init() {
            var xTest = (IEnumerable)new string[] { "String1", "String2", "String3" };
            foreach(string xItem in xTest){
                Console.WriteLine(xItem);                    
            }
        }
    }
}