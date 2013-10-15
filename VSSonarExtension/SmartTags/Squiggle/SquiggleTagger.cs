// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SquiggleTagger.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;

    /// <summary>
    /// The squiggle tagger.
    /// </summary>
    public partial class SquiggleTagger : ITagger<IErrorTag>, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SquiggleTagger"/> class.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <param name="tagAggregator">
        /// The tag aggregator.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// buffer and tagAggregator are null
        /// </exception>
        public SquiggleTagger(ITextBuffer buffer, ITagAggregator<SonarTag> tagAggregator)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (tagAggregator == null)
            {
                throw new ArgumentNullException("tagAggregator");
            }

            this.Buffer = buffer;

            this.SonarTagAggregator = tagAggregator;
            this.SonarTagAggregator.TagsChanged += this.OnSonarAggregatorTagsChanged;
        }

        /// <summary>
        /// The tags changed.
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        public ITextBuffer Buffer { get; private set; }

        /// <summary>
        /// Gets the sonar tag aggregator.
        /// </summary>
        public ITagAggregator<SonarTag> SonarTagAggregator { get; private set; }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The get tags.
        /// </summary>
        /// <param name="spans">
        /// The spans.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Microsoft.VisualStudio.Text.Tagging.ITagSpan`1[T -&gt; Microsoft.VisualStudio.Text.Tagging.IErrorTag]].
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// spans are null
        /// </exception>
        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null)
            {
                throw new ArgumentNullException("spans");
            }

            if (spans.Count == 0)
            {
                yield break;
            }

            foreach (var tagSpan in this.SonarTagAggregator.GetTags(spans))
            {
                var line = tagSpan.Tag.TagIssue[0].Line - 1;

                if (tagSpan.Tag.TagIssue[0].Line == 0)
                {
                    line = tagSpan.Tag.TagIssue[0].Line;
                }

                ITextSnapshotLine textsnapshot;

                try
                {
                    textsnapshot = this.Buffer.CurrentSnapshot.GetLineFromLineNumber(line);
                }
                catch (Exception)
                {
                    yield break;
                }

                var mappedSpan = new SnapshotSpan(this.Buffer.CurrentSnapshot, textsnapshot.Start, textsnapshot.Length);

                yield return new TagSpan<IErrorTag>(mappedSpan, tagSpan.Tag);
            }
        }

        /// <summary>
        /// The on sonar aggregator tags changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// eventArgs are null
        /// </exception>
        public void OnSonarAggregatorTagsChanged(object sender, TagsChangedEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                throw new ArgumentNullException("eventArgs");
            }

            foreach (var span in eventArgs.Span.GetSpans(this.Buffer))
            {
                this.RaiseTagsChangedEvent(span);
            }
        }

        /// <summary>
        /// The raise tags changed event.
        /// </summary>
        /// <param name="snapshotSpan">
        /// The snapshot span.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// snapshotSpan is null
        /// </exception>
        protected void RaiseTagsChangedEvent(SnapshotSpan snapshotSpan)
        {
            if (snapshotSpan == null)
            {
                throw new ArgumentNullException("snapshotSpan");
            }

            var handler = this.TagsChanged;
            if (handler != null)
            {
                handler(this, new SnapshotSpanEventArgs(snapshotSpan));
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!disposing || this.SonarTagAggregator == null)
            {
                return;
            }

            this.SonarTagAggregator.TagsChanged -= this.OnSonarAggregatorTagsChanged;
            this.SonarTagAggregator = null;
        }
    }
}
