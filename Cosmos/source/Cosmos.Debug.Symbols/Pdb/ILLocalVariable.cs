// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Cosmos.Debug.Symbols.Pdb
{
    /// <summary>
    /// Represents information about a local variable within a method body.
    /// </summary>
    public class ILLocalVariable
    {
        public int Slot;
        public string Name;
        public bool CompilerGenerated;
        public Type Type;

        public ILLocalVariable(int aSlot, string aName, bool aCompilerGenerated, Type aType = null)
        {
            Slot = aSlot;
            Name = aName;
            CompilerGenerated = aCompilerGenerated;
            Type = aType;
        }
    }

}