// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisOptionsView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for GeneralOptionsControl.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.View.Configuration
{
    using VSSonarExtensionUi.ViewModel.Configuration;

    /// <summary>
    ///     Interaction logic for GeneralOptionsControl.xaml
    /// </summary>
    public partial class AnalysisOptionsView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisOptionsView"/> class.
        /// </summary>
        public AnalysisOptionsView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisOptionsView"/> class.
        /// </summary>
        /// <param name="controller">
        /// The controller.
        /// </param>
        public AnalysisOptionsView(AnalysisOptionsViewModel controller)
        {
            this.InitializeComponent();
            this.DataContext = controller;
        }

        #endregion
    }
}