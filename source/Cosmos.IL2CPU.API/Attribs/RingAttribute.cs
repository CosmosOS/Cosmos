using System;

namespace Cosmos.IL2CPU.API.Attribs {
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