using System;
using System.Reflection;

namespace Cosmos.Debug.Symbols
{
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    // See the LICENSE file in the project root for more information.

    // Code adapted from https://blogs.msdn.microsoft.com/haibo_luo/2010/04/19/ilvisualizer-2010-solution

    public sealed class ModuleScopeTokenResolver : ITokenResolver
    {
        private readonly Module _module;
        private readonly MethodBase _enclosingMethod;
        private readonly Type[] _methodContext;
        private readonly Type[] _typeContext;

        public ModuleScopeTokenResolver(MethodBase method)
        {
            _enclosingMethod = method;
            _module = method.Module;
            _methodContext = (method is ConstructorInfo) ? null : method.GetGenericArguments();
            _typeContext = (method.DeclaringType == null) ? null : method.DeclaringType.GetGenericArguments();
        }

        public MethodBase AsMethod(int token) => _module.ResolveMethod(token, _typeContext, _methodContext);
        public FieldInfo AsField(int token) => _module.ResolveField(token, _typeContext, _methodContext);
        public Type AsType(int token) => _module.ResolveType(token, _typeContext, _methodContext);
        public MemberInfo AsMember(int token) => _module.ResolveMember(token, _typeContext, _methodContext);
        public string AsString(int token) => _module.ResolveString(token);
        public byte[] AsSignature(int token) => _module.ResolveSignature(token);
    }
}
