using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Cosmos.IL2CPU
{
	// Contains known types and methods, both scanned and unscanned
	// We need both a HashSet and a List. HashSet for speed of checking
	// to see if we already have it. And mItems contains an indexed list
	// so we can scan it as it changes. Foreach can work on HashSet,
	// but if foreach is used while its changed, a collection changed
	// exception will occur and copy on demand for each loop has too
	// much overhead.
	// we use a custom comparer, because the default Hashcode does not work.
	// In .NET 4.0 has the DeclaringType often changed to System.Object,
	// didn't sure if hashcode changed. The situation now in .NET 4.0
	// is that the Contains method in OurHashSet checked only the
	// default Hashcode. With adding DeclaringType in the Hashcode it runs.

	public class OurHashSet<T> : IEnumerable<T> where T : MemberInfo
	{
		private Dictionary<int, T> mItems = new Dictionary<int, T>();

		public bool Contains(T aItem)
		{
			if (aItem == null)
				throw new ArgumentNullException("aItem");
			return mItems.ContainsKey(GetHash(aItem));
		}

		public void Add(T aItem)
		{
			if (aItem == null)
				throw new ArgumentNullException("aItem");
			mItems.Add(GetHash(aItem), aItem);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return (from item in mItems
					select item.Value).GetEnumerator();
		}

		public T GetItemInList(T aItem)
		{
			if (aItem == null)
				throw new ArgumentNullException("aItem");
			T xResult;
			if (mItems.TryGetValue(GetHash(aItem), out xResult))
				return xResult;
			else
				return aItem;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (from item in mItems
					select item.Value).GetEnumerator();
		}

		public static string GetDeclareTypeString(T item)
		{
			var xName = item.DeclaringType;
			return xName == null ? string.Empty : xName.ToString();
		}

		public static int GetHash(T item)
		{
			return (item.ToString() + GetDeclareTypeString(item)).GetHashCode();
		}
	}
}
