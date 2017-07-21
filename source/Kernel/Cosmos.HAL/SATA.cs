using System;

namespace Cosmos.HAL
{
    public abstract class SATA {
        public abstract string GetSomething();

        // In final impl, this would be done better with a real registration list etc.
        static public Type mSataHalType;
        static public SATA New() {
            return (SATA)Activator.CreateInstance(mSataHalType);
        }

        public string GetSomethingNonPlatform() {
            return GetSomething() + " from Platform.SATA";
        }
    }
}
