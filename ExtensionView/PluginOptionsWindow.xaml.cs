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
    using System.Windows.Input;

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

        /// <summary>
        /// The mouse button event handler.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
