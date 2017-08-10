using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace MonoCecilToEcmaCil1
{
    public struct QueuedMethod : IEquatable<QueuedMethod>
    {
        private readonly TypeDefinition type;
        private readonly MethodDefinition method;
        private readonly TypeReference[] args;

        private readonly static TypeReference[] empty = new TypeReference[0];

        public TypeDefinition Type { get { return type; } }
        public MethodDefinition Method { get { return method; } }
        public TypeReference[] Args { get { return args ?? empty; } }

        public QueuedMethod(TypeDefinition type, MethodDefinition method, TypeReference[] args)
        {
            this.type = type;
            this.method = method;
            this.args = args;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                uint n = (uint)(type.GetHashCode() ^ method.GetHashCode() ^ Args.Length);

                for (int i = Args.Length - 1; i >= 0; i--)
                {
                    n = (n << 1) | (n >> 31);
                    n ^= (uint)args[i].GetHashCode();
                }
                return (int)n;
            }
        }
        public bool Equals(QueuedMethod qm)
        {
            if (!qm.type.Equals(type) || !qm.method.Equals(method) || qm.Args.Length != Args.Length)
                return false;
            for (int i = Args.Length - 1; i >= 0; i--)
            {
                if (!args[i].Equals(qm.args[i]))
                    return false;
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            return Equals((QueuedMethod)obj);
        }
    }
}
