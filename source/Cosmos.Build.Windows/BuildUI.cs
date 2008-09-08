using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Windows {
    public class BuildUI {
        public static void Run() {
            var xOptionsWindow = new OptionsWindow();
            xOptionsWindow.Display();
        }
    }
}
