using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;

namespace MatthijsTest {
	class Program {
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
            BuildUI.Run();
        }
		#endregion

		public static void Init() {
			DoTest();

			Console.WriteLine("Tests completed, Halting system now");
			while (true)
				;
		}

		private static void DoTest() {
			SortedList<string, string> xTest = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			xTest.Add("key1", "value1");
			xTest.Add("key2", "value2");
			Console.WriteLine("Done!");
		}
	}
}