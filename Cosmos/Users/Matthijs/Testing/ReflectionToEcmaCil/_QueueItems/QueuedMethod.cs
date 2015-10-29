using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ReflectionToEcmaCil
{
    public struct QueuedMethod : IEquatable<QueuedMethod>
    {
        private readonly Type type;
        private readonly MethodBase method;
        private readonly Type[] args;
        private readonly Type returnType;

        private readonly static Type[] empty = new Type[0];
        private readonly static Type SystemVoid = typeof(void);


        public Type Type { get { return type; } }
        public MethodBase Method { get { return method; } }
        public Type[] Args { get { return args; } }
        public Type ReturnType { get { return returnType; } }

        public QueuedMethod(Type type, MethodBase method, Type[] args, Type returnType)
        {
            this.type = type;
            this.method = method;
            this.args = args;
            if(args == null){
                args=empty;
            }
            this.returnType = returnType;
            if (returnType == null)
            {
                returnType = SystemVoid;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                uint n = (uint)(type.GetHashCode() ^ method.GetHashCode() ^ Args.Length ^ returnType.GetHashCode());

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
            if (!qm.type.Equals(type) || !qm.method.Equals(method) || qm.Args.Length != Args.Length || !qm.returnType.Equals(returnType))
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
