using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectionToEcmaCil
{
    public struct QueuedPointerType : IEquatable<QueuedPointerType>
    {
        public readonly Type PointerType;

        public QueuedPointerType(Type pointerType)
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
