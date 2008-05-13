using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [ManifestResourceStream(ResourceName="MatthijsTest.Test.txt")]
        private static readonly byte[] TheManifestResource;

        public static void Init()
        {
            Console.Clear();
            if (TheManifestResource == null) {
                Console.WriteLine("Field TheManifestResource is null!");
                return; }
            Console.Write("Length: ");
            Console.WriteLine(TheManifestResource.Length.ToString());
            int xLength = TheManifestResource.Length;
            if(xLength > 10) {
                Console.WriteLine("Too much data!");
                return;
            }
            for(int i = 0; i < xLength;i++) {
                Console.WriteLine(TheManifestResource[i].ToString()); 
            }
        }
    }
}
