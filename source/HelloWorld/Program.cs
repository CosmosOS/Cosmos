using System;

namespace HelloWorld {
	public class Program {
        
        public static void CallInteger() {
            Integer();
        }

        public static void Integer() {
            int i = 22;
        }

        public static void StringViaCtor() {
            //object x = new string('t', 45);
        }

		public static void Main() {
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

		public static void OrigMain() {
			CallEmptyMethod();
			CallInteger();
			Integer();
			StringViaCtor();
			LiteralString();
			Main();
			DoWriteLines();
		}

		public static void DoWriteLines() {
            string s = "Test";
			//Console.WriteLine("Hello world!");
			//Console.ReadLine();
		}
	}
}
