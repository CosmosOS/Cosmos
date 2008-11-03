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

        public List<Instruction> Instructions {
            get { return mInstructions; }
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
        protected virtual void OnBeforeFlush() {
        }

        private void BeforeFlush() {
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
            OnBeforeFlush();
            CleanupCode();
        }

        /// <summary>
        /// Cleans up the Instructions list. it evaluates all conditionals. 
        /// </summary>
        protected void CleanupCode() {
            int xCurrentIdx = 0;
            int xIfLevelsToSkip = 0;
            var xDefines = new List<string>();
            while (xCurrentIdx < mInstructions.Count) {
                var xCurrentInstruction = mInstructions[xCurrentIdx];
                var xIfDefined = xCurrentInstruction as IfDefined;
                var xEndIfDefined = xCurrentInstruction as EndIfDefined;
                var xDefine = xCurrentInstruction as Define;
                if(xIfDefined!=null){
                    if(xIfLevelsToSkip>0){
                        xIfLevelsToSkip++;
                    } else {
                        if (!xDefines.Contains(xIfDefined.Symbol.ToLowerInvariant())) {
                            xIfLevelsToSkip++;
                        }
                    }
                    mInstructions.RemoveAt(xCurrentIdx);
                    continue;
                }
                if (xEndIfDefined != null) {
                    if (xIfLevelsToSkip > 0) {
                        xIfLevelsToSkip--;                        
                    }
                    mInstructions.RemoveAt(xCurrentIdx);
                    continue;
                }
                if (xIfLevelsToSkip > 0) {
                    mInstructions.RemoveAt(xCurrentIdx);
                    continue;
                }
                if (xDefine != null) {
                    if (!xDefines.Contains(xDefine.Symbol.ToLowerInvariant())) {
                        xDefines.Add(xDefine.Symbol.ToLowerInvariant());
                    }
                    mInstructions.RemoveAt(xCurrentIdx);
                    continue;
                }
                xCurrentIdx++;
            }
        }

        public virtual void FlushBinary(Stream aOutput, ulong aBaseAddress) {
            BeforeFlush();
            bool xSituationChanged = false;
            var xAssemblerItems = new List<BaseAssemblerElement>();
            xAssemblerItems.AddRange(mDataMembers.Cast<BaseAssemblerElement>());
            xAssemblerItems.AddRange(mInstructions.Cast<BaseAssemblerElement>());
            
            var xIncompleteItems = new List<BaseAssemblerElement>(xAssemblerItems);
            var xCurrentAddresss = aBaseAddress;
            do {
                xSituationChanged = false; 
                int xIdx = 0;
                while (xIdx < xIncompleteItems.Count) {
                    var xCurrentItem = xIncompleteItems[xIdx];
                    xCurrentItem.StartAddress = xCurrentAddresss;
                    ulong xSize = 0;
                    if (xCurrentItem.DetermineSize(this, out xSize)) {
                        xSituationChanged = true;
                        Console.WriteLine("Size of '{0}' = {1}", xCurrentItem, xSize);
                        xCurrentAddresss += xSize;
                        xIncompleteItems.RemoveAt(xIdx);
                        continue;
                    } else {
                        break;
                    }
                }
            } while (xSituationChanged);
            if (!xSituationChanged && xIncompleteItems.Count > 0) {
                throw new Exception("Not all Elements are able to determine size!");
            }
            xIncompleteItems = new List<BaseAssemblerElement>(xAssemblerItems);
            do {
                int xIdx = 0;
                xSituationChanged = false;
                while (xIdx < xIncompleteItems.Count) {
                    var xCurrentItem = xIncompleteItems[xIdx];
                    if (xCurrentItem.IsComplete(this)) {
                        xSituationChanged = true;
                        xIncompleteItems.RemoveAt(xIdx);
                        continue;
                    } else {
                        break;
                    }
                }
            } while (xSituationChanged);
            if (!xSituationChanged && xIncompleteItems.Count > 0) {
                throw new Exception("Not all Elements are complete!");
            }
            foreach (var xItem in xAssemblerItems) {
                var xBuff = xItem.GetData(this);
                aOutput.Write(xBuff, 0, xBuff.Length);
            }
        }

        public virtual void FlushText(TextWriter aOutput) {
            BeforeFlush();
            if (mDataMembers.Count > 0) {
                aOutput.WriteLine();
                foreach (DataMember xMember in mDataMembers) {
                    aOutput.WriteLine("\t" + xMember);
                }
                aOutput.WriteLine();
            }
            if (mInstructions.Count > 0) {
                string xMainLabel="";
                foreach (Instruction x in mInstructions) {
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
                        aOutput.WriteLine();
                        if (x.ToString()[0] == '.') {
                            prefix = "\t\t";
                        } else {
                            prefix = "\t";
                        }
                        aOutput.WriteLine(prefix + xFullName.Replace(".", "__DOT__") + ":");
                        continue;
                    }
                    aOutput.WriteLine(prefix + x);
                }
            }
        }

        public BaseAssemblerElement TryResolveReference(ElementReference aReference) {
            foreach (var xInstruction in mInstructions) {
                var xLabel = xInstruction as Label;
                if (xLabel != null) {
                    if (xLabel.QualifiedName.Equals(aReference.Name, StringComparison.InvariantCultureIgnoreCase)) {
                        return xLabel;
                    }
                }
            }
            foreach (var xDataMember in mDataMembers) {
                if (xDataMember.Name.Equals(aReference.Name, StringComparison.InvariantCultureIgnoreCase)) {
                    return xDataMember;
                }
            }
            return null;
        }
    }
}