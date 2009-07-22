using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cosmos.Compiler.ILScanner
{
    public static class Extensions
    {
        public static string GetFullName(this MethodBase aMethod)
        {
            if (aMethod == null)
            {
                throw new ArgumentNullException("aMethod");
            }
            var xBuilder = new StringBuilder(256);
            var xParts = aMethod.ToString().Split(' ');
            var xParts2 = xParts.Skip(1).ToArray();
            var xMethodInfo = aMethod as MethodInfo;
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
                if (xParams[i].Name == "aThis" && i == 0)
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

        private static string GetFullName(Type aType)
        {   
            if (aType == null)
            {
                return "***NULL TYPE***";
            }
            if (aType.IsGenericParameter)
            {
                return aType.Name;
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
            if (aType.IsGenericType)
            {
                xSB.Append(aType.GetGenericTypeDefinition().FullName);
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
                } if (xArgs.Length == 0) { Console.Write(""); }
                xSB.Append(GetFullName(xArgs.Last()));
                xSB.Append(">");
            }
            //xSB.Append(", ");
            //xSB.Append(aType.Assembly.FullName);
            return xSB.ToString();
        }
    }
}