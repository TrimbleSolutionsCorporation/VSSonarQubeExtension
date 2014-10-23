// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneralOptionsControlView.xaml.cs" company="">
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
    public partial class GeneralOptionsControlView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptionsControlView"/> class.
        /// </summary>
        public GeneralOptionsControlView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptionsControlView"/> class. 
        /// Initializes a new instance of the <see cref="GeneralOptionsControl"/> class.
        /// </summary>
        /// <param name="controller">
        /// The controller.
        /// </param>
        public GeneralOptionsControlView(AnalysisOptions controller)
        {
            this.InitializeComponent();
            this.DataContext = controller;
        }

        #endregion
    }
}