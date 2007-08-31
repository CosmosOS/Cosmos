using System;

namespace HelloWorld {
	public class Program {
        
        public static void Main() {
            Integer();
        }

        public static void Integer() {
            int i = 22;
        }

        public static void NewObject() {
            object x = new string('t', 1);
        }

        public static void ActualMain(string[] args) {
            string s = "Test";
			Console.WriteLine("Hello world!");
			Console.ReadLine();
		}
	}
}
