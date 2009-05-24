using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL;
using System.Reflection;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Plugs;
using System.Runtime.InteropServices;

namespace Indy.IL2CPU.Compiler
{
    partial class AssemblyCompiler: IMetaDataInfoService
    {
        public MethodInformation GetMethodInfo(MethodBase aCurrentMethodForArguments,
                                             MethodBase aCurrentMethodForLocals,
                                             string aMethodName,
                                             TypeInformation aTypeInfo,
                                             bool aDebugMode)
        {
            return GetMethodInfo(aCurrentMethodForArguments, aCurrentMethodForLocals, aMethodName, aTypeInfo, aDebugMode, null);
        }

        public MethodInformation GetMethodInfo(MethodBase aMethod, bool aDebugMode)
        {
            return GetMethodInfo(aMethod, aMethod, Label.GenerateLabelName(aMethod), GetTypeInfo(aMethod.DeclaringType), aDebugMode);
        }

        public MethodInformation GetMethodInfo(MethodBase aCurrentMethodForArguments,
                                              MethodBase aCurrentMethodForLocals,
                                              string aMethodName,
                                              TypeInformation aTypeInfo,
                                              bool aDebugMode,
                                              IDictionary<string, object> aMethodData)
        {
            MethodInformation xMethodInfo;
            {
                MethodInformation.Variable[] xVars = new MethodInformation.Variable[0];
                int xCurOffset = 0;
                // todo:implement check for body
                //if (aCurrentMethodForLocals.HasBody) {
                MethodBody xBody = aCurrentMethodForLocals.GetMethodBody();
                if (xBody != null)
                {
                    xVars = new MethodInformation.Variable[xBody.LocalVariables.Count];
                    foreach (LocalVariableInfo xVarDef in xBody.LocalVariables)
                    {
                        int xVarSize = (int)GetFieldStorageSize(xVarDef.LocalType);
                        if ((xVarSize % 4) != 0)
                        {
                            xVarSize += 4 - (xVarSize % 4);
                        }
                        xVars[xVarDef.LocalIndex] = new MethodInformation.Variable(xCurOffset,
                                                                                   xVarSize,
                                                                                   !xVarDef.LocalType.IsValueType,
                                                                                   xVarDef.LocalType);
                        // todo: implement support for generic parameters?
                        //if (!(xVarDef.VariableType is GenericParameter)) {
                        //RegisterType(xVarDef.LocalType);
                        //}
                        xCurOffset += xVarSize;
                    }
                }
                MethodInformation.Argument[] xArgs;
                if (!aCurrentMethodForArguments.IsStatic)
                {
                    ParameterInfo[] xParameters = aCurrentMethodForArguments.GetParameters();
                    xArgs = new MethodInformation.Argument[xParameters.Length + 1];
                    xCurOffset = 0;
                    uint xArgSize;
                    for (int i = xArgs.Length - 1; i > 0; i--)
                    {
                        ParameterInfo xParamDef = xParameters[i - 1];
                        xArgSize = GetFieldStorageSize(xParamDef.ParameterType);
                        if ((xArgSize % 4) != 0)
                        {
                            xArgSize += 4 - (xArgSize % 4);
                        }
                        MethodInformation.Argument.KindEnum xKind = MethodInformation.Argument.KindEnum.In;
                        if (xParamDef.IsOut)
                        {
                            if (xParamDef.IsIn)
                            {
                                xKind = MethodInformation.Argument.KindEnum.ByRef;
                            }
                            else
                            {
                                xKind = MethodInformation.Argument.KindEnum.Out;
                            }
                        }
                        xArgs[i] = new MethodInformation.Argument(xArgSize,
                                                                  xCurOffset,
                                                                  xKind,
                                                                  !xParamDef.ParameterType.IsValueType,
                                                                  GetTypeInfo(xParamDef.ParameterType),
                                                                  xParamDef.ParameterType);
                        xCurOffset += (int)xArgSize;
                    }
                    xArgSize = 4;
                    // this
                    xArgs[0] = new MethodInformation.Argument(xArgSize,
                                                              xCurOffset,
                                                              MethodInformation.Argument.KindEnum.In,
                                                              !aCurrentMethodForArguments.DeclaringType.IsValueType,
                                                              GetTypeInfo(aCurrentMethodForArguments.DeclaringType),
                                                              aCurrentMethodForArguments.DeclaringType);
                }
                else
                {
                    ParameterInfo[] xParameters = aCurrentMethodForArguments.GetParameters();
                    xArgs = new MethodInformation.Argument[xParameters.Length];
                    xCurOffset = 0;
                    for (int i = xArgs.Length - 1; i >= 0; i--)
                    {
                        ParameterInfo xParamDef = xParameters[i]; //xArgs.Length - i - 1];
                        uint xArgSize = GetFieldStorageSize(xParamDef.ParameterType);
                        if ((xArgSize % 4) != 0)
                        {
                            xArgSize += 4 - (xArgSize % 4);
                        }
                        MethodInformation.Argument.KindEnum xKind = MethodInformation.Argument.KindEnum.In;
                        if (xParamDef.IsOut)
                        {
                            if (xParamDef.IsIn)
                            {
                                xKind = MethodInformation.Argument.KindEnum.ByRef;
                            }
                            else
                            {
                                xKind = MethodInformation.Argument.KindEnum.Out;
                            }
                        }
                        xArgs[i] = new MethodInformation.Argument(xArgSize,
                                                                  xCurOffset,
                                                                  xKind,
                                                                  !xParamDef.ParameterType.IsValueType,
                                                                  GetTypeInfo(xParamDef.ParameterType),
                                                                  xParamDef.ParameterType);
                        xCurOffset += (int)xArgSize;
                    }
                }
                int xResultSize = 0;// GetFieldStorageSize(aCurrentMethodForArguments.ReturnType.ReturnType);
                MethodInfo xMethInfo = aCurrentMethodForArguments as MethodInfo;
                Type xReturnType = typeof(void);
                if (xMethInfo != null)
                {
                    xResultSize = (int)GetFieldStorageSize(xMethInfo.ReturnType);
                    xReturnType = xMethInfo.ReturnType;
                }
                xMethodInfo = new MethodInformation(aMethodName,
                                                    xVars,
                                                    xArgs,
                                                    (uint)xResultSize,
                                                    !aCurrentMethodForArguments.IsStatic,
                                                    aTypeInfo,
                                                    aCurrentMethodForArguments,
                                                    xReturnType,
                                                    aDebugMode,
                                                    aMethodData);
            }
            return xMethodInfo;
        }


        public void GetTypeFieldInfoImpl(List<KeyValuePair<string, TypeInformation.Field>> aTypeFields,
                                                 Type aType,
                                                 ref uint aObjectStorageSize)
        {
            if (aType == null)
            {
                throw new ArgumentNullException("aType");
            }
            Type xActualType = aType;
            Dictionary<string, PlugFieldAttribute> xCurrentPlugFieldList = new Dictionary<string, PlugFieldAttribute>();
            do
            {
                if (mPlugFields.ContainsKey(aType))
                {
                    var xOrigList = mPlugFields[aType];
                    foreach (var item in xOrigList)
                    {
                        xCurrentPlugFieldList.Add(item.Key,
                                                  item.Value);
                    }
                }
                foreach (FieldInfo xField in aType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (xField.IsStatic)
                    {
                        continue;
                    }
                    if (xField.DeclaringType != aType)
                    {
                        // somehow this gives other results than including DeclaredOnly
                        continue;
                    }
                    //if (xField.HasConstant) {
                    //    Console.WriteLine("Field is constant: " + xField.GetFullName());
                    //}
                    // todo: add support for constants?
                    PlugFieldAttribute xPlugFieldAttr = null;
                    if (xCurrentPlugFieldList.ContainsKey(xField.GetFullName()))
                    {
                        xPlugFieldAttr = xCurrentPlugFieldList[xField.GetFullName()];
                        xCurrentPlugFieldList.Remove(xField.GetFullName());
                    }
                    Type xFieldType = null;
                    int xFieldSize;
                    string xFieldId;
                    if (xPlugFieldAttr != null)
                    {
                        xFieldType = xPlugFieldAttr.FieldType;
                        xFieldId = xPlugFieldAttr.FieldId;
                    }
                    else
                    {
                        xFieldId = xField.GetFullName();
                    }
                    if (xFieldType == null)
                    {
                        xFieldType = xField.FieldType;
                    }
                    //if ((!xFieldType.IsValueType && aGCObjects && xFieldType.IsClass) || (xPlugFieldAttr != null && xPlugFieldAttr.IsExternalValue && aGCObjects)) {
                    //    continue;
                    //}
                    if ((xFieldType.IsClass && !xFieldType.IsValueType) || (xPlugFieldAttr != null && xPlugFieldAttr.IsExternalValue))
                    {
                        xFieldSize = 4;
                    }
                    else
                    {
                        xFieldSize = (int)GetFieldStorageSize(xFieldType);
                    }
                    //}
                    if ((from item in aTypeFields
                         where item.Key == xFieldId
                         select item).Count() > 0)
                    {
                        continue;
                    }
                    int xOffset = (int)aObjectStorageSize;
                    FieldOffsetAttribute xOffsetAttrib = xField.GetCustomAttributes(typeof(FieldOffsetAttribute),
                                                                                    true).FirstOrDefault() as FieldOffsetAttribute;
                    if (xOffsetAttrib != null)
                    {
                        xOffset = xOffsetAttrib.Value;
                    }
                    else
                    {
                        aObjectStorageSize += (uint)xFieldSize;
                        xOffset = -1;
                    }
                    aTypeFields.Insert(0,
                                       new KeyValuePair<string, TypeInformation.Field>(xField.GetFullName(),
                                                                                       new TypeInformation.Field(xFieldSize,
                                                                                                                 xFieldType.IsClass && !xFieldType.IsValueType,
                                                                                                                 xFieldType,
                                                                                                                 (xPlugFieldAttr != null && xPlugFieldAttr.IsExternalValue))
                                                                                       {
                                                                                           Offset = xOffset
                                                                                       }));
                }
                while (xCurrentPlugFieldList.Count > 0)
                {
                    var xItem = xCurrentPlugFieldList.Values.First();
                    xCurrentPlugFieldList.Remove(xItem.FieldId);
                    Type xFieldType = xItem.FieldType;
                    int xFieldSize;
                    string xFieldId = xItem.FieldId;
                    if (xFieldType == null)
                    {
                        xFieldType = xItem.FieldType;
                    }
                    if (xFieldType == null)
                    {
                        LogMessage(LogSeverityEnum.Error, "Plugged field {0} not found! (On Type {1})", xItem.FieldId, aType.AssemblyQualifiedName);
                    }
                    if (xItem.IsExternalValue || (xFieldType.IsClass && !xFieldType.IsValueType))
                    {
                        xFieldSize = 4;
                    }
                    else
                    {
                        xFieldSize = (int)GetFieldStorageSize(xFieldType);
                    }
                    int xOffset = (int)aObjectStorageSize;
                    aObjectStorageSize += (uint)xFieldSize;
                    aTypeFields.Insert(0,
                                       new KeyValuePair<string, TypeInformation.Field>(xItem.FieldId,
                                                                                       new TypeInformation.Field(xFieldSize,
                                                                                                                 xFieldType.IsClass && !xFieldType.IsValueType,
                                                                                                                 xFieldType,
                                                                                                                 xItem.IsExternalValue)));
                }
                if (aType.FullName != "System.Object" && aType.BaseType != null)
                {
                    aType = aType.BaseType;
                }
                else
                {
                    break;
                }
            } while (true);
        }

        public TypeInformation GetTypeInfo(Type aType)
        {
            TypeInformation xTypeInfo;
            uint xObjectStorageSize;
            IDictionary<string, TypeInformation.Field> xTypeFields = GetTypeFieldInfo(aType,
                                                                                     out xObjectStorageSize);
            xTypeInfo = new TypeInformation(xObjectStorageSize,
                                            xTypeFields,
                                            aType,
                                            (!aType.IsValueType) && aType.IsClass);
            return xTypeInfo;
        }

        public string GetMethodIdLabel(MethodBase aMethod)
        {
            return Label.FilterStringForIncorrectChars(aMethod.GetFullName() + "__METHOD_ID");
        }

        public string GetTypeIdLabel(Type aType)
        {
            return Label.FilterStringForIncorrectChars(aType.AssemblyQualifiedName + "__TYPE_ID");
        }

        public IDictionary<string, TypeInformation.Field> GetTypeFieldInfo(MethodBase aCurrentMethod,
                                                                                 out uint aObjectStorageSize)
        {
            Type xCurrentInspectedType = aCurrentMethod.DeclaringType;
            return GetTypeFieldInfo(xCurrentInspectedType,
                                    out aObjectStorageSize);
        }

        public IDictionary<string, TypeInformation.Field> GetTypeFieldInfo(Type aType,
                                                                                 out uint aObjectStorageSize)
        {
            var xTypeFields = new List<KeyValuePair<string, TypeInformation.Field>>();
            aObjectStorageSize = 0;
            GetTypeFieldInfoImpl(xTypeFields,
                                 aType,
                                 ref aObjectStorageSize);
            if (aType.IsExplicitLayout)
            {
                var xStructLayout = aType.StructLayoutAttribute;
                if (xStructLayout.Size == 0)
                {
                    aObjectStorageSize = (uint)((from item in xTypeFields
                                                 let xSize = item.Value.Offset + item.Value.Size
                                                 orderby xSize descending
                                                 select xSize).FirstOrDefault());
                }
                else
                {
                    aObjectStorageSize = (uint)xStructLayout.Size;
                }
            }
            int xOffset = 0;
            Dictionary<string, TypeInformation.Field> xResult = new Dictionary<string, TypeInformation.Field>();
            foreach (var item in xTypeFields)
            {
                var xItem = item.Value;
                if (item.Value.Offset == -1)
                {
                    xItem.Offset = xOffset;
                    xOffset += xItem.Size;
                }
                xResult.Add(item.Key,
                            xItem);
            }
            return xResult;
        }

        public uint GetObjectStorageSize(Type aType)
        {
            if (aType == null)
            {
                throw new ArgumentNullException("aType");
            }
            var xTypeInfo = GetTypeInfo(aType);
            return xTypeInfo.NeedsGC
                       ? xTypeInfo.StorageSize + ObjectImpl.FieldDataOffset
                       : xTypeInfo.StorageSize;
        }
    }
}