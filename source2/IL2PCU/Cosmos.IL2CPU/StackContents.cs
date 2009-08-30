using System;
using System.Collections.Generic;
using System.Linq;

namespace Cosmos.IL2CPU {

  public class StackContents {
    #region class Item
    public sealed class Item {
        public Item( int aSize ) {
            Size = aSize;
        }

        public Item( int aSize, Type aType )
            : this( aSize )
        {
            IsNumber = ( aType == typeof( byte )
                || aType == typeof( sbyte )
                || aType == typeof( Boolean )
                || aType == typeof( short )
                || aType == typeof( ushort )
                || aType == typeof( int )
                || aType == typeof( uint )
                || aType == typeof( long )
                || aType == typeof( ulong )
                || aType == typeof( Single )
                || aType == typeof( Double ) );
            IsFloat = ( aType == typeof( Single ) || aType == typeof( Double ) );
            IsSigned = ( aType == typeof( sbyte )
                || aType == typeof( short )
                || aType == typeof( int )
                || aType == typeof( long )
                || aType == typeof( Single )
                || aType == typeof( Double ) );
            ContentType = aType;
        }

        public Item( int aSize, bool aIsNumber, bool aIsFloat, bool aIsSigned )
            : this( aSize )
        {
            IsNumber = aIsNumber;
            IsFloat = aIsFloat;
            IsSigned = aIsSigned;
        }
        public readonly int Size;
        public readonly bool IsNumber = false;
        public readonly bool IsFloat = false;
        public readonly bool IsSigned = false;
        public readonly Type ContentType = null;
        public readonly bool IsBox = false;
    }
    #endregion

    public StackContents() {
    }

    private Stack<Item> mStack = new Stack<Item>();

    public Item Peek() {
      return mStack.Peek();
    }

    public Item Pop() {
      return mStack.Pop();
    }

    public void Push(Item aItem) {
      mStack.Push(aItem);
    }

    public void Push(int aSize) {
      mStack.Push(new Item(aSize));
    }

    public void Push(int aSize, Type aType) {
      mStack.Push(new Item(aSize, aType));
    }

    public void Push(int aSize, bool aIsNumber, bool aIsFloat, bool aIsSigned) {
      mStack.Push(new Item(aSize, aIsNumber, aIsFloat, aIsSigned));
    }

 }
}