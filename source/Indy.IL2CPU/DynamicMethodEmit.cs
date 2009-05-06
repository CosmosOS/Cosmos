using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Indy.IL2CPU
{
    /// <summary>
    /// Provides Compile-Time Service which enables the emission of dynamic methods.
    /// </summary>
    /// <remarks>
    /// If a method has a variable number (or type) of parameters which depend on its context,
    /// this feature can be used to improve code-reuse and optimize performance.
    /// </remarks>
    /// <example>
    /// To create a dynamic method emitter, one must create two classes derived from:
    ///     CEmittedMethodInfo
    ///     CMethodEmitterInfo
    /// And register the CMethodEmitterInfo in the DynamicMethodEmit constructior.
    /// </example>
    public class DynamicMethodEmit
    {
        /// <summary>
        /// Provides information about an emitted method, including the ability to find if
        /// that particular emitted method overides another method.
        /// </summary>
        public abstract class CEmittedMethodInfo
        {
            /// <summary>
            /// Returns true if the emitted method can override the provided method.
            /// </summary>
            /// <param name="method">The method to check overridability for.</param>
            /// <returns>True, if this method is suitable, otherwise, false.</returns>
            public abstract bool GetHandlesMethod(MethodBase method);
            /// <summary>
            /// Gets the MethodInfo of the emitted method which can overide the provided method.
            /// </summary>
            /// <param name="method">The method to retrieve an overide for.</param>
            /// <returns>The MethodInfo of the emitted method which can overide the provided method.</returns>
            public abstract MethodInfo GetHandlerForMethod(MethodBase method);
        }
        /// <summary>
        /// Provides information about a method emitter, including the ability to find if that
        /// particular method emitter can emit a method overides another method.
        /// </summary>
        public abstract class CMethodEmitterInfo
        {
            /// <summary>
            /// Returns true if this emitter can emit a method which overides the provided method.
            /// </summary>
            /// <param name="method">The method to check overrideability of.</param>
            /// <returns>True, if this emitter can emit a method which overides the provided method, otherwise, false.</returns>
            public abstract bool GetEmittsMethod(MethodBase method);
            /// <summary>
            /// Emitts a method which can overide the provided method.
            /// </summary>
            /// <param name="method">The method to overide.</param>
            /// <returns>The MethodInfo of the emitted method which can override the provided method.</returns>
            public abstract MethodInfo EmitMethod(MethodBase method);
        }

        /// <summary>
        /// CEmittedMethodInfo for Multidimensional Arrays
        /// </summary>
        public class MAMGroup : CEmittedMethodInfo
        {
            public Type arrayType;
            public int Ranks;

            public MethodInfo mbctor;
            public MethodInfo mbget;
            public MethodInfo mbset;

            public MAMGroup(Type a, int r, MethodInfo c, MethodInfo g, MethodInfo s)
            {
                arrayType = a;
                Ranks = r;
                mbctor = c;
                mbget = g;
                mbset = s;
            }

            public override bool GetHandlesMethod(MethodBase method)
            {
                return (
                    (method.DeclaringType.FullName.Split('[')[0] == arrayType.FullName) &&
                    (method.DeclaringType.GetArrayRank() == Ranks)
                );
            }

            public override MethodInfo GetHandlerForMethod(MethodBase method)
            {
                switch (method.Name)
                {
                    case ".ctor":
                        return mbctor;
                    case "Get":
                        return mbget;
                    case "Set":
                        return mbset;
                    default:
                        throw new NotImplementedException("MultiArray method '" + method.Name + "' is not defined!");
                }
            }
        }
        /// <summary>
        /// CMethodEmitterInfo for Multidimensional Arrays
        /// </summary>
        public class MAMEmitter : CMethodEmitterInfo
        {
            private static void EmitRecursiveFillArray(int ranknum, int maxranks, Type arrayType, ILGenerator EmitIL)
            {
                byte lCPA = (byte)(ranknum * 2);
                byte lCRN = (byte)(lCPA + 1);
                byte lCAP = (byte)(lCPA + 2);

                EmitIL.Emit(OpCodes.Ldarg_S, (byte)(ranknum + 1));
                EmitIL.Emit(OpCodes.Stloc_S, lCRN);

                Label Lb_CreateArray = EmitIL.DefineLabel();
                EmitIL.MarkLabel(Lb_CreateArray);
                EmitIL.Emit(OpCodes.Ldloc_S, lCRN);
                EmitIL.Emit(OpCodes.Ldc_I4_1);
                EmitIL.Emit(OpCodes.Sub);
                EmitIL.Emit(OpCodes.Stloc_S, lCRN);
                EmitIL.Emit(OpCodes.Ldarg_S, (byte)(ranknum + 2));
                if (ranknum == (maxranks - 1))
                {
                    EmitIL.Emit(OpCodes.Newarr, arrayType);
                }
                else
                {
                    EmitIL.Emit(OpCodes.Newarr, typeof(int[]));
                }
                EmitIL.Emit(OpCodes.Stloc_S, lCAP);
                EmitIL.Emit(OpCodes.Ldloc_S, lCPA);
                EmitIL.Emit(OpCodes.Ldloc_S, lCRN);
                EmitIL.Emit(OpCodes.Ldloc_S, lCAP);
                EmitIL.Emit(OpCodes.Stelem, typeof(int[]));
                if (ranknum < (maxranks - 1))
                    EmitRecursiveFillArray(ranknum + 1, maxranks, arrayType, EmitIL);
                EmitIL.Emit(OpCodes.Ldloc_S, lCRN);
                EmitIL.Emit(OpCodes.Brtrue, Lb_CreateArray);
            }
            private static void EmitWalkArrays(int Ranks, ILGenerator EmitIL)
            {
                EmitIL.Emit(OpCodes.Ldarg_0);

                for (int i = 1;i < Ranks;i++)
                {
                    EmitIL.Emit(OpCodes.Ldarg_S, (byte)i);
                    EmitIL.Emit(OpCodes.Ldelem, typeof(int[]));
                }

                EmitIL.Emit(OpCodes.Ldarg_S, (byte)Ranks);
            }

            public static void Emit_MultiArray_Ctor(Type arrayType, int Ranks)
            {
                String MethodName = "Emit" + (Instance.EmitCount++);

                MethodBuilder EmitMeth = Instance.EmitContType.DefineMethod(MethodName, MethodAttributes.Public | MethodAttributes.Static);

                Type[] MethodParams = new Type[Ranks + 1];
                MethodParams[0] = arrayType.MakeArrayType(Ranks);
                for (int i = 1;i <= Ranks;i++)
                    MethodParams[i] = typeof(int);
                EmitMeth.SetParameters(MethodParams);

                ILGenerator EmitIL = EmitMeth.GetILGenerator();
                for (int i = 1;i < Ranks;i++)
                {
                    EmitIL.DeclareLocal(typeof(int[]));
                    EmitIL.DeclareLocal(typeof(int));
                }
                EmitIL.DeclareLocal(typeof(int[]));

                EmitIL.Emit(OpCodes.Ldarg_0);
                EmitIL.Emit(OpCodes.Stloc_0);

                EmitRecursiveFillArray(0, Ranks - 1, arrayType, EmitIL);

                EmitIL.Emit(OpCodes.Ret);
            }
            public static void Emit_MultiArray_Get(Type arrayType, int Ranks)
            {
                String MethodName = "Emit" + (Instance.EmitCount++);

                MethodBuilder EmitMeth = Instance.EmitContType.DefineMethod(MethodName, MethodAttributes.Public | MethodAttributes.Static);

                Type[] MethodParams = new Type[Ranks + 1];
                MethodParams[0] = arrayType.MakeArrayType(Ranks);
                for (int i = 1;i <= Ranks;i++)
                    MethodParams[i] = typeof(int);
                EmitMeth.SetParameters(MethodParams);
                EmitMeth.SetReturnType(arrayType);

                ILGenerator EmitIL = EmitMeth.GetILGenerator();

                EmitWalkArrays(Ranks, EmitIL);

                EmitIL.Emit(OpCodes.Ldelem, arrayType);
                EmitIL.Emit(OpCodes.Ret);
            }
            public static void Emit_MultiArray_Set(Type arrayType, int Ranks)
            {
                String MethodName = "Emit" + (Instance.EmitCount++);

                MethodBuilder EmitMeth = Instance.EmitContType.DefineMethod(MethodName, MethodAttributes.Public | MethodAttributes.Static);

                Type[] MethodParams = new Type[Ranks + 2];
                MethodParams[0] = arrayType.MakeArrayType(Ranks);
                for (int i = 1;i <= Ranks;i++)
                    MethodParams[i] = typeof(int);
                MethodParams[Ranks + 1] = arrayType;
                EmitMeth.SetParameters(MethodParams);

                ILGenerator EmitIL = EmitMeth.GetILGenerator();

                EmitWalkArrays(Ranks, EmitIL);

                EmitIL.Emit(OpCodes.Ldarg_S, (byte)(Ranks + 1));
                EmitIL.Emit(OpCodes.Stelem, arrayType);
                EmitIL.Emit(OpCodes.Ret);
            }

            public override bool GetEmittsMethod(MethodBase method)
            {
                return (
                    (method.DeclaringType.IsArray) &&
                    (method.DeclaringType.GetArrayRank() > 1)
                );
            }

            public override MethodInfo EmitMethod(MethodBase method)
            {
                Type arrayType = Type.GetType(method.DeclaringType.FullName.Split('[')[0]);
                int Ranks = method.DeclaringType.GetArrayRank();

                BeginType();
                Emit_MultiArray_Ctor(arrayType, Ranks);
                Emit_MultiArray_Get(arrayType, Ranks);
                Emit_MultiArray_Set(arrayType, Ranks);
                MethodInfo[] mbs = EndType();

                Instance.CEMIs.Add(new MAMGroup(arrayType, Ranks, mbs[0], mbs[1], mbs[2]));

                return GetDynamicMethod(method);
            }
        }

        /// <summary>
        /// Operating instance of the DynamicMethodEmit class.
        /// </summary>
        public static DynamicMethodEmit Instance = new DynamicMethodEmit(AppDomain.CurrentDomain);

        /// <summary>
        /// AppDomain into which emission is occuring.
        /// </summary>
        public readonly AppDomain EmitDomain;
        /// <summary>
        /// Assembly into which emission is occuring.
        /// </summary>
        public readonly AssemblyBuilder EmitAssm;
        /// <summary>
        /// Module into which emission is occuring.
        /// </summary>
        public readonly ModuleBuilder EmitModu;

        /// <summary>
        /// Container Type of the methods which are currently being emitted.
        /// </summary>
        public TypeBuilder EmitContType;
        /// <summary>
        /// Number of Emitted menthods in the current Container Type.
        /// </summary>
        private int EmitCount = 0;
        /// <summary>
        /// Current number of Container Types.
        /// </summary>
        private int TypeCount = 0;
        /// <summary>
        /// List of all the currently emitted methods.
        /// </summary>
        /// <remarks>
        /// If a CEmittedMethodInfo is to be used, it must be present in this list.
        /// </remarks>
        List<CEmittedMethodInfo> CEMIs = new List<CEmittedMethodInfo>();
        /// <summary>
        /// List of all the method emitters.
        /// </summary>
        /// <remarks>
        /// If a CMethodEmitterInfo is to be used, it must be present in this list.
        /// </remarks>
        List<CMethodEmitterInfo> CMEIs = new List<CMethodEmitterInfo>();

        public DynamicMethodEmit(AppDomain aEmitDomain)
        {
            EmitDomain = aEmitDomain;
            EmitAssm = EmitDomain.DefineDynamicAssembly(new AssemblyName() { Name = "IndyIL2CPU_EmitAssm" }, AssemblyBuilderAccess.RunAndSave);
            EmitModu = EmitAssm.DefineDynamicModule("IndyIL2CPU_EmitAssm");

            //Register CMethodEmitterInfos Here
            CMEIs.Add(new MAMEmitter());    //Multidimensional Arrays

            Instance = this;
        }

        /// <summary>
        /// Begins a type to which emission will occur.
        /// After all desired methods have been emitted,
        /// call EndType() to get the MethodInfos.
        /// </summary>
        public static void BeginType()
        {
            Instance.EmitContType = Instance.EmitModu.DefineType(
                "Indy.IL2CPU.MultiArrayEmit.ContType" + (Instance.TypeCount++),
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit
            );
            Instance.EmitCount = 0;
        }
        /// <summary>
        /// Finializes emission of a type.
        /// </summary>
        /// <returns>MethodInfos of the emitted methods.</returns>
        public static MethodInfo[] EndType()
        {
            Type t = Instance.EmitContType.CreateType();

            MethodInfo[] mbs = new MethodInfo[Instance.EmitCount];
            for (int i = 0;i < Instance.EmitCount;i++)
                mbs[i] = t.GetMethod("Emit" + i);

            return mbs;
        }

        /// <summary>
        /// Returns true if DynamicMethodEmit Instance currently contains an emitter which can overide the provided Method.
        /// <param name="method">The method to search for an overide emitter for.</param>
        /// <returns>True if a suitable emitter was found, otherwise, false.</returns>
        public static bool GetHasDynamicMethod(MethodBase method)
        {
            foreach (CMethodEmitterInfo cmei in Instance.CMEIs)
            {
                if (cmei.GetEmittsMethod(method))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Returns the MethodInfo of the method which can overide the provided method,
        /// generating it if nessecary.
        /// </summary>
        /// <param name="method">The method to overide.</param>
        /// <returns>The MethodInfo of the suitable override.</returns>
        /// <exception cref="System.NotImplementedException">
        /// Thrown if a sutiable override could not be found or generated.
        /// </exception>
        public static MethodInfo GetDynamicMethod(MethodBase method)
        {
            foreach (CEmittedMethodInfo cemi in Instance.CEMIs)
            {
                if (cemi.GetHandlesMethod(method))
                {
                    return cemi.GetHandlerForMethod(method);
                }
            }
            foreach (CMethodEmitterInfo cmei in Instance.CMEIs)
            {
                if (cmei.GetEmittsMethod(method))
                {
                    return cmei.EmitMethod(method);
                }
            }

            throw new NotImplementedException();
        }
    }
}
