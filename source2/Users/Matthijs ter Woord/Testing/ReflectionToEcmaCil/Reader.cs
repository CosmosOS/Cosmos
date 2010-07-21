using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ReflectionToEcmaCil
{
    public partial class Reader: IDisposable
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

        public IEnumerable<EcmaCil.TypeMeta> Execute(string assembly)
        {
            var xAssemblyDef = Assembly.LoadFile(assembly);
            if (xAssemblyDef.EntryPoint == null)
            {
                throw new ArgumentException("Main assembly should have entry point!");
            }
            //if (xAssemblyDef.EntryPoint.GenericParameters.Count > 0 || xAssemblyDef.EntryPoint.DeclaringType.GenericParameters.Count > 0)
            //{
            //    throw new ArgumentException("Main assembly's entry point has generic parameters. This is not supported!");
            //}
            EnqueueMethod(xAssemblyDef.EntryPoint, null, "entry point");

            // handle queue
            while (mQueue.Count > 0)
            {
                var xItem = mQueue.Dequeue();
                if (xItem is QueuedMethod)
                {
                    var xMethod = (QueuedMethod)xItem;
                    ScanMethod(xMethod, mMethods[xMethod]);
                    continue;
                }
                //if (xItem is QueuedArrayType)
                //{
                //    var xType = (QueuedArrayType)xItem;
                //    ScanArrayType(xType, mArrayTypes[xType]);
                //    continue;
                //}
                //if (xItem is QueuedPointerType)
                //{
                //    var xType = (QueuedPointerType)xItem;
                //    ScanPointerType(xType, mPointerTypes[xType]);
                //    continue;
                //}
                if (xItem is QueuedType)
                {
                    var xType = (QueuedType)xItem;
                    ScanType(xType, mTypes[xType]);
                    continue;
                }
                throw new Exception("Queue item not supported: '" + xItem.GetType().FullName + "'!");
            }

            return mTypes.Values.Cast<EcmaCil.TypeMeta>()
                .Union(mArrayTypes.Values.Cast<EcmaCil.TypeMeta>())
                .Union(mPointerTypes.Values.Cast<EcmaCil.TypeMeta>());
        }
    }
}