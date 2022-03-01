using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace Cosmos.VS.Windows
{
    public class DebuggerUC : UserControl
    {
        protected byte[] mData = new byte[0];

        public CosmosWindowsPackage Package { get; set; }

        public virtual async Task UpdateAsync(string aTag, byte[] aData)
        {
            mData = aData;
            await DoUpdateAsync(aTag);
        }

        protected virtual Task DoUpdateAsync(string aTag)
        {
            return Task.CompletedTask;
        }

        public virtual byte[] GetCurrentState()
        {
            return mData;
        }
        public virtual async Task SetCurrentStateAsync(byte[] aData)
        {
            await UpdateAsync(null, aData);
        }
    }
}
