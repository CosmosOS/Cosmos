/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

#if NET_40 || NET_45
using System.Runtime;
#endif

namespace System.Data.SQLite
{
	internal sealed class KeyToListMap<TKey, TValue> : InternalBase
	{
		// Fields
		private Dictionary<TKey, List<TValue>> m_map;

		// Methods
		internal KeyToListMap(IEqualityComparer<TKey> comparer)
		{
			this.m_map = new Dictionary<TKey, List<TValue>>(comparer);
		}

		internal void Add(TKey key, TValue value)
		{
			List<TValue> list;
			if (!this.m_map.TryGetValue(key, out list))
			{
				list = new List<TValue>();
				this.m_map[key] = list;
			}
			list.Add(value);
		}

		internal void AddRange(TKey key, IEnumerable<TValue> values)
		{
			foreach (TValue local in values)
			{
				this.Add(key, local);
			}
		}

		internal bool ContainsKey(TKey key)
		{
			return this.m_map.ContainsKey(key);
		}

		internal IEnumerable<TValue> EnumerateValues(TKey key)
		{
			List<TValue> values;
			if (m_map.TryGetValue(key, out values))
			{
				foreach (TValue value in values) { yield return value; }
			}
		}

		internal ReadOnlyCollection<TValue> ListForKey(TKey key)
		{
			return new ReadOnlyCollection<TValue>(this.m_map[key]);
		}

		internal bool RemoveKey(TKey key)
		{
			return this.m_map.Remove(key);
		}

		internal override void ToCompactString(StringBuilder builder)
		{
			foreach (TKey local in this.Keys)
			{
				StringUtil.FormatStringBuilder(builder, "{0}", new object[] { local });
				builder.Append(": ");
				IEnumerable<TValue> list = this.ListForKey(local);
				StringUtil.ToSeparatedString(builder, list, ",", "null");
				builder.Append("; ");
			}
		}

		internal bool TryGetListForKey(TKey key, out ReadOnlyCollection<TValue> valueCollection)
		{
			List<TValue> list;
			valueCollection = null;
			if (this.m_map.TryGetValue(key, out list))
			{
				valueCollection = new ReadOnlyCollection<TValue>(list);
				return true;
			}
			return false;
		}

		// Properties
		internal IEnumerable<TValue> AllValues
		{
			get
			{
				foreach (TKey key in Keys)
				{
					foreach (TValue value in ListForKey(key))
					{
						yield return value;
					}
				}
			}
		}

		internal IEnumerable<TKey> Keys
		{
			get
			{
				return this.m_map.Keys;
			}
		}

		internal IEnumerable<KeyValuePair<TKey, List<TValue>>> KeyValuePairs
		{
#if NET_40 || NET_45
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
			get
			{
				return this.m_map;
			}
		}
	}
}
