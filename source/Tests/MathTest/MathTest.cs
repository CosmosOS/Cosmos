using System;
using Cosmos.Build.Windows;

namespace MathTest
{
	class MathTest
	{
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args)
		{
			var xBuilder = new Builder();
			xBuilder.Build();
		}
		#endregion

		// Main entry point of the kernel
		public static void Init()
		{
			Cosmos.Kernel.Boot.Default();
			Console.WriteLine("Done booting");

			LongArithmetics.Test();

			while (true)
				;			
		}
	}
}
