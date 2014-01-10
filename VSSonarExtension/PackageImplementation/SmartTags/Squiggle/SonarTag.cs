// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarTag.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@tekla.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtension.SmartTags.Squiggle
{
    using System;
    using System.Collections.Generic;

    using ExtensionTypes;

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;

    /// <summary>
    /// The spelling tag.
    /// </summary>
    public partial class SonarTag : IErrorTag
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        public const string Identifier = "Sonar Warning";

        /// <summary>
        /// The _ suggestions.
        /// </summary>
        private IEnumerable<string> suggestions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarTag"/> class.
        /// </summary>
        /// <param name="issues">
        /// The issues.
        /// </param>
        /// <param name="span">
        /// The span.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// serviceProvider, trackingSpan and violation
        /// </exception>
        public SonarTag(List<Issue> issues, SnapshotSpan span)
        {
            if (issues == null)
            {
                throw new ArgumentNullException("issues");
            }

            this.Span = span.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
            this.TagIssue = issues;
        }

        /// <summary>
        /// Gets the span.
        /// </summary>
        public ITrackingSpan Span { get; private set; }

        /// <summary>
        /// Gets the misspelled word.
        /// </summary>
        public List<Issue> TagIssue { get; private set; }

        /// <summary>
        /// Gets the error type.
        /// </summary>
        public string ErrorType
        {
            get { return Identifier; }
        }

        /// <summary>
        /// Gets the suggestions.
        /// </summary>
        public IEnumerable<string> Suggestions
        {
            get { return this.suggestions ?? (this.suggestions = this.CreateSuggestions()); }
        }

        /// <summary>
        /// Gets the tool tip content.
        /// </summary>
        public object ToolTipContent
        {
            get
            {
                var violationstxt = string.Empty;
                foreach (var issue in this.TagIssue)
                {
                    violationstxt += "Sonar violation : " + issue.Rule + " : " + issue.Severity + "\r\n";
                    violationstxt += "Rule Name : " + issue.Rule + "\r\n";
                    violationstxt += "Violation Msg : " + issue.Message + "\r\n";
                }

                return string.Format("{0}", violationstxt);
            }
        }

        /// <summary>
        /// The to tag span.
        /// </summary>
        /// <param name="snapshot">
        /// The snapshot.
        /// </param>
        /// <returns>
        /// The <see>
        ///     <cref>ITagSpan</cref>
        /// </see>
        ///     .
        /// </returns>
        public ITagSpan<SonarTag> ToTagSpan(ITextSnapshot snapshot)
        {
            return new TagSpan<SonarTag>(this.Span.GetSpan(snapshot), this);
        }

        /// <summary>
        /// The create mapped tag span when intersects.
        /// </summary>
        /// <param name="spans">
        /// The spans.
        /// </param>
        /// <returns>
        /// The Microsoft.VisualStudio.Text.Tagging.ITagSpan`1[T -&gt; DigitalSamurai.SpellSharp.Vsx.SonarTag].
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// spans error
        /// </exception>
        public ITagSpan<SonarTag> CreateMappedTagSpanWhenIntersects(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null)
            {
                throw new ArgumentNullException("spans");
            }

            if (spans.Count == 0)
            {
                return null;
            }

            return new TagSpan<SonarTag>(spans[0], this);
        }

        /// <summary>
        /// The create suggestions.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; System.String].
        /// </returns>
        private IEnumerable<string> CreateSuggestions()
        {
            var createSuggestions = new List<string> { "Suggestions to By Implemented" };
            createSuggestions.Sort();
            return createSuggestions;
        }
    }
}
