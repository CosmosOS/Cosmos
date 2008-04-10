using System;
using Cosmos.Build.Windows;

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
            Cosmos.Kernel.Boot.Default();

            Cosmos.Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
            stages.Enqueue(new TestsStage());

            stages.Run();
            stages.Teardown();

            while (true)
				;
		}
	}
}