/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

using System.Text;

#if NET_40 || NET_45
using System.Runtime;
#endif

namespace System.Data.SQLite
{
	internal abstract class InternalBase
	{
		// Methods
#if NET_40 || NET_45
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
		protected InternalBase()
		{
		}

		internal abstract void ToCompactString(StringBuilder builder);
		internal virtual string ToFullString()
		{
			StringBuilder builder = new StringBuilder();
			this.ToFullString(builder);
			return builder.ToString();
		}

#if NET_40 || NET_45
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
		internal virtual void ToFullString(StringBuilder builder)
		{
			this.ToCompactString(builder);
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			this.ToCompactString(builder);
			return builder.ToString();
		}
	}
}
