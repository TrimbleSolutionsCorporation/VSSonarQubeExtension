// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionOptionsWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for PluginOptionsWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VSSonarExtensionUi.View.Configuration
{
    using VSSonarExtensionUi.ViewModel.Configuration;

    /// <summary>
    ///     Interaction logic for PluginOptionsWindow.xaml
    /// </summary>
    public partial class ExtensionOptionsWindow
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExtensionOptionsWindow" /> class.
        /// </summary>
        public ExtensionOptionsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionOptionsWindow"/> class.
        /// </summary>
        /// <param name="dataViewModel">
        /// The data view model.
        /// </param>
        public ExtensionOptionsWindow(VSonarQubeOptionsViewModel dataViewModel)
        {
            this.InitializeComponent();
            if (dataViewModel != null)
            {
                dataViewModel.RequestClose += (s, e) => this.Close();
            }

            this.DataContext = dataViewModel;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The refresh data context.
        /// </summary>
        /// <param name="vSonarQubeOptionsViewData">
        /// The v sonar qube options view data.
        /// </param>
        public void RefreshDataContext(VSonarQubeOptionsViewModel vSonarQubeOptionsViewData)
        {
            if (vSonarQubeOptionsViewData != null)
            {
                vSonarQubeOptionsViewData.RequestClose += (s, e) => this.Close();
            }

            this.DataContext = null;
            this.DataContext = vSonarQubeOptionsViewData;
        }

        #endregion
    }
}