using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Common
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class RingAttribute: Attribute
    {
        private readonly Ring mRing;

        public RingAttribute(Ring ring)
        {
            mRing = ring;
        }

        public Ring Ring
        {
            get
            {
                return mRing;
            }
        }
    }
}