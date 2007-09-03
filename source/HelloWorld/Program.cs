using System;

namespace HelloWorld {
	public class Program {
        
        public static void CallInteger() {
            Integer();
        }

        public static void Integer() {
            int i = 22;
        }

        public static void Main() {
            object x = new string('t', 45);
        }

        public static void ActualMain(string[] args) {
            string s = "Test";
			Console.WriteLine("Hello world!");
			Console.ReadLine();
		}
	}
}
