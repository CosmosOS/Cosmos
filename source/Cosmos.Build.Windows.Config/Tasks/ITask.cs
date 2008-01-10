using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Windows.Config.Tasks
{
    /// <summary>
    /// Represents a simple task.
    /// </summary>
    public abstract class Task
    {
        /// <summary>
        /// Occurs when the task has a status message.
        /// </summary>
        public event EventHandler<TaskStatusEventArgs> Status;

        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Occurs when a status message is received.
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="message"></param>
        protected void OnStatus(float percentage, string message)
        {
            TaskStatusEventArgs args = new TaskStatusEventArgs();
            args.TaskName = Name;
            args.Percentage = percentage;
            args.Message = message;

            if (Status != null)
                Status(this, args);
        }
    }

    /// <summary>
    /// Represents the current task's status.
    /// </summary>
    public class TaskStatusEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string TaskName { get; set; }
        public float Percentage { get; set; }
    }
}
