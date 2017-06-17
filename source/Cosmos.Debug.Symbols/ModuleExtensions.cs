using System;
using System.Linq;
using System.Reflection;

namespace Cosmos.Debug.Symbols
{
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    // See the LICENSE file in the project root for more information.

    // Code adapted from https://blogs.msdn.microsoft.com/haibo_luo/2010/04/19/ilvisualizer-2010-solution

    public static class ModuleExtensions
    {
        private static readonly MethodInfo s_resolveMethod = GetMethodInfo(nameof(ResolveMethod));
        private static readonly MethodInfo s_resolveField = GetMethodInfo(nameof(ResolveField));
        private static readonly MethodInfo s_resolveType = GetMethodInfo(nameof(ResolveType));
        private static readonly MethodInfo s_resolveMember = GetMethodInfo(nameof(ResolveMember));
        private static readonly MethodInfo s_resolveString = GetMethodInfo(nameof(ResolveString));
        private static readonly MethodInfo s_resolveSignature = GetMethodInfo(nameof(ResolveSignature));

        public static MethodBase ResolveMethod(this Module module, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments) => Invoke<MethodBase>(s_resolveMethod, module, metadataToken, genericTypeArguments, genericMethodArguments);
        public static FieldInfo ResolveField(this Module module, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments) => Invoke<FieldInfo>(s_resolveField, module, metadataToken, genericTypeArguments, genericMethodArguments);
        public static Type ResolveType(this Module module, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments) => Invoke<Type>(s_resolveType, module, metadataToken, genericTypeArguments, genericMethodArguments);
        public static MemberInfo ResolveMember(this Module module, int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments) => Invoke<MemberInfo>(s_resolveMember, module, metadataToken, genericTypeArguments, genericMethodArguments);
        public static byte[] ResolveSignature(this Module module, int metadataToken) => Invoke<byte[]>(s_resolveSignature, module, metadataToken);
        public static string ResolveString(this Module module, int metadataToken) => Invoke<string>(s_resolveString, module, metadataToken);

        private static MethodInfo GetMethodInfo(string name)
        {
            Type[] parameterTypes = typeof(ModuleExtensions).GetMethod(name).GetParameters().Skip(1).Select(p => p.ParameterType).ToArray();
            return typeof(Module).GetMethod(name, parameterTypes);
        }

        private static T Invoke<T>(MethodInfo method, Module module, params object[] args) => (T) method.Invoke(module, args);
    }
}
