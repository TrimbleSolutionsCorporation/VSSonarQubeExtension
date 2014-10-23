// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOptionsViewModelBase.cs" company="">
//   
// </copyright>
// <summary>
//   The OptionsViewModelBase interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtensionUi.ViewModel.Helpers
{
    using ExtensionTypes;

    /// <summary>
    /// The OptionsViewModelBase interface.
    /// </summary>
    public interface IOptionsViewModelBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The apply.
        /// </summary>
        void Apply();

        /// <summary>
        /// The end data association.
        /// </summary>
        void EndDataAssociation();

        /// <summary>
        /// The exit.
        /// </summary>
        void Exit();

        /// <summary>
        /// The init data association.
        /// </summary>
        /// <param name="associatedProject">
        /// The associated project.
        /// </param>
        /// <param name="userConnectionConfig">
        /// The user connection config.
        /// </param>
        /// <param name="workingDir">
        /// The working dir.
        /// </param>
        void InitDataAssociation(Resource associatedProject, ISonarConfiguration userConnectionConfig, string workingDir);

        /// <summary>
        /// The save and close.
        /// </summary>
        void SaveAndClose();

        #endregion
    }
}