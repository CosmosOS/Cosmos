// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Cosmos.Debug.Symbols.Pdb
{
    /// <summary>
    /// Represents a sequence point within an IL method body.
    /// Sequence point describes a point in the method body at which all side effects of
    /// previous evaluations have been performed.
    /// </summary>
    public struct ILSequencePoint
    {
        public readonly int Offset;
        public readonly string Document;
        public readonly int LineNumber;
        // TODO: The remaining info

        public ILSequencePoint(int offset, string document, int lineNumber)
        {
            Offset = offset;
            Document = document;
            LineNumber = lineNumber;
        }
    }
}