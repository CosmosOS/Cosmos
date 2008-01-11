using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Build.Windows.Config.Tasks
{
    public class TaskQueue : Task
    {
        /// <summary>
        /// The tasks.
        /// </summary>
        private Queue<Task> _tasks = new Queue<Task>();

        private int _lastCount;
        private int _count;

        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public void BeginExecute()
        {
            ThreadStart start = new ThreadStart(Execute);
            start.BeginInvoke(null, null);
        }

        public override void Execute()
        {
            lock (_tasks)
            {
				try {
					_lastCount = _tasks.Count;
					while (true) {
						if (_tasks.Count == 0)
							break;
						Task task = _tasks.Dequeue();
						_count = _tasks.Count;

						task.Status += new EventHandler<TaskStatusEventArgs>(task_Status);
						task.Execute();
					}
					OnStatus(100, "Done");
				} catch (Exception E) {
					OnStatus(100, "Error: " + E.Message);
				}
            }
        }

        void task_Status(object sender, TaskStatusEventArgs e)
        {
            //float p = e.Percentage / _lastCount;
            //p += (_count / _lastCount) * 100;
            _name = e.TaskName;
            OnStatus(e.Percentage, e.Message);
        }

        public void Add(Task task)
        {
            lock(_tasks)
                _tasks.Enqueue(task);
        }
    }
}
