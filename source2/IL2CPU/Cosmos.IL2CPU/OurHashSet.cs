using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Cosmos.IL2CPU {
    // Contains known types and methods, both scanned and unscanned
    // We need both a HashSet and a List. HashSet for speed of checking
    // to see if we already have it. And mItems contains an indexed list
    // so we can scan it as it changes. Foreach can work on HashSet,
    // but if foreach is used while its changed, a collection changed
    // exception will occur and copy on demand for each loop has too
    // much overhead.
    // we use a custom comparer, because the default one does some intelligent magic, which breaks lookups. is probably related
    // to comparing different types
    // Its possible .NET 4.0 may have a better replacement, but be careful about the object compare issue (see note about custom
    // comparer in ILScanner)

    public class OurHashSet<T> : IEnumerable<T> {
    private Dictionary<int, T> mItems = new Dictionary<int, T>();

    public bool Contains(T aItem) {
      if(aItem==null){
        throw new ArgumentNullException("aItem");
      }
      return mItems.ContainsKey(aItem.GetHashCode());
    }

    public void Add(T aItem) {
      if (aItem == null) {
        throw new ArgumentNullException("aItem");
      }
      mItems.Add(aItem.GetHashCode(), aItem);
    }

    public IEnumerator<T> GetEnumerator() {
      return (from item in mItems
              select item.Value).GetEnumerator();
    }

    public T GetItemInList(T aItem) {
      if (aItem == null) {
        throw new ArgumentNullException("aItem");
      }
      T xResult;
      if (mItems.TryGetValue(aItem.GetHashCode(), out xResult)) {
        return xResult;
      } else {
        return aItem;
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return (from item in mItems
              select item.Value).GetEnumerator();
    }
  }
}