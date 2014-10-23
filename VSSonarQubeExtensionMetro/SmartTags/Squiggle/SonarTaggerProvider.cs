// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarTaggerProvider.cs" company="Copyright © 2014 Tekla Corporation. Tekla is a Trimble Company">
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
namespace VSSonarQubeExtension.SmartTags.Squiggle
{
    using System;
    using System.ComponentModel.Composition;

    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Operations;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// The sonar tagger.
    /// </summary>
    public partial class SonarTagger
    {
        /// <summary>
        /// The sonar tagger provider.
        /// </summary>
        [ContentType("text")]
        [Export(typeof(IViewTaggerProvider))]
        [TagType(typeof(SonarTag))]
        public class SonarTaggerProvider : IViewTaggerProvider
        {
            /// <summary>
            /// Gets or sets the service provider.
            /// </summary>
            [Import]
            public SVsServiceProvider ServiceProvider { get; set; }

            /// <summary>
            /// Gets or sets the text search service.
            /// </summary>
            [Import]
            internal ITextSearchService TextSearchService { get; set; }

            /// <summary>
            /// Gets or sets the text structure navigator selector.
            /// </summary>
            [Import]
            internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }

            /// <summary>
            /// The create tagger.
            /// </summary>
            /// <param name="textView">
            /// The text View.
            /// </param>
            /// <param name="buffer">
            /// The buffer.
            /// </param>
            /// <typeparam name="T">
            /// the itag
            /// </typeparam>
            /// <returns>
            /// The Microsoft.VisualStudio.Text.Tagging.ITagger`1[T -&gt; T].
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// buffer is null
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// ServiceProvider has not been set.
            /// </exception>
            public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }

                if (this.ServiceProvider == null)
                {
                    throw new InvalidOperationException("ServiceProvider has not been set.");
                }

                return buffer.Properties.GetOrCreateSingletonProperty(() => new SonarTagger(buffer)) as ITagger<T>;
            }
        }
    }
}
