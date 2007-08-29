using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU;

namespace IL2CPU {
	class Program {
		static void Main(string[] args) {
			Engine e = new Engine();
			e.Execute("HelloWorld.exe");
			Console.ReadLine();
		}
	}
}
