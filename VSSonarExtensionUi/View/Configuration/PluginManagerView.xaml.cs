// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginManagerView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for GeneralOptionsControl.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.View.Configuration
{
    using VSSonarExtension.MainView;

    using VSSonarExtensionUi.ViewModel.Configuration;

    /// <summary>
    ///     Interaction logic for GeneralOptionsControl.xaml
    /// </summary>
    public partial class PluginManagerView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManagerView"/> class. 
        ///     Initializes a new instance of the <see cref="GeneralOptionsControl"/> class.
        /// </summary>
        public PluginManagerView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManagerView"/> class. 
        /// Initializes a new instance of the <see cref="GeneralOptionsControl"/> class.
        /// </summary>
        /// <param name="controller">
        /// The controller.
        /// </param>
        public PluginManagerView(PluginManagerModel controller)
        {
            this.InitializeComponent();
            this.DataContext = controller;
        }

        #endregion
    }
}