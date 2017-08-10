using System;
using System.Collections.Generic;
using System.Text;
using PlugViewer.TreeViewNodes;

namespace PlugViewer.Errors
{
    /// <summary>
    /// The base class for all errors.
    /// </summary>
    public abstract class BaseError
    {
        /// <summary>
        /// The type of node this error can apply to.
        /// </summary>
        public abstract TreeNodeType AppliesTo { get; }
        /// <summary>
        /// Evaluate the given node.
        /// </summary>
        /// <param name="node">The node to evaluate.</param>
        public abstract void EvaluateNode(OTreeNode node);
        /// <summary>
        /// Gets the name of the error.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Gets the description of the error,
        /// and how it applies in the current situation.
        /// </summary>
        public abstract string Description { get; }
    }
}
