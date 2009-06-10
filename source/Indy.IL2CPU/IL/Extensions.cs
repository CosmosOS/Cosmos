#define old
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Indy.IL2CPU.Assembler;

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
            var xResult = GenerateFullName(aMethod);
            if (xResult == "System.Int32  System.Array.IndexOf<>(T[], T)")
            {
                return GenerateFullName(aMethod);
            }
            return xResult;
        }
#endif

        private static string GetFullName(this Type aType)
        {
            if(aType.IsGenericParameter)
            {
                return aType.Name;
            }
            var xSB = new StringBuilder();
            if(aType.IsArray)
            {
                xSB.Append(aType.GetElementType().GetFullName());
                xSB.Append("[");
                int xRank = aType.GetArrayRank();
                while(xRank > 1)
                {
                    xSB.Append(",");
                    xRank--;
                }
                xSB.Append("]");
                return xSB.ToString();
            }
            if(aType.IsByRef && aType.HasElementType)
            {
                return "&" + aType.GetElementType().GetFullName();
            }
            if (aType.IsGenericType)
            {
                xSB.Append(aType.GetGenericTypeDefinition().FullName);
            }
            else
            {
                xSB.Append(aType.FullName);
            }
            if(aType.ContainsGenericParameters)
            {
                xSB.Append("<");
                var xArgs = aType.GetGenericArguments();
                for(int i = 0; i < xArgs.Length-1;i++)
                {
                    xSB.Append(GetFullName(xArgs[i]));
                    xSB.Append(", ");
                }      if(xArgs.Length==0){Console.Write("");}
                xSB.Append(GetFullName(xArgs.Last()));
                xSB.Append(">");
            }
            return xSB.ToString();
        }

        private static string GenerateFullName(MethodBase aMethod)
        {
            return Label.GetFullName(aMethod);
        }

        public static string GetFullName(this FieldInfo aField)
        {
            return aField.FieldType.GetFullName() + " " + aField.DeclaringType.GetFullName() + "." + aField.Name;
            //var xSB = new StringBuilder(aField.FieldType.FullName.Length + 1 + aField.DeclaringType.FullName.Length + 1 + aField.Name);
            //xSB.Append(aField.FieldType.FullName);
            //xSB.Append(" ");
            //xSB.Append(aField.DeclaringType.FullName);
            //xSB.Append(".");
            //xSB.Append(aField.Name);
            //return String.Intern(xSB.ToString());
        }
    }
}