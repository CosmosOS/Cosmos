using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cosmos.Assembler;

namespace Cosmos.IL2CPU {
  public static class Extensions {
    public static string GetFullName(this MethodBase aMethod) {
      var xResult = GenerateFullName(aMethod);
      return xResult;
    }

    private static string GetFullName(this Type aType) {
      if (aType.IsGenericParameter) {
        return aType.Name;
      }
      var xSB = new StringBuilder();
      if (aType.IsArray) {
        xSB.Append(aType.GetElementType().GetFullName());
        xSB.Append("[");
        int xRank = aType.GetArrayRank();
        while (xRank > 1) {
          xSB.Append(",");
          xRank--;
        }
        xSB.Append("]");
        return xSB.ToString();
      }
      if (aType.IsByRef && aType.HasElementType) {
        return "&" + aType.GetElementType().GetFullName();
      }
      if (aType.IsGenericType) {
        xSB.Append(aType.GetGenericTypeDefinition().FullName);
      } else {
        xSB.Append(aType.FullName);
      }
      if (aType.ContainsGenericParameters) {
        xSB.Append("<");
        var xArgs = aType.GetGenericArguments();
        for (int i = 0; i < xArgs.Length - 1; i++) {
          xSB.Append(GetFullName(xArgs[i]));
          xSB.Append(", ");
        } if (xArgs.Length == 0) { Console.Write(""); }
        xSB.Append(GetFullName(xArgs.Last()));
        xSB.Append(">");
      }
      return xSB.ToString();
    }

    private static string GenerateFullName(MethodBase aMethod) {
      return LabelName.Get(aMethod);
    }

    public static string GetFullName(this FieldInfo aField) {
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
