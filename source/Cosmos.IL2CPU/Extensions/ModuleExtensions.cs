using System;
using System.Reflection;
using System.Reflection.Metadata;

using Cosmos.Debug.Symbols;

namespace Cosmos.IL2CPU
{
    public static class ModuleExtensions
    {
        // TODO: All this methods will be available in .NET Standard 2.0
        public static FieldInfo ResolveField(this Module aThis, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            foreach (var xType in aThis.Assembly.GetExportedTypes())
            {
                foreach (var xField in xType.GetRuntimeFields())
                {
                    if (xField.Module == aThis
                        && xField.MetadataToken == metadataToken
                        && TypeArraysAreEqual(xField.DeclaringType.GetGenericArguments(), genericTypeArguments))
                    {
                        return xField;
                    }
                }
            }

            //foreach (var xField in aThis.GetFields())
            //{
            //    if (xField.MetadataToken == metadataToken
            //        && TypeArraysAreEqual(xField.DeclaringType.GenericTypeArguments, genericTypeArguments))
            //    {
            //        return xField;
            //    }
            //}

            throw new Exception("Field doesn't exist");
        }

        public static MethodBase ResolveMethod(this Module aThis, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            foreach (var xType in aThis.Assembly.GetExportedTypes())
            {
                foreach (var xMethod in xType.GetRuntimeMethods())
                {
                    if (xMethod.Module == aThis
                        //&& xMethod.MetadataToken == metadataToken
                        //&& TypeArraysAreEqual(xMethod.DeclaringType.GetGenericArguments(), genericTypeArguments)
                        /*&& TypeArraysAreEqual(xMethod.GetGenericArguments(), genericMethodArguments)*/)
                    {
                        if (xMethod.MetadataToken == metadataToken)
                        {

                        }

                        if (TypeArraysAreEqual(xMethod.DeclaringType.GetGenericArguments(), genericTypeArguments))
                        {

                        }

                        if (TypeArraysAreEqual(xMethod.GetGenericArguments(), genericMethodArguments))
                        {

                        }

                        return xMethod;
                    }
                }
            }

            throw new Exception("Method doesn't exist");
        }

        public static string ResolveString(this Module aThis, int metadataToken)
        {
            return DebugSymbolReader.GetReader(aThis.Assembly.Location).GetString(metadataToken);
        }

        public static Type ResolveType(this Module aThis, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            foreach (var xType in aThis.Assembly.GetTypes())
            {
                if (xType.GetTypeInfo().Module == aThis
                    && xType.GetTypeInfo().MetadataToken == metadataToken
                    && TypeArraysAreEqual(xType.DeclaringType.GenericTypeArguments, genericTypeArguments)
                    && TypeArraysAreEqual(xType.GetGenericArguments(), genericMethodArguments))
                {
                    return xType;
                }
            }

            throw new Exception("Type doesn't exist");
        }

        private static bool TypeArraysAreEqual(Type[] a, Type[] b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
