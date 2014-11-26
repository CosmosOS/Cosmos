using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.IL2CPU.CustomImplementation.System.Text
{           
    // no need for plug, after 64 bit support  
	//[Plug(Target = typeof(StringBuilder))]
	//public static class StringBuilderImpl
	//{
	//    public static string ToString(StringBuilder aThis)
	//    {
	//        var xResult = new char[aThis.Length];
	//        int xLength = aThis.Length;
	//        for (int i = 0; i < xLength; i++)
	//        {
	//            xResult[i] = aThis[i];
	//        }
	//        var xResultStr = new String(xResult);
	//        return xResultStr;
	//    }
	//}
}
