// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginOptionsWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for PluginOptionsWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VSSonarExtensionUi.View
{
    using VSSonarExtensionUi.ViewModel.Configuration;

    /// <summary>
    /// Interaction logic for PluginOptionsWindow.xaml
    /// </summary>
    public partial class ExtensionOptionsWindow
    {

        public ExtensionOptionsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginOptionsWindow"/> class.
        /// </summary>
        /// <param name="dataViewModel">
        /// The data viewModel.
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

        public void RefreshDataContext(VSonarQubeOptionsViewModel vSonarQubeOptionsViewData)
        {
            if (vSonarQubeOptionsViewData != null)
            {
                vSonarQubeOptionsViewData.RequestClose += (s, e) => this.Close();
            }

            this.DataContext = null;
            this.DataContext = vSonarQubeOptionsViewData;
        }
    }
}
