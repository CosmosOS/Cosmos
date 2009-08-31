using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace Cosmos.IL2CPU {
    public abstract class Assembler {
      protected ILOp[] mILOpsLo = new ILOp[ 256 ];
      protected ILOp[] mILOpsHi = new ILOp[ 256 ];

      // Contains info on the current stack structure. What type are on the stack, etc
      public readonly StackContents Stack = new StackContents();

        private static ReaderWriterLocker mCurrentInstanceLocker = new ReaderWriterLocker();
        private static SortedList<int, Stack<Assembler>> mCurrentInstance = new SortedList<int, Stack<Assembler>>();
        protected internal List<Instruction> mInstructions = new List<Instruction>();
        private List<DataMember> mDataMembers = new List<DataMember>();

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

		public static ulong ConstructLabel(uint aMethod, uint aOpCode, byte aSubLabel)
		{
			/* Explanation:
			 * * This method generates labels. labels are 64bit:
			 * * First 24 bits (high to low) is the method number
			 * * then 32 bits is the opcode offset in the il
			 * * then 8 bits for a sub label.
			 */
			if (aMethod > 0x00FFFFFF)
			{
				throw new Exception("Error Method id too high!");
			}
			ulong xResult = aMethod << 40;
			xResult |= aOpCode << 8;
			xResult |= aSubLabel;
			return xResult;
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
			// todo: MtW: how to do this? we need some extra space.
			//		see ConstructLabel for extra info
			if(aMethod.UID > 0x00FFFFFF){
				throw new Exception("For now, too much methods");
			}

          foreach( var xOpCode in aOpCodes) {
            uint xOpCodeVal = (uint)xOpCode.OpCode;
            ILOp xILOp;
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