using System;
using Cosmos.Build.Windows;
using Cosmos.Kernel;

namespace TestSuite {
	class Program {
		#region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
			BuildUI.Run();
		}
		#endregion

		// Main entry point of the kernel
		public static void Init() {
			System.Diagnostics.Debugger.Break();
			Heap.CheckInit();

			Cosmos.Hardware.PC.Global.Init(true, true, true);

			Console.WriteLine("Init Keyboard");
			System.Diagnostics.Debugger.Break();
			Keyboard.Initialize();

            Cosmos.Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
            stages.Enqueue(new TestsStage());

            stages.Run();
            stages.Teardown();

            while (true)
				;
		}
	}
}