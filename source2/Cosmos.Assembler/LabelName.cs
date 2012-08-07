using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Cosmos.Assembler {
  public static class LabelName {

    // Label bases can be up to 200 chars. If larger they will be shortened with an included hash.
    // This leaves up to 56 chars for suffix information.

    public static int LabelCount { get; private set; }
    // Max length of labels at 256. We use 220 here so that we still have room for suffixes
    // for IL positions, etc.
    const int MaxLengthWithoutSuffix = 200;

    public static string Get(MethodBase aMethod) {
      return Final(GenerateFullName(aMethod));
    }

    const string IllegalIdentifierChars = "&.,+$<>{}-`\'/\\ ()[]*!=";
    static string FilterStringForIncorrectChars(string aName) {
      // Comment DataMember.FilterStringForIncorrectChars
      string xTempResult = aName;
      foreach (char c in IllegalIdentifierChars) {
        //TODO Use empty, not _. We need shorter names, and _ can be used for explicit demarkation.
        // Need to add _ to illegal chars, and cant change currently as it goofs stuff up.
        xTempResult = xTempResult.Replace(c, '_');
      }
      return String.Intern(xTempResult);
    }

    public static string Final(string xName) {
      xName = FilterStringForIncorrectChars(xName);

      if (xName.Length > MaxLengthWithoutSuffix) {
        using (var xHash = MD5.Create()) {
          var xSB = new StringBuilder();
          foreach (var xByte in xHash.ComputeHash(Encoding.Default.GetBytes(xName))) {
            xSB.Append(xByte.ToString("X2"));
          }
          // Keep length max 200
          xName = xName.Substring(0, MaxLengthWithoutSuffix - xSB.Length) + xSB.ToString();
        }
      }

      LabelCount++;
      return xName;
    }

    public static string GetFullName(Type aType) {
      if (aType.IsGenericParameter) {
        return aType.FullName;
      }
      var xSB = new StringBuilder();
      if (aType.IsArray) {
        xSB.Append(GetFullName(aType.GetElementType()));
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
        return "&" + GetFullName(aType.GetElementType());
      }
      if (aType.IsGenericType && !aType.IsGenericTypeDefinition) {
        xSB.Append(GetFullName(aType.GetGenericTypeDefinition()));
      } else {
        xSB.Append(aType.FullName);
      }
      if (aType.IsGenericType) {
        xSB.Append("<");
        var xArgs = aType.GetGenericArguments();
        for (int i = 0; i < xArgs.Length - 1; i++) {
          xSB.Append(GetFullName(xArgs[i]));
          xSB.Append(", ");
        }
        xSB.Append(GetFullName(xArgs.Last()));
        xSB.Append(">");
      }
      return xSB.ToString();
    }

    public static string GenerateFullName(MethodBase aMethod) {
      if (aMethod == null) {
        throw new ArgumentNullException("aMethod");
      }
      var xBuilder = new StringBuilder(256);
      var xParts = aMethod.ToString().Split(' ');
      var xParts2 = xParts.Skip(1).ToArray();
      var xMethodInfo = aMethod as System.Reflection.MethodInfo;
      if (xMethodInfo != null) {
        xBuilder.Append(GetFullName(xMethodInfo.ReturnType));
      } else {
        var xCtor = aMethod as ConstructorInfo;
        if (xCtor != null) {
          xBuilder.Append(typeof(void).FullName);
        } else {
          xBuilder.Append(xParts[0]);
        }
      }
      xBuilder.Append("  ");
      if (aMethod.DeclaringType != null) {
        xBuilder.Append(GetFullName(aMethod.DeclaringType));
      } else {
        xBuilder.Append("dynamic_method");
      }
      xBuilder.Append(".");
      xBuilder.Append(aMethod.Name);
      if (aMethod.IsGenericMethod || aMethod.IsGenericMethodDefinition) {
        var xGenArgs = aMethod.GetGenericArguments();
        if (xGenArgs.Length > 0) {
          xBuilder.Append("<");
          for (int i = 0; i < xGenArgs.Length - 1; i++) {
            xBuilder.Append(GetFullName(xGenArgs[i]));
            xBuilder.Append(", ");
          }
          xBuilder.Append(GetFullName(xGenArgs.Last()));
          xBuilder.Append(">");
        }
      }
      xBuilder.Append("(");
      var xParams = aMethod.GetParameters();
      for (var i = 0; i < xParams.Length; i++) {
        if (xParams[i].Name == "aThis" && i == 0) {
          continue;
        }
        xBuilder.Append(GetFullName(xParams[i].ParameterType));
        if (i < (xParams.Length - 1)) {
          xBuilder.Append(", ");
        }
      }
      xBuilder.Append(")");
      return String.Intern(xBuilder.ToString());
    }

    public static string GetFullName(FieldInfo aField) {
      return GetFullName(aField.FieldType) + " " + GetFullName(aField.DeclaringType) + "." + aField.Name;
    }
  }
}
