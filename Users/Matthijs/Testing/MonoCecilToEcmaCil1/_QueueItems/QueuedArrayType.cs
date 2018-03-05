using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace MonoCecilToEcmaCil1
{
    public struct QueuedArrayType : IEquatable<QueuedArrayType>
    {
        public readonly ArrayType ArrayType;

        public QueuedArrayType(ArrayType arrayType)
        {
            ArrayType = arrayType;
        }

        public override int GetHashCode()
        {
            return ArrayType.GetHashCode();
        }
        public bool Equals(QueuedArrayType qm)
        {
            if (!qm.ArrayType.Equals(ArrayType))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals((QueuedArrayType)obj);
        }
    }
}
