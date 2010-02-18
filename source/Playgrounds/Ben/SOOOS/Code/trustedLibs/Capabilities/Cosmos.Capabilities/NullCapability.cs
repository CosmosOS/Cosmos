using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ben.Kernel.Capabilities
{
    public class NullCapability :ICapability
    {
        public NullCapability()
        {

        }

        #region ICapability Members

        public void Revoke()
        {
            throw new NotImplementedException();
        }

        public bool IsNull
        {
            get
            {
                return true; 
            }
        }

        #endregion
    }
}
