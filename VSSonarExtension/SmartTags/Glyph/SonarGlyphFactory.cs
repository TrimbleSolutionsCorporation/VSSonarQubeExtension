// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SonarGlyphFactory.cs" company="">
//   
// </copyright>
// <summary>
//   This class implements IGlyphFactory, which provides the visual
//   element that will appear in the glyph margin.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TeklaOy.VSSonarExtension.SmartTags.Glyph
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Formatting;

    /// <summary>
    /// This class implements IGlyphFactory, which provides the visual
    /// element that will appear in the glyph margin.
    /// </summary>
    internal class SonarGlyphFactory : IGlyphFactory
    {
        /// <summary>
        /// The glyph size.
        /// </summary>
        private const double GlyphSize = 10.0;

        /// <summary>
        /// The generate glyph.
        /// </summary>
        /// <param name="line">
        /// The line.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <returns>
        /// The System.Windows.UIElement.
        /// </returns>
        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            if (tag == null || !(tag is SonarGlyphTag))
            {
                return null;
            }

            var ellipse = new Rectangle
                              {
                                  Fill = Brushes.Green,
                                  StrokeThickness = 1,
                                  Stroke = Brushes.Red,
                                  Height = GlyphSize,
                                  Width = GlyphSize
                              };

            return ellipse;
        }
    }
}
