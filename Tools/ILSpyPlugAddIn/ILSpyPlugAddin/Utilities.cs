using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.ILSpy;
using Mono.Cecil;

namespace Cosmos.ILSpyPlugs.Plugin
{
    public static class Utilities
    {
        public static string GenerateTypePlugEntry(TypeDefinition type)
        {
            var xString = new StringBuilder();
            xString.AppendFormat(
                type.IsPublic
                    ? "[Plug(Target = typeof(global::{0}))]"
                    : "[Plug(TargetName = \"{0}, {1}\")]", Utilities.GetCSharpTypeName(type), type.Module.Assembly.Name);
            xString.AppendLine();
            xString.AppendFormat("public static class {0}Impl", type.Name);
            xString.AppendLine();
            xString.AppendLine("{");
            xString.AppendLine("}");
            return xString.ToString();
        }

        public static string GenerateMethodPlugEntry(MethodDefinition method)
        {
            var xSB = new StringBuilder();

            xSB.Append($"public static {Utilities.GetCSharpTypeName(method.ReturnType)} {Utilities.GetMethodName(method)}(");
            var xAddComma = false;

            if (!method.IsStatic)
            {
                if (method.DeclaringType.IsValueType)
                {
                    xSB.Append("ref ");
                }
                if (method.DeclaringType.IsPublic)
                {
                    xSB.Append(Utilities.GetCSharpTypeName(method.DeclaringType));
                }
                else
                {
                    xSB.Append("object");
                }
                xSB.Append(" ");
                xSB.Append("aThis");
                xAddComma = true;
            }

            foreach (var xParameter in method.Parameters)
            {
                if (xAddComma)
                {
                    xSB.Append(", ");
                }
                xAddComma = true;
                var xParameterTypeDef = xParameter.ParameterType as TypeDefinition;
                if (xParameterTypeDef != null
                    && xParameterTypeDef.IsPublic)
                {
                    xSB.Append(Utilities.GetCSharpTypeName(xParameter.ParameterType));
                }
                else
                {
                    xSB.Append("object");
                }
                xSB.Append(" ");
                xSB.Append(xParameter.Name);
            }

            xSB.AppendLine(")");
            xSB.AppendLine("{");
            xSB.AppendLine("}");
            return xSB.ToString();
        }

        public static string GenerateFieldAccessPlugEntry(FieldDefinition field)
        {
            StringBuilder xString = new StringBuilder();
            xString.Append($"[FieldAccess(Name = \"{field.FieldType.FullName} {field.DeclaringType.FullName}.{field.Name}\")] ref {Utilities.GetCSharpTypeName(field.FieldType)} field{field.Name}");
            return xString.ToString();
        }

        public static string GetCSharpTypeName(TypeReference reference)
        {
            var xCSharp = Languages.GetLanguage("C#");

            return xCSharp.TypeToString(reference, true);
        }

        public static string GetMethodName(MethodDefinition method)
        {
            if (method.IsConstructor)
            {
                if (method.IsStatic)
                {
                    return "Cctor";
                }
                else
                {
                    return "Ctor";
                }
            }
            return method.Name;
        }
    }
}
