using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace MonoCecilToEcmaCil1
{
    public struct QueuedPointerType : IEquatable<QueuedPointerType>
    {
        public readonly PointerType PointerType;

        public QueuedPointerType(PointerType pointerType)
        {
            PointerType = pointerType;
        }

        public override int GetHashCode()
        {
            return PointerType.GetHashCode();
        }
        public bool Equals(QueuedPointerType qm)
        {
            if (!qm.PointerType.Equals(PointerType))
                return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals((QueuedPointerType)obj);
        }
    }
}
