using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU {
  public static class ExceptionHelper {
    public static void ThrowNotImplemented(string aError) {
      Console.WriteLine(aError);
      throw new NotImplementedException(aError);
    }
  }
}
