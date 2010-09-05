using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System {
    [Plug(Target = typeof(Int32))]
    public class Int32Impl {
        protected static readonly string Digits = "0123456789";

        // error during compile about "Plug needed."
       //     Error	11	Plug needed. System.RuntimeTypeHandle  System.Type.get_TypeHandle()
       //at Cosmos.IL2CPU.ILScanner.ScanMethod(MethodBase aMethod, Boolean aIsPlug) in M:\source\Cosmos\source2\IL2CPU\Cosmos.IL2CPU\ILScanner.cs:line 658
       //at Cosmos.IL2CPU.ILScanner.ScanQueue() in M:\source\Cosmos\source2\IL2CPU\Cosmos.IL2CPU\ILScanner.cs:line 774
       //at Cosmos.IL2CPU.ILScanner.Execute(MethodBase aStartMethod) in M:\source\Cosmos\source2\IL2CPU\Cosmos.IL2CPU\ILScanner.cs:line 279
       //at Cosmos.Build.MSBuild.IL2CPU.Execute() in M:\source\Cosmos\source2\Build\Cosmos.Build.MSBuild\IL2CPU.cs:line 250	C:\Program Files (x86)\MSBuild\Cosmos\Cosmos.targets	32	10	Guess (source2\Demos\Guess\Guess)

        // for instance ones still declare as static but add a aThis argument first of same type as target
        public static int Parse(string s) {
            int xResult = 0;
            for (int i = s.Length - 1; i >= 0; i--) {
                xResult = xResult * 10;
                int j = Digits.IndexOf(s[i]);
                if (j == -1) {
                    throw new Exception("Non numeric digit found in int.parse");
                }
                xResult = xResult + j;
            }
            return xResult;
        }
    }
}
