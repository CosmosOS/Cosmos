using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectionToEcmaCil
{
    public struct QueuedType : IEquatable<QueuedType>
    {
        public readonly Type Type;
        public readonly Type[] Args;

        private readonly static Type[] empty = new Type[0];

        public QueuedType(Type type, Type[] args)
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
