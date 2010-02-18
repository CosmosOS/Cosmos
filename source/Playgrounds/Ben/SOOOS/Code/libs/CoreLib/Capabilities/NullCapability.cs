using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.Capabilities
{

    public class NullCapability :ICapability
    {
        public NullCapability() 
        {
        }

        #region ICapability Members

        public bool IsNull
        {
            get
            {
                return true; 
            }
        }

        #endregion

        public IRevokeCapability GetRevokeCapability()
        {
            throw new InvalidOperationException();
        }
    }
}
