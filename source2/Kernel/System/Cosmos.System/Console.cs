using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System {
    public class Console {
        // Dont expose these. See notes about mixing in .html
        protected int mX = 0;
        protected int mY = 0;

        protected Hardware.TextScreen mText = Hardware.Global.TextScreen;

        public void Clear() {
            mText.Clear();
            mX = 0;
            mY = 0;
            mText.SetCursorPos(mX, mY);
        }

        public void WriteLine(string aText) {
            
        }

        public void Write(string aText) {
        }

    }
}
