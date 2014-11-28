using System;
using System.Collections.Generic;
using System.Text;

// This is released under the public domain.
// Originally created by Orvid King for OForms.


namespace Cosmos.Common
{
	/// <summary>
	/// A linked queue implementation.
	/// This class is thread-safe.
	/// </summary>
	public class LinkedQueue<T>
	{
		/// <summary>
		/// This class represents a single item
		/// in the Queue.
		/// </summary>
		private class QueueItem<T2>
		{
			/// <summary>
			/// The next object in the
			/// queue.
			/// </summary>
			public QueueItem<T2> NextObject;
			/// <summary>
			/// The previous object in 
			/// the queue.
			/// </summary>
			public QueueItem<T2> PreviousObject;
			/// <summary>
			/// The actual value in this queue item.
			/// </summary>
			public T2 Value;
		}

		private QueueItem<T> FrontItem;
		private QueueItem<T> BackItem;
		private int depth;

		/// <summary>
		/// Creates a new instance of the 
		/// LinkedQueue class.
		/// </summary>
		public LinkedQueue() { }

		/// <summary>
		/// The number of items
		/// in the queue.
		/// </summary>
		public int Count { get { return depth; } }

		/// <summary>
		/// Add the specified
		/// value at the back
		/// of the queue.
		/// </summary>
		/// <param name="value">The value to add.</param>
		public void Enqueue(T value)
		{
			lock (this)
			{
				if (FrontItem == null)
				{
					// This is the first
					// item to be added to the queue.
					QueueItem<T> val = new QueueItem<T>();
					val.Value = value;

					FrontItem = val;
					BackItem = val;
				}
				else
				{
					QueueItem<T> bkItem = BackItem;
					QueueItem<T> val = new QueueItem<T>();
					val.Value = value;

					bkItem.NextObject = val;
					val.PreviousObject = bkItem;
					this.BackItem = val;
				}
				this.depth++;
			}
		}

		/// <summary>
		/// Retrieve the first item
		/// in the queue.
		/// </summary>
		/// <returns>
		/// The first value in the queue.
		/// </returns>
		public T Dequeue()
		{
			lock (this)
			{
				if (FrontItem == null)
					throw new Exception("No item in the queue!");
				this.depth--;
				QueueItem<T> val = FrontItem;
				if (val.NextObject == null)
				{
					BackItem = null;
					FrontItem = null;
				}
				else
				{
					FrontItem = FrontItem.NextObject;
					FrontItem.PreviousObject = null;
				}
				return val.Value;
			}
		}
	}
}
