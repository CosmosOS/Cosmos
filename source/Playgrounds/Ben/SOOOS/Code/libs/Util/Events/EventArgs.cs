

using System;

namespace Util
{
	/// <summary>
	/// try to use custome event args 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class EventArgs<T> : EventArgs
	{
		private T _data;

		public EventArgs(T data)
		{
			_data = data;
		}

		public T Data
		{
			get { return _data; }
		}
	}
}

