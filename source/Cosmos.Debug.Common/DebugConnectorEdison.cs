using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Debug.Common
{
    public class DebugConnectorEdison : DebugConnectorSerial
    {
        public DebugConnectorEdison(string aPort) : base(aPort)
        {

        }
    }
}
