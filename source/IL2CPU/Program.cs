using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU;

namespace IL2CPU {
	public class Program {
		public static void Main(string[] args) {
			try
			{
				Engine e = new Engine();
				e.Execute("HelloWorld.exe");
			} catch (Exception E) {
				Console.WriteLine(E.ToString());
			}
			Console.ReadLine();
		}
	}
}
