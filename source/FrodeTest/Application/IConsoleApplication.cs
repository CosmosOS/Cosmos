using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Application
{
    interface IConsoleApplication
    {
        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>0 for success, negative number for general error</returns>
        int Execute(string[] args);

        /// <summary>
        /// The single-word command.
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// A descriptive text for this command.
        /// </summary>
        string Description { get; }
    }
}
