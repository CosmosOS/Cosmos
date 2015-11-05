using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using EcmaCil.IL;

namespace EcmaCil
{
    public class Dump
    {
        private static void DumpType(TypeMeta item, XmlWriter output)
        {
            var xArrayType = item as ArrayTypeMeta;
            if (xArrayType != null)
            {
                DumpArrayType(xArrayType, output);
                return;
            }
            var xPointerType = item as PointerTypeMeta;
            if (xPointerType != null)
            {
                DumpPointerType(xPointerType, output);
                return;
            }
            if (item.GetType() == typeof(TypeMeta))
            {
                DumpTypeMeta(item, output);
                return;
            }
            throw new NotImplementedException("Type '" + item.GetType().FullName + "' not implemented");
        }

        private static void DumpPointerType(PointerTypeMeta xPointerType, XmlWriter output)
        {
            throw new NotImplementedException();
        }

        private static void DumpTypeMeta(TypeMeta item, XmlWriter output)
        {
            output.WriteStartElement("TypeMeta");
            {
#if DEBUG
                output.WriteAttributeString("MetaId", (string)item.Data[EcmaCil.DataIds.DebugMetaId]);
#endif
                //output.WriteAttributeString("FullName", type.FullName);
                //output.WriteAttributeString("BaseType", (type.BaseType != null ? "#" + type.BaseType.MetaId : ""));
#if DEBUG
                if (item.BaseType != null)
                {
                    output.WriteAttributeString("BaseType", (string)item.BaseType.Data[EcmaCil.DataIds.DebugMetaId]);
                }
                if (item.Descendants.Count > 0)
                {
                    output.WriteStartElement("Descendants");
                    {
                        foreach (var xDescendant in item.Descendants)
                        {
                            output.WriteStartElement("Descendant");
                            {
                                output.WriteAttributeString("Type", (string)xDescendant.Data[EcmaCil.DataIds.DebugMetaId]); 
                            }
                            output.WriteEndElement(); // Descendant
                        }
                    }
                    output.WriteEndElement(); // descendants
                }

#else
                if (item.BaseType != null)
                {
                    output.WriteAttributeString("HasBaseType", "true");
                }
#endif
                output.WriteStartElement("Methods");
                {
                    foreach (var xMethod in item.Methods)
                    {
                        DumpMethod(xMethod, output);
                    }
                }
                output.WriteEndElement();
            }
            output.WriteEndElement();
        }

        private static void DumpArrayType(ArrayTypeMeta aArrayType, XmlWriter output)
        {
            output.WriteStartElement("Array");
            {
                output.WriteAttributeString("Dimensions", aArrayType.Dimensions.ToString());
#if DEBUG
                output.WriteAttributeString("ElementType", (string)aArrayType.ElementType.Data[EcmaCil.DataIds.DebugMetaId]);
#endif
            }
            output.WriteEndElement();
        }

        private static void DumpMethod(MethodMeta method, XmlWriter output)
        {
            output.WriteStartElement("Method");
            {
#if DEBUG
                output.WriteAttributeString("MetaId", (string)method.Data[DataIds.DebugMetaId]);
#endif
                //output.WriteAttributeString("FullName", method.Name);
                output.WriteAttributeString("IsStatic", method.IsStatic.ToString());
                output.WriteAttributeString("IsVirtual", method.IsVirtual.ToString());
                output.WriteAttributeString("StartsNewVirtualTree", method.StartsNewVirtualTree.ToString());
#if DEBUG
                if (method.Overrides!=null)
                {
                    output.WriteAttributeString("Overrides", (string)method.Overrides.Data[DataIds.DebugMetaId]);
                }
                if (method.ReturnType != null)
                {
                    output.WriteAttributeString("ReturnType",  (string)method.ReturnType.Data[DataIds.DebugMetaId]);
                }
#endif
                output.WriteStartElement("Parameters");
                {
                    foreach (var xParam in method.Parameters)
                    {
                        DumpParameter(xParam, output);
                    }
                }
                output.WriteEndElement();
                if (method.Body != null)
                {
                    output.WriteStartElement("Body");
                    {
                        #region Locals
                        output.WriteAttributeString("InitLocals", method.Body.InitLocals.ToString());

                        output.WriteStartElement("Locals");
                        {
                            for (int i = 0; i < method.Body.LocalVariables.Length; i++)
                            {
                                output.WriteStartElement("Local");
                                {
                                    output.WriteAttributeString("Index", i.ToString());
#if DEBUG
                                    output.WriteAttributeString("Type", (string)method.Body.LocalVariables[i].Data[EcmaCil.DataIds.DebugMetaId]);
#endif
                                    //output.WriteAttributeString("IsPinned", method.Body.LocalVariables[i].IsPinned.ToString());
                                    //output.WriteAttributeString("LocalType", "#" + method.Body.LocalVariables[i].LocalType.MetaId);
                                }
                                output.WriteEndElement();
                            }
                        }
                        output.WriteEndElement();
                        #endregion
                        #region ExceptionHandlingClauses
                        output.WriteStartElement("ExceptionHandlingClauses");
                        {
                            if (method.Body.ExceptionHandlingClauses != null)
                            {
                                foreach (var xClause in method.Body.ExceptionHandlingClauses)
                                {
                                    output.WriteStartElement("Clause");
                                    {
                                        output.WriteAttributeString("Flags", xClause.Flags.ToString());
                                        output.WriteAttributeString("HandlerStart", xClause.HandlerStart.ToString());
                                        output.WriteAttributeString("HandlerEnd", xClause.HandlerEnd.ToString());
                                        output.WriteAttributeString("TryStart", xClause.TryStart.ToString());
                                        output.WriteAttributeString("TryEnd", xClause.TryEnd.ToString());
                                        output.WriteAttributeString("FilterStart", xClause.FilterStart.ToString());
                                        if (xClause.CatchType != null)
                                        {
                                            //                                        output.WriteAttributeString("CatchType", "#" + xClause.CatchType.MetaId);
                                            output.WriteAttributeString("HasCatchType", "true");
                                        }
                                    }
                                    output.WriteEndElement();
                                }
                            }
                        }
                        output.WriteEndElement();
                        #endregion
                        #region instructions
                        output.WriteStartElement("Instructions");
                        {
                            for (int i = 0; i < method.Body.Instructions.Length; i++)
                            {
                                var xInstruction = method.Body.Instructions[i];
                                output.WriteStartElement("Instruction");
                                {
                                    output.WriteAttributeString("Index", i.ToString());
                                    output.WriteAttributeString("Kind", xInstruction.InstructionKind.ToString());
                                    switch (xInstruction.GetType().Name)
                                    {
                                        case "InstructionNone":
                                            break;
                                        case "InstructionInt32":
                                            var xInstr32 = (InstructionInt32)xInstruction;
                                            output.WriteAttributeString("Value", xInstr32.Value.ToString());
                                            break;
                                        case "InstructionLocal":
                                            var xInstrLocal = (InstructionLocal)xInstruction;
                                            output.WriteAttributeString("LocalIndex", Array.IndexOf<LocalVariableMeta>(method.Body.LocalVariables, xInstrLocal.LocalVariable).ToString());
                                            break;
                                        case "InstructionMethod":
                                            var xInstrMethod = (InstructionMethod)xInstruction;
#if DEBUG
                                            output.WriteAttributeString("Method", (string)xInstrMethod.Value.Data[EcmaCil.DataIds.DebugMetaId]);
#endif
                                            //output.WriteAttributeString("Method", "#" + xInstrMethod.Value.MetaId);
                                            break;
                                        case "InstructionArgument":
                                            var xInstrArg = (InstructionArgument)xInstruction;
#if DEBUG
                                            output.WriteAttributeString("Argument", (string)xInstrArg.Argument.Data[EcmaCil.DataIds.DebugMetaId]);
#endif
                                            break;
                                        case "InstructionBranch":
                                            var xInstrBranch = (InstructionBranch)xInstruction;
                                            output.WriteAttributeString("TargetIdx", xInstrBranch.Target.InstructionIndex.ToString());
                                            break;
                                        case "InstructionType":
                                            var xInstrType = (InstructionType)xInstruction;
                                            //output.WriteAttributeString("Type", xInstrType.Type.MetaId);
                                            break;
                                        case "InstructionField":
                                            var xInstrField = (InstructionField)xInstruction;
                                            //output.WriteAttributeString("Field", xInstrField.Field.Name);
                                            break;
                                        case "InstructionString":
                                            var xInstrString = (InstructionString)xInstruction;
                                            output.WriteAttributeString("LiteralString", xInstrString.LiteralString);
                                            break;
                                        case "InstructionSwitch":
                                            break;
                                        case "InstructionToken":
                                            break;
                                        case "InstructionSingle":
                                            var xInstrSingle = (InstructionSingle)xInstruction;
                                            output.WriteAttributeString("Value", xInstrSingle.Value.ToString());
                                            break;
                                        case "InstructionDouble":
                                            var xInstrDouble = (InstructionDouble)xInstruction;
                                            output.WriteAttributeString("Value", xInstrDouble.Value.ToString());
                                            break;
                                        case "InstructionInt64":
                                            var xInstrInt64 = (InstructionInt64)xInstruction;
                                            output.WriteAttributeString("Value", xInstrInt64.Value.ToString());
                                            break;
                                        default:
                                            throw new NotImplementedException("Instruction kind not implemented: " + xInstruction.GetType().Name);
                                    }
                                }
                                output.WriteEndElement();
                            }
                        }
                        output.WriteEndElement();
                        #endregion
                    }
                    output.WriteEndElement();
                }
            }
            output.WriteEndElement();
        }

        private static void DumpParameter(MethodParameterMeta param, XmlWriter output)
        {
            output.WriteStartElement("Parameter");
            {
                if (param == null)
                {
                    Console.Write("");
                }
                output.WriteAttributeString("IsByRef", param.IsByRef.ToString());
#if DEBUG
                output.WriteAttributeString("MetaId", (string)param.Data[EcmaCil.DataIds.DebugMetaId]);
#endif
                //output.WriteAttributeString("Name", param.Name);
                //output.WriteAttributeString("IsPointer", param.IsPointer.ToString());
                //output.WriteAttributeString("ParameterType", "#" + param.PropertyType.MetaId);
            }
            output.WriteEndElement();
        }

        public static void DumpTypes(IEnumerable<TypeMeta> types, XmlWriter output)
        {
            output.WriteStartDocument();
            {
                output.WriteStartElement("Types");
                {
                    foreach (var xType in types)
                    {
                        DumpType(xType, output);
                    }
                }
                output.WriteEndElement();
            }
            output.WriteEndDocument();
        }

        #region instructions dumping
        #endregion
    }
}
