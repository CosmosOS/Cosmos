using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;

namespace Cosmos.Shell.Console {
	public class Program {

        #region Build Console
        // This contains code to launch the build console. Most users should not chagne this.
        [STAThread]
        public static void Main() {
			var xBuilder = new Builder();
			xBuilder.Build();
        }
        #endregion

        // Here is where your Cosmos code goes. This is the code that will be executed during Cosmos boot.
        // Write your code, and run. Cosmos build console will appear, select your target, and thats it!
        public static void Init() {
            Cosmos.Kernel.Boot.Default();

			Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
			stages.Enqueue(new Prompter());

			stages.Run();
			stages.Teardown();

            // Halt system.
            while (true) ;
		}
	}
}
