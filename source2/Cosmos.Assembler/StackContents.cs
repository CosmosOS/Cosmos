using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Diagnostics.Contracts;

namespace Cosmos.Assembler {

  public class StackContents: IEnumerable<StackContents.Item> {
    #region class Item
	public sealed class Item {
		private Item(uint aSize)
		{
#if DOTNETCOMPATIBLE
			Contract.Requires(
				aSize == 4 || aSize == 8,
				"The size could not exist, because always is pushed Int32 or Int64!");
#endif
			Size = aSize;
		}

		/// <summary>
		/// A Item for the Stack.
		/// </summary>
		/// <param name="aSize">The Size 4 or 8 to be pushed.</param>
		/// <param name="aType">The real type behind the Int32 or Int64.</param>
		public Item(uint aSize, Type aType)
			: this(aSize) {
			IsNumber = (aType == typeof(byte)
				|| aType == typeof(sbyte)
				|| aType == typeof(Boolean)
				|| aType == typeof(short)
				|| aType == typeof(ushort)
				|| aType == typeof(int)
				|| aType == typeof(uint)
				|| aType == typeof(long)
				|| aType == typeof(ulong)
				|| aType == typeof(Single)
				|| aType == typeof(Double));
			IsFloat = (aType == typeof(Single) || aType == typeof(Double));
			IsSigned = (aType == typeof(sbyte)
				|| aType == typeof(short)
				|| aType == typeof(int)
				|| aType == typeof(long)
				|| aType == typeof(Single)
				|| aType == typeof(Double));
			ContentType = aType;
		}

      public readonly uint Size;
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

    public int Count { get { return mStack.Count; } }
    public Item Peek() {
      return mStack.Peek();
    }

    public void Clear() {
      mStack.Clear();
    }

    public Item Pop() {
      return mStack.Pop();
    }

    public void Push(Item aItem) {
      mStack.Push(aItem);
    }

    public void Push(uint aSize, Type aType) {
      mStack.Push(new Item(aSize, aType));
    }

    public IEnumerator<Item> GetEnumerator() {
      foreach (var item in mStack) {
        yield return item;
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      foreach (var xItem in mStack) {
        yield return xItem;
      }
    }
  }
}