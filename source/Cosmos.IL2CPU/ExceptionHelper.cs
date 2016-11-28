using System;
using FieldInfo = System.Reflection.FieldInfo;

namespace Cosmos.IL2CPU {
  public static class ExceptionHelper {
    public static Exception CurrentException;

    public static void ThrowNotImplemented(string aError) {
      Console.WriteLine(aError);
      throw new NotImplementedException(aError);
    }

	public static void ThrowOverflow() {
		string xError = "Arithmetic operation gets an overflow!";
		Console.WriteLine(xError);
		throw new OverflowException(xError);
	}
  }

  public static class ExceptionHelperRefs
  {
      public static readonly FieldInfo CurrentExceptionRef;

      static ExceptionHelperRefs() {
          CurrentExceptionRef = typeof(ExceptionHelper).GetField("CurrentException");
      }
  }
}
