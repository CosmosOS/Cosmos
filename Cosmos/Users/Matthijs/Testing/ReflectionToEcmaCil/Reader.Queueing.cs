using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ReflectionToEcmaCil
{
    partial class Reader
    {
        private Queue<object> mQueue = new Queue<object>();

        private EcmaCil.MethodMeta EnqueueMethod(MethodBase aMethod, object aSource, string aSourceType)
        {
            if (mLogEnabled)
            {
                LogMapPoint(aSource, aSourceType, aMethod);
            }
            if (aMethod.IsGenericMethodDefinition)
            {
                throw new Exception("Cannot queue generic method definitions");
            }
            Type xReturnType = null;
            var xMethodInfo = aMethod as MethodInfo;
            if (xMethodInfo != null)
            {
                xReturnType = xMethodInfo.ReturnType;
            }
            else
            {
                xReturnType = typeof(void);
            }
            var xQueuedMethod = new QueuedMethod(aMethod.DeclaringType, aMethod, (from item in aMethod.GetParameters()
                                                                                  select item.ParameterType).ToArray(), xReturnType);
            EcmaCil.MethodMeta xMethodMeta;
            if(mMethods.TryGetValue(xQueuedMethod, out xMethodMeta)){
                return xMethodMeta;
            }
            var xDeclaringType = EnqueueType(aMethod.DeclaringType, aMethod, "Declaring type");
            xMethodMeta = new EcmaCil.MethodMeta();
            mMethodMetaToMethod.Add(xMethodMeta, aMethod);
            xMethodMeta.DeclaringType = xDeclaringType;
            xDeclaringType.Methods.Add(xMethodMeta);
            mMethods.Add(xQueuedMethod, xMethodMeta);
            mQueue.Enqueue(xQueuedMethod);
            return xMethodMeta;
        }

        private EcmaCil.TypeMeta EnqueueType(Type aTypeRef, object aSource, string aSourceType)
        {
            if (mLogEnabled)
            {
                LogMapPoint(aSource, aSourceType, aTypeRef);
            }
            List<Type> xTypeArgs = null;
            if (aTypeRef.IsArray)
            {
                var xQueuedArrayType = new QueuedArrayType(aTypeRef);
                EcmaCil.ArrayTypeMeta xArrayMeta;
                if (mArrayTypes.TryGetValue(xQueuedArrayType, out xArrayMeta))
                {
                    return xArrayMeta;
                }
                var xElemMeta = EnqueueType(aTypeRef.GetElementType(), aTypeRef, "Array element type");
                xArrayMeta = new EcmaCil.ArrayTypeMeta { ElementType = xElemMeta };
                mArrayTypes.Add(xQueuedArrayType, xArrayMeta);
                mQueue.Enqueue(xQueuedArrayType);
                return xArrayMeta;
            }
            if (aTypeRef.IsPointer)
            {
                var xQueuedPointerType = new QueuedPointerType(aTypeRef);
                EcmaCil.PointerTypeMeta xPointerMeta;
                if (mPointerTypes.TryGetValue(xQueuedPointerType, out xPointerMeta))
                {
                    return xPointerMeta;
                }
                var xElemMeta = EnqueueType(aTypeRef.GetElementType(), aTypeRef, "Array element type");
                xPointerMeta = new EcmaCil.PointerTypeMeta { ElementType = xElemMeta };
                mPointerTypes.Add(xQueuedPointerType, xPointerMeta);
                mQueue.Enqueue(xQueuedPointerType);
                return xPointerMeta;
            }
            if (aTypeRef.IsGenericType)
            {
                xTypeArgs = new List<Type>(aTypeRef.GetGenericArguments());
            }
            var xQueuedType = new QueuedType(aTypeRef, (xTypeArgs == null ? null : xTypeArgs.ToArray()));
            EcmaCil.TypeMeta xTypeMeta;
            if (mTypes.TryGetValue(xQueuedType, out xTypeMeta))
            {
                return xTypeMeta;
            }
            xTypeMeta = new EcmaCil.TypeMeta();
            mTypeMetaToType.Add(xTypeMeta, aTypeRef);
            if (aTypeRef.BaseType != null)
            {
                xTypeMeta.BaseType = EnqueueType(aTypeRef.BaseType, aTypeRef, "Base type");
            }
            mTypes.Add(xQueuedType, xTypeMeta);                         
            mQueue.Enqueue(xQueuedType);
            return xTypeMeta;
        }
    }
}
