using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

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

        private uint mDataMemberCounter = 0;

        public string GetIdentifier(string aPrefix) {
            mDataMemberCounter++;
            return aPrefix + mDataMemberCounter.ToString("X8").ToUpper();
        }

        public Assembler() {
            CurrentInstance.Push(this);
            //mInstructions.AddComplexIndexDefinition(
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
            //if (mAllAssemblerElements != null)
            //{
            //    mAllAssemblerElements.Clear();
            //}
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

        private bool mFlushInitializationDone = false;
        protected void BeforeFlush() {
            if (mFlushInitializationDone) {
                return;
            }
            mFlushInitializationDone = true;
            using (Assembler.mCurrentInstanceLocker.AcquireReaderLock()) {
                foreach (var xItem in mCurrentInstance.Values) {
                    if (xItem.Count > 0) {
                        var xAsm = xItem.Peek();
                        if (xAsm != this) {
                            mDataMembers.AddRange(xAsm.mDataMembers);
                            mInstructions.AddRange(xAsm.mInstructions);
                            xItem.Pop();
                        }
                    }
                }
            }
            OnBeforeFlush();
            //MergeAllElements();
        }

        //private IEnumerable<BaseAssemblerElement> EnumerateThroughAllElements()
        //{
        //    foreach(var xInstr in Instructions)
        //    {
        //        yield return xInstr;
        //    }
        //    foreach (var xData in DataMembers)
        //    {
        //        yield return xData;
        //    }
        //}

        ///// <summary>
        ///// Cleans up the Instructions list. it evaluates all conditionals. 
        ///// </summary>
        //protected void MergeAllElements() {
        //    int xIfLevelsToSkip = 0;
        //    var xDefines = new List<string>();
        //    var xNewAssemblerElements = new List<BaseAssemblerElement>((Instructions.Count + DataMembers.Count));
        //    Console.WriteLine("Assembler Element Count: {0}", (Instructions.Count + DataMembers.Count));
        //    Console.WriteLine("Memory in use: {0}", System.Diagnostics.Process.GetCurrentProcess().WorkingSet64);
        //    //Console.ReadLine();
        //    foreach (var xCurrentInstruction in EnumerateThroughAllElements())
        //    {
        //        var xIfDefined = xCurrentInstruction as IIfDefined;
        //        var xEndIfDefined = xCurrentInstruction as IEndIfDefined;
        //        var xDefine = xCurrentInstruction as IDefine;
        //        var xIfNotDefined = xCurrentInstruction as IIfNotDefined;
        //        if (xCurrentInstruction is Comment) {
        //            continue;
        //        }
        //        if (xIfDefined != null) {
        //            if (xIfLevelsToSkip > 0) {
        //                xIfLevelsToSkip++;
        //            } else if (!xDefines.Contains(xIfDefined.Symbol.ToLowerInvariant())) {
        //                xIfLevelsToSkip++;
        //            }
        //            continue;
        //        }
        //        if (xIfNotDefined != null) {
        //            if (xIfLevelsToSkip > 0) {
        //                xIfLevelsToSkip++;
        //            } else if (xDefines.Contains(xIfNotDefined.Symbol.ToLower())) {
        //                xIfLevelsToSkip++;
        //            }
        //            continue;
        //        }
        //        if (xEndIfDefined != null) {
        //            if (xIfLevelsToSkip > 0) {
        //                xIfLevelsToSkip--;
        //            }
        //            continue;
        //        }
        //        if (xIfLevelsToSkip > 0) {
        //            continue;
        //        }
        //        if (xDefine != null) {
        //            var xSymbol = xDefine.Symbol.ToLowerInvariant();
        //            if (!xDefines.Contains(xSymbol)) {
        //                xDefines.Add(xSymbol);
        //            }
        //            continue;
        //        }
        //        xNewAssemblerElements.Add(xCurrentInstruction);
        //    }
        //    mAllAssemblerElements = xNewAssemblerElements;
        //}

        //internal List<BaseAssemblerElement> mAllAssemblerElements;
        internal int AllAssemblerElementCount
        {
            get
            {
                return mInstructions.Count + mDataMembers.Count;
            }
        }

        public BaseAssemblerElement GetAssemblerElement(int aIndex)
        {
            if (aIndex >= mInstructions.Count)
            {
                return mDataMembers[aIndex - mInstructions.Count];
            }
            return mInstructions[aIndex];
        }

        public void FlushDebug(StreamWriter aOutput) {
            BeforeFlush();
            throw new Exception("not implemented");
//            var xMax = AllAssemblerElementCount;
//for(int i = 0; i < xMax;i++){
//                var xItem = GetAssemblerElement()
//                aOutput.WriteLine("{0} - '{1}'", (mAllAssemblerElements.Count - i), xItem.ToString());
//            }
        }

        public virtual void FlushBinary(Stream aOutput, ulong aBaseAddress) {
            BeforeFlush();
            var xMax = AllAssemblerElementCount;
            var xCurrentAddresss = aBaseAddress;
            for(int i = 0; i <xMax;i++){
                GetAssemblerElement(i).UpdateAddress(this, ref xCurrentAddresss);
            }
            aOutput.SetLength(aOutput.Length + (long)(xCurrentAddresss - aBaseAddress));
            for(int i = 0; i <xMax;i++){
                var xItem = GetAssemblerElement(i);
                if (!xItem.IsComplete(this)) {
                    throw new Exception("Incomplete element encountered.");
                }
                //var xBuff = xItem.GetData(this);
                //aOutput.Write(xBuff, 0, xBuff.Length);
                xItem.WriteData(this, aOutput);
            }
        }

        public virtual void FlushText(TextWriter aOutput) {
            BeforeFlush();
            if (mDataMembers.Count > 0) {
                aOutput.WriteLine();
                foreach (DataMember xMember in mDataMembers) {
                    aOutput.Write("\t");
                    xMember.WriteText(this, aOutput);
                    aOutput.WriteLine();
                }
                aOutput.WriteLine();
            }
            if (mInstructions.Count > 0) {
                string xMainLabel="";
                for(int i = 0; i < mInstructions.Count;i++){
                //foreach (Instruction x in mInstructions) {
                    var x = mInstructions[i];
                    string prefix = "\t\t\t";
                    Label xLabel = x as Label;
                    if (xLabel != null) {
                        if (xLabel.Name[0] == '.')
                        {
                            prefix = "\t\t";
                        }
                        else
                        {
                            prefix = "\t";
                        }
                        string xFullName;
                        aOutput.Write(prefix);
                        if (xLabel.Name[0] != '.') {
                            xMainLabel = xLabel.Name;
                            aOutput.Write(xMainLabel);
                        } else {
                            aOutput.Write(xMainLabel);
                            aOutput.Write(xLabel.Name);
                        }
                        aOutput.WriteLine();
                        //aOutput.WriteLine(prefix + Label.FilterStringForIncorrectChars(xFullName) + ":");
                        continue;
                    }
                    aOutput.Write(prefix);
                    x.WriteText(this, aOutput);
                    aOutput.WriteLine();
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