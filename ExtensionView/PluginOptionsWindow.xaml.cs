// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginOptionsWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for PluginOptionsWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ExtensionView
{
    using ExtensionViewModel.ViewModel;

    /// <summary>
    /// Interaction logic for PluginOptionsWindow.xaml
    /// </summary>
    public partial class PluginOptionsWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginOptionsWindow"/> class.
        /// </summary>
        /// <param name="dataModel">
        /// The data model.
        /// </param>
        public PluginOptionsWindow(PluginsOptionsModel dataModel)
        {
            this.InitializeComponent();
            dataModel.RequestClose += (s, e) => this.Close();
            this.DataContext = dataModel;
        }
    }
}
