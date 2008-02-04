using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Indy.IL2CPU;

namespace Cosmos.Build.Tasks
{
    public class IL2CPU : Task
    {
        private TaskItem[] plugs;
        /// <summary>
        /// Gets or sets the plugs files to include in the build.
        /// </summary>
        public TaskItem[] Plugs
        {
            get { return plugs; }
            set { plugs = value; }
        }

        public override bool Execute()
        {
            Engine e = new Engine();


            return true;
        }
    }
}
