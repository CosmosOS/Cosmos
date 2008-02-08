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
			var xBuilder = new Builder();
			xBuilder.Build();
		}
		#endregion

		public static void Init() {
			DoTest();

			Console.WriteLine("Tests completed, Halting system now");
			while (true)
				;
		}

		private static void DoTest() {
			TestStruct xTest = new TestStruct(true);
			xTest.Value1 = 1;
			xTest.Value2 = 2;
			Console.WriteLine("Value1 = " + xTest.Value1);
			Console.WriteLine("Value2 = " + xTest.Value2);
			Test1(xTest);
			Test2(xTest);
			mValue = xTest;
			Test3();
			Test4();
			var x = new MyTestObj();
			x.mTest = xTest;
			x.Test5();
			x.Test6();
		}

		private static TestStruct mValue;

		private static void Test1(TestStruct aTest) {
			var xTest = aTest;
			Console.WriteLine("Value3 = " + xTest.Value1);
			Console.WriteLine("Value4 = " + xTest.Value2);
		}

		private static void Test2(TestStruct aTest2) {
			Console.WriteLine("Value5 = " + aTest2.Value1);
			Console.WriteLine("Value6 = " + aTest2.Value2);
		}

		private static void Test3() {
			Console.WriteLine("Value7 = " + mValue.Value1);
			Console.WriteLine("Value8 = " + mValue.Value2);
		}

		private static void Test4() {
			var xValue = mValue;
			Console.WriteLine("Value9 = " + xValue.Value1);
			Console.WriteLine("ValueA = " + xValue.Value2);
		}

		public class MyTestObj {
			public TestStruct mTest;
			
			public void Test5() {
				System.Diagnostics.Debugger.Break();
				Console.WriteLine("ValueB = " + mTest.Value1);
				Console.WriteLine("ValueC = " + mTest.Value2);
			}

			public void Test6() {
				var xValue = mTest;
				Console.WriteLine("ValueD = " + xValue.Value1);
				Console.WriteLine("ValueE = " + xValue.Value2);
			}
		}
	}

	public struct TestStruct {
		public uint Value1;
		public uint Value2;
		public TestStruct(bool aIsTest) {
			Value1 = 0;
			Value2 = 0;
		}
	}
}