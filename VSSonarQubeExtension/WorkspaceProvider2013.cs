namespace VSSonarQubeExtension
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;

    /// <summary>
    /// workspace provider
    /// </summary>
    public static class WorspaceProvider
    {
        /// <summary>
        /// Provides the current roslyn solution.
        /// </summary>
        /// <returns></returns>
        internal static Solution ProvideCurrentRoslynSolution()
        {
            return null;
        }
    }
}
