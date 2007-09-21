using System;
using System.Runtime.InteropServices;

namespace HelloWorld {
	public class TestObject {
		public uint theValue;
		public TestObject() {
		}

		public void IncrementValue() {
			IncrementValue(1);
		}

		public void IncrementValue(uint by) {
			theValue = theValue + by;
		}

		public uint Value {
			get {
				return theValue;
			}
		}
//		[DllImport("kernel32.dll")]
//		static extern IntPtr GetStdHandle(int nStdHandle);
//		[DllImport("kernel32.dll")]
//		static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer,
//		   uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten,
//		   IntPtr lpReserved);
	}
	public class Program {
		public static void UseTestObject() {
			TestObject Hello = new TestObject();
			TestObject World = new TestObject();
			Hello.IncrementValue(7);
			World.IncrementValue(5);
			uint theValue = Hello.Value + World.Value;
			//World.IncrementValue(6);
			//World.DoWrite();
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
