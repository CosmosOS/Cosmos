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

        private Dictionary<EcmaCil.TypeMeta, Type> mTypeMetaToType = new Dictionary<EcmaCil.TypeMeta, Type>();
        private Dictionary<EcmaCil.MethodMeta, MethodBase> mMethodMetaToMethod = new Dictionary<EcmaCil.MethodMeta, MethodBase>();


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
            EnqueueMethod(xAssemblyDef.EntryPoint, null, "entry point");

            // handle queue
            do
            {
                while (mQueue.Count > 0)
                {
                    var xItem = mQueue.Dequeue();
                    if (xItem is QueuedMethod)
                    {
                        var xMethod = (QueuedMethod)xItem;
                        ScanMethod(xMethod, mMethods[xMethod]);
                        continue;
                    }
                    if (xItem is QueuedArrayType)
                    {
                        var xType = (QueuedArrayType)xItem;
                        ScanArrayType(xType, mArrayTypes[xType]);
                        continue;
                    }
                    if (xItem is QueuedType)
                    {
                        var xType = (QueuedType)xItem;
                        ScanType(xType, mTypes[xType]);
                        continue;
                    }
                    throw new Exception("Queue item not supported: '" + xItem.GetType().FullName + "'!");
                }
                DoVMTScan();
            } while (mQueue.Count > 0);

            return mTypes.Values.Cast<EcmaCil.TypeMeta>()
                .Union(mArrayTypes.Values.Cast<EcmaCil.TypeMeta>())
                .Union(mPointerTypes.Values.Cast<EcmaCil.TypeMeta>()).ToArray();
        }

        private void DoVMTScan()
        {
            var xAllTypes = mTypes.ToArray();
            foreach(var xTypePair in xAllTypes){
                var xQueuedType = xTypePair.Key;
                var xTypeMeta = xTypePair.Value;

                foreach (var xMethod in xTypeMeta.Methods)
                {
                    if (!xMethod.IsVirtual)
                    {
                        continue;
                    }

                    MethodBase xBaseMethod;
                    if (!mMethodMetaToMethod.TryGetValue(xMethod, out xBaseMethod))
                    {
                        throw new Exception("Couldn't find method!");
                    }

                    foreach (var xSubTypeMeta in xTypeMeta.Descendants)
                    {
                        if((from method in xSubTypeMeta.Methods
                            where method.Overrides == xMethod
                            select method).Any()){
                            continue;
                        }

                        Type xSubType;
                        if (!mTypeMetaToType.TryGetValue(xSubTypeMeta, out xSubType))
                        {
                            throw new Exception("Couldn't find type!");
                        }
                        var xBindFlags = BindingFlags.Instance;
                        if (xMethod.IsPublic)
                        {
                            xBindFlags |= BindingFlags.Public;
                        }
                        else
                        {
                            xBindFlags |= BindingFlags.NonPublic;
                        }
                        
                        var xFoundMethod = xSubType.GetMethod(xBaseMethod.Name,
                                xBindFlags, null, (from item in xBaseMethod.GetParameters()
                                                   select item.ParameterType).ToArray(), null);
                        if (xFoundMethod != null)
                        {
                            EnqueueMethod(xFoundMethod, xMethod, "Overridden method");
                        }
                        else
                        {
                            // apparantly, this type doesn't override the method. to speed up scanning,
                            // we add a dummy method, just calling base

                        }
                        
                    }
                }

            }
        }
    }
}