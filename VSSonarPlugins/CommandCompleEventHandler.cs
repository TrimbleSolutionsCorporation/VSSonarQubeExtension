
using System;
using System.Collections.Generic;
using VSSonarPlugins.Types;

namespace VSSonarPlugins
{
    /// <summary>
    /// command complete event handler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The arguments.</param>
    public delegate void CommandCompleEventHandler(object sender, CommandCompleEventHandlerArgs args);

    public class CommandCompleEventHandlerArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the issues.
        /// </summary>
        /// <value>
        /// The issues.
        /// </value>
        public List<Issue> Issues { get; set; }
    }
}