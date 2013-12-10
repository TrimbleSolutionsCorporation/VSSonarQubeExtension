// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BufferProvider.cs" company="Trimble Navigation Limited">
//     Copyright (C) 2013 [Jorge Costa, Jorge.Costa@trimble.com]
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

namespace VSSonarExtension.SmartTags.BufferUpdate
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Windows;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// Export a <see cref="ITaggerProvider"/>
    /// </summary>
    [Export(typeof(ITaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(IClassificationTag))]
    public class BufferUpdateProvider : ITaggerProvider
    {
        /// <summary>
        /// The all buffer taggger.
        /// </summary>
        public static readonly Dictionary<string, BufferTagger> AllBufferTaggger = new Dictionary<string, BufferTagger>();

        /// <summary>
        /// The create tagger.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <typeparam name="T">
        /// the tag
        /// </typeparam>
        /// <returns>
        /// The Microsoft.VisualStudio.Text.Tagging.ITagger`1[T -&gt; T].
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// buffer is null
        /// </exception>
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            var filePath = string.Empty;

            try
            {
                var document = BufferTagger.GetPropertyFromBuffer<ITextDocument>(buffer);

                filePath = document.FilePath;

                BufferTagger taginstance;

                if (!AllBufferTaggger.ContainsKey(filePath))
                {
                    taginstance = new BufferTagger(buffer, filePath, true);
                    AllBufferTaggger.Add(document.FilePath, taginstance);
                }
                else
                {
                    taginstance = new BufferTagger(buffer, filePath, false);
                }

                return taginstance as ITagger<T>;
            }
            catch (Exception ex)
            {
                if (!PackageImplementation.VsSonarExtensionPackage.ExtensionModelData.DisableEditorTags)
                {
                    MessageBox.Show("Ups Something Went Wrong: " + ex.Message + "\r\n" + "StackTrace: " + ex.StackTrace + "\r\n At FilePath: " + filePath);
                }

                var emptyTag = new BufferTagger();
                return emptyTag as ITagger<T>;
            }
        }
    }
}
