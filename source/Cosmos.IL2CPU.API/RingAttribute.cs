using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.IL2CPU.API {
    public enum Ring {
        Core = 0,
        HAL = 1,
        System = 2,
        User = 3
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class RingAttribute : Attribute {

        private readonly Ring mRing;

        public RingAttribute(Ring aRing) {
            mRing = aRing;
        }

        public Ring Ring {
            get {
                return mRing;
            }
        }
    }
}