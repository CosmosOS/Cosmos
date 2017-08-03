using System;

namespace Cosmos.IL2CPU.API.Attribs {
  // Leave Attribute suffix. Prevents conflict with enum above, and C# will auto add the Attribute suffix anyway so Ring can be used as attribute in usage.
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
  public class RingAttribute : Attribute {
    public enum RingEnum {
      Core = 0,
      HAL = 1,
      System = 2,
      User = 3
    }

    private readonly RingEnum _mRing;
    public RingEnum Ring {
      get {
        return _mRing;
      }
    }

    public RingAttribute(RingEnum aRing) {
      _mRing = aRing;
    }

  }
}