using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;

namespace MatthijsTest
{
    class Program
    {
        #region Cosmos Builder logic
		// Most users wont touch this. This will call the Cosmos Build tool
		[STAThread]
		static void Main(string[] args) {
			var xBuilder = new Builder();
			xBuilder.Build();
		}
		#endregion

        public static void Init()
        {
            Cosmos.Kernel.Boot.Default();
            Cosmos.Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();

            // Put any further stages here.

            stages.Run();

            // Put your code here.

            stages.Teardown();
            while (true)
                ;
        }
    }
}
