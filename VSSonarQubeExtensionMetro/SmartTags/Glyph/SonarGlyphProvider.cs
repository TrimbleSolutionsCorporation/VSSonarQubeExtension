// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarGlyphProvider.cs" company="">
//   
// </copyright>
// <summary>
//   Export a <see cref="ITaggerProvider" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TeklaOy.VSSonarExtension.SmartTags.Glyph
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    using TeklaOy.VSSonarExtension.SmartTags.BufferUpdate;

    /// <summary>
    /// Export a <see cref="ITaggerProvider"/>
    /// </summary>
    [Export(typeof(ITaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(SonarGlyphTag))]
    public class SonarGlyphProvider : ITaggerProvider
    {
        /// <summary>
        /// The all spelling tags.
        /// </summary>
        public static readonly Dictionary<string, SonarGlyphTagger> AllSpellingTags = new Dictionary<string, SonarGlyphTagger>();

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

            var document = BufferTagger.GetPropertyFromBuffer<ITextDocument>(buffer);

            SonarGlyphTagger taginstance;

            if (!AllSpellingTags.ContainsKey(document.FilePath))
            {
                taginstance = new SonarGlyphTagger(buffer, true, document.FilePath);
                AllSpellingTags.Add(document.FilePath, taginstance);
            }
            else
            {
                taginstance = new SonarGlyphTagger(buffer, false, document.FilePath);
            }

            return taginstance as ITagger<T>;
        }
    }
}
