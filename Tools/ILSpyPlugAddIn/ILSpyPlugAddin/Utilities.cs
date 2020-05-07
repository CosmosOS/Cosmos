using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpy;

namespace Cosmos.ILSpyPlugs.Plugin
{
    public static class Utilities
    {
        public static string GenerateTypePlugEntry(ITypeDefinition type)
        {
            var xSB = new StringBuilder();
            xSB.AppendFormat(
                type.Accessibility == Accessibility.Public
                    ? "[Plug(Target = typeof(global::{0}))]"
                    : "[Plug(TargetName = \"{0}, {1}\")]", Utilities.GetCSharpTypeName(type), type.ParentModule.AssemblyName);
            xSB.AppendLine();
            xSB.AppendFormat("public static class {0}Impl", type.Name);
            xSB.AppendLine();
            xSB.AppendLine("{");
            xSB.AppendLine("}");
            return xSB.ToString();
        }

        public static string GenerateMethodPlugEntry(IMethod method)
        {
            var xSB = new StringBuilder();

            xSB.Append($"public static {Utilities.GetCSharpTypeName(method.ReturnType)} {Utilities.GetMethodName(method)}(");
            var xAddComma = false;

            if (!method.IsStatic)
            {
                if (method.DeclaringType.IsReferenceType ?? false)
                {
                    xSB.Append("ref ");
                }
                if (method.DeclaringType.GetDefinition().Accessibility == Accessibility.Public)
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
                var xParameterTypeDef = xParameter.Type.GetDefinition();
                if (xParameterTypeDef != null
                    && xParameterTypeDef.Accessibility == Accessibility.Public)
                {
                    xSB.Append(Utilities.GetCSharpTypeName(xParameter.Type));
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

        public static string GenerateFieldAccessPlugEntry(IField field)
        {
            var xSB = new StringBuilder();
            xSB.Append($"[FieldAccess(Name = \"{field.Type.FullName} {field.DeclaringType.FullName}.{field.Name}\")] ref {Utilities.GetCSharpTypeName(field.Type)} field{field.Name}");
            return xSB.ToString();
        }

        public static string GeneratePropertyPlugEntry(IProperty property)
        {
            var xSB = new StringBuilder();
            if (property.Getter != null)
            {
                xSB.AppendLine(GenerateMethodPlugEntry(property.Getter));
                xSB.AppendLine();
            }
            if (property.Setter != null)
            {
                xSB.AppendLine(GenerateMethodPlugEntry(property.Setter));
                xSB.AppendLine();
            }
            return xSB.ToString();
        }

        private static string GetCSharpTypeName(IType type)
        {
            var xCSharp = Languages.GetLanguage("C#");

            return xCSharp.TypeToString(type, true);
        }

        private static string GetMethodName(IMethod method)
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
