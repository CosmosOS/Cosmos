#define old
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace System
{
    public static class Extensions
    {
#if !old
        private static readonly Dictionary<IntPtr, string> mMethod_FullNameCache =
            new Dictionary<IntPtr, string>();

        private static readonly ReaderWriterLockSlim mMethod_FullNameCache_Locker = new ReaderWriterLockSlim();

        public static string GetFullName(this MethodBase aMethod)
        {
            mMethod_FullNameCache_Locker.EnterReadLock();
            try
            {
                if (mMethod_FullNameCache.ContainsKey(aMethod.MethodHandle.Value))
                {
                    var xResult = mMethod_FullNameCache[aMethod.MethodHandle.Value];
                    if (aMethod.GetHashCode() == 2031104736)
                    {
                        ;
                    }
                    if(!xResult.Equals(GenerateFullName(aMethod)))
                    {
                        throw new Exception("");
                    }
                    return mMethod_FullNameCache[aMethod.MethodHandle.Value];
                }
            }
            finally
            {
                mMethod_FullNameCache_Locker.ExitReadLock();
            }
            mMethod_FullNameCache_Locker.EnterWriteLock();
            try
            {
                if (mMethod_FullNameCache.ContainsKey(aMethod.MethodHandle.Value))
                {
                    var xResult = mMethod_FullNameCache[aMethod.MethodHandle.Value];
                    if (!xResult.Equals(GenerateFullName(aMethod)))
                    {
                        throw new Exception("");
                    }
                    return mMethod_FullNameCache[aMethod.MethodHandle.Value];
                }
                var xName = GenerateFullName(aMethod);
                mMethod_FullNameCache.Add(aMethod.MethodHandle.Value, xName);
                return xName;
            }
            finally
            {
                mMethod_FullNameCache_Locker.ExitWriteLock();
            }

        }
#else
        public static string GetFullName(this MethodBase aMethod)
        {
            return GenerateFullName(aMethod);
        }
#endif

        private static string GenerateFullName(MethodBase aMethod)
        {
            var xBuilder = new StringBuilder();
            var xParts = aMethod.ToString().Split(' ');
            var xParts2 = xParts.Skip(1).ToArray();
            var xMethodInfo = aMethod as MethodInfo;
            if (xMethodInfo != null)
            {
                xBuilder.Append(xMethodInfo.ReturnType.FullName);
            }
            else
            {
                var xCtor = aMethod as ConstructorInfo;
                if (xCtor != null)
                {
                    xBuilder.Append(typeof (void).FullName);
                }
                else
                {
                    xBuilder.Append(xParts[0]);
                }
            }
            xBuilder.Append("  ");
            xBuilder.Append(aMethod.DeclaringType.FullName);
            xBuilder.Append(".");
            xBuilder.Append(aMethod.Name);
            xBuilder.Append("(");
            var xParams = aMethod.GetParameters();
            for (var i = 0; i < xParams.Length; i++)
            {
                if (xParams[i].Name == "aThis" && i == 0)
                {
                    continue;
                }
                xBuilder.Append(xParams[i].ParameterType.FullName);
                if (i < (xParams.Length - 1))
                {
                    xBuilder.Append(", ");
                }
            }
            xBuilder.Append(")");
            return xBuilder.ToString();
        }

        public static string GetFullName(this FieldInfo aField)
        {
            return aField.FieldType.FullName + " " + aField.DeclaringType.FullName + "." + aField.Name;
        }
    }
}