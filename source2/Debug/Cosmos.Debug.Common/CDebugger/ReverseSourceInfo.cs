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
            aAddress = UInt32.MaxValue;
            foreach (var xItem in mSourceInfos)
            {
                if (!documentName.Equals(xItem.Value.SourceFile, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                // todo: improve check to also consider column
                if (xItem.Value.Line == aLine)
                {
                    if (aAddress > xItem.Key)
                    {
                        aAddress = xItem.Key;
                    }
                }
            }
            return aAddress < UInt32.MaxValue;
        }
    }
}