using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PlugViewer
{
    public static class NameBuilder
    {
        public static string BuildMethodName(MethodInfo m)
        {
            return m.ReturnType.Name + "\t\t" + BuildMethodDisplayName(m);
        }

        public static string BuildMethodDisplayName(MethodInfo m)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(m.Name);
            sb.Append("(");
            int cnt = 0;
            bool optional = false;
            string defValue = "";
            ParameterInfo[] prms = m.GetParameters();
            while (cnt < prms.Length)
            {
                if (cnt > 0)
                {
                    sb.Append(", ");
                }
                ParameterInfo pd = prms[cnt];
                foreach (object c in pd.GetCustomAttributes(true))
                {
                    if (c is OutAttribute)
                    {
                        sb.Append("out ");
                    }
                    else if (c is InAttribute)
                    {
                        sb.Append("in ");
                    }
                    else if (c is OptionalAttribute)
                    {
                        optional = true;
                    }
                    else if (c is DefaultParameterValueAttribute)
                    {
                        DefaultParameterValueAttribute d = (DefaultParameterValueAttribute)c;
                        String s = d.Value.ToString();
                        Object o = d.Value;
                        switch (s)
                        {
                            case "System.SByte":
                                defValue = ((SByte)o).ToString();
                                break;
                            case "System.Int16":
                                defValue = ((Int16)o).ToString();
                                break;
                            case "System.Int32":
                                defValue = ((Int32)o).ToString();
                                break;
                            case "System.Int64":
                                defValue = ((Int64)o).ToString();
                                break;
                            case "System.Byte":
                                defValue = ((Byte)o).ToString();
                                break;
                            case "System.UInt16":
                                defValue = ((UInt16)o).ToString();
                                break;
                            case "System.UInt32":
                                defValue = ((UInt32)o).ToString();
                                break;
                            case "System.UInt64":
                                defValue = ((UInt64)o).ToString();
                                break;
                            case "System.Decimal":
                                defValue = ((Decimal)o).ToString();
                                break;
                            case "System.Double":
                                defValue = ((Double)o).ToString();
                                break;
                            case "System.Single":
                                defValue = ((Single)o).ToString();
                                break;
                            case "System.String":
                                defValue = "\"" + ((String)o).ToString() + "\"";
                                break;
                            case "System.Char":
                                defValue = "'" + ((Char)o).ToString() + "'";
                                break;

                            default:
                                //if (c.ConstructorArguments[0].Type.Resolve().IsEnum)
                                //{

                                //}
                                defValue = "Default Value was of a custom type. Unable to serialize.";
                                break;
                        }
                    }
                }
                sb.Append(pd.ParameterType.Name);
                sb.Append(" ");
                sb.Append(pd.Name);
                if (optional)
                {
                    sb.Append(" = ");
                    sb.Append(defValue);
                    optional = false;
                }
                cnt++;
            }
            sb.Append(")");
            //throw new Exception();
            return sb.ToString();
        }

    }
}
