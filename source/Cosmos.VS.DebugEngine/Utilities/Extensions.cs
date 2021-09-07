using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.Utilities
{
    public static class Extensions
    {
        public static string GetFullName(this MethodBase aMethod)
        {
            if (aMethod == null)
            {
                throw new ArgumentNullException(nameof(aMethod));
            }
            var xBuilder = new StringBuilder(256);
            var xParts = aMethod.ToString().Split(' ');
            var xParts2 = xParts.Skip(1).ToArray();
            var xMethodInfo = aMethod as System.Reflection.MethodInfo;
            if (xMethodInfo != null)
            {
                xBuilder.Append(GetFullName(xMethodInfo.ReturnType));
            }
            else
            {
                var xCtor = aMethod as ConstructorInfo;
                if (xCtor != null)
                {
                    xBuilder.Append(typeof(void).FullName);
                }
                else
                {
                    xBuilder.Append(xParts[0]);
                }
            }
            xBuilder.Append("  ");
            xBuilder.Append(GetFullName(aMethod.DeclaringType));
            xBuilder.Append(".");
            xBuilder.Append(aMethod.Name);
            if (aMethod.IsGenericMethod || aMethod.IsGenericMethodDefinition)
            {
                var xGenArgs = aMethod.GetGenericArguments();
                if (xGenArgs.Length > 0)
                {
                    xBuilder.Append("<");
                    for (int i = 0; i < xGenArgs.Length - 1; i++)
                    {
                        xBuilder.Append(GetFullName(xGenArgs[i]));
                        xBuilder.Append(", ");
                    }
                    xBuilder.Append(GetFullName(xGenArgs.Last()));
                    xBuilder.Append(">");
                }
            }
            xBuilder.Append("(");
            var xParams = aMethod.GetParameters();
            for (var i = 0; i < xParams.Length; i++)
            {
                if (i == 0 && xParams[i].Name == "aThis")
                {
                    continue;
                }
                xBuilder.Append(GetFullName(xParams[i].ParameterType));
                if (i < (xParams.Length - 1))
                {
                    xBuilder.Append(", ");
                }
            }
            xBuilder.Append(")");
            return String.Intern(xBuilder.ToString());
        }

        public static string GetFullName(this Type aType)
        {
            if (aType.IsGenericParameter)
            {
                return aType.FullName;
            }
            var xSB = new StringBuilder();
            if (aType.IsArray)
            {
                xSB.Append(GetFullName(aType.GetElementType()));
                xSB.Append("[");
                int xRank = aType.GetArrayRank();
                while (xRank > 1)
                {
                    xSB.Append(",");
                    xRank--;
                }
                xSB.Append("]");
                return xSB.ToString();
            }
            if (aType.IsByRef && aType.HasElementType)
            {
                return "&" + GetFullName(aType.GetElementType());
            }
            if (aType.IsGenericType && !aType.IsGenericTypeDefinition)
            {
                xSB.Append(GetFullName(aType.GetGenericTypeDefinition()));
            }
            else
            {
                xSB.Append(aType.FullName);
            }
            if (aType.IsGenericType)
            {
                xSB.Append("<");
                var xArgs = aType.GetGenericArguments();
                for (int i = 0; i < xArgs.Length - 1; i++)
                {
                    xSB.Append(GetFullName(xArgs[i]));
                    xSB.Append(", ");
                }
                xSB.Append(GetFullName(xArgs.Last()));
                xSB.Append(">");
            }
            //xSB.Append(", ");
            //xSB.Append(aType.Assembly.FullName);
            return xSB.ToString();
        }

        public static bool HasFlag(this enum_FRAMEINFO_FLAGS aThis, enum_FRAMEINFO_FLAGS aFlag)
        {
            return ((aThis & aFlag) == aFlag);
        }
    }
}
