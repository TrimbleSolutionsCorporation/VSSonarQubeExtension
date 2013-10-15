// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SquiggleTaggerProvider.cs" company="Copyright © 2013 Tekla Corporation. Tekla is a Trimble Company">
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
    using System.ComponentModel.Composition;

    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// The squiggle tagger.
    /// </summary>
    public partial class SquiggleTagger
    {
        /// <summary>
        /// The provider.
        /// </summary>
        [ContentType("text")]
        [Export(typeof(IViewTaggerProvider))]
        [TagType(typeof(IErrorTag))]
        public class Provider : IViewTaggerProvider
        {
            /// <summary>
            /// Gets or sets the factory service.
            /// </summary>
            [Import(typeof(IBufferTagAggregatorFactoryService))]
            public IBufferTagAggregatorFactoryService FactoryService { get; set; }

            /// <summary>
            /// The create tagger.
            /// </summary>
            /// <param name="textView">
            /// The text view.
            /// </param>
            /// <param name="buffer">
            /// The buffer.
            /// </param>
            /// <typeparam name="T">
            /// the Tag param
            /// </typeparam>
            /// <returns>
            /// The Microsoft.VisualStudio.Text.Tagging.ITagger`1[T -&gt; T].
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// textView and buffer
            /// </exception>
            /// <exception cref="InvalidOperationException">
            /// FactoryService is not set.
            /// </exception>
            public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
            {
                if (textView == null)
                {
                    throw new ArgumentNullException("textView");
                }

                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }

                if (this.FactoryService == null)
                {
                    throw new InvalidOperationException("FactoryService is not set.");
                }

                if (buffer != textView.TextBuffer)
                {
                    return null;
                }

                return (ITagger<T>)new SquiggleTagger(buffer, this.FactoryService.CreateTagAggregator<SonarTag>(buffer));
            }
        }
    }
}
