using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Indy.IL2CPU.IL {
    // TODO: abstract this one out to a X86 specific one
    public class MethodInformation {
        public struct Variable {
            public Variable(int aOffset,
                            int aSize,
                            bool aIsReferenceTypeField,
                            Type aVariableType) {
                Offset = aOffset;
                Size = aSize;
                VirtualAddresses = new int[Size / 4];
                for (int i = 0; i < (Size / 4); i++) {
                    VirtualAddresses[i] =0-(Offset + ((i + 1) * 4) + 0);
                }
                IsReferenceType = aIsReferenceTypeField;
                VariableType = aVariableType;
            }

            public readonly int Offset;
            public readonly int Size;
            public readonly bool IsReferenceType;
            public readonly Type VariableType;

            /// <summary>
            /// Gives the list of addresses to access this variable. This field contains multiple entries if the <see cref="Size"/> is larger than 4.
            /// </summary>
            public readonly int[] VirtualAddresses;
        }

        public struct Argument {
            public enum KindEnum {
                In,
                ByRef,
                Out
            }

            public Argument(uint aSize,
                            int aOffset,
                            KindEnum aKind,
                            bool aIsReferenceType,
                TypeInformation aTypeInfo,
                            Type aArgumentType) {
                mSize = aSize;
                mVirtualAddresses = new int[mSize / 4];
                mKind = aKind;
                mArgumentType = aArgumentType;
                mIsReferenceType = aIsReferenceType;
                mOffset = -1;
                TypeInfo = aTypeInfo;
                Offset = aOffset;
            }

            private int[] mVirtualAddresses;

            public int[] VirtualAddresses {
                get {
                    return mVirtualAddresses;
                }
                internal set {
                    mVirtualAddresses = value;
                }
            }

            private uint mSize;

            public uint Size {
                get {
                    return mSize;
                }
                internal set {
                    mSize = value;
                }
            }

            private bool mIsReferenceType;

            public bool IsReferenceType {
                get {
                    return mIsReferenceType;
                }
                internal set {
                    mIsReferenceType = value;
                }
            }

            private int mOffset;

            public int Offset {
                get {
                    return mOffset;
                }
                internal set {
                    if (mOffset != value) {
                        mOffset = value;
                        for (int i = 0; i < (mSize / 4); i++) {
                            mVirtualAddresses[i] = (mOffset + ((i + 1) * 4) + 4);
                        }
                    }
                }
            }

            private KindEnum mKind;

            public KindEnum Kind {
                get {
                    return mKind;
                }
                internal set {
                    mKind = value;
                }
            }

            private Type mArgumentType;

            public Type ArgumentType {
                get {
                    return mArgumentType;
                }
                internal set {
                    mArgumentType = value;
                }
            }

            public readonly TypeInformation TypeInfo;
        }

        public MethodInformation(string aLabelName,
                                 Variable[] aLocals,
                                 Argument[] aArguments,
                                 uint aReturnSize,
                                 bool aIsInstanceMethod,
                                 TypeInformation aTypeInfo,
                                 MethodBase aMethod,
                                 Type aReturnType,
                                 bool debugMode,
                                 IDictionary<string, object> aMethodData) {
            Locals = aLocals;
            DebugMode = debugMode;
            LabelName = aLabelName;
            Arguments = aArguments;
            ReturnSize = aReturnSize;
            IsInstanceMethod = aIsInstanceMethod;
            TypeInfo = aTypeInfo;
            Method = aMethod;
            ReturnType = aReturnType;
            MethodData = aMethodData;
            LocalsSize = (from item in aLocals
                          let xSize = (item.Size % 4 == 0)
                                          ? item.Size
                                          : (item.Size + (4 - (item.Size % 4)))
                          select xSize).Sum();
            if (aMethod != null) {
                IsNonDebuggable = aMethod.GetCustomAttributes(typeof(DebuggerStepThroughAttribute),
                                                              false).Length != 0 || aMethod.DeclaringType.GetCustomAttributes(typeof(DebuggerStepThroughAttribute),
                                                                                                                              false).Length != 0 || aMethod.DeclaringType.Module.GetCustomAttributes(typeof(DebuggerStepThroughAttribute),
                                                                                                                                                                                                     false).Length != 0 || aMethod.DeclaringType.Assembly.GetCustomAttributes(typeof(DebuggerStepThroughAttribute),
                                                                                                                                                                                                                                                                              false).Length != 0;
            }
            var xRoundedSize = ReturnSize;
            if (xRoundedSize % 4 > 0) {
                xRoundedSize += (4 - (ReturnSize % 4));
            }

            ExtraStackSize = (int)xRoundedSize;
            foreach (var xItem in aArguments) {
                ExtraStackSize -= (int)xItem.Size;
            }
            if (ExtraStackSize > 0) {
                for (int i = 0; i < Arguments.Length; i++) {
                    Arguments[i].Offset += ExtraStackSize;
                }
            }
        }

        public readonly int ExtraStackSize;
        public readonly IDictionary<string, object> MethodData;

        /// <summary>
        /// This variable is only updated when the MethodInformation instance is supplied by the Engine.ProcessAllMethods method
        /// </summary>
        public ExceptionHandlingClause CurrentHandler;

        public readonly MethodBase Method;
        public readonly string LabelName;
        public readonly Variable[] Locals;
        public readonly Argument[] Arguments;
        public readonly uint ReturnSize;
        public readonly Type ReturnType;
        public readonly bool IsInstanceMethod;
        public readonly TypeInformation TypeInfo;
        public readonly int LocalsSize;
        public readonly bool DebugMode;
        public readonly bool IsNonDebuggable;

        public override string ToString() {
            var xSB = new StringBuilder();
            xSB.AppendLine(String.Format("Method '{0}'\r\n",
                                         Method.GetFullName()));
            xSB.AppendLine("Locals:");
            if (Locals.Length == 0) {
                xSB.AppendLine("\t(none)");
            }
            var xCurIndex = 0;
            foreach (var xVar in Locals) {
                xSB.AppendFormat("\t({0}) {1}\t{2}\t{3} (Type = {4})\r\n\r\n",
                                 xCurIndex++,
                                 xVar.Offset,
                                 xVar.Size,
                                 xVar.VirtualAddresses.FirstOrDefault(),
                                 xVar.VariableType.FullName);
            }
            xSB.AppendLine("Arguments:");
            if (Arguments.Length == 0) {
                xSB.AppendLine("\t(none)");
            }
            xCurIndex = 0;
            foreach (var xArg in Arguments) {
                xSB.AppendLine(String.Format("\t({0}) {1}\t{2}\t{3} (Type = {4})\r\n",
                                             xCurIndex++,
                                             xArg.Offset,
                                             xArg.Size,
                                             xArg.VirtualAddresses.FirstOrDefault(),
                                             xArg.ArgumentType.FullName));
            }
            xSB.AppendLine("\tReturnSize: " + ReturnSize);
            return xSB.ToString();
        }
    }
}