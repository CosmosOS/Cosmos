using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;

namespace PlaygroundHAL
{
    public static class TestTextScreen
    {
        public static void DoIt()
        {
            var xD = new Debugger("", "");
            xD.SendChannelCommand(129, 0, new byte[]
                                          {
                                              65,
                                              66,
                                              67,
                                              68,
                                              69,
                                              13,
                                              10
                                          });
            xD.Send("AAAA");
        }
    }
}
