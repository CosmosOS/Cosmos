using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows.Controls;

namespace Cosmos.VS.Windows
{
    public class DebuggerUC : UserControl
    {
        protected byte[] mData = new byte[0];

        public CosmosWindowsPackage Package { get; set; }

        public virtual void Update(string aTag, byte[] aData)
        {
            mData = aData;
            DoUpdate(aTag);
        }

        protected virtual void DoUpdate(string aTag)
        {
        }

        public virtual byte[] GetCurrentState()
        {
            return mData;
        }
        public virtual void SetCurrentState(byte[] aData)
        {
            Update(null, aData);
        }
    }
}