using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Cosmos.Debug.Symbols
{
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    // See the LICENSE file in the project root for more information.

    // Code adapted from https://blogs.msdn.microsoft.com/haibo_luo/2010/04/19/ilvisualizer-2010-solution

    public interface ITokenResolver
    {
        MethodBase AsMethod(int token);
        FieldInfo AsField(int token);
        Type AsType(int token);
        string AsString(int token);
        MemberInfo AsMember(int token);
        byte[] AsSignature(int token);
    }
}
