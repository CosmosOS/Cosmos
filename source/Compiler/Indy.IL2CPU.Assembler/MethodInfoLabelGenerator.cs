using System.Linq;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;


namespace Indy.IL2CPU.Assembler
{
    public static class MethodInfoLabelGenerator
    {

        public static string GenerateLabelName(MethodBase aMethod)
        {
            string xResult = DataMember.FilterStringForIncorrectChars(GetFullName(aMethod));
            if (xResult.Length > 245)
            {
                using (var xHash = MD5.Create())
                {
                    xResult = xHash.ComputeHash(
                        Encoding.Default.GetBytes(xResult)).Aggregate("_", (r, x) => r + x.ToString("X2"));
                }
            }
            return xResult;
        }

        static string GetFullName(MethodBase aMethod)
        {
            var xBuilder = new StringBuilder();
            string[] xParts = aMethod.ToString().Split(' ');
            string[] xParts2 = xParts.Skip(1).ToArray();
            var xMethodInfo = aMethod as MethodInfo;
            if (xMethodInfo != null)
            {
                xBuilder.Append(xMethodInfo.ReturnType.FullName);
            }
            else
            {
                ConstructorInfo xCtor = aMethod as ConstructorInfo;
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
            xBuilder.Append(aMethod.DeclaringType.FullName);
            xBuilder.Append(".");
            xBuilder.Append(aMethod.Name);
            xBuilder.Append("(");
            ParameterInfo[] xParams = aMethod.GetParameters();
            for (int i = 0; i < xParams.Length; i++)
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

    }
}
