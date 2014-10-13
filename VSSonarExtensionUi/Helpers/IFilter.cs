// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFilter.cs" company="">
//   
// </copyright>
// <summary>
//   The Filter interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.Helpers
{
    /// <summary>
    /// The Filter interface.
    /// </summary>
    public interface IFilter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The filter function.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool FilterFunction(object parameter);

        #endregion
    }
}