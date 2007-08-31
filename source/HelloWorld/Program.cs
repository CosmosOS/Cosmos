using System;

namespace HelloWorld {
	public class Program {
        
        public static void Jump() {
            Integer();
        }

        public static void Integer() {
            int i = 22;
        }

        public static void NewObject() {
            object x = new string('t', 1);
        }

        public static void Main(string[] args) {
            string s = "Test";
			Console.WriteLine("Hello world!");
			Console.ReadLine();
		}
	}
}
