using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    public class Interrupts {

        //TODO: Remove
		public static void DoTest() {
			Hardware.Interrupts.IncludeAllHandlers();
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			Console.WriteLine("Blaat");
			do {
				Console.Write("Please press a key: ");
				char c = Keyboard.ReadChar();
				ushort xValue = (ushort)c;
				DebugUtil.SendNumber("Kernel", "Character read, Unicode Value", xValue, 16);
				Console.Write(c);
				Console.WriteLine();
				if (c == 'A') {
					Console.WriteLine("Good Boy, you pressed uppercase A");
				} else {
					Console.WriteLine("Good Boy, you pressed lowercase a");
				}
			} while (true);
		}
    }
}
