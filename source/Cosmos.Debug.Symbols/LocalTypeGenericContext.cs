using System;
using System.Collections.Immutable;

namespace Cosmos.Debug.Symbols
{
    public class LocalTypeGenericContext
    {
        public LocalTypeGenericContext(ImmutableArray<Type> typeParameters, ImmutableArray<Type> methodParameters)
        {
            MethodParameters = methodParameters;
            TypeParameters = typeParameters;
        }

        public ImmutableArray<Type> MethodParameters { get; }
        public ImmutableArray<Type> TypeParameters { get; }
    }
}
