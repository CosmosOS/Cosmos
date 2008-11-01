using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;

namespace Indy.IL2CPU.Assembler {
    public abstract class Assembler : IDisposable {
        // TODO: When threading is being worked on, fix this to work multithreaded!
        //public const string CurrentExceptionDataMember = "__CURRENT_EXCEPTION__";
        public const string SignatureLabelName = "____SIGNATURE___";
        public static Exception CurrentException;

        public static void PrintException() {
            // The RSOD - Red Screen of Death
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Clear();

            Console.WriteLine("Cosmos Kernel. Copyright 2007-2008, The Cosmos Project.");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine("");
            Console.WriteLine("An unhandled kernel exception occurred.");
            Console.WriteLine("");
            Console.WriteLine(CurrentException.ToString());
            Console.WriteLine("");
            Console.WriteLine("The Cosmos Project would appreciate your feedback about this issue.");
        }

        private static FieldInfo mCurrentExceptionRef;

        public static FieldInfo CurrentExceptionRef {
            get {
                if (mCurrentExceptionRef == null) {
                    var xThisType = typeof(Assembler);
                    mCurrentExceptionRef = xThisType.GetField("CurrentException");
                    if (mCurrentExceptionRef == null) {
                        throw new Exception("Couldn't find CurrentException field!");
                    }
                }
                return mCurrentExceptionRef;
            }
        }

        private static MethodInfo mCurrentExceptionOccurredRef;

        public static MethodInfo CurrentExceptionOccurredRef {
            get {
                if (mCurrentExceptionOccurredRef == null) {
                    var xThisType = typeof(Assembler);
                    mCurrentExceptionOccurredRef = xThisType.GetMethod("ExceptionOccurred");
                    if (mCurrentExceptionOccurredRef == null) {
                        throw new Exception("Couldn't find ExceptionOccurred method!");
                    }
                }
                return mCurrentExceptionOccurredRef;
            }
        }

        public byte[] Signature { get; set; }

        public static void ExceptionOccurred() {
            System.Diagnostics.Debugger.Break();
        }

        public const string EntryPointName = "__ENGINE_ENTRYPOINT__";
        protected internal List<Instruction> mInstructions = new List<Instruction>();
        private List<DataMember> mDataMembers = new List<DataMember>();
        public readonly Stack<StackContent> StackContents = new Stack<StackContent>();

        private static ReaderWriterLocker mCurrentInstanceLocker = new ReaderWriterLocker();
        private static SortedList<int, Stack<Assembler>> mCurrentInstance = new SortedList<int, Stack<Assembler>>();

        public static Stack<Assembler> CurrentInstance {
            get {
                using(mCurrentInstanceLocker.AcquireReaderLock()) {
                    if(mCurrentInstance.ContainsKey(Thread.CurrentThread.ManagedThreadId)) {
                        return mCurrentInstance[Thread.CurrentThread.ManagedThreadId];
                    }
                }
                using(mCurrentInstanceLocker.AcquireWriterLock()) {
                    if (mCurrentInstance.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    {
                        return mCurrentInstance[Thread.CurrentThread.ManagedThreadId];
                    }
                    var xResult = new Stack<Assembler>();
                    mCurrentInstance.Add(Thread.CurrentThread.ManagedThreadId, xResult);
                    return xResult;
                }
            }
        }

        private Func<string, string> mGetFileNameForGroup;

        private uint mDataMemberCounter = 0;

        public string GetIdentifier(string aPrefix) {
            mDataMemberCounter++;
            return aPrefix + mDataMemberCounter.ToString("X8").ToUpper();
        }

        public Assembler(Func<string, string> aGetFileNameForGroup) {
            mGetFileNameForGroup = aGetFileNameForGroup;
            CurrentInstance.Push(this);
            //mInstructions.AddComplexIndexDefinition(
        }

        public List<KeyValuePair<string, Instruction>> GetInstructions() {
//using (mInstructionsLocker.AcquireReaderLock()) {
//    if(mInstructions.ContainsKey(Thread.CurrentThread.ManagedThreadId)) {
//        return mInstructions[Thread.CurrentThread.ManagedThreadId];
//    }
//}
//            using(mInstructionsLocker.AcquireWriterLock()) {
//                // do this check again, between the two locks, the situation might have changed.
//                if (mInstructions.ContainsKey(Thread.CurrentThread.ManagedThreadId))
//                {
//                    return mInstructions[Thread.CurrentThread.ManagedThreadId];
//                }
//                var xResult = new
//            }
            throw new NotImplementedException("After multi-threaded refactorings, this hasn't been implemented again, yet");
        }

        public List<DataMember> DataMembers {
            get { return mDataMembers; }
        }

        public void Dispose() {
            // MtW: I know, IDisposable usage for this isn't really nice, but for now this should be fine.
            //		Anyhow, we need a way to clear the CurrentInstance property
            mInstructions.Clear();
            mDataMembers.Clear();
            CurrentInstance.Pop();
        }

        public void Add(params Instruction[] aReaders) {
            foreach (Instruction xInstruction in aReaders) {
                mInstructions.Add(xInstruction);
            }
        }

        public virtual void Initialize() {

        }

        /// <summary>
        /// allows to emit footers to the code and datamember sections
        /// </summary>
        protected virtual void BeforeFlush() {
        }


        public virtual void FlushText(TextWriter aOutput) {
            using (Assembler.mCurrentInstanceLocker.AcquireReaderLock()) {
                foreach (var xItem in mCurrentInstance.Values) {
                    if (xItem.Count > 0) {
                        var xAsm = xItem.Peek();
                        if (xAsm != this) {
                            mDataMembers.AddRange(xAsm.mDataMembers);
                            mInstructions.AddRange(xAsm.mInstructions);
                        }
                    }
                }
            }
            BeforeFlush();
            if (mDataMembers.Count > 0) {
                aOutput.WriteLine();
                foreach (DataMember xMember in mDataMembers) {
                    aOutput.WriteLine("\t" + xMember);
                }
                aOutput.WriteLine();
            }
            if (mInstructions.Count > 0) {
                EmitCodeSection(aOutput,
                                mInstructions);
            }
            EmitFooter(aOutput);
        }

        protected void EmitCodeSection(TextWriter aOutputWriter, List<Instruction> aInstructions) {
            EmitCodeSectionHeader(aOutputWriter);
            aOutputWriter.WriteLine();
            string xMainLabel = "";
            foreach (Instruction x in aInstructions) {
                string prefix = "\t\t\t";
                Label xLabel = x as Label;
                if (xLabel != null) {
                    string xFullName;
                    if (xLabel.Name[0] != '.') {
                        xMainLabel = xLabel.Name;
                        xFullName = xMainLabel;
                    } else {
                        xFullName = xMainLabel + xLabel.Name;
                    }
                    aOutputWriter.WriteLine();
                    if (x.ToString()[0] == '.') {
                        prefix = "\t\t";
                    } else {
                        prefix = "\t";
                    }
                    aOutputWriter.WriteLine(prefix + xFullName.Replace(".", "__DOT__") + ":");
                    continue;
                }
                aOutputWriter.WriteLine(prefix + x);
            }
            EmitCodeSectionFooter(aOutputWriter);
            aOutputWriter.WriteLine();
        }

        protected virtual void EmitIncludes(TextWriter aOutputWriter) {
        }

        protected virtual void EmitCodeSectionHeader(TextWriter aOutputWriter) {
        }

        protected virtual void EmitCodeSectionFooter(TextWriter aOutputWriter) {
        }

        protected virtual void EmitDataSectionHeader(TextWriter aOutputWriter) { }

        protected virtual void EmitDataSectionFooter(TextWriter aOutputWriter) { }
        protected virtual void EmitFooter(TextWriter aOutputWriter) {  }
        public string MainGroup { get; set; }
    }
}