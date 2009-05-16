using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Builder
{
    public class DebugWindowController
    {
        private DebugWindow window;

        public SourceInfos mSourceMappings { get; set; } 
        public DebugConnector mDebugConnector {get; set; } 

        public void Show()
        {
            window = new DebugWindow(); 
                window.SetSourceInfoMap(mSourceMappings ,mDebugConnector);
            window.Show(); 

        }
    }
}
