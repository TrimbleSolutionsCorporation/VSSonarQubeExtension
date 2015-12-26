namespace VSSonarPlugins.Types
{
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// holds language and diagnostic
    /// </summary>
    public class DiagnosticAnalyzerType
    {
        /// <summary>
        /// Gets or sets the descriptor.
        /// </summary>
        /// <value>
        /// The descriptor.
        /// </value>
        public DiagnosticAnalyzer Diagnostic { get; set; }

        /// <summary>
        /// Gets or sets the languages.
        /// </summary>
        /// <value>
        /// The languages.
        /// </value>
        public string[] Languages { get; set; }
    }
}