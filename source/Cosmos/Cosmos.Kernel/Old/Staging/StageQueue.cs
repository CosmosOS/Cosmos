using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Staging {
	/// <summary>
	/// Represents a stage queue.
	/// </summary>
	public class StageQueue {
		/// <summary>
		/// The list of initialize stages.
		/// </summary>
		private Queue<StageBase> _initialize = new Queue<StageBase>(16);

		/// <summary>
		/// The list of teardown stages.
		/// </summary>
		private Stack<StageBase> _teardown = new Stack<StageBase>(16);

		private StageBase _current;
		/// <summary>
		/// Gets the current kernel stage.
		/// </summary>
		public StageBase Current {
			get {
				return _current;
			}
		}

		/// <summary>
		/// Enqueues a stage.
		/// </summary>
		/// <param name="stage"></param>
		public void Enqueue(StageBase stage) {
			_initialize.Enqueue(stage);
		}

		/// <summary>
		/// Runs the tasks.
		/// </summary>
		public void Run() {
			while (_initialize.Count != 0) {
				_current = (StageBase)_initialize.Dequeue();

				Console.Write("Entering stage ");
				System.Diagnostics.Debugger.Break();
				Console.Write(Current.Name);
				Console.WriteLine(".");

				_current.Initialize();
				_teardown.Push(Current);
			}
		}

		/// <summary>
		/// Runs the teardown.
		/// </summary>
		public void Teardown() {
			while (_teardown.Count != 0) {
				_current = (StageBase)_teardown.Pop();

				Console.Write("Leaving stage ");
				Console.Write(Current.Name);
				Console.WriteLine(".");

				_current.Teardown();
			}
		}
	}
}
