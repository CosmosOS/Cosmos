using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Cosmos.IL2CPU {
  public class OurHashSet<T>: IEnumerable<T> {
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