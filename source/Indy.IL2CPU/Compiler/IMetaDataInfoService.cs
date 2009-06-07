using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL;
using System.Reflection;

namespace Indy.IL2CPU.Compiler
{
#warning Todo: Cleanup this interface
    // Make sure that this interface is cleaned up soon!
    public interface IMetaDataInfoService
    {
        uint GetFieldStorageSize(Type aType);
        IDictionary<string, TypeInformation.Field> GetTypeFieldInfo(MethodBase aCurrentMethod,
                                                                                 out uint aObjectStorageSize);
        IDictionary<string, TypeInformation.Field> GetTypeFieldInfo(Type aType,
                                                                                 out uint aObjectStorageSize);
        TypeInformation GetTypeInfo(Type aType);
        MethodInformation GetMethodInfo(MethodBase aCurrentMethodForArguments,
                                                      MethodBase aCurrentMethodForLocals,
                                                      string aMethodName,
                                                      TypeInformation aTypeInfo,
                                                      bool aDebugMode,
                                                      IDictionary<string, object> aMethodData);
        MethodInformation GetMethodInfo(MethodBase aCurrentMethodForArguments,
                                                      MethodBase aCurrentMethodForLocals,
                                                      string aMethodName,
                                                      TypeInformation aTypeInfo,
                                                      bool aDebugMode);
        MethodInformation GetMethodInfo(MethodBase CtorDef, bool aDebugMode);

        uint GetObjectStorageSize(Type aType);

        void LogMessage(LogSeverityEnum aSeverity, string aMessage, params object[] aArgs);

        string GetStaticFieldLabel(FieldInfo aField);

        // VMT support
        string GetTypeIdLabel(Type aType);

        string GetMethodIdLabel(MethodBase xMethodDef);



    }
}