using System.Collections.Generic;

namespace XSharp.Common
{
  public static class Extensions
  {
    public static void AddRange<K, V>(this IDictionary<K, V> aThis, IDictionary<K, V> source)
    {
      foreach (var xItem in source)
      {
        aThis.Add(xItem.Key, xItem.Value);
      }
    }
  }
}
