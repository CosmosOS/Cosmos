using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.IL2CPU {
  public static class ExceptionHelper {
      public static Exception CurrentException;

    public static void ThrowNotImplemented(string aError) {
      Console.WriteLine(aError);
      throw new NotImplementedException(aError);
    }
  }

  public static class ExceptionHelperRefs
  {
      public static readonly FieldInfo CurrentExceptionRef;
      static ExceptionHelperRefs()
      {
          CurrentExceptionRef = typeof(ExceptionHelper).GetField("CurrentException");
      }
  }
}
