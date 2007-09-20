using System;
using System.Runtime.InteropServices;

namespace HelloWorld {
	public class TestObject {
		public uint theValue;
		private readonly string TheMessage;
		public TestObject(string aMessage) {
			TheMessage = aMessage;
		}

		public void IncrementValue() {
			IncrementValue(1);
		}

		public void IncrementValue(uint by) {
			theValue = theValue + by;
		}

		public void DoWrite() {
			IntPtr xHandle = GetStdHandle(-11);
			uint xCharsWritten;
			WriteConsole(xHandle, TheMessage, theValue, out xCharsWritten, IntPtr.Zero);
		}
		[DllImport("kernel32.dll")]
		static extern IntPtr GetStdHandle(int nStdHandle);
		[DllImport("kernel32.dll")]
		static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer,
		   uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten,
		   IntPtr lpReserved);
	}
	public class Program {
		public static void UseTestObject() {
			TestObject Hello = new TestObject("Hello, There!");
			//TestObject World = new TestObject("World Control!");
			Hello.IncrementValue(7);
			Hello.DoWrite();
//			Hello.IncrementValue(6);
//			Hello.DoWrite();
		}

		public static void CallInteger() {
			Integer();
		}

		public static void Integer() {
			int i = 22;
		}

		public static void StringViaCtor() {
			//object x = new string('t', 45);
		}

		public static void NewObject() {
			object x = new Object();
		}

		public static void LiteralString() {
			object x = "Hello, World!";
		}

		public static void EmptyMethod() {
		}

		public static void CallEmptyMethod() {
			EmptyMethod();
		}

		public static void Main() {
			CallEmptyMethod();
			CallInteger();
			Integer();
			StringViaCtor();
			LiteralString();
			NewObject();
			UseTestObject();
			DoWriteLines();
		}

		public static void DoWriteLines() {
			string s = "Test";
			//Console.WriteLine("Hello world!");
			//Console.ReadLine();
		}
	}
}
