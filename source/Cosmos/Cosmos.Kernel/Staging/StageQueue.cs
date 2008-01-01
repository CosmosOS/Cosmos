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
		private Queue<IStage> _initialize = new Queue<IStage> ();

		/// <summary>
		/// The list of teardown stages.
		/// </summary>
		private Stack<IStage> _teardown = new Stack<IStage> ();

		private IStage _current;
		/// <summary>
		/// Gets the current kernel stage.
		/// </summary>
		public IStage Current {
			get {
				return _current;
			}
		}

		/// <summary>
		/// Enqueues a stage.
		/// </summary>
		/// <param name="stage"></param>
		public void Enqueue(IStage stage) {
			_initialize.Enqueue (stage);
		}

		/// <summary>
		/// Runs the tasks.
		/// </summary>
		public void Run() {
			while (_initialize.Count != 0) {
				_current = _initialize.Dequeue ();
				_current.Initialize ();

				Console.Write ("Entered stage ");
				Console.Write (Current.Name);
				Console.WriteLine (".");

				_teardown.Push (Current);
			}
		}

		/// <summary>
		/// Runs the teardown.
		/// </summary>
		public void Teardown() {
			while (_teardown.Count != 0) {
				_current = _teardown.Pop ();
				_current.Teardown ();
				Console.Write ("Left stage ");
				Console.Write (Current.Name);
				Console.WriteLine (".");
			}
		}
	}
}
