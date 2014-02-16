// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiffListTextFile.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// All credit goes to http://www.codeproject.com/Articles/6943/A-Generic-Reusable-Diff-Algorithm-in-C-II#_comments
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------

namespace DifferenceEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The diff list_ text file.
    /// </summary>
    public class DiffListTextFile : IDiffList
    {
        #region Constants

        /// <summary>
        /// The max line length.
        /// </summary>
        private const int MaxLineLength = 1024;

        #endregion

        #region Fields

        /// <summary>
        /// The _lines.
        /// </summary>
        private readonly ArrayList lines;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffListTextFile"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <exception>
        ///     <cref>InvalidOperationException</cref>
        /// </exception>
        public DiffListTextFile(string fileName)
        {
            this.lines = new ArrayList();
            using (var sr = new StreamReader(fileName))
            {
                string line;

                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length > MaxLineLength)
                    {
                        throw new InvalidOperationException(
                            string.Format("File contains a line greater than {0} characters.", MaxLineLength));
                    }

                    this.lines.Add(new TextLine(line));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffListTextFile"/> class.
        /// </summary>
        /// <param name="sourceData">
        /// The source data.
        /// </param>
        /// <param name="separator">
        /// The separator.
        /// </param>
        /// <exception>
        ///     <cref>InvalidOperationException</cref>
        /// </exception>
        public DiffListTextFile(string sourceData, string separator)
        {
            this.lines = new ArrayList();

            string[] linesAll = Regex.Split(sourceData, separator);

            foreach (string line in linesAll)
            {
                if (line.Length > MaxLineLength)
                {
                    throw new InvalidOperationException(string.Format("File contains a line greater than {0} characters.", MaxLineLength));
                }

                this.lines.Add(new TextLine(line));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffListTextFile"/> class.
        /// </summary>
        /// <param name="sourceData">
        /// The source data.
        /// </param>
        /// <exception>
        ///     <cref>InvalidOperationException</cref>
        /// </exception>
        public DiffListTextFile(IEnumerable<string> sourceData)
        {
            this.lines = new ArrayList();

            foreach (string line in sourceData)
            {
                if (line.Length > MaxLineLength)
                {
                    throw new InvalidOperationException(
                        string.Format("File contains a line greater than {0} characters.", MaxLineLength));
                }

                this.lines.Add(new TextLine(line));
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The count.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Count()
        {
            return this.lines.Count;
        }

        /// <summary>
        /// The get by index.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The <see cref="IComparable"/>.
        /// </returns>
        public IComparable GetByIndex(int index)
        {
            return (TextLine)this.lines[index];
        }

        #endregion
    }
}