using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace MonoCecilToEcmaCil1
{
    public struct QueuedType : IEquatable<QueuedType>
    {
        public readonly TypeDefinition Type;
        public readonly TypeReference[] Args;

        private readonly static TypeReference[] empty = new TypeReference[0];

        public QueuedType(TypeDefinition type, TypeReference[] args)
        {
            Type = type;
            if (args == null)
            {
                Args = empty;
            }
            else
            {
                Args = args;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                uint n = (uint)(Type.GetHashCode());
                if (Args != null)
                {
                    n ^= (uint)Args.Length;
                    for (int i = Args.Length - 1; i >= 0; i--)
                    {
                        n = (n << 1) | (n >> 31);
                        n ^= (uint)Args[i].GetHashCode();
                    }
                }
                return (int)n;
            }
        }
        public bool Equals(QueuedType qm)
        {
            if (!qm.Type.Equals(Type) || qm.Args.Length != Args.Length)
                return false;
            for (int i = Args.Length - 1; i >= 0; i--)
            {
                if (!Args[i].Equals(qm.Args[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals((QueuedType)obj);
        }
    }
}
