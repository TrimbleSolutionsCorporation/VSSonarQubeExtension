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
    using System.Windows.Input;
    using VSSonarExtensionUi.ViewModel;


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
        /// <param name="dataModel">
        /// The data model.
        /// </param>
        public ExtensionOptionsWindow(ExtensionOptionsModel dataModel)
        {
            this.InitializeComponent();
            if (dataModel != null)
            {
                dataModel.RequestClose += (s, e) => this.Close();
            }            
            this.DataContext = dataModel;
        }

        public void RefreshDataContext(ExtensionOptionsModel extensionOptionsData)
        {
            if (extensionOptionsData != null)
            {
                extensionOptionsData.RequestClose += (s, e) => this.Close();
            }

            this.DataContext = null;
            this.DataContext = extensionOptionsData;
        }
    }
}
