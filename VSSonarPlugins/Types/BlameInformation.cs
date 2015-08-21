using System;
using System.Collections.Generic;

namespace VSSonarPlugins.Types
{
    /// <summary>
    /// Blame Line Hunk
    /// </summary>
    public class BlameLine
    {
        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        /// <value>
        /// The author.
        /// </value>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the commit date.
        /// </summary>
        /// <value>
        /// The commit date.
        /// </value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        /// <value>
        /// The line.
        /// </value>
        public int Line { get; set; }
    }

    /// <summary>
    /// blame information
    /// </summary>
    public interface IBlameInformation
    {
        /// <summary>
        /// Adds the blame line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="author">The author.</param>
        /// <param name="dateModified">The date modified.</param>
        void AddBlameLine(int line, string author, DateTime dateModified);

        /// <summary>
        /// Gets the blame by line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        BlameLine GetBlameByLine(int line);
    }
}