// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarGlyphFactoryProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The to do glyph factory provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TeklaOy.VSSonarExtension.SmartTags.Glyph
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// The glyph factory provider.
    /// </summary>
    [Export(typeof(IGlyphFactoryProvider))]
    [Name("SonarGlyph")]
    [Order(After = "VsTextMarker")]
    [ContentType("code")]
    [TagType(typeof(SonarGlyphTag))]
    internal sealed class GlyphFactoryProvider : IGlyphFactoryProvider
    {
        /// <summary>
        /// The get glyph factory.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        /// <param name="margin">
        /// The margin.
        /// </param>
        /// <returns>
        /// The Microsoft.VisualStudio.Text.Editor.IGlyphFactory.
        /// </returns>
        public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
        {
            return new SonarGlyphFactory();
        }
    }
}
