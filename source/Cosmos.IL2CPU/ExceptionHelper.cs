using System;
using System.Reflection;

namespace Cosmos.IL2CPU
{
    public static class ExceptionHelper
    {
        public static Exception CurrentException;

        public static void ThrowArgumentOutOfRange(string aError)
        {
            Console.WriteLine(aError);
            throw new ArgumentOutOfRangeException(aError);
        }

        public static void ThrowInvalidOperation(string aError)
        {
            Console.WriteLine(aError);
            throw new InvalidOperationException(aError);
        }

        public static void ThrowNotImplemented(string aError)
        {
            Console.WriteLine(aError);
            throw new NotImplementedException(aError);
        }

        public static void ThrowOverflow()
        {
            string xError = "Arithmetic operation gets an overflow!";
            Console.WriteLine(xError);
            throw new OverflowException(xError);
        }
    }

    public static class ExceptionHelperRefs
    {
        public static readonly FieldInfo CurrentExceptionRef;

        static ExceptionHelperRefs()
        {
            CurrentExceptionRef = typeof(ExceptionHelper).GetTypeInfo().GetField("CurrentException");
        }
    }
}
