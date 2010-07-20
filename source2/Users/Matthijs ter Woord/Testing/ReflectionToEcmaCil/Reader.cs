using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectionToEcmaCil
{
    public class Reader: IDisposable
    {
        private Dictionary<QueuedMethod, EcmaCil.MethodMeta> mMethods = new Dictionary<QueuedMethod, EcmaCil.MethodMeta>();
        private Dictionary<QueuedType, EcmaCil.TypeMeta> mTypes = new Dictionary<QueuedType, EcmaCil.TypeMeta>();
        private Dictionary<QueuedArrayType, EcmaCil.ArrayTypeMeta> mArrayTypes = new Dictionary<QueuedArrayType, EcmaCil.ArrayTypeMeta>();
        private Dictionary<QueuedPointerType, EcmaCil.PointerTypeMeta> mPointerTypes = new Dictionary<QueuedPointerType, EcmaCil.PointerTypeMeta>();
        private Dictionary<QueuedMethod, EcmaCil.MethodMeta> mVirtuals = new Dictionary<QueuedMethod, EcmaCil.MethodMeta>();


        public void Dispose()
        {
            if (mMethods != null)
            {
                Clear();
                mMethods = null;
                mTypes = null;
                mArrayTypes = null;
                mPointerTypes = null;
                mVirtuals = null;
            }
            GC.SuppressFinalize(this);
        }

        public void Clear()
        {
            mMethods.Clear();
            mTypes.Clear();
            mArrayTypes.Clear();
            mPointerTypes.Clear();
            mVirtuals.Clear();
        }
    }
}