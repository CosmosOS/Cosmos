using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util.Collections
{
  public delegate bool MruEqualityComparer<K, T>(K key, T item);

  public class MruList<K, T>

  {

    private LinkedList<T> items;
    private int maxCapacity ;
    private MruEqualityComparer<K, T> compareFunc;

    public MruList(int maxCap, MruEqualityComparer<K,T> compFunc)
    {
      maxCapacity = maxCap;
      compareFunc = compFunc;
      items = new LinkedList<T>();
    }

    public T this[K key]
    {
        get
        {
            return Find(key);
        }
    }

    public T Find(K key)
    {
      LinkedListNode<T> node = FindNode(key);
      if (node != null)
      {
        items.Remove(node);
        items.AddFirst(node);
        return node.Value;
      }
      return default(T);
    }

    private LinkedListNode<T> FindNode(K key)
    {
      LinkedListNode<T> node = items.First;
      while (node != null)
      {
        if (compareFunc(key, node.Value))
          return node;
        node = node.Next;
      }
      return null;
    }

    public void Add(T item)
    {
      // remove items until the list is no longer full.
      while (items.Count >= maxCapacity)
      {
        items.RemoveLast();
      }
      items.AddFirst(item);
    }

    public void Remove(T item)
    {
        items.Remove(item); 
    }
  }

}



