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
            object x = new string('t', 45);
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
		}

		public static void ActualMain(string[] args) {
            string s = "Test";
			Console.WriteLine("Hello world!");
			Console.ReadLine();
		}
	}
}
