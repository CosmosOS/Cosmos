using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace Cosmos.IL2CPU
{
    public abstract class Assembler
    {
        #region Fields

        protected ILOp[] mILOpsLo = new ILOp[ 256 ];
        protected ILOp[] mILOpsHi = new ILOp[ 256 ];

        public readonly Stack<StackContent> StackContents = new Stack<StackContent>();

        private static ReaderWriterLocker mCurrentInstanceLocker = new ReaderWriterLocker();
        private static SortedList<int, Stack<Assembler>> mCurrentInstance = new SortedList<int, Stack<Assembler>>();
        protected internal List<Instruction> mInstructions = new List<Instruction>();
        private List<DataMember> mDataMembers = new List<DataMember>();

        #endregion

        #region Properties

        public static Stack<Assembler> CurrentInstance
        {
            get
            {
                using( mCurrentInstanceLocker.AcquireReaderLock() )
                {
                    if( mCurrentInstance.ContainsKey( Thread.CurrentThread.ManagedThreadId ) )
                    {
                        return mCurrentInstance[ Thread.CurrentThread.ManagedThreadId ];
                    }
                }
                using( mCurrentInstanceLocker.AcquireWriterLock() )
                {
                    if( mCurrentInstance.ContainsKey( Thread.CurrentThread.ManagedThreadId ) )
                    {
                        return mCurrentInstance[ Thread.CurrentThread.ManagedThreadId ];
                    }
                    var xResult = new Stack<Assembler>();
                    mCurrentInstance.Add( Thread.CurrentThread.ManagedThreadId, xResult );
                    return xResult;
                }
            }
        }

        internal int AllAssemblerElementCount
        {
            get
            {
                return mInstructions.Count + mDataMembers.Count;
            }
        }

        #endregion

        public Assembler()
        {
            InitILOps();
            CurrentInstance.Push( this );
        }

        public void Dispose()
        {
            // MtW: I know, IDisposable usage for this isn't really nice, but for now this should be fine.
            //		Anyhow, we need a way to clear the CurrentInstance property
            //mInstructions.Clear();
            //mDataMembers.Clear();
            CurrentInstance.Pop();
            //if (mAllAssemblerElements != null)
            //{
            //    mAllAssemblerElements.Clear();
            //}
        }

         public BaseAssemblerElement GetAssemblerElement( int aIndex )
        {
            if( aIndex >= mInstructions.Count )
            {
                return mDataMembers[ aIndex - mInstructions.Count ];
            }
            return mInstructions[ aIndex ];
        }

        public BaseAssemblerElement TryResolveReference( ElementReference aReference )
        {
            foreach( var xInstruction in mInstructions )
            {
                var xLabel = xInstruction as Label;
                if( xLabel != null )
                {
                    if( xLabel.QualifiedName.Equals( aReference.Name, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        return xLabel;
                    }
                }
            }
            foreach( var xDataMember in mDataMembers )
            {
                if( xDataMember.Name.Equals( aReference.Name, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    return xDataMember;
                }
            }
            return null;
        }

        public void Add( params Instruction[] aReaders )
        {
            foreach( Instruction xInstruction in aReaders )
            {
                mInstructions.Add( xInstruction );
            }
        }

        public void ProcessMethod( MethodInfo aMethod, List<ILOpCode> aOpCodes ) {
          if (aOpCodes.Count == 0) {
            return;
          }

          ILOpCode xOpCode = aOpCodes[0];
          ILOp xILOp = null;
          uint xOpCodeVal = 0;

          for( int i = 1; i < aOpCodes.Count; i++ ) {
              xOpCodeVal = ( uint )xOpCode.OpCode;
              if (xOpCodeVal <= 0xFF) {
                xILOp = mILOpsLo[xOpCodeVal];
              } else {
                xILOp = mILOpsHi[xOpCodeVal & 0xFF];
              }
              xILOp.Execute(aMethod, xOpCode);
          }
        }

        protected abstract void InitILOps();

        protected void InitILOps( Type aAssemblerBaseOp ) {
            foreach( var xType in aAssemblerBaseOp.Assembly.GetExportedTypes() ) {
                if( xType.IsSubclassOf( aAssemblerBaseOp ) ) {
                    var xAttribs = ( OpCodeAttribute[] )xType.GetCustomAttributes( typeof( OpCodeAttribute ), false );
                    foreach( var xAttrib in xAttribs ) {
                        var xOpCode = ( ushort )xAttrib.OpCode;
                        var xCtor = xType.GetConstructor( new Type[] { typeof( Assembler ) } );
                        var xILOp = ( ILOp )xCtor.Invoke( new Object[] { this } );
                        if( xOpCode <= 0xFF ) {
                            mILOpsLo[ xOpCode ] = xILOp;
                        } else {
                            mILOpsHi[ xOpCode & 0xFF ] = xILOp;
                        }
                    }
                }
            }
        }
 
    }
}