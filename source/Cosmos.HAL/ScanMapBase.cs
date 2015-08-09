using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL
{
    public abstract class ScanMapBase
    {
        public abstract KeyEvent ConvertScanCode(byte scan, bool ctrl, bool shift, bool alt, bool num, bool caps, bool scroll);
    }
}
