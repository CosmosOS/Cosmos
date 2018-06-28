using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Common
{
	public static class ExtensionMethods
	{
		public static uint Sum(this IEnumerable<uint> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			ulong sum = 0Lu;
			foreach (uint val in source)
			{
				sum += val;
			}
			return (uint)sum;
		}
	}
}