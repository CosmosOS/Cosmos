using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Samples.Debugging.CorSymbolStore;
using System.Diagnostics.SymbolStore;
using System.IO;

namespace Cosmos.Debug.Common.CDebugger
{
	public class ReverseSourceInfos {
        private readonly SourceInfos mSourceInfos;
        public ReverseSourceInfos(SourceInfos aSourceInfos)
        {
            mSourceInfos = aSourceInfos;            
        }

        public bool FindAddressForSourceLocation(string documentName, uint aLine, uint aColumn, out uint aAddress)
        {
            foreach (var xItem in mSourceInfos)
            {
                if (!documentName.Equals(xItem.Value.SourceFile, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                if (xItem.Value.Line == aLine)
                {
                    aAddress= xItem.Key;
                    return true;
                }
            }
            aAddress = 0;
            return false;
        }
    }
}