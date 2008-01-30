using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cosmos.GdbClient.BasicCommands
{
    public abstract class CommandBase<T>
    {
        private GdbController _controller;

        /// <summary>
        /// Gets the associated controller.
        /// </summary>
        public GdbController Controller
        {
            get { return _controller; }
        }

        private ManualResetEvent _done = new ManualResetEvent(false);

        /// <summary>
        /// Creates a new instance of the <see cref="CommandBase"/> class.
        /// </summary>
        /// <param name="controller"></param>
        public CommandBase(GdbController controller)
        {
            _controller = controller;

        }

        public T Send()
        {
            _done.Reset();
            Execute();
            _done.WaitOne();
            return _result;
        }

        protected abstract void Execute();

        private T _result;
        protected void Done(T result)
        {
            _result = result;
            _done.Set();
        }
    }
}
